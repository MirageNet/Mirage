using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private static void AnalyzeFieldSerialization(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (fieldSymbol.ContainingType != null && MirageAttributes.NetworkMessage.Has(fieldSymbol.ContainingType))
            {
                AnalyzeMessageOrRpc(context, fieldSymbol, fieldSymbol.Type, "NetworkMessage field");

                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                if (!IsTypeSerializable(context.Compilation, fieldSymbol.Type, serializers, visited))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.FieldTypeSerializationRule, fieldSymbol.Locations[0], fieldSymbol.Type.Name, "NetworkMessage field");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzePropertySerialization(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            if (propertySymbol.ContainingType != null && MirageAttributes.NetworkMessage.Has(propertySymbol.ContainingType))
            {
                AnalyzeMessageOrRpc(context, propertySymbol, propertySymbol.Type, "NetworkMessage property");

                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                if (!IsTypeSerializable(context.Compilation, propertySymbol.Type, serializers, visited))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.FieldTypeSerializationRule, propertySymbol.Locations[0], propertySymbol.Type.Name, "NetworkMessage property");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeMethodSerialization(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (IsRpcMethod(methodSymbol))
            {
                var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                if (returnType != null && returnType.IsGenericType && returnType.TypeArguments.Length > 0)
                {
                    var originalDefinition = returnType.OriginalDefinition;
                    if (originalDefinition != null && MirageTypes.UniTask.Is(returnType))
                    {
                        var typeArgument = returnType.TypeArguments[0];
                        AnalyzeMessageOrRpc(context, methodSymbol, typeArgument, "RPC return type");

                        var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                        if (!IsTypeSerializable(context.Compilation, typeArgument, serializers, visited))
                        {
                            var diagnostic = Diagnostic.Create(MirageRules.FieldTypeSerializationRule, methodSymbol.Locations[0], typeArgument.Name, "RPC return type");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static void AnalyzeParameterSerialization(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            var parameterSymbol = (IParameterSymbol)context.Symbol;
            var containingMethod = parameterSymbol.ContainingSymbol as IMethodSymbol;

            if (containingMethod != null && IsRpcMethod(containingMethod))
            {
                AnalyzeMessageOrRpc(context, parameterSymbol, parameterSymbol.Type, "RPC parameter");

                var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                if (!IsTypeSerializable(context.Compilation, parameterSymbol.Type, serializers, visited))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.FieldTypeSerializationRule, parameterSymbol.Locations[0], parameterSymbol.Type.Name, "RPC parameter");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void ReportMismatchedSerialization(CompilationAnalysisContext context, CustomSerializers serializers)
        {
            foreach (var kvp in serializers.Writers)
            {
                var type = kvp.Key;
                var method = kvp.Value;
                if (!serializers.Readers.ContainsKey(type))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.MismatchedSerializationRule, method.Locations[0], type.Name, $"Custom writer defined for '{type.Name}' but matching custom reader is missing.");
                    context.ReportDiagnostic(diagnostic);
                }
            }

            foreach (var kvp in serializers.Readers)
            {
                var type = kvp.Key;
                var method = kvp.Value;
                if (!serializers.Writers.ContainsKey(type))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.MismatchedSerializationRule, method.Locations[0], type.Name, $"Custom reader defined for '{type.Name}' but matching custom writer is missing.");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeMessageOrRpc(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol typeSymbol, string locationDescription)
        {
            if (IsBasicSafeType(typeSymbol))
                return;

            if (IsExplicitlyMarkedSafe(symbol, typeSymbol))
                return;

            var diagnostic = Diagnostic.Create(MirageRules.MessageOrRpcRule, symbol.Locations[0], locationDescription, symbol.Name, typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsTypeSerializable(Compilation compilation, ITypeSymbol type, CustomSerializers serializers, HashSet<ITypeSymbol> visited)
        {
            if (type == null)
                return true;

            if (type.SpecialType != SpecialType.None)
            {
                if (type.SpecialType == SpecialType.System_Void)
                    return false;
                return true;
            }

            if (type.TypeKind == TypeKind.Enum)
                return true;

            if (MirageTypes.NetworkBehaviour.IsOrInherits(type) ||
                MirageTypes.NetworkIdentity.IsOrInherits(type) ||
                MirageTypes.GameObject.IsOrInherits(type))
            {
                return true;
            }

            if (serializers.Writers.ContainsKey(type) && serializers.Readers.ContainsKey(type))
                return true;

            if (type.ContainingNamespace?.ToDisplayString() == "UnityEngine")
            {
                switch (type.Name)
                {
                    case "Vector2":
                    case "Vector3":
                    case "Vector4":
                    case "Vector2Int":
                    case "Vector3Int":
                    case "Color":
                    case "Color32":
                    case "Rect":
                    case "Plane":
                    case "Ray":
                    case "Matrix4x4":
                    case "Quaternion":
                        return true;
                }
            }

            if (type.ContainingNamespace?.ToDisplayString() == "System")
            {
                if (type.Name == "Guid" || type.Name == "DateTime")
                    return true;
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                if (arrayType.Rank > 1)
                    return false;
                return IsTypeSerializable(compilation, arrayType.ElementType, serializers, visited);
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (MirageTypes.IEnumerable.Implements(namedType))
                {
                    if (namedType.TypeArguments.Length > 0)
                    {
                        foreach (var arg in namedType.TypeArguments)
                            if (!IsTypeSerializable(compilation, arg, serializers, visited))
                                return false;

                        return true;
                    }
                    return false;
                }

                if (namedType.IsGenericType)
                {
                    foreach (var arg in namedType.TypeArguments)
                    {
                        if (arg.TypeKind == TypeKind.TypeParameter)
                            return false;
                    }
                }

                if (namedType.TypeKind == TypeKind.Struct || namedType.TypeKind == TypeKind.Class)
                {
                    if (!visited.Add(namedType))
                        return false;

                    var ns = namedType.ContainingNamespace?.ToDisplayString();
                    if (ns != null && (ns.StartsWith("System") || ns.StartsWith("UnityEngine")))
                        return false;

                    foreach (var member in namedType.GetMembers())
                    {
                        if (member is IFieldSymbol field && !field.IsStatic && field.DeclaredAccessibility == Accessibility.Public)
                        {
                            if (!IsTypeSerializable(compilation, field.Type, serializers, visited))
                                return false;
                        }
                    }
                    return true;
                }
            }

            return false;
        }
    }
}
