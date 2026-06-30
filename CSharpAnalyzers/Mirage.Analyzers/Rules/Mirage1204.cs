using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1204
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (Helpers.IsRpcMethod(methodSymbol, symbols) && methodSymbol.IsStatic)
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.RpcStaticRule,
                    methodSymbol.Locations[0],
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
