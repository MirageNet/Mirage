using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RpcSignatureAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MIRAGE1201";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "RPC method must be non-generic and return void or UniTask",
            "RPC method '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Methods marked with ServerRpc or ClientRpc cannot be generic and must return void or UniTask.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1201");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            // Stop analysis if the method is not a ServerRpc or ClientRpc, as RPC rules only apply to them.
            if (!IsRpcMethod(methodSymbol))
                return;

            if (methodSymbol.IsGenericMethod)
            {
                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name, "cannot have generic parameters");
                context.ReportDiagnostic(diagnostic);
            }

            if (!IsVoidOrUniTask(methodSymbol.ReturnType))
            {
                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name, $"cannot return '{methodSymbol.ReturnType.ToDisplayString()}' (must return void or UniTask)");
                context.ReportDiagnostic(diagnostic);
            }
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

        private static bool IsVoidOrUniTask(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.SpecialType == SpecialType.System_Void)
                return true;

            var originalDefinition = typeSymbol.OriginalDefinition;
            if (originalDefinition == null)
                return false;

            if (originalDefinition.Name == "UniTask")
            {
                var originalString = originalDefinition.ToDisplayString();
                if (originalString == "Cysharp.Threading.Tasks.UniTask" || originalString.StartsWith("Cysharp.Threading.Tasks.UniTask<"))
                    return true;
            }

            return false;
        }
    }
}
