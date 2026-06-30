using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1201
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
                if (symbols.HasAttribute(typeSymbol, symbols.WeaverSafeClassAttribute))
                    return;

                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic && !field.IsImplicitlyDeclared && (field.DeclaredAccessibility == Accessibility.Public || field.DeclaredAccessibility == Accessibility.Internal))
                    {
                        if (symbols.HasAttribute(field, symbols.WeaverSafeClassAttribute))
                            continue;

                        if (IsUnsafeClass(field.Type, symbols, out var unsafeType))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.MessageOrRpcRule,
                                field.Locations[0],
                                "NetworkMessage field",
                                field.Name,
                                unsafeType.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                    else if (member is IPropertySymbol property && !property.IsStatic && Helpers.IsAutoProperty(property) && (property.DeclaredAccessibility == Accessibility.Public || property.DeclaredAccessibility == Accessibility.Internal))
                    {
                        if (symbols.HasAttribute(property, symbols.WeaverSafeClassAttribute))
                            continue;

                        if (IsUnsafeClass(property.Type, symbols, out var unsafeType))
                        {
                            var diagnostic = Diagnostic.Create(
                                MirageRules.MessageOrRpcRule,
                                property.Locations[0],
                                "NetworkMessage property",
                                property.Name,
                                unsafeType.Name);
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
                foreach (var parameter in methodSymbol.Parameters)
                {
                    if (symbols.HasAttribute(parameter, symbols.WeaverSafeClassAttribute))
                        continue;

                    if (IsUnsafeClass(parameter.Type, symbols, out var unsafeType))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.MessageOrRpcRule,
                            parameter.Locations[0],
                            "RPC parameter",
                            parameter.Name,
                            unsafeType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }

                var returnType = methodSymbol.ReturnType;
                ITypeSymbol? checkType = returnType;
                if (Helpers.IsGenericUniTask(returnType, out var wrapped))
                {
                    checkType = wrapped;
                }

                if (checkType != null && IsUnsafeClass(checkType, symbols, out var unsafeReturn))
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.MessageOrRpcRule,
                        methodSymbol.Locations[0],
                        "RPC return type",
                        methodSymbol.Name,
                        unsafeReturn.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsUnsafeClass(ITypeSymbol typeSymbol, MirageSymbols symbols, out ITypeSymbol unsafeType)
        {
            unsafeType = null!;
            if (typeSymbol == null)
                return false;

            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return IsUnsafeClass(arrayType.ElementType, symbols, out unsafeType);
            }

            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType && 
                (namedType.Name == "List" || symbols.Implements(namedType, symbols.IEnumerable)))
            {
                foreach (var arg in namedType.TypeArguments)
                {
                    if (IsUnsafeClass(arg, symbols, out unsafeType))
                        return true;
                }
                return false;
            }

            if (typeSymbol.IsReferenceType && typeSymbol.SpecialType != SpecialType.System_String)
            {
                if (symbols.IsOrInherits(typeSymbol, symbols.NetworkIdentity) ||
                    symbols.IsOrInherits(typeSymbol, symbols.GameObject) ||
                    symbols.IsOrInherits(typeSymbol, symbols.Transform) ||
                    symbols.IsOrInherits(typeSymbol, symbols.NetworkBehaviour) ||
                    symbols.IsNetworkPlayerOrConnection(typeSymbol))
                {
                    return false;
                }

                if (symbols.HasAttribute(typeSymbol, symbols.WeaverSafeClassAttribute))
                    return false;

                unsafeType = typeSymbol;
                return true;
            }

            return false;
        }
    }
}
