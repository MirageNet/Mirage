using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mirage.Analyzers
{
    public partial class MirageAnalyzer
    {
        private static void AnalyzeMethodRpcs(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            AnalyzeNetworkAttributes(context, methodSymbol);

            if (IsRpcMethod(methodSymbol))
            {
                if (methodSymbol.IsGenericMethod)
                {
                    var diagnostic = Diagnostic.Create(MirageRules.RpcSignatureRule, methodSymbol.Locations[0], methodSymbol.Name, "cannot have generic parameters");
                    context.ReportDiagnostic(diagnostic);
                }

                if (!IsVoidOrUniTask(methodSymbol.ReturnType))
                {
                    var diagnostic = Diagnostic.Create(MirageRules.RpcSignatureRule, methodSymbol.Locations[0], methodSymbol.Name, $"cannot return '{methodSymbol.ReturnType.ToDisplayString()}' (must return void or UniTask)");
                    context.ReportDiagnostic(diagnostic);
                }

                // MIRAGE1202: RPC parameters cannot have ref/out parameter modifiers
                foreach (var param in methodSymbol.Parameters)
                {
                    if (param.RefKind == RefKind.Ref || param.RefKind == RefKind.Out)
                    {
                        var diagnostic = Diagnostic.Create(MirageRules.RpcRefOutRule, param.Locations[0], methodSymbol.Name, param.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }

                // MIRAGE1203: Static RPC Methods
                if (methodSymbol.IsStatic)
                {
                    var diagnostic = Diagnostic.Create(MirageRules.RpcStaticRule, methodSymbol.Locations[0], methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }

                // MIRAGE1204: Invalid ClientRpc Target Configurations
                if (MirageAttributes.ClientRpc.TryGet(methodSymbol, out var clientRpcAttr))
                {
                    int targetVal = 1; // Default is RpcTarget.Observers (1)
                    MirageAttributes.ClientRpc.TryGetNamedArgument(clientRpcAttr, "target", out targetVal);

                    if (targetVal == 1 && !methodSymbol.ReturnsVoid)
                    {
                        var diagnostic = Diagnostic.Create(MirageRules.ClientRpcTargetRule, methodSymbol.Locations[0], methodSymbol.Name, "must return void when target is Observers");
                        context.ReportDiagnostic(diagnostic);
                    }

                    if (targetVal == 2)
                    {
                        bool firstParamIsValid = false;
                        if (methodSymbol.Parameters.Length > 0)
                        {
                            var firstParamType = methodSymbol.Parameters[0].Type;
                            if (MirageTypes.IsNetworkPlayerOrConnection(firstParamType))
                                firstParamIsValid = true;
                        }
                        if (!firstParamIsValid)
                        {
                            var diagnostic = Diagnostic.Create(MirageRules.ClientRpcTargetRule, methodSymbol.Locations[0], methodSymbol.Name, "method with target = Player requires first parameter to be INetworkPlayer or NetworkConnection");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }

                // MIRAGE1205: Invalid RateLimit Attribute Settings
                bool hasRateLimit = MirageAttributes.RateLimit.TryGet(methodSymbol, out var rateLimitAttr);
                if (hasRateLimit)
                {
                    float interval = 1f;
                    int refill = 50;
                    int maxTokens = 200;
                    MirageAttributes.RateLimit.TryGetNamedArgument(rateLimitAttr, "Interval", out interval);
                    MirageAttributes.RateLimit.TryGetNamedArgument(rateLimitAttr, "Refill", out refill);
                    MirageAttributes.RateLimit.TryGetNamedArgument(rateLimitAttr, "MaxTokens", out maxTokens);

                    var errors = new List<string>();
                    if (interval <= 0)
                        errors.Add("Interval must be greater than zero");
                    if (refill <= 0)
                        errors.Add("Refill must be greater than zero");
                    if (maxTokens <= 0)
                        errors.Add("MaxTokens must be greater than zero");
                    if (maxTokens > 0 && refill > 0 && maxTokens < refill)
                        errors.Add("MaxTokens must be greater than or equal to Refill");

                    if (errors.Count > 0)
                    {
                        var diagnostic = Diagnostic.Create(MirageRules.RateLimitSettingsRule, methodSymbol.Locations[0], methodSymbol.Name, string.Join(", ", errors));
                        context.ReportDiagnostic(diagnostic);
                    }
                }

                // MIRAGE1206: Missing RateLimit on ServerRpc
                if (MirageAttributes.ServerRpc.TryGet(methodSymbol, out _) && !hasRateLimit)
                {
                    var diagnostic = Diagnostic.Create(MirageRules.ServerRpcMissingRateLimitRule, methodSymbol.Locations[0], methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeParameterRpcs(SymbolAnalysisContext context)
        {
            // RPC parameter validation is handled inside AnalyzeMethodRpcs.
        }
    }
}
