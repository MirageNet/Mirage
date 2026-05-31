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
| [MIRAGE1101](MIRAGE1101.md) | Misplaced Network Attribute Error | Error | Prevents Mirage network attributes from being declared inside classes that do not inherit from `NetworkBehaviour`. |
| [MIRAGE1201](MIRAGE1201.md) | RPC Signature Error | Error | Disallows generic parameters on RPC methods and enforces valid return types (`void`, `UniTask`, or `UniTask<T>`). |
| [MIRAGE1202](MIRAGE1202.md) | Pass-by-Reference Modifiers in RPCs | Error | Prohibits `ref` or `out` modifiers on RPC parameters since they cannot be serialized over a one-way boundary. |
| [MIRAGE1203](MIRAGE1203.md) | Static RPC Methods | Error | Disallows declaring RPC methods as `static` to preserve the `NetworkBehaviour` instance context. |
| [MIRAGE1204](MIRAGE1204.md) | Invalid ClientRpc Target Configurations | Error | Validates `[ClientRpc]` target settings, ensuring correct return types and connection parameters. |
| [MIRAGE1205](MIRAGE1205.md) | Invalid RateLimit Attribute Settings | Error | Enforces valid positive configurations for interval, refill, and max tokens inside `[RateLimit]` attributes. |
| [MIRAGE1206](MIRAGE1206.md) | Missing RateLimit on ServerRpc | Warning | Recommends decorating `[ServerRpc]` methods with `[RateLimit]` to prevent server denial of service (DoS) attacks. |
| [MIRAGE1301](MIRAGE1301.md) | Message or RPC Class Warning | Warning | Warns about class types used inside network messages or RPC parameters because they cause GC allocations. |
| [MIRAGE1302](MIRAGE1302.md) | Field Type Serialization Validation | Error | Confirms all fields in network messages/RPCs are serializable by Mirage or have registered custom serializers. |
| [MIRAGE1303](MIRAGE1303.md) | Mismatched Custom Serialization Methods | Error | Requires custom serializers to contain matching, properly-signed reader and writer extension methods. |
| [MIRAGE1401](MIRAGE1401.md) | Accessing Network State in Awake/Start | Warning | Warns against accessing network states like `IsServer` or `SyncVars` during early Unity lifecycle phases. |
| [MIRAGE1402](MIRAGE1402.md) | Missing base Call in OnSerialize/OnDeserialize | Warning | Ensures overriding `OnSerialize` or `OnDeserialize` in derived classes calls the base implementation. |
| [MIRAGE1501](MIRAGE1501.md) | Network Message Exceeds Safe MTU | Warning | Warns when a message exceeds the safe Maximum Transmission Unit (MTU) to prevent IP fragmentation. |
| [MIRAGE1502](MIRAGE1502.md) | Unbounded String or Collection | Warning | Warns about unbounded strings/collections in network messages that could trigger memory exploitation. |
| [MIRAGE1503](MIRAGE1503.md) | High Bit-Overhead Primitive Type | Warning | Recommends bit-packing/compression attributes on primitive types to optimize network bandwidth. |

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

---

### Group 2: NetworkBehaviour & Attribute Placement

#### [MIRAGE1101: Misplaced Network Attribute Error](MIRAGE1101.md)
Mirage-specific attributes like `[SyncVar]`, `[Server]`, `[Client]`, or RPC attributes are only valid within classes inheriting from `NetworkBehaviour`. Placing these on methods, properties, or fields of a regular `MonoBehaviour` or plain C# class will cause a compile-time error. Fix this by ensuring the target class inherits from `NetworkBehaviour`.

---

### Group 3: Remote Procedure Calls

#### [MIRAGE1201: RPC Signature Error](MIRAGE1201.md)
RPC methods (decorated with `[ServerRpc]` or `[ClientRpc]`) cannot be generic and must return `void`, `UniTask`, or `UniTask<T>`. Non-generic signatures and approved return wrappers are mandatory for the Weaver to generate remote routing code. Resolve this by removing generic type parameters and correcting the return type.

#### [MIRAGE1202: Pass-by-Reference Modifiers in RPCs](MIRAGE1202.md)
Using `ref` or `out` modifiers on RPC method parameters is prohibited because pass-by-reference semantics cannot span a one-way network serialization boundary. All RPC arguments must be passed by value. To share modified state back to the caller, use an async RPC returning `UniTask<T>` or a synchronized `[SyncVar]` property.

