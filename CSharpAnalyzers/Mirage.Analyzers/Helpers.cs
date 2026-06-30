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

        public static bool IsBasicSafeType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return true;

            if (typeSymbol.IsValueType || typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Enum)
                return true;

            if (typeSymbol.SpecialType == SpecialType.System_String)
                return true;

            if (MirageTypes.NetworkIdentity.IsOrInherits(typeSymbol))
                return true;

            if (MirageTypes.GameObject.IsOrInherits(typeSymbol))
                return true;

            if (MirageTypes.NetworkBehaviour.IsOrInherits(typeSymbol))
                return true;

            return false;
        }

        public static bool IsExplicitlyMarkedSafe(ISymbol symbol, ITypeSymbol typeSymbol)
        {
            if (MirageAttributes.WeaverSafeClass.Has(symbol))
                return true;

            if (typeSymbol != null && MirageAttributes.WeaverSafeClass.Has(typeSymbol))
                return true;

            if (symbol.ContainingType != null && MirageAttributes.WeaverSafeClass.Has(symbol.ContainingType))
                return true;

            return false;
        }

        public static bool IsRpcMethod(IMethodSymbol methodSymbol)
        {
            return MirageAttributes.ServerRpc.Has(methodSymbol) || MirageAttributes.ClientRpc.Has(methodSymbol);
        }

        public static bool IsVoidOrUniTask(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.SpecialType == SpecialType.System_Void)
                return true;

            if (MirageTypes.UniTask.Is(typeSymbol))
                return true;

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

        public static bool IsSyncListOrDictionary(ITypeSymbol typeSymbol)
        {
            return MirageTypes.SyncList.IsOrInherits(typeSymbol) ||
                   MirageTypes.SyncDictionary.IsOrInherits(typeSymbol) ||
                   MirageTypes.SyncIDictionary.IsOrInherits(typeSymbol);
        }

        public static bool IsNetworkWriter(ITypeSymbol type)
        {
            return MirageTypes.NetworkWriter.Is(type);
        }

        public static bool IsNetworkReader(ITypeSymbol type)
        {
            return MirageTypes.NetworkReader.Is(type);
        }
    }
}
