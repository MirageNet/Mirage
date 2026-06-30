using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1004
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, symbols), SymbolKind.Field);
        }

        private static void AnalyzeField(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (symbols.TryGetAttribute(fieldSymbol, symbols.SyncVarAttribute, out var syncVarAttr))
            {
                if (MirageSymbols.TryGetNamedArgument<string>(syncVarAttr, "hook", out var hookName) && !string.IsNullOrEmpty(hookName))
                {
                    var declaringType = fieldSymbol.ContainingType;
                    if (declaringType == null)
                        return;

                    var methods = declaringType.GetMembers(hookName).OfType<IMethodSymbol>().ToList();
                    if (methods.Count == 0)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.SyncVarHookRule,
                            syncVarAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? fieldSymbol.Locations[0],
                            hookName,
                            $"could not find hook method with name '{hookName}'");
                        context.ReportDiagnostic(diagnostic);
                        return;
                    }

                    var method = methods[0];
                    if (method.IsStatic)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.SyncVarHookRule,
                            syncVarAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? fieldSymbol.Locations[0],
                            hookName,
                            "hook method cannot be static");
                        context.ReportDiagnostic(diagnostic);
                        return;
                    }

                    var fieldType = fieldSymbol.Type;
                    var parameters = method.Parameters;

                    var isValid = false;
                    if (parameters.Length == 1)
                    {
                        if (SymbolEqualityComparer.Default.Equals(parameters[0].Type, fieldType))
                            isValid = true;
                    }
                    else if (parameters.Length == 2)
                    {
                        if (SymbolEqualityComparer.Default.Equals(parameters[0].Type, fieldType) &&
                            SymbolEqualityComparer.Default.Equals(parameters[1].Type, fieldType))
                        {
                            isValid = true;
                        }
                    }

                    if (!isValid)
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.SyncVarHookRule,
                            syncVarAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? fieldSymbol.Locations[0],
                            hookName,
                            $"parameter type mismatch (must be '{fieldType.Name}' or '{fieldType.Name}, {fieldType.Name}')");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
