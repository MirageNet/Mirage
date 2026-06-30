using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1403
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSyntaxNodeAction(nodeContext => AnalyzeMemberAccess(nodeContext, symbols), SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, MirageSymbols symbols)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            if (memberAccess.Name.Identifier.Text == "enabled")
            {
                var typeSymbol = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (typeSymbol != null)
                {
                    if (SymbolEqualityComparer.Default.Equals(typeSymbol, symbols.NetworkServer) ||
                        SymbolEqualityComparer.Default.Equals(typeSymbol, symbols.NetworkClient) ||
                        SymbolEqualityComparer.Default.Equals(typeSymbol, symbols.NetworkIdentity))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.EnabledPropertyCheckRule,
                            memberAccess.Name.GetLocation(),
                            typeSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
