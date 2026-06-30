using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1305
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSyntaxNodeAction(nodeContext => AnalyzeInvocation(nodeContext, symbols), SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, MirageSymbols symbols)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol && methodSymbol.IsGenericMethod)
            {
                if (methodSymbol.Name == "Send" || methodSymbol.Name == "RegisterHandler" ||
                    methodSymbol.Name == "UnregisterHandler" || methodSymbol.Name == "SendToAll" ||
                    methodSymbol.Name == "SendToMany" || methodSymbol.Name == "Pack" ||
                    methodSymbol.Name == "Unpack" || methodSymbol.Name == "GetId")
                {
                    var typeArgument = methodSymbol.TypeArguments[0];
                    if (IsCustomType(typeArgument) && !symbols.HasAttribute(typeArgument, symbols.NetworkMessageAttribute))
                    {
                        var location = GetTypeArgumentLocation(invocation);
                        var diagnostic = Diagnostic.Create(
                            MirageRules.MissingNetworkMessageRule,
                            location,
                            typeArgument.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsCustomType(ITypeSymbol type)
        {
            if (type == null)
                return false;

            if (type.SpecialType != SpecialType.None)
                return false;

            var ns = type.ContainingNamespace?.ToDisplayString();
            if (ns == "System" || ns?.StartsWith("System.") == true)
                return false;

            return true;
        }

        private static Location GetTypeArgumentLocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName && genericName.TypeArgumentList.Arguments.Count > 0)
                {
                    return genericName.TypeArgumentList.Arguments[0].GetLocation();
                }
            }
            return invocation.GetLocation();
        }
    }
}
