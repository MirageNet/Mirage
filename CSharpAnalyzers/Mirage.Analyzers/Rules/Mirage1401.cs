using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1401
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSyntaxNodeAction(nodeContext => AnalyzeIdentifier(nodeContext, symbols), SyntaxKind.IdentifierName);
        }

        private static void AnalyzeIdentifier(SyntaxNodeAnalysisContext context, MirageSymbols symbols)
        {
            var identifier = (IdentifierNameSyntax)context.Node;

            // 1. Are we inside Awake() or Start() of a NetworkBehaviour?
            var methodDecl = identifier.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDecl == null)
                return;

            var methodName = methodDecl.Identifier.Text;
            if (methodName != "Awake" && methodName != "Start")
                return;

            var classDecl = methodDecl.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol == null || !symbols.IsOrInherits(classSymbol, symbols.NetworkBehaviour))
                return;

            // 2. Get the symbol we are referencing
            var symbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol == null)
                return;

            bool isUnsafe = false;

            if (symbol is IPropertySymbol || symbol is IFieldSymbol)
            {
                var containingType = symbol.ContainingType;
                if (containingType != null)
                {
                    if (symbols.IsOrInherits(containingType, symbols.NetworkBehaviour))
                    {
                        var name = symbol.Name;
                        if (name == "IsServer" || name == "IsClient" || name == "IsLocalPlayer" || name == "HasAuthority" ||
                            name == "Server" || name == "Client" || name == "World" ||
                            name == "ServerObjectManager" || name == "ClientObjectManager")
                        {
                            isUnsafe = true;
                        }
                    }
                    else if (symbols.IsOrInherits(containingType, symbols.NetworkIdentity))
                    {
                        var name = symbol.Name;
                        if (name == "Visibility" || name == "SyncVarSender")
                        {
                            isUnsafe = true;
                        }
                    }
                }
            }
            else if (symbol is IMethodSymbol methodSymbol)
            {
                if (symbols.HasAttribute(methodSymbol, symbols.ServerRpcAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.ClientRpcAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.ServerAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.ClientAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.HasAuthorityAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.LocalPlayerAttribute) ||
                    symbols.HasAttribute(methodSymbol, symbols.NetworkMethodAttribute))
                {
                    isUnsafe = true;
                }
            }

            if (isUnsafe)
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.LifecycleNetworkStateRule,
                    identifier.GetLocation(),
                    symbol.Name,
                    methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
