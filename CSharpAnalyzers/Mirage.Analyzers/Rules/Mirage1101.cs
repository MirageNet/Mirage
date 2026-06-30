using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1101
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeSymbol(symbolContext, symbols), SymbolKind.Field, SymbolKind.Method, SymbolKind.Property);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var symbol = context.Symbol;
            var declaringType = symbol.ContainingType;
            if (declaringType == null)
                return;

            if (symbols.IsOrInherits(declaringType, symbols.NetworkBehaviour))
                return;

            var networkAttributes = new[]
            {
                symbols.SyncVarAttribute,
                symbols.ServerAttribute,
                symbols.ClientAttribute,
                symbols.HasAuthorityAttribute,
                symbols.LocalPlayerAttribute,
                symbols.ServerRpcAttribute,
                symbols.ClientRpcAttribute,
                symbols.NetworkMethodAttribute
            };

            foreach (var attrType in networkAttributes)
            {
                if (attrType != null && symbols.TryGetAttribute(symbol, attrType, out var attrData))
                {
                    var location = attrData.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? symbol.Locations[0];
                    var diagnostic = Diagnostic.Create(
                        MirageRules.NetworkBehaviourAttributeRule,
                        location,
                        attrData.AttributeClass?.Name ?? attrType.Name,
                        symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
