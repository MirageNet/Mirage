using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1102
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            var hasServerRpc = symbols.HasAttribute(methodSymbol, symbols.ServerRpcAttribute);
            var hasClientRpc = symbols.HasAttribute(methodSymbol, symbols.ClientRpcAttribute);

            if (hasServerRpc && symbols.TryGetAttribute(methodSymbol, symbols.ServerAttribute, out var serverAttr))
            {
                var location = serverAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? methodSymbol.Locations[0];
                var diagnostic = Diagnostic.Create(
                    MirageRules.RedundantRpcAttributeRule,
                    location,
                    "Server",
                    "ServerRpc",
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            if (hasClientRpc && symbols.TryGetAttribute(methodSymbol, symbols.ClientAttribute, out var clientAttr))
            {
                var location = clientAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? methodSymbol.Locations[0];
                var diagnostic = Diagnostic.Create(
                    MirageRules.RedundantRpcAttributeRule,
                    location,
                    "Client",
                    "ClientRpc",
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
