using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mirage.Analyzers
{
    public static class MirageRules
    {
        public const string SyncVarDiagnosticId = "MIRAGE1001";
        public const string AutoPropertyDiagnosticId = "MIRAGE1002";
        public const string DirectMutationDiagnosticId = "MIRAGE1003";
        public const string ReassignmentDiagnosticId = "MIRAGE1004";
        public const string NetworkBehaviourAttributeDiagnosticId = "MIRAGE1101";
        public const string MessageOrRpcDiagnosticId = "MIRAGE1201";
        public const string RpcSignatureDiagnosticId = "MIRAGE1202";
        public const string RpcRefOutDiagnosticId = "MIRAGE1203";
        public const string RpcStaticDiagnosticId = "MIRAGE1204";
        public const string ClientRpcTargetDiagnosticId = "MIRAGE1205";
        public const string RateLimitSettingsDiagnosticId = "MIRAGE1206";
        public const string ServerRpcMissingRateLimitDiagnosticId = "MIRAGE1207";
        public const string FieldTypeSerializationDiagnosticId = "MIRAGE1301";
        public const string MismatchedSerializationDiagnosticId = "MIRAGE1303";
        public const string LifecycleNetworkStateDiagnosticId = "MIRAGE1401";
        public const string LifecycleMissingBaseCallDiagnosticId = "MIRAGE1402";
        public const string PerformanceMtuExceededDiagnosticId = "MIRAGE1501";
        public const string PerformanceUnboundedCollectionDiagnosticId = "MIRAGE1502";
        public const string PerformanceHighOverheadDiagnosticId = "MIRAGE1503";

        public static readonly DiagnosticDescriptor SyncVarRule = new DiagnosticDescriptor(
            SyncVarDiagnosticId,
            "SyncVar cannot be a class type unless marked safe",
            "SyncVar or SyncObject '{0}' is or contains class type '{1}'. Class-based SyncVars/SyncObjects allocate memory, do not support polymorphism (only declared fields serialize), and cannot track internal changes automatically (meaning modifications won't trigger sync hooks). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this SyncVar/SyncObject with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class types used as SyncVars or SyncObjects should be value types or marked with [WeaverSafeClass] to avoid allocations and hook tracking issues.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1001");

        public static readonly DiagnosticDescriptor AutoPropertyRule = new DiagnosticDescriptor(
            AutoPropertyDiagnosticId,
            "SyncVar property must be an auto-property",
            "SyncVar property '{0}' must be a non-static auto-property with both get and set accessors",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Properties marked with [SyncVar] must be automatic properties with both getter and setter, and cannot be static.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1002");

        public static readonly DiagnosticDescriptor DirectMutationRule = new DiagnosticDescriptor(
            DirectMutationDiagnosticId,
            "Direct mutation of elements inside SyncList/SyncDictionary",
            "Direct mutation of elements inside '{0}' is not supported because changes cannot be tracked. Use SetItemDirty or modify the collection directly.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Direct mutation of elements inside SyncList or SyncDictionary won't trigger sync updates. Use SetItemDirty or modify the collection directly.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1003");

        public static readonly DiagnosticDescriptor ReassignmentRule = new DiagnosticDescriptor(
            ReassignmentDiagnosticId,
            "Reassignment of ISyncObject fields",
            "ISyncObject field '{0}' must be marked readonly and cannot be reassigned",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Fields implementing ISyncObject must be marked readonly and cannot be reassigned after initialization.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1004");

        public static readonly DiagnosticDescriptor NetworkBehaviourAttributeRule = new DiagnosticDescriptor(
            NetworkBehaviourAttributeDiagnosticId,
            "Network attributes can only be used on NetworkBehaviour classes",
            "Attribute '{0}' cannot be used on '{1}' because its declaring class does not inherit from NetworkBehaviour",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Network attributes like SyncVar, Server, Client, HasAuthority, LocalPlayer, ServerRpc, ClientRpc, and NetworkMethod are only valid inside NetworkBehaviour classes.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1101");

        public static readonly DiagnosticDescriptor MessageOrRpcRule = new DiagnosticDescriptor(
            MessageOrRpcDiagnosticId,
            "Class type used in NetworkMessage or RPC without WeaverSafeClass attribute",
            "{0} '{1}' is a class type '{2}'. Class-based types allocate memory upon deserialization and do not support polymorphism (only declared fields serialize). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this member/parameter with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class types used as NetworkMessage fields or RPC parameters/returns should be value types or marked with [WeaverSafeClass] to avoid allocations and polymorphism bugs.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1201");

        public static readonly DiagnosticDescriptor RpcSignatureRule = new DiagnosticDescriptor(
            RpcSignatureDiagnosticId,
            "RPC method must be non-generic and return void or UniTask",
            "RPC method '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Methods marked with ServerRpc or ClientRpc cannot be generic and must return void or UniTask.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1202");

        public static readonly DiagnosticDescriptor RpcRefOutRule = new DiagnosticDescriptor(
            RpcRefOutDiagnosticId,
            "Pass-by-Reference Modifiers in RPCs",
            "RPC method '{0}' cannot have ref or out parameter modifiers: parameter '{1}' is ref or out",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "RPC parameters cannot be pass-by-reference (ref/out).",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1203");

        public static readonly DiagnosticDescriptor RpcStaticRule = new DiagnosticDescriptor(
            RpcStaticDiagnosticId,
            "Static RPC Methods",
            "RPC method '{0}' cannot be static",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "RPC methods cannot be static.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1204");

        public static readonly DiagnosticDescriptor ClientRpcTargetRule = new DiagnosticDescriptor(
            ClientRpcTargetDiagnosticId,
            "Invalid ClientRpc Target Configurations",
            "ClientRpc method '{0}' target configuration is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ClientRpc target configurations must be valid.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1205");

        public static readonly DiagnosticDescriptor RateLimitSettingsRule = new DiagnosticDescriptor(
            RateLimitSettingsDiagnosticId,
            "Invalid RateLimit Attribute Settings",
            "RateLimit attribute on '{0}' has invalid settings: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "RateLimit parameters must be positive and MaxTokens >= Refill.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1206");

        public static readonly DiagnosticDescriptor ServerRpcMissingRateLimitRule = new DiagnosticDescriptor(
            ServerRpcMissingRateLimitDiagnosticId,
            "Missing RateLimit on ServerRpc",
            "ServerRpc method '{0}' should have a [RateLimit] attribute to prevent spam",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Every ServerRpc should be protected by a [RateLimit] attribute.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1207");

        public static readonly DiagnosticDescriptor FieldTypeSerializationRule = new DiagnosticDescriptor(
            FieldTypeSerializationDiagnosticId,
            "Field Type Serialization Validation",
            "Type '{0}' used in '{1}' is not serializable by Mirage",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "All fields in NetworkMessages and parameters in RPCs must be serializable by Mirage.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1301");

        public static readonly DiagnosticDescriptor MismatchedSerializationRule = new DiagnosticDescriptor(
            MismatchedSerializationDiagnosticId,
            "Mismatched Custom Serialization Methods",
            "Custom serialization for type '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Custom writer and reader extension methods must have matching signatures.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1303");

        public static readonly DiagnosticDescriptor LifecycleNetworkStateRule = new DiagnosticDescriptor(
            LifecycleNetworkStateDiagnosticId,
            "Accessing Network State in Awake/Start",
            "Network state member or SyncVar '{0}' should not be accessed in {1}",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Network states are not yet initialized during Awake or Start.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1401");

        public static readonly DiagnosticDescriptor LifecycleMissingBaseCallRule = new DiagnosticDescriptor(
            LifecycleMissingBaseCallDiagnosticId,
            "Missing base Call in OnSerialize/OnDeserialize",
            "Overridden method '{0}' is missing a call to its base implementation",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Overriding OnSerialize or OnDeserialize in a class that inherits from a class with synchronized state must call the base method.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1402");

        public static readonly DiagnosticDescriptor PerformanceMtuExceededRule = new DiagnosticDescriptor(
            PerformanceMtuExceededDiagnosticId,
            "Network Message Exceeds Safe MTU",
            "NetworkMessage '{0}' has an estimated serialized size of {1} bytes, which exceeds the safe MTU of {2} bytes",
            "Performance",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Network messages should remain within the safe MTU to avoid fragmentation.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1501");

        public static readonly DiagnosticDescriptor PerformanceUnboundedCollectionRule = new DiagnosticDescriptor(
            PerformanceUnboundedCollectionDiagnosticId,
            "Unbounded String or Collection",
            "Field/property/parameter '{0}' of type '{1}' is unbounded. Restrict its size using [BitCount] or another packing attribute.",
            "Performance",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Unbounded strings or collections can be exploited to cause memory exhaustion.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1502");

        public static readonly DiagnosticDescriptor PerformanceHighOverheadRule = new DiagnosticDescriptor(
            PerformanceHighOverheadDiagnosticId,
            "High Bit-Overhead Primitive Type",
            "Field/property/parameter '{0}' of type '{1}' is uncompressed. Consider applying a bit-packing or compression attribute.",
            "Performance",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Uncompressed primitives or vectors consume unnecessary bandwidth.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1503");

        public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics = ImmutableArray.Create(
            SyncVarRule,
            AutoPropertyRule,
            DirectMutationRule,
            ReassignmentRule,
            NetworkBehaviourAttributeRule,
            MessageOrRpcRule,
            RpcSignatureRule,
            RpcRefOutRule,
            RpcStaticRule,
            ClientRpcTargetRule,
            RateLimitSettingsRule,
            ServerRpcMissingRateLimitRule,
            FieldTypeSerializationRule,
            MismatchedSerializationRule,
            LifecycleNetworkStateRule,
            LifecycleMissingBaseCallRule,
            PerformanceMtuExceededRule,
            PerformanceUnboundedCollectionRule,
            PerformanceHighOverheadRule
        );
    }
}
