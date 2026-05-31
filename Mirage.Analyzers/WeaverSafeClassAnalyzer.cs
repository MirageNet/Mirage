using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WeaverSafeClassAnalyzer : DiagnosticAnalyzer
    {
        public const string SyncVarDiagnosticId = "MIRAGE1001";
        public const string AutoPropertyDiagnosticId = "MIRAGE1002";
        public const string MessageOrRpcDiagnosticId = "MIRAGE1301";

        private static readonly DiagnosticDescriptor SyncVarRule = new DiagnosticDescriptor(
            SyncVarDiagnosticId,
            "SyncVar cannot be a class type unless marked safe",
            "SyncVar '{0}' is a class type '{1}'. Class-based SyncVars allocate memory, do not support polymorphism (only declared fields serialize), and cannot track internal changes automatically (meaning modifications won't trigger sync hooks). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this SyncVar with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class types used as SyncVars should be value types or marked with [WeaverSafeClass] to avoid allocations and hook tracking issues.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1001");

        private static readonly DiagnosticDescriptor AutoPropertyRule = new DiagnosticDescriptor(
            AutoPropertyDiagnosticId,
            "SyncVar property must be an auto-property",
            "SyncVar property '{0}' must be a non-static auto-property with both get and set accessors",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Properties marked with [SyncVar] must be automatic properties with both getter and setter, and cannot be static.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1002");

        private static readonly DiagnosticDescriptor MessageOrRpcRule = new DiagnosticDescriptor(
            MessageOrRpcDiagnosticId,
            "Class type used in NetworkMessage or RPC without WeaverSafeClass attribute",
            "{0} '{1}' is a class type '{2}'. Class-based types allocate memory upon deserialization and do not support polymorphism (only declared fields serialize). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this member/parameter with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class types used as NetworkMessage fields or RPC parameters/returns should be value types or marked with [WeaverSafeClass] to avoid allocations and polymorphism bugs.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1301");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SyncVarRule, AutoPropertyRule, MessageOrRpcRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
            context.RegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeField(SymbolAnalysisContext context)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;
            
            if (HasAttribute(fieldSymbol, "Mirage.SyncVarAttribute"))
            {
                AnalyzeSyncVar(context, fieldSymbol, fieldSymbol.Type);
                return;
            }

            if (fieldSymbol.ContainingType != null && HasAttribute(fieldSymbol.ContainingType, "Mirage.NetworkMessageAttribute"))
            {
                AnalyzeMessageOrRpc(context, fieldSymbol, fieldSymbol.Type, "NetworkMessage field");
            }
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            if (HasAttribute(propertySymbol, "Mirage.SyncVarAttribute"))
            {
                if (propertySymbol.GetMethod == null || propertySymbol.SetMethod == null || propertySymbol.IsStatic || !IsAutoProperty(propertySymbol))
                {
                    var diagnostic = Diagnostic.Create(AutoPropertyRule, propertySymbol.Locations[0], propertySymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                AnalyzeSyncVar(context, propertySymbol, propertySymbol.Type);
                return;
            }

            if (propertySymbol.ContainingType != null && HasAttribute(propertySymbol.ContainingType, "Mirage.NetworkMessageAttribute"))
            {
                AnalyzeMessageOrRpc(context, propertySymbol, propertySymbol.Type, "NetworkMessage property");
            }
        }

        private static void AnalyzeParameter(SymbolAnalysisContext context)
        {
            var parameterSymbol = (IParameterSymbol)context.Symbol;
            var containingMethod = parameterSymbol.ContainingSymbol as IMethodSymbol;

            if (containingMethod == null)
                return;

            if (IsRpcMethod(containingMethod))
            {
                AnalyzeMessageOrRpc(context, parameterSymbol, parameterSymbol.Type, "RPC parameter");
            }
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (IsRpcMethod(methodSymbol))
            {
                var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                if (returnType != null && returnType.IsGenericType && returnType.TypeArguments.Length > 0)
                {
                    var originalDefinition = returnType.OriginalDefinition;
                    if (originalDefinition != null && originalDefinition.Name == "UniTask")
                    {
                        var originalString = originalDefinition.ToDisplayString();
                        if (originalString == "Cysharp.Threading.Tasks.UniTask<T>")
                        {
                            var typeArgument = returnType.TypeArguments[0];
                            AnalyzeMessageOrRpc(context, methodSymbol, typeArgument, "RPC return type");
                        }
                    }
                }
            }
        }

        private static bool IsBasicSafeType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return true;

            if (typeSymbol.IsValueType || typeSymbol.TypeKind == TypeKind.Struct || typeSymbol.TypeKind == TypeKind.Enum)
                return true;

            if (typeSymbol.TypeKind != TypeKind.Class)
                return true;

            if (typeSymbol.SpecialType == SpecialType.System_String)
                return true;

            if (IsOrInheritsFrom(typeSymbol, "Mirage.NetworkIdentity"))
                return true;
            if (IsOrInheritsFrom(typeSymbol, "UnityEngine.GameObject"))
                return true;
            if (IsOrInheritsFrom(typeSymbol, "Mirage.NetworkBehaviour"))
                return true;

            return false;
        }

        private static bool IsExplicitlyMarkedSafe(ISymbol symbol, ITypeSymbol typeSymbol)
        {
            if (HasAttribute(symbol, "Mirage.WeaverSafeClassAttribute"))
                return true;

            if (typeSymbol != null && HasAttribute(typeSymbol, "Mirage.WeaverSafeClassAttribute"))
                return true;

            if (symbol.ContainingType != null && HasAttribute(symbol.ContainingType, "Mirage.WeaverSafeClassAttribute"))
                return true;

            return false;
        }

        private static void AnalyzeSyncVar(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol typeSymbol)
        {
            if (IsBasicSafeType(typeSymbol))
                return;

            if (IsExplicitlyMarkedSafe(symbol, typeSymbol))
                return;

            var diagnostic = Diagnostic.Create(SyncVarRule, symbol.Locations[0], symbol.Name, typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static void AnalyzeMessageOrRpc(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol typeSymbol, string contextName)
        {
            if (IsBasicSafeType(typeSymbol))
                return;

            if (IsExplicitlyMarkedSafe(symbol, typeSymbol))
                return;

            var ns = typeSymbol.ContainingNamespace?.ToDisplayString();
            if (ns != null && (ns == "System.Collections.Generic" || ns == "System.Collections"))
                return;

            var diagnostic = Diagnostic.Create(MessageOrRpcRule, symbol.Locations[0], contextName, symbol.Name, typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsRpcMethod(IMethodSymbol methodSymbol)
        {
            foreach (var attr in methodSymbol.GetAttributes())
            {
                if (attr.AttributeClass != null && (attr.AttributeClass.Name == "ServerRpcAttribute" || attr.AttributeClass.Name == "ClientRpcAttribute"))
                {
                    var fullName = attr.AttributeClass.ToDisplayString();
                    if (fullName == "Mirage.ServerRpcAttribute" || fullName == "Mirage.ClientRpcAttribute")
                        return true;
                }
            }
            return false;
        }

        private static bool HasAttribute(ISymbol symbol, string fullyQualifiedName)
        {
            var lastDot = fullyQualifiedName.LastIndexOf('.');
            var shortName = lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass != null && attr.AttributeClass.Name == shortName)
                {
                    if (attr.AttributeClass.ToDisplayString() == fullyQualifiedName)
                        return true;
                }
            }
            return false;
        }

        private static bool IsOrInheritsFrom(ITypeSymbol typeSymbol, string fullyQualifiedName)
        {
            var lastDot = fullyQualifiedName.LastIndexOf('.');
            var shortName = lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;

            var current = typeSymbol;
            while (current != null)
            {
                if (current.Name == shortName && current.ToDisplayString() == fullyQualifiedName)
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private static bool IsAutoProperty(IPropertySymbol propertySymbol)
        {
            var backingFieldName = $"<{propertySymbol.Name}>k__BackingField";
            foreach (var member in propertySymbol.ContainingType.GetMembers())
            {
                if (member is IFieldSymbol field && field.IsImplicitlyDeclared && field.Name == backingFieldName)
                    return true;
            }
            return false;
        }
    }
}
