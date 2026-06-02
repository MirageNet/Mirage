using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private static void AnalyzeNamedTypePerformance(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (MirageAttributes.NetworkMessage.Has(typeSymbol))
            {
                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                int estimatedSize = EstimateSerializedSize(typeSymbol, visited);
                var diagnostic = Diagnostic.Create(MirageRules.PerformanceMessageSizeRule, typeSymbol.Locations[0], typeSymbol.Name, estimatedSize);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static int EstimateSerializedSize(ITypeSymbol type, HashSet<ITypeSymbol> visited)
        {
            if (type == null)
                return 0;

            if (!visited.Add(type))
                return 0;

            if (MirageTypes.GameObject.IsOrInherits(type) ||
                MirageTypes.NetworkBehaviour.IsOrInherits(type) ||
                MirageTypes.NetworkIdentity.IsOrInherits(type))
            {
                return 2;
            }

            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                    return 1;
                case SpecialType.System_Char:
                    return 2;
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                    return 2;
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                    return 1; // 1 byte default for typical uint/int
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return 8;
                case SpecialType.System_Single:
                    return 4;
                case SpecialType.System_Double:
                    return 8;
                case SpecialType.System_Decimal:
                    return 16;
                case SpecialType.System_String:
                    return 0; // Dynamic type: skip/treat as variable
            }

            if (type.ContainingNamespace?.ToDisplayString() == "UnityEngine")
            {
                switch (type.Name)
                {
                    case "Vector2": return 8;
                    case "Vector3": return 12;
                    case "Vector4": return 16;
                    case "Quaternion": return 16;
                    case "Color": return 16;
                    case "Color32": return 4;
                    case "Vector2Int": return 2;
                    case "Vector3Int": return 3;
                }
            }

            if (type.TypeKind == TypeKind.Enum)
            {
                var underlying = (type as INamedTypeSymbol)?.EnumUnderlyingType;
                return underlying != null ? EstimateSerializedSize(underlying, visited) : 1;
            }

            if (type is IArrayTypeSymbol arrayType)
                return 0; // Dynamic type: skip/treat as variable

            if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                if (MirageTypes.IEnumerable.Implements(namedType))
                {
                    return 0; // Dynamic type: skip/treat as variable
                }
            }

            if (type.TypeKind == TypeKind.Struct || type.TypeKind == TypeKind.Class)
            {
                int sum = 0;
                foreach (var member in type.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic && field.DeclaredAccessibility == Accessibility.Public)
                        sum += EstimateMemberSerializedSize(field, field.Type, visited);
                }
                return sum;
            }

            return 0;
        }

        private static int EstimateMemberSerializedSize(ISymbol symbol, ITypeSymbol type, HashSet<ITypeSymbol> visited)
        {
            if (MirageAttributes.BitCount.TryGet(symbol, out var bitCountAttr) && bitCountAttr.ConstructorArguments.Length > 0)
                if (bitCountAttr.ConstructorArguments[0].Value is int bits)
                    return (bits + 7) / 8;

            if (MirageAttributes.FloatPack.TryGet(symbol, out var floatPackAttr))
            {
                if (floatPackAttr.ConstructorArguments.Length >= 2)
                    if (floatPackAttr.ConstructorArguments[1].Value is int bits)
                        return (bits + 7) / 8;

                return 4;
            }

            return EstimateSerializedSize(type, visited);
        }
    }
}
