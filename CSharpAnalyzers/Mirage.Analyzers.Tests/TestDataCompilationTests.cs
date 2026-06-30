using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class TestDataCompilationTests
    {
        private static readonly Regex MarkupRegex = new Regex(@"\{\|[^:]+:(.*?)\|\}", RegexOptions.Compiled);

        private static IEnumerable<string> GetTestDataFiles()
        {
            var testDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            if (!Directory.Exists(testDataDir))
                yield break;

            var files = Directory.GetFiles(testDataDir, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
                yield return file;
        }

        [Test]
        [TestCaseSource(nameof(GetTestDataFiles))]
        public void TestCompilation(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var cleanedContent = StripMarkup(content);

            var syntaxTree = CSharpSyntaxTree.ParseText(cleanedContent);

            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                    paths.Add(assembly.Location);
            }

            paths.Add(@"d:\UnityProjects\Mirage\Library\ScriptAssemblies\Mirage.dll");
            paths.Add(@"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Data\Managed\UnityEngine\UnityEngine.dll");
            paths.Add(@"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll");

            var references = new List<MetadataReference>();
            foreach (var path in paths)
            {
                if (File.Exists(path))
                    references.Add(MetadataReference.CreateFromFile(path));
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: "TestCompilation_" + Path.GetFileNameWithoutExtension(filePath),
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = compilation.GetDiagnostics();
            var errors = new List<Diagnostic>();
            foreach (var diag in diagnostics)
            {
                if (diag.Severity == DiagnosticSeverity.Error)
                    errors.Add(diag);
            }

            if (errors.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Compilation failed for file: {filePath}");
                foreach (var err in errors)
                    sb.AppendLine(err.ToString());
                Assert.Fail(sb.ToString());
            }
        }

        private static string StripMarkup(string content)
        {
            var cleaned = content;
            while (MarkupRegex.IsMatch(cleaned))
                cleaned = MarkupRegex.Replace(cleaned, "$1");
            return cleaned;
        }
    }
}
