using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private static void AnalyzeFieldSyncVars(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            // Reassigning fields implementing ISyncObject is prohibited because doing so replaces the synchronized collection instance, breaking replication and weavers' internal reference tracking.
            // SyncObject fields must be readonly to guarantee their references remain constant after initialization, avoiding desynchronization.
            if (MirageTypes.ISyncObject.Implements(fieldSymbol.Type) && !fieldSymbol.IsReadOnly)
            {
                var diagnostic = Diagnostic.Create(MirageRules.ReassignmentRule, fieldSymbol.Locations[0], fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            AnalyzeNetworkAttributes(context, fieldSymbol);

            if (MirageAttributes.SyncVar.Has(fieldSymbol))
            {
                AnalyzeSyncVar(context, fieldSymbol, fieldSymbol.Type);
                return;
            }

            if (MirageTypes.ISyncObject.Implements(fieldSymbol.Type))
            {
                AnalyzeSyncObject(context, fieldSymbol, fieldSymbol.Type);
                return;
            }
        }

        private static void AnalyzePropertySyncVars(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            AnalyzeNetworkAttributes(context, propertySymbol);

            if (MirageAttributes.SyncVar.Has(propertySymbol))
            {
                if (propertySymbol.GetMethod == null || propertySymbol.SetMethod == null || propertySymbol.IsStatic || !IsAutoProperty(propertySymbol))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.AutoPropertyRule, propertySymbol.Locations[0], propertySymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                AnalyzeSyncVar(context, propertySymbol, propertySymbol.Type);
                return;
            }

            if (MirageTypes.ISyncObject.Implements(propertySymbol.Type))
            {
                AnalyzeSyncObject(context, propertySymbol, propertySymbol.Type);
                return;
            }
        }

        private static void AnalyzeSyncVar(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol typeSymbol)
        {
            if (IsBasicSafeType(typeSymbol))
                return;

            if (IsExplicitlyMarkedSafe(symbol, typeSymbol))
                return;

            var diagnostic = Diagnostic.Create(MirageRules.SyncVarRule, symbol.Locations[0], symbol.Name, typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static void AnalyzeSyncObject(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol typeSymbol)
        {
            var current = typeSymbol;
            while (current != null)
            {
                if (current is INamedTypeSymbol namedType && namedType.IsGenericType && MirageTypes.ISyncObject.Implements(namedType))
                {
                    foreach (var arg in namedType.TypeArguments)
                        AnalyzeSyncVar(context, symbol, arg);
                    break;
                }
                current = current.BaseType;
            }
        }

        private static void AnalyzeMutation(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;
            var target = GetMutationTarget(node);
            if (target == null)
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(target);
            var symbol = symbolInfo.Symbol;

            if (symbol is IFieldSymbol fieldSymbol && MirageTypes.ISyncObject.Implements(fieldSymbol.Type) && !IsInsideConstructor(node))
            {
                var diagnostic = Diagnostic.Create(MirageRules.ReassignmentRule, target.GetLocation(), fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            // Mutating members of elements inside SyncList or SyncDictionary directly does not trigger serialization or synchronization because the collection has no mechanism to detect nested modifications.
            if (target is MemberAccessExpressionSyntax memberAccess)
            {
                var current = memberAccess.Expression;
                while (current is MemberAccessExpressionSyntax nestedMember)
                    current = nestedMember.Expression;

                if (current is ElementAccessExpressionSyntax elementAccess)
                {
                    var collectionType = context.SemanticModel.GetTypeInfo(elementAccess.Expression).Type;
                    if (collectionType != null && IsSyncListOrDictionary(collectionType))
                    {
                        var diagnostic = Diagnostic.Create(MirageRules.DirectMutationRule, target.GetLocation(), elementAccess.Expression.ToString());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static ExpressionSyntax? GetMutationTarget(SyntaxNode node)
        {
            if (node is AssignmentExpressionSyntax assignment)
                return assignment.Left;

            if (node is PostfixUnaryExpressionSyntax postfix && (postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression)))
                return postfix.Operand;

            if (node is PrefixUnaryExpressionSyntax prefix && (prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression)))
                return prefix.Operand;

            if (node is ArgumentSyntax argument && (argument.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword) || argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword)))
                return argument.Expression;

            return null;
        }

        private static bool IsInsideConstructor(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is AnonymousFunctionExpressionSyntax || current is LocalFunctionStatementSyntax)
                    return false;

                if (current is ConstructorDeclarationSyntax)
                    return true;

                current = current.Parent;
            }
            return false;
        }

        private static bool IsSyncListOrDictionary(ITypeSymbol typeSymbol)
        {
            return MirageTypes.SyncList.IsOrInherits(typeSymbol) ||
                   MirageTypes.SyncDictionary.IsOrInherits(typeSymbol) ||
                   MirageTypes.SyncIDictionary.IsOrInherits(typeSymbol);
        }
    }
}
