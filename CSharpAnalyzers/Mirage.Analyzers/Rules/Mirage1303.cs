using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1303
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (symbols.CustomWriters.ContainsValue(methodSymbol))
            {
                var type = methodSymbol.Parameters[1].Type;
                if (!symbols.CustomReaders.ContainsKey(type))
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.MismatchedSerializationRule,
                        methodSymbol.Locations[0],
                        type.ToDisplayString(),
                        $"Custom writer defined for '{type.ToDisplayString()}' but matching custom reader is missing.");
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else if (symbols.CustomReaders.ContainsValue(methodSymbol))
            {
                var type = methodSymbol.ReturnType;
                if (!symbols.CustomWriters.ContainsKey(type))
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.MismatchedSerializationRule,
                        methodSymbol.Locations[0],
                        type.ToDisplayString(),
                        $"Custom reader defined for '{type.ToDisplayString()}' but matching custom writer is missing.");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
