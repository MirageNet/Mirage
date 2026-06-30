using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mirage.Analyzers
{
    public static class Helpers
    {
        public static bool IsInsideConstructor(SyntaxNode node)
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

        public static ExpressionSyntax? GetMutationTarget(SyntaxNode node)
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

        public static bool IsBasicSafeType(ITypeSymbol typeSymbol, MirageSymbols symbols)
        {
            if (typeSymbol == null)
                return true;

            if (typeSymbol.IsValueType || typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Enum)
                return true;

            if (typeSymbol.SpecialType == SpecialType.System_String)
                return true;

            if (symbols.IsOrInherits(typeSymbol, symbols.NetworkIdentity))
                return true;

            if (symbols.IsOrInherits(typeSymbol, symbols.GameObject))
                return true;

            if (symbols.IsOrInherits(typeSymbol, symbols.NetworkBehaviour))
                return true;

            return false;
        }

        public static bool IsExplicitlyMarkedSafe(ISymbol symbol, ITypeSymbol typeSymbol, MirageSymbols symbols)
        {
            if (symbols.HasAttribute(symbol, symbols.WeaverSafeClassAttribute))
                return true;

            if (typeSymbol != null && symbols.HasAttribute(typeSymbol, symbols.WeaverSafeClassAttribute))
                return true;

            if (symbol.ContainingType != null && symbols.HasAttribute(symbol.ContainingType, symbols.WeaverSafeClassAttribute))
                return true;

            return false;
        }

        public static bool IsRpcMethod(IMethodSymbol methodSymbol, MirageSymbols symbols)
        {
            return symbols.HasAttribute(methodSymbol, symbols.ServerRpcAttribute) || symbols.HasAttribute(methodSymbol, symbols.ClientRpcAttribute);
        }

        public static bool IsVoidOrUniTask(ITypeSymbol typeSymbol, MirageSymbols symbols)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.SpecialType == SpecialType.System_Void)
                return true;

            if (symbols.UniTask != null && SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, symbols.UniTask))
                return true;

            return false;
        }

        public static bool IsGenericUniTask(ITypeSymbol type, out ITypeSymbol? wrappedType)
        {
            wrappedType = null;
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.Name == "UniTask" && namedType.ContainingNamespace?.ToDisplayString() == "Cysharp.Threading.Tasks")
            {
                wrappedType = namedType.TypeArguments[0];
                return true;
            }
            return false;
        }

        public static bool IsAutoProperty(IPropertySymbol propertySymbol)
        {
            var backingFieldName = $"<{propertySymbol.Name}>k__BackingField";
            foreach (var member in propertySymbol.ContainingType.GetMembers())
            {
                if (member is IFieldSymbol field && field.IsImplicitlyDeclared && field.Name == backingFieldName)
                    return true;
            }
            return false;
        }

        public static bool IsSyncListOrDictionary(ITypeSymbol typeSymbol, MirageSymbols symbols)
        {
            return symbols.IsOrInherits(typeSymbol, symbols.SyncList) ||
                   symbols.IsOrInherits(typeSymbol, symbols.SyncDictionary) ||
                   symbols.IsOrInherits(typeSymbol, symbols.SyncIDictionary);
        }

        public static bool IsNetworkWriter(ITypeSymbol type, MirageSymbols symbols)
        {
            return symbols.IsOrInherits(type, symbols.NetworkWriter);
        }

        public static bool IsNetworkReader(ITypeSymbol type, MirageSymbols symbols)
        {
            return symbols.IsOrInherits(type, symbols.NetworkReader);
        }
    }
}
