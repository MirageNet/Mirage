# Mirage Roslyn Analyzers

Mirage uses Roslyn Analyzers to provide compile-time validation for network code, replacing or augmenting post-compilation IL weaving diagnostics. By catching configuration errors, unsafe serialization patterns, and improper API usage directly in your IDE during compilation, analyzers shorten the feedback loop, prevent runtime exceptions, and help write secure, high-performance multiplayer code.

---

## Rule Summary Table

| Rule ID | Name | Severity | Short Summary |
| --- | --- | --- | --- |
| [MIRAGE1001](MIRAGE1001.md) | SyncVar Class Warning | Warning | Warns against using class types for `[SyncVar]` properties due to allocations and change-tracking limitations. |
| [MIRAGE1002](MIRAGE1002.md) | SyncVar Auto-Property Error | Error | Ensures `[SyncVar]` properties are non-static auto-properties with both getter and setter. |
| [MIRAGE1003](MIRAGE1003.md) | Direct Mutation of SyncCollection Elements | Warning | Flags direct modification of elements within SyncCollections because the changes cannot be detected or synced. |
| [MIRAGE1004](MIRAGE1004.md) | Reassignment of SyncObject Fields | Error | Restricts reassignment of fields implementing `ISyncObject` (like `SyncList`), requiring them to be marked `readonly`. |
| [MIRAGE1005](MIRAGE1005.md) | Invalid SyncVar Hook Method | Error | Ensures `[SyncVar]` hook methods or events are correctly declared, matched by parameter type, and that hook events are non-static. |
| [MIRAGE1101](MIRAGE1101.md) | Misplaced Network Attribute Error | Error | Prevents Mirage network attributes from being declared inside classes that do not inherit from `NetworkBehaviour`. |
| [MIRAGE1102](MIRAGE1102.md) | Redundant Server/Client Attribute on RPC | Warning | Warns if an RPC method is decorated with both a routing attribute ([ServerRpc]/[ClientRpc]) and an active guard ([Server]/[Client]). |
| [MIRAGE1201](MIRAGE1201.md) | NetworkMessage/RPC Class Warning | Warning | Warns about class types used inside network messages or RPC parameters because they cause GC allocations. |
| [MIRAGE1202](MIRAGE1202.md) | RPC Signature Error | Error | Disallows generic parameters on RPC methods and enforces valid return types (`void`, `UniTask`, or `UniTask<T>`). |
| [MIRAGE1203](MIRAGE1203.md) | Pass-by-Reference Modifiers in RPCs | Error | Prohibits `ref` or `out` modifiers on RPC parameters since they cannot be serialized over a one-way boundary. |
| [MIRAGE1204](MIRAGE1204.md) | Static RPC Methods | Error | Disallows declaring RPC methods as `static` to preserve the `NetworkBehaviour` instance context. |
| [MIRAGE1205](MIRAGE1205.md) | Invalid ClientRpc Target Configurations | Error | Validates `[ClientRpc]` target settings, ensuring correct return types and connection parameters. |
| [MIRAGE1206](MIRAGE1206.md) | Invalid RateLimit Attribute Settings | Error | Enforces valid positive configurations for interval, refill, and max tokens inside `[RateLimit]` attributes. |
| [MIRAGE1207](MIRAGE1207.md) | Missing RateLimit on ServerRpc | Warning | Recommends decorating `[ServerRpc]` methods with `[RateLimit]` to prevent server denial of service (DoS) attacks. |
| [MIRAGE1301](MIRAGE1301.md) | Field Type Serialization Validation | Error | Confirms all fields in network messages/RPCs are serializable by Mirage or have registered custom serializers. |
| [MIRAGE1302](MIRAGE1302.md) | Unserialized Private Field Warning | Warning | (SKIP) Warns if a private field or property in a NetworkMessage will not be serialized. |
| [MIRAGE1303](MIRAGE1303.md) | Mismatched Custom Serialization Methods | Error | Requires custom serializers to contain matching, properly-signed reader and writer extension methods. |
| [MIRAGE1304](MIRAGE1304.md) | Non-Serializable MonoBehaviour Parameter | Error | Prohibits passing a plain `MonoBehaviour` (not inheriting from `NetworkBehaviour`) in RPCs or message fields. |
| [MIRAGE1305](MIRAGE1305.md) | Missing NetworkMessage Attribute | Warning | Warns if a type is sent or registered as a message, but lacks the `[NetworkMessage]` attribute. |
| [MIRAGE1401](MIRAGE1401.md) | Accessing Network State in Awake/Start | Warning | Warns against accessing network states like `IsServer` during early Unity lifecycle phases. |
| [MIRAGE1402](MIRAGE1402.md) | Missing base Call in OnSerialize/OnDeserialize | Warning | Ensures overriding `OnSerialize` or `OnDeserialize` in derived classes calls the base implementation. |
| [MIRAGE1501](MIRAGE1501.md) | Network Message Serialized Size Estimation | Info | Estimates the serialized size of all `[NetworkMessage]` types to help analyze bandwidth usage. |

