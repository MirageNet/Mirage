using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private struct SizeEstimate
        {
            public int MinBits;
            public int MaxBits;
            public int AvgBits;
            public bool HasDynamic;

            public SizeEstimate(int minBits, int maxBits, int avgBits, bool hasDynamic)
            {
                MinBits = minBits;
                MaxBits = maxBits;
                AvgBits = avgBits;
                HasDynamic = hasDynamic;
            }

            public void Add(SizeEstimate other)
            {
                MinBits += other.MinBits;
                MaxBits += other.MaxBits;
                AvgBits += other.AvgBits;
                HasDynamic |= other.HasDynamic;
            }

            public override string ToString()
            {
                int minBytes = (MinBits + 7) / 8;
                int maxBytes = (MaxBits + 7) / 8;
                int avgBytes = (AvgBits + 7) / 8;

                if (HasDynamic)
                    return $"{minBytes}+ (Average ~{avgBytes} + dynamic content)";

                if (MinBits == MaxBits)
                    return $"{minBytes}";

                return $"{minBytes} to {maxBytes} (Average ~{avgBytes})";
            }
        }

        private static void AnalyzeNamedTypePerformance(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (MirageAttributes.NetworkMessage.Has(typeSymbol))
            {
                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var estimatedSize = EstimateSerializedSize(typeSymbol, visited);
                var diagnostic = Diagnostic.Create(
                    MirageRules.PerformanceMessageSizeRule,
                    typeSymbol.Locations[0],
                    typeSymbol.Name,
                    estimatedSize.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static SizeEstimate EstimateSerializedSize(ITypeSymbol type, HashSet<ITypeSymbol> visited)
        {
            if (type == null)
                return new SizeEstimate(0, 0, 0, false);

            if (!visited.Add(type))
                return new SizeEstimate(0, 0, 0, false);

            if (MirageTypes.GameObject.IsOrInherits(type) ||
                MirageTypes.NetworkBehaviour.IsOrInherits(type) ||
                MirageTypes.NetworkIdentity.IsOrInherits(type))
            {
                // Mirage identities and behaviors use packed netId + component index, which varies in size
                return new SizeEstimate(16, 80, 24, false);
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                    return new SizeEstimate(1, 1, 1, false);
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                    return new SizeEstimate(8, 8, 8, false);
                case SpecialType.System_Char:
                    return new SizeEstimate(16, 16, 16, false);
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                    return new SizeEstimate(16, 16, 16, false);
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                    // Packed uint32 uses variable length zigzag encoding
                    return new SizeEstimate(8, 40, 16, false);
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    // Packed uint64 uses variable length zigzag encoding
                    return new SizeEstimate(8, 72, 24, false);
                case SpecialType.System_Single:
                    return new SizeEstimate(32, 32, 32, false);
                case SpecialType.System_Double:
                    return new SizeEstimate(64, 64, 64, false);
                case SpecialType.System_Decimal:
                    return new SizeEstimate(128, 128, 128, false);
                case SpecialType.System_String:
                    // Strings are dynamic and length prefix takes 1 to 5 bytes
                    return new SizeEstimate(8, 8, 8, true);
            }

            if (type.ContainingNamespace?.ToDisplayString() == "UnityEngine")
            {
                switch (type.Name)
                {
                    case "Vector2":
                        return new SizeEstimate(64, 64, 64, false);
                    case "Vector3":
                        return new SizeEstimate(96, 96, 96, false);
                    case "Vector4":
                        return new SizeEstimate(128, 128, 128, false);
                    case "Quaternion":
                        return new SizeEstimate(128, 128, 128, false);
                    case "Color":
                        return new SizeEstimate(128, 128, 128, false);
                    case "Color32":
                        return new SizeEstimate(32, 32, 32, false);
                    case "Vector2Int":
                        // Two packed int32 values
                        return new SizeEstimate(16, 80, 32, false);
                    case "Vector3Int":
                        // Three packed int32 values
                        return new SizeEstimate(24, 120, 48, false);
                }
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                var underlying = (type as INamedTypeSymbol)?.EnumUnderlyingType;
                return underlying != null ? EstimateSerializedSize(underlying, visited) : new SizeEstimate(8, 40, 16, false);
            }

            if (type is IArrayTypeSymbol arrayType)
                // Collections are dynamic and have a var-int length prefix
                return new SizeEstimate(8, 8, 8, true);

            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                if (MirageTypes.IEnumerable.Implements(namedType))
                    return new SizeEstimate(8, 8, 8, true);
            }

            if (type.TypeKind == TypeKind.Struct || type.TypeKind == TypeKind.Class)
            {
                var sum = new SizeEstimate(0, 0, 0, false);
                foreach (var member in type.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic && field.DeclaredAccessibility == Accessibility.Public)
                    {
                        var memberEstimate = EstimateMemberSerializedSize(field, field.Type, visited);
                        sum.Add(memberEstimate);
                    }
                }

                if (type.TypeKind == TypeKind.Class)
                {
                    // Add 1 bit for null-check bool prefix
                    sum.MinBits += 1;
                    sum.MaxBits += 1;
                    sum.AvgBits += 1;
                }
                return sum;
            }

            return new SizeEstimate(0, 0, 0, false);
        }

        private static SizeEstimate EstimateMemberSerializedSize(ISymbol symbol, ITypeSymbol type, HashSet<ITypeSymbol> visited)
        {
            if (MirageAttributes.BitCount.TryGet(symbol, out var bitCountAttr) && bitCountAttr.ConstructorArguments.Length > 0)
            {
                if (bitCountAttr.ConstructorArguments[0].Value is int bits)
                    return new SizeEstimate(bits, bits, bits, false);
            }

            if (MirageAttributes.FloatPack.TryGet(symbol, out var floatPackAttr))
            {
                if (floatPackAttr.ConstructorArguments.Length >= 2)
                {
                    if (floatPackAttr.ConstructorArguments[1].Value is int bits)
                        return new SizeEstimate(bits, bits, bits, false);
                }

                return new SizeEstimate(32, 32, 32, false);
            }

            return EstimateSerializedSize(type, visited);
        }
    }
}
