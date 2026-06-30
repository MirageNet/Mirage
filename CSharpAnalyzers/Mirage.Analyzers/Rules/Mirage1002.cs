using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1002
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSyntaxNodeAction(nodeContext => AnalyzeNode(nodeContext, symbols),
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression,
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PreIncrementExpression,
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.Argument);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context, MirageSymbols symbols)
        {
            var mutationTarget = Helpers.GetMutationTarget(context.Node);
            if (mutationTarget == null)
                return;

            var current = mutationTarget;
            var traversedMemberAccess = false;
            while (current is MemberAccessExpressionSyntax memberAccess)
            {
                traversedMemberAccess = true;
                current = memberAccess.Expression;
            }

            if (traversedMemberAccess && current is ElementAccessExpressionSyntax elementAccess)
            {
                var collectionInfo = context.SemanticModel.GetSymbolInfo(elementAccess.Expression);
                var collectionSymbol = collectionInfo.Symbol;
                if (collectionSymbol != null)
                {
                    var typeSymbol = context.SemanticModel.GetTypeInfo(elementAccess.Expression).Type;
                    if (typeSymbol != null && Helpers.IsSyncListOrDictionary(typeSymbol, symbols))
                    {
                        var diagnostic = Diagnostic.Create(
                            MirageRules.DirectMutationRule,
                            elementAccess.GetLocation(),
                            collectionSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
