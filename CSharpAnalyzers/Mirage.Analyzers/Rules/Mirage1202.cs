using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1202
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
                if (methodSymbol.IsGenericMethod)
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.RpcSignatureRule,
                        methodSymbol.Locations[0],
                        methodSymbol.Name,
                        "cannot have generic parameters");
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                if (!methodSymbol.ReturnsVoid && !Helpers.IsGenericUniTask(methodSymbol.ReturnType, out _))
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.RpcSignatureRule,
                        methodSymbol.Locations[0],
                        methodSymbol.Name,
                        $"cannot return '{methodSymbol.ReturnType.ToDisplayString()}' (must return void or UniTask<T>)");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
