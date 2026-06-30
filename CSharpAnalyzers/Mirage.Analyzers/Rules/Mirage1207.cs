using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1207
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            var containingType = methodSymbol.ContainingType;
            if (containingType == null || !symbols.IsOrInherits(containingType, symbols.NetworkBehaviour))
                return;

            if (symbols.HasAttribute(methodSymbol, symbols.ServerRpcAttribute) && !symbols.HasAttribute(methodSymbol, symbols.RateLimitAttribute))
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.ServerRpcMissingRateLimitRule,
                    methodSymbol.Locations[0],
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
