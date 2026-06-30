using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1302
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
                foreach (var member in typeSymbol.GetMembers())
                {
                    if (member is IFieldSymbol field && !field.IsStatic && !field.IsImplicitlyDeclared)
                    {
                        if (field.DeclaredAccessibility != Accessibility.Public && field.DeclaredAccessibility != Accessibility.Internal)
                        {
                            if (HasNonSerializedAttribute(field))
                                continue;

                            var diagnostic = Diagnostic.Create(
                                MirageRules.UnserializedPrivateFieldRule,
                                field.Locations[0],
                                field.Name,
                                typeSymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                    else if (member is IPropertySymbol property && !property.IsStatic && Helpers.IsAutoProperty(property))
                    {
                        if (property.DeclaredAccessibility != Accessibility.Public && property.DeclaredAccessibility != Accessibility.Internal)
                        {
                            if (HasNonSerializedAttribute(property))
                                continue;

                            var diagnostic = Diagnostic.Create(
                                MirageRules.UnserializedPrivateFieldRule,
                                property.Locations[0],
                                property.Name,
                                typeSymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private static bool HasNonSerializedAttribute(ISymbol symbol)
        {
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "System.NonSerializedAttribute")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
