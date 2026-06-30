using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1301
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeType(symbolContext, symbols), SymbolKind.NamedType);
            context.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, symbols), SymbolKind.Method);
        }

        private static void AnalyzeType(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (symbols.HasAttribute(typeSymbol, symbols.NetworkMessageAttribute))
            {

                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic && (field.DeclaredAccessibility == Accessibility.Public || field.DeclaredAccessibility == Accessibility.Internal))
                    {

                        if (IsPlainMonoBehaviour(field.Type, symbols))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.MonoBehaviourParameterRule,
                                field.Locations[0],
                                GetTypeName(field.Type),
                                "NetworkMessage field");
                            context.ReportDiagnostic(diagnostic);
                            continue;
                        }

                        if (!IsSerializable(field.Type, context.Compilation, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), symbols))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.FieldTypeSerializationRule,
                                field.Locations[0],
                                GetTypeName(field.Type),
                                "NetworkMessage field");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                    else if (member is IPropertySymbol property && !property.IsStatic && Helpers.IsAutoProperty(property) && (property.DeclaredAccessibility == Accessibility.Public || property.DeclaredAccessibility == Accessibility.Internal))
                    {

                        if (IsPlainMonoBehaviour(property.Type, symbols))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.MonoBehaviourParameterRule,
                                property.Locations[0],
                                GetTypeName(property.Type),
                                "NetworkMessage property");
                            context.ReportDiagnostic(diagnostic);
                            continue;
                        }

                        if (!IsSerializable(property.Type, context.Compilation, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), symbols))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.FieldTypeSerializationRule,
                                property.Locations[0],
                                GetTypeName(property.Type),
                                "NetworkMessage property");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (Helpers.IsRpcMethod(methodSymbol, symbols))
            {
                bool isPlayerClientRpc = false;
                if (symbols.HasAttribute(methodSymbol, symbols.ClientRpcAttribute))
                {
                    if (symbols.TryGetAttribute(methodSymbol, symbols.ClientRpcAttribute, out var clientRpcAttr))
                    {
                        var targetArg = clientRpcAttr.NamedArguments.FirstOrDefault(arg => arg.Key == "target");
                        if (targetArg.Value.Value is int targetVal && targetVal == 1)
                        {
                            isPlayerClientRpc = true;
                        }
                    }
                }

                for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                {
                    if (isPlayerClientRpc && i == 0)
                        continue;

                    var parameter = methodSymbol.Parameters[i];

                    if (IsPlainMonoBehaviour(parameter.Type, symbols))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.MonoBehaviourParameterRule,
                            parameter.Locations[0],
                            GetTypeName(parameter.Type),
                            "RPC parameter");
                        context.ReportDiagnostic(diagnostic);
                        continue;
                    }

                    if (!IsSerializable(parameter.Type, context.Compilation, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), symbols))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.FieldTypeSerializationRule,
                            parameter.Locations[0],
                            GetTypeName(parameter.Type),
                            "RPC parameter");
                        context.ReportDiagnostic(diagnostic);
                    }
                }

                if (!methodSymbol.ReturnsVoid)
                {
                    var returnType = methodSymbol.ReturnType;
                    if (IsGenericUniTask(returnType, out var wrappedType))
                    {
                        if (wrappedType != null && !IsSerializable(wrappedType, context.Compilation, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default), symbols))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.FieldTypeSerializationRule,
                                methodSymbol.Locations[0],
                                GetTypeName(wrappedType),
                                "RPC return type");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static bool IsGenericUniTask(ITypeSymbol type, out ITypeSymbol? wrappedType)
        {
            wrappedType = null;
            if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.Name == "UniTask" && namedType.ContainingNamespace?.ToDisplayString() == "Cysharp.Threading.Tasks")
            {
                wrappedType = namedType.TypeArguments[0];
                return true;
            }
            return false;
        }

        public static bool IsPlainMonoBehaviour(ITypeSymbol type, MirageSymbols symbols)
        {
            if (type == null)
                return false;

            return symbols.IsOrInherits(type, symbols.MonoBehaviour) &&
                   !symbols.IsOrInherits(type, symbols.NetworkBehaviour) &&
                   !SymbolEqualityComparer.Default.Equals(type, symbols.NetworkIdentity);
        }

        public static bool IsSerializable(ITypeSymbol type, Compilation compilation, HashSet<ITypeSymbol> visited, MirageSymbols symbols)
        {
            if (type == null)
                return false;

            if (visited.Contains(type))
                return false;

            if (symbols.CustomSerializableTypes.Contains(type))
                return true;

            if (type.SpecialType == SpecialType.System_Boolean ||
                type.SpecialType == SpecialType.System_Byte ||
                type.SpecialType == SpecialType.System_SByte ||
                type.SpecialType == SpecialType.System_Char ||
                type.SpecialType == SpecialType.System_Double ||
                type.SpecialType == SpecialType.System_Single ||
                type.SpecialType == SpecialType.System_Int32 ||
                type.SpecialType == SpecialType.System_UInt32 ||
                type.SpecialType == SpecialType.System_Int64 ||
                type.SpecialType == SpecialType.System_UInt64 ||
                type.SpecialType == SpecialType.System_Int16 ||
                type.SpecialType == SpecialType.System_UInt16 ||
                type.SpecialType == SpecialType.System_String)
            {
                return true;
            }

            if (type.TypeKind == TypeKind.Enum)
                return true;

            if (symbols.IsOrInherits(type, symbols.NetworkIdentity) ||
                symbols.IsOrInherits(type, symbols.GameObject) ||
                symbols.IsOrInherits(type, symbols.Transform) ||
                symbols.IsOrInherits(type, symbols.NetworkBehaviour))
            {
                return true;
            }

            if (type is IArrayTypeSymbol arrayType)
            {
                if (arrayType.Rank > 1)
                    return false;

                visited.Add(type);
                var isElemSerializable = IsSerializable(arrayType.ElementType, compilation, visited, symbols);
                visited.Remove(type);
                return isElemSerializable;
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.IsGenericType && (namedType.Name == "List" || symbols.Implements(namedType, symbols.IEnumerable)))
                {
                    var elementType = namedType.TypeArguments[0];
                    visited.Add(type);
                    var isElemSerializable = IsSerializable(elementType, compilation, visited, symbols);
                    visited.Remove(type);
                    return isElemSerializable;
                }

                if (namedType.TypeKind == TypeKind.Struct)
                {
                    visited.Add(type);
                    foreach (var member in namedType.GetMembers())
                    {
                        if (member is IFieldSymbol field && !field.IsStatic)
                        {
                            if (!IsSerializable(field.Type, compilation, visited, symbols))
                            {
                                visited.Remove(type);
                                return false;
                            }
                        }
                    }
                    visited.Remove(type);
                    return true;
                }
            }

            return false;
        }

        private static string GetTypeName(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return GetTypeName(arrayType.ElementType);
            }
            return type.Name;
        }
    }
}
