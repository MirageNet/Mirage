using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1206
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (Helpers.IsRpcMethod(methodSymbol, symbols) && symbols.TryGetAttribute(methodSymbol, symbols.RateLimitAttribute, out var rateLimitAttr))
            {
                var interval = 1.0f;
                var refill = 50;
                var maxTokens = 200;

                foreach (var arg in rateLimitAttr.NamedArguments)
                {
                    if (arg.Key == "Interval")
                    {
                        if (arg.Value.Value is float f)
                            interval = f;
                        else if (arg.Value.Value is double d)
                            interval = (float)d;
                    }
                    else if (arg.Key == "Refill" && arg.Value.Value is int r)
                    {
                        refill = r;
                    }
                    else if (arg.Key == "MaxTokens" && arg.Value.Value is int m)
                    {
                        maxTokens = m;
                    }
                }

                var errors = new List<string>();
                if (interval <= 0)
                    errors.Add("Interval must be greater than zero");
                if (refill <= 0)
                    errors.Add("Refill must be greater than zero");
                if (maxTokens <= 0)
                    errors.Add("MaxTokens must be greater than zero");

                if (refill > 0 && maxTokens > 0 && maxTokens < refill)
                    errors.Add("MaxTokens must be greater than or equal to Refill");

                if (errors.Count > 0)
                {
                    var message = string.Join(", ", errors);
                    var diagnostic = Diagnostic.Create(
                        MirageRules.RateLimitSettingsRule,
                        methodSymbol.Locations[0],
                        methodSymbol.Name,
                        message);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
