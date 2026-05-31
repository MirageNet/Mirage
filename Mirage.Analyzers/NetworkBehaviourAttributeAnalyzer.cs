using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetworkBehaviourAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MIRAGE1101";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Network attributes can only be used on NetworkBehaviour classes",
            "Attribute '{0}' cannot be used on '{1}' because its declaring class does not inherit from NetworkBehaviour",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Network attributes like SyncVar, Server, Client, HasAuthority, LocalPlayer, ServerRpc, ClientRpc, and NetworkMethod are only valid inside NetworkBehaviour classes.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1101");

        private static readonly ImmutableHashSet<string> NetworkAttributes = ImmutableHashSet.Create(
            "Mirage.SyncVarAttribute",
            "Mirage.ServerAttribute",
            "Mirage.ClientAttribute",
            "Mirage.HasAuthorityAttribute",
            "Mirage.LocalPlayerAttribute",
            "Mirage.ServerRpcAttribute",
            "Mirage.ClientRpcAttribute",
            "Mirage.NetworkMethodAttribute"
        );

        private static readonly ImmutableHashSet<string> ShortNetworkAttributes = ImmutableHashSet.Create(
            "SyncVarAttribute",
            "ServerAttribute",
            "ClientAttribute",
            "HasAuthorityAttribute",
            "LocalPlayerAttribute",
            "ServerRpcAttribute",
            "ClientRpcAttribute",
            "NetworkMethodAttribute"
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;

            var attributes = symbol.GetAttributes();
            if (attributes.IsEmpty)
                return;

            var hasNetworkAttribute = false;
            foreach (var attr in attributes)
            {
                if (attr.AttributeClass != null && ShortNetworkAttributes.Contains(attr.AttributeClass.Name))
                {
                    var fullName = attr.AttributeClass.ToDisplayString();
                    if (NetworkAttributes.Contains(fullName))
                    {
                        hasNetworkAttribute = true;
                        break;
                    }
                }
            }

            if (!hasNetworkAttribute)
                return;

            var containingType = symbol.ContainingType;

            // Stop analysis if the declaring type inherits from NetworkBehaviour, as network attributes are valid in this context.
            if (containingType != null && IsOrInheritsFrom(containingType, "Mirage.NetworkBehaviour"))
                return;

            foreach (var attr in attributes)
            {
                if (attr.AttributeClass != null && ShortNetworkAttributes.Contains(attr.AttributeClass.Name))
                {
                    var fullName = attr.AttributeClass.ToDisplayString();
                    if (NetworkAttributes.Contains(fullName))
                    {
                        var location = attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? symbol.Locations[0];
                        var diagnostic = Diagnostic.Create(Rule, location, attr.AttributeClass.Name, symbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsOrInheritsFrom(ITypeSymbol typeSymbol, string fullyQualifiedName)
        {
            var lastDot = fullyQualifiedName.LastIndexOf('.');
            var shortName = lastDot >= 0 ? fullyQualifiedName.Substring(lastDot + 1) : fullyQualifiedName;

            var current = typeSymbol;
            while (current != null)
            {
                if (current.Name == shortName && current.ToDisplayString() == fullyQualifiedName)
                    return true;
                current = current.BaseType;
            }
            return false;
        }
    }
}
