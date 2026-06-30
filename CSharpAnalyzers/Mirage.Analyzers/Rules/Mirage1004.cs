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

                    var fieldType = fieldSymbol.Type;

                    var hookType = 0; // Default: Automatic
                    foreach (var arg in syncVarAttr.NamedArguments)
                    {
                        if (arg.Key == "hookType")
                        {
                            if (arg.Value.Value is int val)
                                hookType = val;
                            break;
                        }
                    }

                    var members = declaringType.GetMembers(hookName);
                    var methods = members.OfType<IMethodSymbol>().ToList();
                    var events = members.OfType<IEventSymbol>().ToList();

                    if (hookType == 0) // Automatic
                    {
                        AnalyzeAutomatic(context, syncVarAttr, fieldSymbol, fieldType, hookName, methods, events);
                    }
                    else if (hookType >= 1 && hookType <= 3) // MethodWith0Arg, MethodWith1Arg, MethodWith2Arg
                    {
                        var expectedArgs = hookType - 1;
                        AnalyzeExplicitMethod(context, syncVarAttr, fieldSymbol, fieldType, hookName, methods, expectedArgs);
                    }
                    else if (hookType >= 4 && hookType <= 6) // EventWith0Arg, EventWith1Arg, EventWith2Arg
                    {
                        var expectedArgs = hookType - 4;
                        AnalyzeExplicitEvent(context, syncVarAttr, fieldSymbol, fieldType, hookName, events, expectedArgs);
                    }
                }
            }
        }

        private static bool IsSystemAction(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol namedType && namedType.TypeKind == TypeKind.Delegate)
            {
                if (namedType.ContainingNamespace?.ToDisplayString() == "System")
                {
                    return namedType.MetadataName == "Action" || namedType.MetadataName.StartsWith("Action`");
                }
            }

            return false;
        }

        private static void AnalyzeAutomatic(
            SymbolAnalysisContext context,
            AttributeData syncVarAttr,
            IFieldSymbol fieldSymbol,
            ITypeSymbol fieldType,
            string hookName,
            System.Collections.Generic.List<IMethodSymbol> methods,
            System.Collections.Generic.List<IEventSymbol> events)
        {
            ISymbol? foundHook = null;
            var multipleFound = false;

            for (var i = 0; i <= 2; i++)
            {
                var methodsWithCount = methods.Where(m => m.Parameters.Length == i).ToList();
                if (methodsWithCount.Count == 0)
                    continue;

                IMethodSymbol? match = null;
                foreach (var method in methodsWithCount)
                {
                    var matchParams = true;
                    foreach (var p in method.Parameters)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(p.Type, fieldType))
                        {
                            matchParams = false;
                            break;
                        }
                    }

                    if (matchParams)
                    {
                        match = method;
                        break;
                    }
                }

                if (match != null)
                {
                    if (foundHook != null)
                    {
                        multipleFound = true;
                    }
                    else
                    {
                        foundHook = match;
                    }
                }
                else
                {
                    ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                        $"parameter type mismatch (must be '{fieldType.Name}' or '{fieldType.Name}, {fieldType.Name}')");
                    return;
                }
            }

            if (events.Count > 0)
            {
                var ev = events[0];
                var eventType = ev.Type;
                if (!IsSystemAction(eventType))
                {
                    ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                        $"Hook Event for '{fieldSymbol.Name}' is invalid '{eventType.ToDisplayString()}', Error Type: Not System.Action");
                    return;
                }

                if (eventType is INamedTypeSymbol namedType)
                {
                    var argsCount = namedType.IsGenericType ? namedType.TypeArguments.Length : 0;
                    if (argsCount > 2)
                    {
                        ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                            $"parameter type mismatch (must be '{fieldType.Name}' or '{fieldType.Name}, {fieldType.Name}')");
                        return;
                    }

                    var paramTypesMatch = true;
                    if (namedType.IsGenericType)
                    {
                        foreach (var arg in namedType.TypeArguments)
                        {
                            if (!SymbolEqualityComparer.Default.Equals(arg, fieldType))
                            {
                                paramTypesMatch = false;
                                break;
                            }
                        }
                    }

                    if (!paramTypesMatch)
                    {
                        ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                            $"parameter type mismatch (must be '{fieldType.Name}' or '{fieldType.Name}, {fieldType.Name}')");
                        return;
                    }

                    if (foundHook != null)
                    {
                        multipleFound = true;
                    }
                    else
                    {
                        foundHook = ev;
                    }
                }
            }

            if (multipleFound)
            {
                ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                    $"Mutliple hooks found for '{fieldSymbol.Name}', hook name '{hookName}'. Please set HookType or remove one of the overloads");
                return;
            }

            if (foundHook == null)
            {
                ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                    $"could not find hook method with name '{hookName}'");
                return;
            }


        }

        private static void AnalyzeExplicitMethod(
            SymbolAnalysisContext context,
            AttributeData syncVarAttr,
            IFieldSymbol fieldSymbol,
            ITypeSymbol fieldType,
            string hookName,
            System.Collections.Generic.List<IMethodSymbol> methods,
            int expectedArgs)
        {
            var methodsWithParams = methods.Where(m => m.Parameters.Length == expectedArgs).ToList();
            if (methodsWithParams.Count == 0)
            {
                ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                    $"could not find hook method with name '{hookName}'");
                return;
            }

            foreach (var method in methodsWithParams)
            {
                var matchParams = true;
                foreach (var p in method.Parameters)
                {
                    if (!SymbolEqualityComparer.Default.Equals(p.Type, fieldType))
                    {
                        matchParams = false;
                        break;
                    }
                }

                if (matchParams)
                {
                    return;
                }
            }

            ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                $"parameter type mismatch (must be '{fieldType.Name}' or '{fieldType.Name}, {fieldType.Name}')");
        }

        private static void AnalyzeExplicitEvent(
            SymbolAnalysisContext context,
            AttributeData syncVarAttr,
            IFieldSymbol fieldSymbol,
            ITypeSymbol fieldType,
            string hookName,
            System.Collections.Generic.List<IEventSymbol> events,
            int expectedArgs)
        {
            if (events.Count == 0)
            {
                ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                    $"could not find hook method with name '{hookName}'");
                return;
            }

            var ev = events[0];

            var eventType = ev.Type;
            if (!IsSystemAction(eventType))
            {
                ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                    $"Hook Event for '{fieldSymbol.Name}' is invalid '{eventType.ToDisplayString()}', Error Type: Not System.Action");
                return;
            }

            if (eventType is INamedTypeSymbol namedType)
            {
                var actualArgCount = namedType.IsGenericType ? namedType.TypeArguments.Length : 0;
                if (actualArgCount != expectedArgs)
                {
                    ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                        $"Hook Event for '{fieldSymbol.Name}' is invalid '{eventType.ToDisplayString()}', Error Type: Arg mismatch");
                    return;
                }

                var paramTypesMatch = true;
                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(arg, fieldType))
                        {
                            paramTypesMatch = false;
                            break;
                        }
                    }
                }

                if (!paramTypesMatch)
                {
                    ReportDiagnostic(context, syncVarAttr, fieldSymbol, hookName,
                        $"Hook Event for '{fieldSymbol.Name}' is invalid '{eventType.ToDisplayString()}', Error Type: Param mismatch");
                    return;
                }
            }
        }

        private static void ReportDiagnostic(
            SymbolAnalysisContext context,
            AttributeData syncVarAttr,
            IFieldSymbol fieldSymbol,
            string hookName,
            string message)
        {
            var diagnostic = Diagnostic.Create(
                MirageRules.SyncVarHookRule,
                fieldSymbol.Locations[0],
                hookName,
                message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
