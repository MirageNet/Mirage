using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1203
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (Helpers.IsRpcMethod(methodSymbol, symbols))
            {
                foreach (var parameter in methodSymbol.Parameters)
                {
                    if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.RpcRefOutRule,
                            parameter.Locations[0],
                            methodSymbol.Name,
                            parameter.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
