using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Mirage.Analyzers.Tests
{
    public static class VerifyCS
    {
        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            return CSharpAnalyzerVerifier<MirageAnalyzer, NUnitVerifier>.Diagnostic(diagnosticId);
        }

        public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new Test
            {
                TestCode = source,
            };

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync();
        }

        public static string LoadTestData(string relativePath)
        {
            var fullPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "TestData", relativePath);
            return System.IO.File.ReadAllText(fullPath);
        }

        private class Test : CSharpAnalyzerTest<MirageAnalyzer, NUnitVerifier>
        {
            public Test()
            {
                ReferenceAssemblies = ReferenceAssemblies.Default;
                TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"d:\UnityProjects\Mirage\Library\ScriptAssemblies\Mirage.dll"));
                TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Data\Managed\UnityEngine\UnityEngine.dll"));
                TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(@"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll"));

                SolutionTransforms.Add((solution, projectId) =>
                {
                    var project = solution.GetProject(projectId);
                    var compilationOptions = project.CompilationOptions;
                    if (compilationOptions == null)
                        return solution;

                    var expectedIds = new System.Collections.Generic.HashSet<string>();
                    foreach (var exp in ExpectedDiagnostics)
                    {
                        expectedIds.Add(exp.Id);
                    }

                    // ignore info rule, because it will be added to almost everywhere
                    // (unless expectedIds has the rule, then keep it)
                    if (!expectedIds.Contains("MIRAGE1501"))
                    {
                        var newOptions = compilationOptions.SpecificDiagnosticOptions.SetItem("MIRAGE1501", ReportDiagnostic.Suppress);
                        project = project.WithCompilationOptions(compilationOptions.WithSpecificDiagnosticOptions(newOptions));
                    }
                    return project.Solution;
                });
            }
        }
    }
}
