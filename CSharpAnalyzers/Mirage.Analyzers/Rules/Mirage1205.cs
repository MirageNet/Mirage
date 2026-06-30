using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1205
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (symbols.TryGetAttribute(methodSymbol, symbols.ClientRpcAttribute, out var clientRpcAttr))
            {
                var targetValue = 1; // Default to Observers (1)
                foreach (var arg in clientRpcAttr.NamedArguments)
                {
                    if (arg.Key == "target")
                    {
                        if (arg.Value.Value is int val)
                            targetValue = val;
                        break;
                    }
                }

                if (targetValue == 1) // Observers
                {
                    if (!methodSymbol.ReturnsVoid)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.ClientRpcTargetRule,
                            methodSymbol.Locations[0],
                            methodSymbol.Name,
                            "must return void when target is Observers");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else if (targetValue == 2) // Player
                {
                    var hasConnectionParam = methodSymbol.Parameters.Length > 0 &&
                                             symbols.IsNetworkPlayerOrConnection(methodSymbol.Parameters[0].Type);

                    if (!hasConnectionParam)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.ClientRpcTargetRule,
                            methodSymbol.Locations[0],
                            methodSymbol.Name,
                            "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
