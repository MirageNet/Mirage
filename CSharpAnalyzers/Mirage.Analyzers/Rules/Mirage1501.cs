using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1501
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, symbols), SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (symbols.HasAttribute(typeSymbol, symbols.NetworkMessageAttribute))
            {
                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var sizeInfo = EstimateTypeSize(typeSymbol, symbols, visited, isRoot: true);

                string sizeString;
                if (sizeInfo.Min == sizeInfo.Max && !sizeInfo.IsDynamic)
                {
                    sizeString = sizeInfo.Min.ToString();
                }
                else if (sizeInfo.IsDynamic)
                {
                    sizeString = $"{sizeInfo.Min}+ (Average ~{sizeInfo.Avg} + dynamic content)";
                }
                else
                {
                    sizeString = $"{sizeInfo.Min} to {sizeInfo.Max} (Average ~{sizeInfo.Avg})";
                }

                var diagnostic = Diagnostic.Create(
                    MirageRules.PerformanceMessageSizeRule,
                    typeSymbol.Locations[0],
                    typeSymbol.Name,
                    sizeString);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private struct SizeInfo
        {
            public int Min;
            public int Max;
            public int Avg;
            public bool IsDynamic;

            public SizeInfo(int min, int max, int avg, bool isDynamic)
            {
                Min = min;
                Max = max;
                Avg = avg;
                IsDynamic = isDynamic;
            }
        }

        private static SizeInfo EstimateTypeSize(ITypeSymbol type, MirageSymbols symbols, HashSet<ITypeSymbol> visited, bool isRoot = false)
        {
            if (type == null)
                return new SizeInfo(0, 0, 0, false);

            if (!isRoot)
            {
                // Handle primitive type sizes
                switch (type.SpecialType)
                {
                    case SpecialType.System_Boolean:
                    case SpecialType.System_Byte:
                    case SpecialType.System_SByte:
                    case SpecialType.System_Char:
                        return new SizeInfo(1, 1, 1, false);

                    case SpecialType.System_Int16:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt64:
                        return new SizeInfo(1, 5, 2, false);

                    case SpecialType.System_Single:
                        return new SizeInfo(4, 4, 4, false);

                    case SpecialType.System_Double:
                        return new SizeInfo(8, 8, 8, false);

                    case SpecialType.System_String:
                        return new SizeInfo(1, 1, 1, true);
                }

                if (type.TypeKind == TypeKind.Enum)
                {
                    return new SizeInfo(1, 5, 2, false); // Estimate enum as int
                }

                // Unity structs
                if (SymbolEqualityComparer.Default.Equals(type, symbols.Vector2))
                    return new SizeInfo(8, 8, 8, false);
                if (SymbolEqualityComparer.Default.Equals(type, symbols.Vector3))
                    return new SizeInfo(12, 12, 12, false);
                if (SymbolEqualityComparer.Default.Equals(type, symbols.Quaternion))
                    return new SizeInfo(16, 16, 16, false);

                // Collections / Arrays
                if (type is IArrayTypeSymbol || (type is INamedTypeSymbol named && named.IsGenericType && symbols.Implements(named, symbols.IEnumerable)))
                {
                    return new SizeInfo(1, 1, 1, true);
                }

                // Reference type contributions (classes)
                if (type.IsReferenceType)
                {
                    return new SizeInfo(1, 1, 1, false); // reference null-flag
                }
            }

            // For root type or nested struct, evaluate fields:
            if (type is INamedTypeSymbol namedStruct && (isRoot || namedStruct.TypeKind == TypeKind.Struct))
            {
                if (visited.Contains(type))
                    return new SizeInfo(1, 1, 1, false); // break recursion

                visited.Add(type);
                int min = 0, max = 0, avg = 0;
                bool isDynamic = false;

                foreach (var member in namedStruct.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic)
                    {
                        var fieldSize = EstimateFieldSize(field, symbols, visited);
                        min += fieldSize.Min;
                        max += fieldSize.Max;
                        avg += fieldSize.Avg;
                        if (fieldSize.IsDynamic)
                            isDynamic = true;
                    }
                }

                visited.Remove(type);
                return new SizeInfo(min, max, avg, isDynamic);
            }

            return new SizeInfo(0, 0, 0, false);
        }

        private static SizeInfo EstimateFieldSize(IFieldSymbol field, MirageSymbols symbols, HashSet<ITypeSymbol> visited)
        {
            // Check packing attributes
            if (symbols.HasAttribute(field, symbols.FloatPackAttribute) ||
                symbols.HasAttribute(field, symbols.BitCountAttribute) ||
                symbols.HasAttribute(field, symbols.BitCountFromRangeAttribute) ||
                symbols.HasAttribute(field, symbols.VarIntAttribute) ||
                symbols.HasAttribute(field, symbols.VarIntBlocksAttribute))
            {
                // Rounded up to 1 byte for simplicity in estimation
                return new SizeInfo(1, 1, 1, false);
            }

            return EstimateTypeSize(field.Type, symbols, visited, isRoot: false);
        }
    }
}
