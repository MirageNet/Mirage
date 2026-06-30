using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MirageAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => MirageRules.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var symbols = new MirageSymbols(compilationContext.Compilation);
                if (symbols.NetworkBehaviour == null)
                    return;

                Mirage1001.Register(compilationContext, symbols);
                Mirage1002.Register(compilationContext, symbols);
                Mirage1003.Register(compilationContext, symbols);
                Mirage1004.Register(compilationContext, symbols);
                Mirage1005.Register(compilationContext, symbols);
            });
        }
    }
}
