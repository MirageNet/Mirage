using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mirage.SourceGenerator
{
    [Generator]
    public class MainGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is MySyntaxReceiver receiver))
                return;


            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                if (!IsNetworkBehaviour(context.Compilation, classDeclaration))
                    continue;

                try
                {
                    var generator = new SyncVarSetterGenerator(context, classDeclaration);
                    generator.Execute();
                }
                catch (Exception e)
                {
                    Report(context, classDeclaration, DiagnosticSeverity.Error,
                        "Exception Creating Setter", e.ToString());
                }
            }
        }

        private static bool IsNetworkBehaviour(Compilation compilation, ClassDeclarationSyntax classDeclaration)
        {
            var baseList = classDeclaration.BaseList;
            if (baseList == null)
                return false;


            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            foreach (var baseType in baseList.Types)
            {
                var typeInfo = model.GetTypeInfo(baseType.Type);
                if (typeInfo.Type != null && IsSubclassOfNetworkBehaviour(typeInfo.Type))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsSubclassOfNetworkBehaviour(ITypeSymbol type)
        {
            while (type != null)
            {
                if (type.Name == "NetworkBehaviour")
                    return true;

                type = type.BaseType;
            }

            return false;
        }


        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        private static void Report(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration, DiagnosticSeverity level, string title, string message)
        {
            var descriptor = new DiagnosticDescriptor(
                "SG9999",
                title,
                message,
                "Mirage.SourceGenerator",
                level,
                true
            );

            context.ReportDiagnostic(Diagnostic.Create(descriptor, classDeclaration.GetLocation()));
        }
    }

    internal class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is ClassDeclarationSyntax classDeclarationSyntax))
                return;

            // if it is a type with base classes, we want to check it
            // we dont have enough context to check if it is NetworkBehaviour here, we need 
            var baseList = classDeclarationSyntax.BaseList;
            if (baseList == null)
                return;

            CandidateClasses.Add(classDeclarationSyntax);
        }
    }

    public class SyncVarSetterGenerator
    {
        private List<List<string>> methods = new List<List<string>>();
        private HashSet<TypeSyntax> types = new HashSet<TypeSyntax>();
        private readonly GeneratorExecutionContext _context;
        private readonly ClassDeclarationSyntax _classDeclaration;

        public SyncVarSetterGenerator(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration)
        {
            _context = context;
            _classDeclaration = classDeclaration;
        }

        private void Report(DiagnosticSeverity level, string title, string message, string id = "SG0001")
        {
            var descriptor = new DiagnosticDescriptor(
                id,
                title,
                message,
                "Mirage.SourceGenerator",
                level,
                true
            );

            _context.ReportDiagnostic(Diagnostic.Create(descriptor, _classDeclaration.GetLocation()));
        }

        public void Execute()
        {
            var model = _context.Compilation.GetSemanticModel(_classDeclaration.SyntaxTree);
            var fields = _classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>();

            foreach (var field in fields)
            {
                ProcessField(field);
            }

            //Report(DiagnosticSeverity.Info, "Found Methods", $"Found {methods.Count} in {_classDeclaration.Identifier.ValueText}");

            if (methods.Count > 0)
            {
                var isPartial = _classDeclaration.Modifiers
                                .Any(m => m.IsKind(SyntaxKind.PartialKeyword));

                var lines = WrapInClass(_context.Compilation, _classDeclaration, isPartial);
                var generatedCode = Build(lines);

                LogFile(generatedCode);
                AddSource(generatedCode);

                //if (!isPartial)
                //{
                //    var className = _classDeclaration.Identifier.ValueText.TrimEnd();
                //    if (className != "ReadyCheck")
                //    {
                //        return;
                //    }

                //    var newClass = _classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
                //    var tree = model.SyntaxTree;
                //    var newTree = tree.GetRoot().ReplaceNode(_classDeclaration, newClass);
                //    _context.AddSource($"{className}.g.cs", newTree.GetText(Encoding.UTF8));
                //}
                //var typeInfo = model.GetTypeInfo(_classDeclaration);
                //typeInfo.Type.
                //typeInfo

                //_classDeclaration


            }
        }

        private void LogFile(string generatedCode)
        {
            var @namespace = Helper.GetNamespace(_classDeclaration);
            var fileName = string.IsNullOrEmpty(@namespace)
                                    ? $"_root/{_classDeclaration.Identifier.ValueText}_SyncVar.g.cs"
                                    : $"{@namespace}/{_classDeclaration.Identifier.ValueText}_SyncVar.g.cs";

            var folder = Path.GetDirectoryName($"./Logs/SyncVarSettingGenerator/{fileName}");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllText($"./Logs/SyncVarSettingGenerator/{fileName}", generatedCode);

            //Report(DiagnosticSeverity.Info, "Saving Debug", $"Saving Generated file to {Path.GetFullPath(fileName)}");
        }

        private void AddSource(string generatedCode)
        {
            var @namespace = Helper.GetNamespace(_classDeclaration);
            var hintName = string.IsNullOrEmpty(@namespace)
                ? $"_root.{_classDeclaration.Identifier.ValueText}_SyncVar.g.cs"
                : $"{@namespace}.{_classDeclaration.Identifier.ValueText}_SyncVar.g.cs";
            _context.AddSource(hintName, SourceText.From(generatedCode, Encoding.UTF8));
        }

        private void ProcessField(FieldDeclarationSyntax field)
        {
            var attributes = field.AttributeLists.SelectMany(a => a.Attributes);
            if (!attributes.Any(a => a.Name.ToString() == "SyncVar"))
                return;

            //LogField(context, field);

            var fieldName = field.Declaration.Variables.First().Identifier.ValueText;
            var fieldType = field.Declaration.Type;
            types.Add(fieldType);

            //field.Declaration.gen
            //fieldType.gneric

            //fieldType.

            //if (fieldType.ContainingType != null)
            //{

            //}
            var config = default(SyncVarConfig);// SyncVarConfig.FromData(att)

            //var typeName = _classDeclaration.to

            var lines = GenerateSetNetworkMethod(fieldName, fieldType.ToString(), config);
            methods.Add(lines);
        }
        private static List<string> GenerateSetNetworkMethod(string fieldName, string fieldType, SyncVarConfig config)
        {
            //config.

            return new List<string>
            {
                $"public static void SetNetwork__{fieldName}(NetworkBehaviour nb, ref {fieldType} field, {fieldType} value)",
                $"{{",
                $"    {fieldType} oldValue = field;",
                $"    if (NetworkBehaviour.SyncVarEqual<{fieldType}>(value, oldValue))",
                $"    {{",
                $"        return;",
                $"    }}",
                $"",
                $"    field = value;",
                $"    nb.SetDirtyBit(1UL);",
                //$"    if (base.IsServer && !base.GetSyncVarHookGuard(1UL))",
                //$"    {{",
                //$"        base.SetSyncVarHookGuard(1UL, true);",
                //$"        Action<{fieldType}> on{fieldName}Changed = this.On{fieldName}Changed;",
                //$"        if (on{fieldName}Changed != null)",
                //$"        {{",
                //$"            on{fieldName}Changed.Invoke(value);",
                //$"        }}",
                //$"        base.SetSyncVarHookGuard(1UL, false);",
                //$"    }}",
                $"}}"
            };
        }

        private List<string> WrapInClass(Compilation compilation, ClassDeclarationSyntax classDeclaration, bool isPartial)
        {
            var builder = new LineBuilder();
            var imports = Helper.GetImports(compilation, types);
            imports.Add($"using Mirage;");
            foreach (var import in imports)
            {
                builder.Add(import);
            }
            builder.Add("");


            var @namespace = Helper.GetNamespace(classDeclaration);
            if (!string.IsNullOrEmpty(@namespace))
            {
                builder.Add($"namespace {@namespace}");
                builder.StartIndent();
            }

            var className = classDeclaration.Identifier.ValueText.TrimEnd();
            //DebugGeneric(compilation, classDeclaration, className);
            var generics = GetGenerics(classDeclaration);

            if (isPartial)
            {
                builder.Add($"public partial class {className}{generics}");
            }
            else
            {
                builder.Add($"public class {className}_SyncVar{generics}");
            }
            builder.StartIndent();


            foreach (var methodLines in methods)
                foreach (var line in methodLines)
                    builder.Add($"{line}");


            builder.EndIndent();
            if (!string.IsNullOrEmpty(@namespace))
            {
                builder.EndIndent();
            }
            return builder.lines;
        }

        public string GetGenerics(ClassDeclarationSyntax type)
        {
            var typeParams = type.TypeParameterList;
            if (typeParams == null)
                return "";

            return typeParams.ToFullString();
        }

        private void DebugGeneric(Compilation compilation, ClassDeclarationSyntax classDeclaration, string className)
        {
            var typeParams = classDeclaration.TypeParameterList;
            if (typeParams != null)
            {
                Report(DiagnosticSeverity.Warning, className, $"Full:'{typeParams.ToFullString()}'", id: "SG0003");
            }
            else
            {
                Report(DiagnosticSeverity.Warning, className, $"Null typeParams", id: "SG0003");
            }
            //var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            //var typeSymbol = semanticModel.GetSymbolInfo(type).Symbol;

            //var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            //var typeSymbol = semanticModel.GetSymbolInfo(classDeclaration).Symbol;
            //if (typeSymbol == null)
            //{
            //    Report(DiagnosticSeverity.Warning, className, $"Null Symbol", id: "SG0003");
            //}
            //else
            //{
            //    Report(DiagnosticSeverity.Warning, className, $"Kind:{typeSymbol.Kind}, MetadataName:{typeSymbol.MetadataName}", id: "SG0003");
            //}
        }

        private static string Build(List<string> lines)
        {
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                builder.AppendLine($"{line}");
            }
            var generatedCode = builder.ToString();
            return generatedCode;
        }

    }

    internal class Helper
    {
        public static string GetNamespace(ClassDeclarationSyntax classDeclaration)
        {
            var parent = classDeclaration.Parent;
            while (parent != null && !(parent is NamespaceDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return namespaceDeclaration.Name.ToString();
            }

            return null;
        }

        public static HashSet<string> GetImports(Compilation compilation, HashSet<TypeSyntax> types)
        {
            var imports = new HashSet<string>();
            foreach (var type in types)
            {
                // Get the semantic model for the syntax tree that contains the type
                var semanticModel = compilation.GetSemanticModel(type.SyntaxTree);

                // Get the symbol for the type
                var typeSymbol = semanticModel.GetSymbolInfo(type).Symbol;

                // Get the full namespace of the type
                var @namespace = typeSymbol.ContainingNamespace.ToDisplayString();

                if (typeSymbol.ContainingType != null)
                {
                    var typeName = typeSymbol.Name;
                    var containerName = typeSymbol.ContainingType;
                    var kind = typeSymbol.Kind;
                    var ignore = "";
                    if (kind == SymbolKind.TypeParameter)
                    {
                        ignore = "//";
                    }

                    imports.Add($"{ignore} using {typeName} = {@namespace}.{containerName.Name}.{typeName}; // {kind}");
                }
                else
                {
                    imports.Add($"using {@namespace};");
                }
            }
            return imports;
        }

    }

    internal class LineBuilder
    {
        public List<string> lines = new List<string>();
        public int indent;
        public void Add(string text)
        {
            var pad = "".PadLeft(indent * 4, ' ');
            lines.Add($"{pad}{text}");
        }

        public void StartIndent()
        {
            Add($"{{");
            indent++;
        }

        public void EndIndent()
        {
            indent--;
            Add($"}}");
        }
    }

    internal struct SyncVarConfig
    {
        public string Hook;
        public bool InitialOnly;
        public bool InvokeHookOnServer;
        public int HookType;

        public static SyncVarConfig FromData(AttributeData attributeData)
        {
            var result = new SyncVarConfig();

            foreach (var namedArgument in attributeData.NamedArguments)
            {
                switch (namedArgument.Key)
                {
                    case "hook":
                        result.Hook = (string)namedArgument.Value.Value;
                        break;
                    case "initialOnly":
                        result.InitialOnly = (bool)namedArgument.Value.Value;
                        break;
                    case "invokeHookOnServer":
                        result.InvokeHookOnServer = (bool)namedArgument.Value.Value;
                        break;
                    case "hookType":
                        result.HookType = (int)namedArgument.Value.Value;
                        break;
                }
            }

            return result;
        }
    }
}
