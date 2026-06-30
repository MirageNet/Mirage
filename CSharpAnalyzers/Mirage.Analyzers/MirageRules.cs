using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mirage.Analyzers
{
    public static class MirageRules
    {
        public const string SyncVarDiagnosticId = "MIRAGE1001";
        public const string DirectMutationDiagnosticId = "MIRAGE1002";
        public const string ReassignmentDiagnosticId = "MIRAGE1003";
        public const string SyncVarHookDiagnosticId = "MIRAGE1004";
        public const string ReadonlySyncVarDiagnosticId = "MIRAGE1005";
        public const string NetworkBehaviourAttributeDiagnosticId = "MIRAGE1101";
        public const string RedundantRpcAttributeDiagnosticId = "MIRAGE1102";
        public const string MessageOrRpcDiagnosticId = "MIRAGE1201";
        public const string RpcSignatureDiagnosticId = "MIRAGE1202";
        public const string RpcRefOutDiagnosticId = "MIRAGE1203";
        public const string RpcStaticDiagnosticId = "MIRAGE1204";
        public const string ClientRpcTargetDiagnosticId = "MIRAGE1205";
        public const string RateLimitSettingsDiagnosticId = "MIRAGE1206";
        public const string ServerRpcMissingRateLimitDiagnosticId = "MIRAGE1207";
        public const string FieldTypeSerializationDiagnosticId = "MIRAGE1301";
        public const string UnserializedPrivateFieldDiagnosticId = "MIRAGE1302";
        public const string MismatchedSerializationDiagnosticId = "MIRAGE1303";
        public const string MonoBehaviourParameterDiagnosticId = "MIRAGE1304";
        public const string MissingNetworkMessageDiagnosticId = "MIRAGE1305";
        public const string LifecycleNetworkStateDiagnosticId = "MIRAGE1401";
        public const string LifecycleMissingBaseCallDiagnosticId = "MIRAGE1402";
        public const string EnabledPropertyCheckDiagnosticId = "MIRAGE1403";
        public const string PerformanceMessageSizeDiagnosticId = "MIRAGE1501";

        public static readonly DiagnosticDescriptor SyncVarRule = new DiagnosticDescriptor(
            SyncVarDiagnosticId,
            "SyncVar cannot be a class type unless marked safe",
            "SyncVar or SyncObject '{0}' is or contains class type '{1}'. Class-based SyncVars/SyncObjects allocate memory, do not support polymorphism (only declared fields serialize), and cannot track internal changes automatically (meaning modifications won't trigger sync hooks). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this SyncVar/SyncObject with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class types used as SyncVars or SyncObjects should be value types or marked with [WeaverSafeClass] to avoid allocations and hook tracking issues.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1001");

        public static readonly DiagnosticDescriptor DirectMutationRule = new DiagnosticDescriptor(
            DirectMutationDiagnosticId,
            "Direct Mutation of SyncCollection Elements",
            "Direct mutation of elements inside '{0}' is not supported because changes cannot be tracked. Retrieve the element, modify it, and assign it back using the collection indexer.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Direct modification of elements within SyncCollections because the changes cannot be detected or synced.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1002");

        public static readonly DiagnosticDescriptor ReassignmentRule = new DiagnosticDescriptor(
            ReassignmentDiagnosticId,
            "Reassignment of ISyncObject fields",
            "ISyncObject field '{0}' must be marked readonly and cannot be reassigned",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Restricts reassignment of fields implementing ISyncObject (like SyncList), requiring them to be marked readonly.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1003");

        public static readonly DiagnosticDescriptor SyncVarHookRule = new DiagnosticDescriptor(
            SyncVarHookDiagnosticId,
            "Invalid SyncVar Hook Method",
            "SyncVar hook '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Ensures [SyncVar] hook methods or events are correctly declared and matched by parameter type.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1004");

        public static readonly DiagnosticDescriptor ReadonlySyncVarRule = new DiagnosticDescriptor(
            ReadonlySyncVarDiagnosticId,
            "Readonly SyncVar Field",
            "SyncVar field '{0}' cannot be marked readonly",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Restricts declaring fields marked with [SyncVar] as readonly to ensure they are mutable at runtime.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1005");

        public static readonly DiagnosticDescriptor NetworkBehaviourAttributeRule = new DiagnosticDescriptor(
            NetworkBehaviourAttributeDiagnosticId,
            "Network attributes can only be used on NetworkBehaviour classes",
            "Attribute '{0}' cannot be used on '{1}' because its declaring class does not inherit from NetworkBehaviour",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Prevents Mirage network attributes from being declared inside classes that do not inherit from NetworkBehaviour.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1101");

        public static readonly DiagnosticDescriptor RedundantRpcAttributeRule = new DiagnosticDescriptor(
            RedundantRpcAttributeDiagnosticId,
            "Redundant Server/Client Attribute on RPC",
            "RPC method '{0}' is decorated with redundant active guard attribute '{1}'",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns if an RPC method is decorated with both a routing attribute ([ServerRpc]/[ClientRpc]) and an active guard ([Server]/[Client]).",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1102");

        public static readonly DiagnosticDescriptor MessageOrRpcRule = new DiagnosticDescriptor(
            MessageOrRpcDiagnosticId,
            "NetworkMessage/RPC Class Warning",
            "{0} '{1}' is a class type '{2}'. Class-based types allocate memory upon deserialization and do not support polymorphism (only declared fields serialize). Consider using a struct, implementing custom serialization and marking the class with [WeaverSafeClass], or decorating this member/parameter with [WeaverSafeClass] to ignore.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns about class types used inside network messages or RPC parameters because they cause GC allocations.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1201");

        public static readonly DiagnosticDescriptor RpcSignatureRule = new DiagnosticDescriptor(
            RpcSignatureDiagnosticId,
            "RPC Signature Error",
            "RPC method '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Disallows generic parameters on RPC methods and enforces valid return types (void, UniTask, or UniTask<T>).",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1202");

        public static readonly DiagnosticDescriptor RpcRefOutRule = new DiagnosticDescriptor(
            RpcRefOutDiagnosticId,
            "Pass-by-Reference Modifiers in RPCs",
            "RPC method '{0}' cannot have ref or out parameter modifiers: parameter '{1}' is ref or out",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Prohibits ref or out modifiers on RPC parameters since they cannot be serialized over a one-way boundary.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1203");

        public static readonly DiagnosticDescriptor RpcStaticRule = new DiagnosticDescriptor(
            RpcStaticDiagnosticId,
            "Static RPC Methods",
            "RPC method '{0}' cannot be static",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Disallows declaring RPC methods as static to preserve the NetworkBehaviour instance context.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1204");

        public static readonly DiagnosticDescriptor ClientRpcTargetRule = new DiagnosticDescriptor(
            ClientRpcTargetDiagnosticId,
            "Invalid ClientRpc Target Configurations",
            "ClientRpc method '{0}' target configuration is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Validates [ClientRpc] target settings, ensuring correct return types and connection parameters.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1205");

        public static readonly DiagnosticDescriptor RateLimitSettingsRule = new DiagnosticDescriptor(
            RateLimitSettingsDiagnosticId,
            "Invalid RateLimit Attribute Settings",
            "RateLimit attribute on '{0}' has invalid settings: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Enforces valid positive configurations for interval, refill, and max tokens inside [RateLimit] attributes.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1206");

        public static readonly DiagnosticDescriptor ServerRpcMissingRateLimitRule = new DiagnosticDescriptor(
            ServerRpcMissingRateLimitDiagnosticId,
            "Missing RateLimit on ServerRpc",
            "ServerRpc method '{0}' should have a [RateLimit] attribute to prevent spam",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Recommends decorating [ServerRpc] methods with [RateLimit] to prevent server denial of service (DoS) attacks.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1207");

        public static readonly DiagnosticDescriptor FieldTypeSerializationRule = new DiagnosticDescriptor(
            FieldTypeSerializationDiagnosticId,
            "Field Type Serialization Validation",
            "Type '{0}' used in '{1}' is not serializable by Mirage",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Confirms all fields in network messages/RPCs are serializable by Mirage or have registered custom serializers.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1301");

        public static readonly DiagnosticDescriptor UnserializedPrivateFieldRule = new DiagnosticDescriptor(
            UnserializedPrivateFieldDiagnosticId,
            "Unserialized Private Field Warning",
            "Private field or property '{0}' in NetworkMessage '{1}' is not serialized",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns if a private field or property in a NetworkMessage will not be serialized.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1302");

        public static readonly DiagnosticDescriptor MismatchedSerializationRule = new DiagnosticDescriptor(
            MismatchedSerializationDiagnosticId,
            "Mismatched Custom Serialization Methods",
            "Custom serialization for type '{0}' is invalid: {1}",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Requires custom serializers to contain matching, properly-signed reader and writer extension methods.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1303");

        public static readonly DiagnosticDescriptor MonoBehaviourParameterRule = new DiagnosticDescriptor(
            MonoBehaviourParameterDiagnosticId,
            "Non-Serializable MonoBehaviour Parameter",
            "MonoBehaviour type '{0}' used in '{1}' is not serializable by Mirage",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Prohibits passing a plain MonoBehaviour (not inheriting from NetworkBehaviour) in RPCs or message fields.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1304");

        public static readonly DiagnosticDescriptor MissingNetworkMessageRule = new DiagnosticDescriptor(
            MissingNetworkMessageDiagnosticId,
            "Missing NetworkMessage Attribute",
            "Type '{0}' is used as a network message but lacks the [NetworkMessage] attribute",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns if a type is sent or registered as a message, but lacks the [NetworkMessage] attribute.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1305");

        public static readonly DiagnosticDescriptor LifecycleNetworkStateRule = new DiagnosticDescriptor(
            LifecycleNetworkStateDiagnosticId,
            "Accessing Network State in Awake/Start",
            "Network state member or SyncVar '{0}' should not be accessed in {1}",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns against accessing network states like IsServer during early Unity lifecycle phases.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1401");

        public static readonly DiagnosticDescriptor LifecycleMissingBaseCallRule = new DiagnosticDescriptor(
            LifecycleMissingBaseCallDiagnosticId,
            "Missing base Call in OnSerialize/OnDeserialize",
            "Overridden method '{0}' is missing a call to its base implementation",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Ensures overriding OnSerialize or OnDeserialize in derived classes calls the base implementation.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1402");

        public static readonly DiagnosticDescriptor EnabledPropertyCheckRule = new DiagnosticDescriptor(
            EnabledPropertyCheckDiagnosticId,
            "Enabled property check on NetworkServer/Client/NetworkIdentity",
            "Checking or setting .enabled on '{0}' is not recommended. Use .Active or .IsSpawned instead.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Warns if checking/setting .enabled on NetworkServer, NetworkClient, or NetworkIdentity.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1403");

        public static readonly DiagnosticDescriptor PerformanceMessageSizeRule = new DiagnosticDescriptor(
            PerformanceMessageSizeDiagnosticId,
            "Network Message Serialized Size Estimation",
            "NetworkMessage '{0}' has an estimated serialized size of {1} bytes",
            "Performance",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Estimates the serialized size of all [NetworkMessage] types to help analyze bandwidth usage.",
            helpLinkUri: "https://miragenet.github.io/Mirage/docs/analyzers/MIRAGE1501");

        public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics = ImmutableArray.Create(
            SyncVarRule,
            DirectMutationRule,
            ReassignmentRule,
            SyncVarHookRule,
            ReadonlySyncVarRule,
            NetworkBehaviourAttributeRule,
            RedundantRpcAttributeRule,
            MessageOrRpcRule,
            RpcSignatureRule,
            RpcRefOutRule,
            RpcStaticRule,
            ClientRpcTargetRule,
            RateLimitSettingsRule,
            ServerRpcMissingRateLimitRule,
            FieldTypeSerializationRule,
            UnserializedPrivateFieldRule,
            MismatchedSerializationRule,
            MonoBehaviourParameterRule,
            MissingNetworkMessageRule,
            LifecycleNetworkStateRule,
            LifecycleMissingBaseCallRule,
            EnabledPropertyCheckRule,
            PerformanceMessageSizeRule
        );
    }
}
