using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
                return;

            var containingType = methodSymbol.ContainingType;
            if (containingType == null || !MirageTypes.NetworkBehaviour.IsOrInherits(containingType))
                return;

            // MIRAGE1401: Accessing Network State in Awake/Start
            if (methodSymbol.Name == "Awake" || methodSymbol.Name == "Start")
            {
                if (methodDeclaration.Body != null)
                {
                    var walker = new NetworkStateAccessWalker(context.SemanticModel, containingType, context);
                    walker.Visit(methodDeclaration.Body);
                }
            }

            // MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize
            if (methodSymbol.IsOverride && (methodSymbol.Name == "OnSerialize" || methodSymbol.Name == "OnDeserialize"))
            {
                var baseHierarchyHasSyncState = false;
                var baseType = containingType.BaseType;
                while (baseType != null && !MirageTypes.NetworkBehaviour.Is(baseType) && baseType.ToDisplayString() != "object")
                {
                    if (MirageTypes.HasSynchronizedState(baseType))
                    {
                        baseHierarchyHasSyncState = true;
                        break;
                    }
                    baseType = baseType.BaseType;
                }

                if (baseHierarchyHasSyncState)
                {
                    if (methodDeclaration.Body != null)
                    {
                        var walker = new BaseCallWalker(context.SemanticModel, methodSymbol.Name);
                        walker.Visit(methodDeclaration.Body);
                        if (!walker.HasBaseCall)
                        {
                            var diagnostic = Diagnostic.Create(MirageRules.LifecycleMissingBaseCallRule, methodSymbol.Locations[0], methodSymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private class NetworkStateAccessWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly INamedTypeSymbol _containingType;
            private readonly SyntaxNodeAnalysisContext _context;

            public NetworkStateAccessWalker(SemanticModel semanticModel, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context)
            {
                _semanticModel = semanticModel;
                _containingType = containingType;
                _context = context;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                CheckNode(node);
                base.VisitIdentifierName(node);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                CheckNode(node);
                base.VisitMemberAccessExpression(node);
            }

            private void CheckNode(ExpressionSyntax node)
            {
                var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                if (symbol == null)
                    return;

                if (symbol is IMethodSymbol methodSymbol)
                {
                    if (MirageAttributes.ServerRpc.Has(methodSymbol) ||
                        MirageAttributes.ClientRpc.Has(methodSymbol) ||
                        MirageAttributes.Server.Has(methodSymbol) ||
                        MirageAttributes.Client.Has(methodSymbol) ||
                        MirageAttributes.HasAuthority.Has(methodSymbol) ||
                        MirageAttributes.LocalPlayer.Has(methodSymbol) ||
                        MirageAttributes.NetworkMethod.Has(methodSymbol))
                    {
                        var diagnostic = Diagnostic.Create(MirageRules.LifecycleNetworkStateRule, node.GetLocation(), methodSymbol.Name, _context.ContainingSymbol?.Name ?? "Unknown");
                        _context.ReportDiagnostic(diagnostic);
                        return;
                    }
                }
                else if (symbol is IPropertySymbol || symbol is IFieldSymbol)
                {
                    var name = symbol.Name;
                    if (name == "IsServer" || name == "IsClient" || name == "HasAuthority" ||
                        name == "IsLocalPlayer" || name == "IsOwner" || name == "IsHost" ||
                        name == "Server" || name == "World" || name == "SyncVarSender" ||
                        name == "ServerObjectManager" || name == "Client" || name == "ClientObjectManager" ||
                        name == "Visibility")
                    {
                        var containingType = symbol.ContainingType;
                        if (containingType != null &&
                            (MirageTypes.NetworkBehaviour.IsOrInherits(containingType) ||
                             MirageTypes.NetworkIdentity.IsOrInherits(containingType)))
                        {
                            var diagnostic = Diagnostic.Create(MirageRules.LifecycleNetworkStateRule, node.GetLocation(), name, _context.ContainingSymbol?.Name ?? "Unknown");
                            _context.ReportDiagnostic(diagnostic);
                            return;
                        }
                    }
                }
            }
        }

        private class BaseCallWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly string _methodName;
            public bool HasBaseCall { get; private set; }

            public BaseCallWalker(SemanticModel semanticModel, string methodName)
            {
                _semanticModel = semanticModel;
                _methodName = methodName;
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                if (node.Expression is BaseExpressionSyntax)
                {
                    var symbol = _semanticModel.GetSymbolInfo(node.Name).Symbol;
                    if (symbol != null && symbol.Name == _methodName)
                        HasBaseCall = true;
                }
                base.VisitMemberAccessExpression(node);
            }
        }
    }
}
