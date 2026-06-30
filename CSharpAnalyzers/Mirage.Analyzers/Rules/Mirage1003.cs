using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public static class Mirage1003
    {
        public static void Register(CompilationStartAnalysisContext context, MirageSymbols symbols)
        {
            context.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, symbols), SymbolKind.Field);
            context.RegisterSyntaxNodeAction(nodeContext => AnalyzeAssignment(nodeContext, symbols), SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeField(SymbolAnalysisContext context, MirageSymbols symbols)
        {
            var fieldSymbol = (IFieldSymbol)context.Symbol;

            if (symbols.Implements(fieldSymbol.Type, symbols.ISyncObject) && !fieldSymbol.IsReadOnly)
            {
                var diagnostic = Diagnostic.Create(
                    MirageRules.ReassignmentRule,
                    fieldSymbol.Locations[0],
                    fieldSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, MirageSymbols symbols)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;

            if (symbol is IFieldSymbol fieldSymbol && symbols.Implements(fieldSymbol.Type, symbols.ISyncObject))
            {
                if (!Helpers.IsInsideConstructor(assignment))
                {
                    var diagnostic = Diagnostic.Create(
                        MirageRules.ReassignmentRule,
                        assignment.Left.GetLocation(),
                        fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
