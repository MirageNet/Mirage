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
                if (!symbols.CustomReaders.Keys.Any(k => IsSameType(k, type)))
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
                if (!symbols.CustomWriters.Keys.Any(k => IsSameType(k, type)))
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

        private static bool IsSameType(ITypeSymbol a, ITypeSymbol b)
        {
            if (SymbolEqualityComparer.Default.Equals(a, b))
                return true;

            if (a.TypeKind == TypeKind.TypeParameter && b.TypeKind == TypeKind.TypeParameter)
                return true;

            if (a is INamedTypeSymbol namedA && b is INamedTypeSymbol namedB)
            {
                if (namedA.IsGenericType && namedB.IsGenericType)
                {
                    if (!SymbolEqualityComparer.Default.Equals(namedA.OriginalDefinition, namedB.OriginalDefinition))
                        return false;

                    if (namedA.TypeArguments.Length != namedB.TypeArguments.Length)
                        return false;

                    for (int i = 0; i < namedA.TypeArguments.Length; i++)
                    {
                        var ta = namedA.TypeArguments[i];
                        var tb = namedB.TypeArguments[i];

                        if (ta.TypeKind == TypeKind.TypeParameter && tb.TypeKind == TypeKind.TypeParameter)
                        {
                            continue;
                        }

                        if (!IsSameType(ta, tb))
                            return false;
                    }

                    return true;
                }
            }

            if (a is IArrayTypeSymbol arrayA && b is IArrayTypeSymbol arrayB)
            {
                return arrayA.Rank == arrayB.Rank && IsSameType(arrayA.ElementType, arrayB.ElementType);
            }

            return false;
        }
    }
}
