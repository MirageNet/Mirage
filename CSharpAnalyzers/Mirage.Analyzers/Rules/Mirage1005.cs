using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1005
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, symbols), SymbolKind.Field);
        }

        private static void AnalyzeField(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (symbols.HasAttribute(fieldSymbol, symbols.SyncVarAttribute) && fieldSymbol.IsReadOnly)
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.ReadonlySyncVarRule,
                    fieldSymbol.Locations[0],
                    fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