---

## Detailed Rule Breakdown

### Group 1: SyncVars & SyncObjects

#### [MIRAGE1001: SyncVar Class Warning](MIRAGE1001.md)
Using class types inside properties decorated with `[SyncVar]` triggers this warning. Reference types cause heap allocations during deserialization and do not support automatic modification detection because their reference does not change. To resolve this, convert the class to a struct or use `[WeaverSafeClass]` with custom serialization.

#### [MIRAGE1002: SyncVar Auto-Property Error](MIRAGE1002.md)
Properties marked with `[SyncVar]` must be declared as non-static automatic properties with both getter and setter. Mirage's post-processing IL Weaver requires this structure to successfully inject dirty-tracking and sync hook calls. Correct this by removing custom backing fields and declaring the property as a standard auto-property.

#### [MIRAGE1003: Direct Mutation of SyncCollection Elements](MIRAGE1003.md)
Modifying the properties of an element inside a `SyncList` or `SyncDictionary` directly (without setting it back) prevents Mirage from triggering change tracking. Because structs are value types, mutating them directly only changes a local copy. To resolve this, retrieve the element, modify it, and assign it back using the collection indexer.

#### [MIRAGE1004: Reassignment of SyncObject Fields](MIRAGE1004.md)
Fields implementing `ISyncObject` (such as `SyncList` or `SyncHashSet`) must be declared as `readonly` and must not be reassigned after construction. Reassigning these fields breaks internal Weaver injection and delta synchronization. To reset the collection, use the collection's `.Clear()` method instead of creating a new instance.

#### [MIRAGE1005: Invalid SyncVar Hook Method](MIRAGE1005.md)
Specifying a hook name in a `[SyncVar]` attribute that cannot be resolved, has mismatched parameter types, or is an unsupported static event triggers a compile-time error. Under automatic overload resolution, ambiguous hook names (matching multiple signatures) will also trigger this error. Correct this by verifying parameter types match the SyncVar's type, making events instance-based, or using explicit `hookType` parameters.

---

### Group 2: NetworkBehaviour & Attribute Placement

#### [MIRAGE1101: Misplaced Network Attribute Error](MIRAGE1101.md)
Mirage-specific attributes like `[SyncVar]`, `[Server]`, `[Client]`, or RPC attributes are only valid within classes inheriting from `NetworkBehaviour`. Placing these on methods, properties, or fields of a regular `MonoBehaviour` or plain C# class will cause a compile-time error. Fix this by ensuring the target class inherits from `NetworkBehaviour`.

#### [MIRAGE1102: Redundant Server/Client Attribute on RPC](MIRAGE1102.md)
Decorating an RPC method with both a routing attribute (`[ServerRpc]` or `[ClientRpc]`) and its corresponding active guard attribute (`[Server]` or `[Client]`) is redundant. For example, `[ServerRpc]` implies the method executes on the server, making `[Server]` redundant. Placing both generates a warning. Resolve this by removing the redundant active guard attribute.

---

### Group 3: Remote Procedure Calls

#### [MIRAGE1201: NetworkMessage/RPC Class Warning](MIRAGE1201.md)
Using class types inside properties decorated with `[NetworkMessage]` or parameters/returns in RPCs triggers this warning. Reference types cause heap allocations during deserialization and do not support polymorphism. To resolve this, convert the class to a struct, use `[WeaverSafeClass]` with custom serialization, or use `[WeaverSafeClass]` to ignore.

#### [MIRAGE1202: RPC Signature Error](MIRAGE1202.md)
RPC methods (decorated with `[ServerRpc]` or `[ClientRpc]`) cannot be generic and must return `void`, `UniTask`, or `UniTask<T>`. Non-generic signatures and approved return wrappers are mandatory for the Weaver to generate remote routing code. Resolve this by removing generic type parameters and correcting the return type.

#### [MIRAGE1203: Pass-by-Reference Modifiers in RPCs](MIRAGE1203.md)
Using `ref` or `out` modifiers on RPC method parameters is prohibited because pass-by-reference semantics cannot span a one-way network serialization boundary. All RPC arguments must be passed by value. To share modified state back to the caller, use an async RPC returning `UniTask<T>` or a synchronized `[SyncVar]` property.

