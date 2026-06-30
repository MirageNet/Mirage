using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1001
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, symbols), SymbolKind.Field);
        }

        private static void AnalyzeField(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            var isSyncVar = symbols.HasAttribute(fieldSymbol, symbols.SyncVarAttribute);
            var isSyncObject = symbols.Implements(fieldSymbol.Type, symbols.ISyncObject);

            if (isSyncVar || isSyncObject)
            {
                if (symbols.HasAttribute(fieldSymbol, symbols.WeaverSafeClassAttribute) ||
                    (fieldSymbol.ContainingType != null && symbols.HasAttribute(fieldSymbol.ContainingType, symbols.WeaverSafeClassAttribute)))
                {
                    return;
                }

                if (isSyncVar)
                {
                    if (IsOrContainsUnsafeClass(fieldSymbol.Type, symbols, out var unsafeType))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.SyncVarRule,
                            fieldSymbol.Locations[0],
                            fieldSymbol.Name,
                            unsafeType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                else if (isSyncObject)
                {
                    if (fieldSymbol.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
                    {
                        foreach (var arg in namedType.TypeArguments)
                        {
                            if (IsOrContainsUnsafeClass(arg, symbols, out var unsafeType))
                            {
                                var diagnostic = Diagnostic.Create(
                                    MirageRules.SyncVarRule,
                                    fieldSymbol.Locations[0],
                                    fieldSymbol.Name,
                                    unsafeType.Name);
                                context.ReportDiagnostic(diagnostic);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsOrContainsUnsafeClass(ITypeSymbol typeSymbol, MirageSymbols symbols, out ITypeSymbol unsafeClassType)
        {
            unsafeClassType = null!;
            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsReferenceType && typeSymbol.SpecialType != SpecialType.System_String)
            {
                if (symbols.IsOrInherits(typeSymbol, symbols.NetworkIdentity) ||
                    symbols.IsOrInherits(typeSymbol, symbols.GameObject) ||
                    symbols.IsOrInherits(typeSymbol, symbols.Transform) ||
                    symbols.IsOrInherits(typeSymbol, symbols.NetworkBehaviour))
                {
                    return false;
                }

                if (symbols.HasAttribute(typeSymbol, symbols.WeaverSafeClassAttribute))
                    return false;

                unsafeClassType = typeSymbol;
                return true;
            }

            if (typeSymbol.IsValueType && typeSymbol.TypeKind == TypeKind.Struct)
            {
                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic)
                    {
                        if (IsOrContainsUnsafeClass(field.Type, symbols, out unsafeClassType))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