#### [MIRAGE1203: Static RPC Methods](MIRAGE1203.md)
Declaring an RPC method as `static` causes a compile error because Mirage needs an instance context (`NetworkBehaviour`) to determine the target object identity. Remove the `static` modifier to run the RPC within the instance context of a spawned GameObject.

#### [MIRAGE1204: Invalid ClientRpc Target Configurations](MIRAGE1204.md)
This rule validates that `[ClientRpc]` target configurations are logically sound. For example, returning values (`UniTask`) is invalid when targeting `RpcTarget.Observers`, and targeting `RpcTarget.Player` requires the first parameter to be an `INetworkPlayer`. Correct the target configuration or adjust the method parameters to resolve the error.

#### [MIRAGE1205: Invalid RateLimit Attribute Settings](MIRAGE1205.md)
The `[RateLimit]` attribute settings are validated to ensure they configure positive, non-zero values for `Interval`, `Refill`, and `MaxTokens`. Additionally, `MaxTokens` must be greater than or equal to the `Refill` rate to prevent logic loops or server starvation. Update the attribute parameters with valid positive configurations.

#### [MIRAGE1206: Missing RateLimit on ServerRpc](MIRAGE1206.md)
To protect servers against client RPC spamming and potential denial of service (DoS) attacks, all `[ServerRpc]` methods should be protected with a `[RateLimit]` attribute. This warning highlights unprotected methods. Resolve this by applying a `[RateLimit]` attribute specifying appropriate timing and capacity for the method.

---

### Group 4: Serialization

#### [MIRAGE1301: Message or RPC Class Warning](MIRAGE1301.md)
Using class types as fields in a `[NetworkMessage]` or as arguments/return types in RPCs triggers a warning due to garbage collection allocations during deserialization. Standard collections like `List<T>` are ignored, but custom classes should be converted to structs. Alternatively, you can use `[WeaverSafeClass]` with custom read/write extension methods to suppress the warning.

#### [MIRAGE1302: Field Type Serialization Validation](MIRAGE1302.md)
All fields in a `[NetworkMessage]` or parameters in RPCs must be serializable by Mirage. If a type cannot be automatically serialized by the Weaver and lacks registered custom read/write extension methods, compile-time errors occur. Make sure fields use serializable types or implement custom `NetworkWriter` and `NetworkReader` extensions.

#### [MIRAGE1303: Mismatched Custom Serialization Methods](MIRAGE1303.md)
Custom serialization requires registering both a writer extension method and a reader extension method with matching signatures. If one of them is missing or has signature differences, Mirage cannot pair them up, causing compilation failure. To resolve this, ensure both matching methods are fully defined.

---

### Group 5: Lifecycles & API Safety

#### [MIRAGE1401: Accessing Network State in Awake/Start](MIRAGE1401.md)
Accessing network properties like `IsServer`, `IsClient`, or authority states in Unity's standard `Awake` or `Start` lifecycle methods is unsafe because the network identity is not yet spawned. This leads to incorrect initialization or race conditions. Override `OnStartServer`, `OnStartClient`, or other network-specific start callbacks instead.

#### [MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize](MIRAGE1402.md)
Derived classes overriding custom `OnSerialize` or `OnDeserialize` methods must call their base class implementations if the base class also synchronizes state. Failing to call the base method prevents base `SyncVars` and properties from synchronizing properly. Fix this by calling the base method and combining their return values.

---

### Group 6: Performance & Size Estimation

#### [MIRAGE1501: Network Message Exceeds Safe MTU](MIRAGE1501.md)
If a network message's maximum serialized size exceeds the safe Maximum Transmission Unit (MTU) threshold (typically 1200 - 1400 bytes), this warning is triggered. Large packets require IP fragmentation, which dramatically increases network packet loss. To resolve, compress data fields or split large payloads across multiple chunk messages.

#### [MIRAGE1502: Unbounded String or Collection](MIRAGE1502.md)
Declaring string or collection fields in network messages without a size limit introduces security risks, allowing malicious clients to cause memory exhaustion on the server. To resolve, use validation attributes like `[BitCount]` to limit serialization size, or define max sizes on collections.

#### [MIRAGE1503: High Bit-Overhead Primitive Type](MIRAGE1503.md)
Using uncompressed primitive types (such as standard `int`, `long`, or `float`) inside `[SyncVar]` properties or network message fields consumes unnecessary bandwidth. This warning suggests using compression attributes to minimize serialized bit sizes. To resolve this, apply attributes such as `[BitCount]`, `[VarInt]`, or `[FloatPack]`.