#### [MIRAGE1204: Static RPC Methods](MIRAGE1204.md)
Declaring an RPC method as `static` causes a compile error because Mirage needs an instance context (`NetworkBehaviour`) to determine the target object identity. Remove the `static` modifier to run the RPC within the instance context of a spawned GameObject.

#### [MIRAGE1205: Invalid ClientRpc Target Configurations](MIRAGE1205.md)
This rule validates that `[ClientRpc]` target configurations are logically sound. For example, returning values (`UniTask`) is invalid when targeting `RpcTarget.Observers`, and targeting `RpcTarget.Player` requires the first parameter to be an `INetworkPlayer`. Correct the target configuration or adjust the method parameters to resolve the error.

#### [MIRAGE1206: Invalid RateLimit Attribute Settings](MIRAGE1206.md)
The `[RateLimit]` attribute settings are validated to ensure they configure positive, non-zero values for `Interval`, `Refill`, and `MaxTokens`. Additionally, `MaxTokens` must be greater than or equal to the `Refill` rate to prevent logic loops or server starvation. Update the attribute parameters with valid positive configurations.

#### [MIRAGE1207: Missing RateLimit on ServerRpc](MIRAGE1207.md)
To protect servers against client RPC spamming and potential denial of service (DoS) attacks, all `[ServerRpc]` methods should be protected with a `[RateLimit]` attribute. This warning highlights unprotected methods. Resolve this by applying a `[RateLimit]` attribute specifying appropriate timing and capacity for the method.

---

### Group 4: Serialization

#### [MIRAGE1301: Field Type Serialization Validation](MIRAGE1301.md)
All fields in a `[NetworkMessage]` or parameters in RPCs must be serializable by Mirage. If a type cannot be automatically serialized by the Weaver and lacks registered custom read/write extension methods, compile-time errors occur. Make sure fields use serializable types or implement custom `NetworkWriter` and `NetworkReader` extensions.

#### [MIRAGE1302: Unserialized Private Field Warning](MIRAGE1302.md)
Private fields and properties in a `[NetworkMessage]` are ignored by the Weaver during serialization. This warning alerts developers that these fields will not be synced over the network, helping avoid confusion. Fix this by making the field public or utilizing public properties with backing fields if custom serialization is implemented.

#### [MIRAGE1303: Mismatched Custom Serialization Methods](MIRAGE1303.md)
Custom serialization requires registering both a writer extension method and a reader extension method with matching signatures. If one of them is missing or has signature differences, Mirage cannot pair them up, causing compilation failure. To resolve this, ensure both matching methods are fully defined.

#### [MIRAGE1304: Non-Serializable MonoBehaviour Parameter](MIRAGE1304.md)
RPC parameters and `[NetworkMessage]` fields cannot be plain `MonoBehaviour` types. Because plain `MonoBehaviour` components do not inherit from `NetworkBehaviour` and lack a `NetworkIdentity`, Mirage cannot serialize or locate them across the network. Change the class to inherit from `NetworkBehaviour` or use a serializable reference (like `NetworkIdentity`).

#### [MIRAGE1305: Missing NetworkMessage Attribute](MIRAGE1305.md)
All custom types used to send messages (via `Send<T>()`) or handle messages (via `RegisterHandler<T>()`) must be decorated with the `[NetworkMessage]` attribute. Without it, the Weaver does not generate the necessary message type IDs and serialization code, which leads to runtime errors or unhandled message warnings.

---

### Group 5: Lifecycles & API Safety

#### [MIRAGE1401: Accessing Network State in Awake/Start](MIRAGE1401.md)
Accessing network properties like `IsServer`, `IsClient`, or authority states in Unity's standard `Awake` or `Start` lifecycle methods is unsafe because the network identity is not yet spawned. This leads to incorrect initialization or race conditions. Subscribe to lifecycle events on `Identity` (such as `Identity.OnStartServer`, `Identity.OnStartClient`, `Identity.OnStartLocalPlayer`, or `Identity.OnAuthorityChanged`) during `Awake()` to execute your network initialization code when the network state is fully ready.

#### [MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize](MIRAGE1402.md)
Derived classes overriding custom `OnSerialize` or `OnDeserialize` methods must call their base class implementations if the base class also synchronizes state. Failing to call the base method prevents base `SyncVars` and properties from synchronizing properly. Fix this by calling the base method and combining their return values.

---

### Group 6: Performance & Size Estimation (`MIRAGE1500 – MIRAGE1599`)

#### [MIRAGE1501: Network Message Serialized Size Estimation](MIRAGE1501.md)
Estimates the serialized size of all `[NetworkMessage]` structs/classes and outputs it as an Info diagnostic to help track and optimize bandwidth usage. Dynamic types like strings, arrays, and lists are treated as variable (skipped in size calculation).
