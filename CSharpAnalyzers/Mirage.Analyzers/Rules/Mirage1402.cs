using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1402
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (methodSymbol.Name != "OnSerialize" && methodSymbol.Name != "OnDeserialize")
                return;

            if (!methodSymbol.IsOverride)
                return;

            var containingClass = methodSymbol.ContainingType;
            if (containingClass == null || !symbols.IsOrInherits(containingClass, symbols.NetworkBehaviour))
                return;

            // Check if any base class has SyncVars or SyncObjects
            if (!HasBaseSyncState(containingClass.BaseType, symbols))
                return;

            // Check if the method body contains a call to the base method
            var syntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
            if (syntax == null)
                return;

            var hasBaseCall = syntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                            memberAccess.Expression is BaseExpressionSyntax &&
                            memberAccess.Name.Identifier.Text == methodSymbol.Name);

            if (!hasBaseCall)
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.LifecycleMissingBaseCallRule,
                    syntax.Identifier.GetLocation(),
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool HasBaseSyncState(INamedTypeSymbol? baseType, MirageSymbols symbols)
        {
            var current = baseType;
            while (current != null)
            {
                // Stop if we hit NetworkBehaviour itself
                if (SymbolEqualityComparer.Default.Equals(current, symbols.NetworkBehaviour))
                    break;

                foreach (var member in current.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic)
                    {
                        if (symbols.HasAttribute(field, symbols.SyncVarAttribute) ||
                            symbols.Implements(field.Type, symbols.ISyncObject))
                        {
                            return true;
                        }
                    }
                    else if (member is IPropertySymbol property && !property.IsStatic)
                    {
                        if (symbols.HasAttribute(property, symbols.SyncVarAttribute) ||
                            symbols.Implements(property.Type, symbols.ISyncObject))
                        {
                            return true;
                        }
                    }
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
