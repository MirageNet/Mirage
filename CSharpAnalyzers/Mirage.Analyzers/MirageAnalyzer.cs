using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class MirageAnalyzer : DiagnosticAnalyzer
    {



        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => MirageRules.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var customSerializers = FindCustomWritersAndReaders(compilationContext.Compilation);

                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, customSerializers), SymbolKind.Field);
                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeProperty(symbolContext, customSerializers), SymbolKind.Property);
                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeMethod(symbolContext, customSerializers), SymbolKind.Method);
                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeParameter(symbolContext, customSerializers), SymbolKind.Parameter);
                compilationContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);

                compilationContext.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);

                compilationContext.RegisterSyntaxNodeAction(AnalyzeMutation,
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.DivideAssignmentExpression,
                    SyntaxKind.ModuloAssignmentExpression,
                    SyntaxKind.AndAssignmentExpression,
                    SyntaxKind.ExclusiveOrAssignmentExpression,
                    SyntaxKind.OrAssignmentExpression,
                    SyntaxKind.LeftShiftAssignmentExpression,
                    SyntaxKind.RightShiftAssignmentExpression,
                    SyntaxKind.CoalesceAssignmentExpression,
                    SyntaxKind.PostIncrementExpression,
                    SyntaxKind.PostDecrementExpression,
                    SyntaxKind.PreIncrementExpression,
                    SyntaxKind.PreDecrementExpression,
                    SyntaxKind.Argument);

                compilationContext.RegisterCompilationEndAction(endContext =>
                {
                    ReportMismatchedSerialization(endContext, customSerializers);
                });
            });
        }

        private static void AnalyzeField(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            AnalyzeFieldSyncVars(context);
            AnalyzeFieldSerialization(context, serializers);
            AnalyzeFieldPerformance(context);
        }

        private static void AnalyzeProperty(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            AnalyzePropertySyncVars(context);
            AnalyzePropertySerialization(context, serializers);
            AnalyzePropertyPerformance(context);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            AnalyzeMethodRpcs(context);
            AnalyzeMethodSerialization(context, serializers);
        }

        private static void AnalyzeParameter(SymbolAnalysisContext context, CustomSerializers serializers)
        {
            AnalyzeParameterRpcs(context);
            AnalyzeParameterSerialization(context, serializers);
            AnalyzeParameterPerformance(context);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            AnalyzeNamedTypePerformance(context);
        }

        public class CustomSerializers
        {
            public Dictionary<ITypeSymbol, IMethodSymbol> Writers { get; } = new Dictionary<ITypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
            public Dictionary<ITypeSymbol, IMethodSymbol> Readers { get; } = new Dictionary<ITypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);
        }

        private static CustomSerializers FindCustomWritersAndReaders(Compilation compilation)
        {
            var serializers = new CustomSerializers();
            VisitNamespace(compilation.Assembly.GlobalNamespace, serializers);
            return serializers;
        }

        private static void VisitNamespace(INamespaceSymbol ns, CustomSerializers serializers)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                    VisitNamespace(nestedNs, serializers);
                else if (member is INamedTypeSymbol type)
                    VisitType(type, serializers);
            }
        }

        private static void VisitType(INamedTypeSymbol type, CustomSerializers serializers)
        {
            if (type.IsStatic)
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is IMethodSymbol method && method.IsStatic && method.IsExtensionMethod)
                    {
                        if (method.Parameters.Length == 2 &&
                            IsNetworkWriter(method.Parameters[0].Type) &&
                            method.ReturnsVoid)
                        {
                            serializers.Writers[method.Parameters[1].Type] = method;
                        }
                        else if (method.Parameters.Length == 1 &&
                                 IsNetworkReader(method.Parameters[0].Type) &&
                                 !method.ReturnsVoid)
                        {
                            serializers.Readers[method.ReturnType] = method;
                        }
                    }
                }
            }

            foreach (var nestedType in type.GetTypeMembers())
                VisitType(nestedType, serializers);
        }

        private static bool IsNetworkWriter(ITypeSymbol type)
        {
            return MirageTypes.NetworkWriter.Is(type);
        }

        private static bool IsNetworkReader(ITypeSymbol type)
        {
            return MirageTypes.NetworkReader.Is(type);
        }

        private static void AnalyzeNetworkAttributes(SymbolAnalysisContext context, ISymbol symbol)
        {
            var containingType = symbol.ContainingType;

            // Stop analysis if the declaring type inherits from NetworkBehaviour, as network attributes are valid in this context.
            if (containingType != null && MirageTypes.NetworkBehaviour.IsOrInherits(containingType))
                return;

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass != null)
                {
                    foreach (var networkAttr in MirageAttributes.NetworkAttributes)
                    {
                        if (networkAttr.Matches(attr.AttributeClass))
                        {
                            var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? symbol.Locations[0];
                            var diagnostic = Diagnostic.Create(MirageRules.NetworkBehaviourAttributeRule, location, attr.AttributeClass.Name, symbol.Name);
                            context.ReportDiagnostic(diagnostic);
                            break;
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

            if (typeSymbol.SpecialType == SpecialType.System_String)
                return true;

            if (MirageTypes.NetworkIdentity.IsOrInherits(typeSymbol))
                return true;
            if (MirageTypes.GameObject.IsOrInherits(typeSymbol))
                return true;
            if (MirageTypes.NetworkBehaviour.IsOrInherits(typeSymbol))
                return true;

            return false;
        }

        private static bool IsExplicitlyMarkedSafe(ISymbol symbol, ITypeSymbol typeSymbol)
        {
            if (MirageAttributes.WeaverSafeClass.Has(symbol))
                return true;

            if (typeSymbol != null && MirageAttributes.WeaverSafeClass.Has(typeSymbol))
                return true;

            if (symbol.ContainingType != null && MirageAttributes.WeaverSafeClass.Has(symbol.ContainingType))
                return true;

            return false;
        }

        private static bool IsRpcMethod(IMethodSymbol methodSymbol)
        {
            return MirageAttributes.ServerRpc.Has(methodSymbol) || MirageAttributes.ClientRpc.Has(methodSymbol);
        }

        private static bool IsVoidOrUniTask(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.SpecialType == SpecialType.System_Void)
                return true;

            if (MirageTypes.UniTask.Is(typeSymbol))
                return true;

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
