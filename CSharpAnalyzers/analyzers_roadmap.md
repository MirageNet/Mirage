# Mirage Roslyn Analyzers Roadmap & Checklist (Updated)

This document tracks the implementation status of the proposed Roslyn Analyzers for Mirage, updated with user feedback to avoid gaps and adjust severities/rules.

note on format: 3 check boxes [ ][ ][ ] 
- researched and added correct details to roadmap
- docs/sample page
- added unit tests for CSharpAnalyzers

IMPORTANT: if boxes arn't checked here, assume existing files are out-of-date and need reviewing as if they do not exist
---

## Group 1: SyncVars & SyncObjects (`MIRAGE1000 – MIRAGE1099`)

* [x][x][x] **`MIRAGE1001`**: Warning: SyncVar Class Warning (Warns on class-based SyncVars / SyncObject type arguments, including types like List<T>, T[], Dictionary<T>).
* [x][x][x] **`MIRAGE1002`**: Error: SyncVar Auto-Property Error (Requires SyncVar properties to be non-static auto-properties with both get/set).
* [x][x][x] **`MIRAGE1003`**: Warning: Direct Mutation of SyncCollection Elements (Warning when modifying fields of class elements inside a `SyncList`/`SyncDictionary` without calling `SetItemDirty`). (ignore sync-by-reference like NetworkIdentity/Behaviour)
* [x][x][x] **`MIRAGE1004`**: Error: Reassignment of SyncObject Fields, error when not assigned and not readonly (Error when `ISyncObject` fields like `SyncList` are reassigned or not marked `readonly`).
* [x][x][ ] **`MIRAGE1005`**: Error: Invalid SyncVar Hook Method (Error when a SyncVar hook method does not exist in the class, or does not match the signature `void Hook()`, `void Hook(T newValue)`, or `void Hook(T oldValue, T newValue)`. For events, it can be an instance event of type `Action`, `Action<T>`, or `Action<T, T>`, but it must not be static). Methods can be static, Events can't be static.

---

## Group 2: Network Behaviour & Attribute Placement (`MIRAGE1100 – MIRAGE1199`)

* [x][x][x] **`MIRAGE1101`**: Error: Misplaced Network Attribute Error (Verifies network attributes are only on classes inheriting from `NetworkBehaviour`).
* [x][x][ ] **`MIRAGE1102`**: Warning: Redundant Server/Client Attribute on RPC (Warning if a method has both `[ServerRpc]` and `[Server]` or `[ClientRpc]` and `[Client]`).

---

## Group 3: Remote Procedure Calls (`MIRAGE1200 – MIRAGE1299`)

* [x][x][x] **`MIRAGE1201`**: Warning: NetworkMessage/RPC Class Warning (Warns on class-based fields/parameters, ignoring types like List<T>, T[], Dictionary<T>).
* [x][x][x] **`MIRAGE1202`**: Error: RPC Signature Error, generic allowed, void allowed, `UniTask<T>` (with return value) allowed, `UniTask` (not return value) not allowed.
* [x][x][x] **`MIRAGE1203`**: Error: Pass-by-Reference Modifiers in RPCs (Error if parameters use `ref`, `in`, or `out`).
* [x][x][x] **`MIRAGE1204`**: Error: Static RPC Methods (Error if an RPC method is marked `static`).
* [x][x][x] **`MIRAGE1205`**: Error: Invalid ClientRpc Target Configurations, if using RpcTarget.Player, first Argument should be INetworkPlayer.
* [x][x][x] **`MIRAGE1206`**: Error: Invalid RateLimit Attribute Settings (Error on zero/negative values inside `[RateLimit]`).
* [x][x][x] **`MIRAGE1207`**: Warning: Missing RateLimit on ServerRpc (Warning if a `[ServerRpc]` method is missing the `[RateLimit]` attribute, as all client->server messages should be limited).

---

## Group 4: Serialization (`MIRAGE1300 – MIRAGE1399`)

* [x][x][x] **`MIRAGE1301`**: Error: Field Type Serialization Validation (Error if type inside `[NetworkMessage]` or RPC is not serializable and has no custom serializer).
* [x][x][ ] **`MIRAGE1302`**: (SKIP: this will be added later when we have source gen instead of Weaver dll edit) Warning: Field Type Serialization Validation, private fields or properties that will not be serialized (eg, structs sent over the network should be clean and minimal to avoid developer confusion).
* [x][x][x] **`MIRAGE1303`**: Error: Mismatched Custom Serialization Methods (Error if custom writer `Write` exists without matching reader `Read` or vice-versa - implemented as Error to prevent Weaver compile errors).
* [x][x][ ] **`MIRAGE1304`**: Error: Non-Serializable MonoBehaviour Parameter (Error if RPC parameter or NetworkMessage field is a `MonoBehaviour` type that does not inherit from `NetworkBehaviour`).
* [x][x][ ] **`MIRAGE1305`**: Warning: Missing `[NetworkMessage]` Attribute (Warning if a type is sent or registered as a message, but is missing the `[NetworkMessage]` attribute).

---

## Group 5: General API & Lifecycles (`MIRAGE1400 – MIRAGE1499`)

* [x][x][x] **`MIRAGE1401`**: Accessing Network State in Awake/Start (Warning if checking `IsServer`/`IsClient`, accessing network behavior/identity properties, or calling RPCs inside `Awake()` or `Start()`).
* [x][x][x] **`MIRAGE1402`**: Missing base Call in OnSerialize/OnDeserialize (Warning if overrides do not call base implementations in components containing SyncVars or SyncObjects).

---

## Group 6: Network Performance & Size Estimation (`MIRAGE1500 – MIRAGE1599`)
*Note: These rules are designated as `DiagnosticSeverity.Warning` to warn about security/performance issues, and output a parser-friendly format (e.g. JSON-like summary of size calculations) to facilitate future CodeLens or editor tooling integration.*

* [x][x][x] **`MIRAGE1501`**: Network Message Exceeds Safe MTU (Warning if estimated serialization size exceeds 1200 bytes).
* [x][x][x] **`MIRAGE1502`**: Unbounded String or Collection (Warning if string/collection has no defined size bounds).
* [x][x][x] **`MIRAGE1503`**: High Bit-Over-Head Primitive Type (Warning on uncompressed float/vector transfers).

---

## Extra notes
### Classes for network syncing
note on MIRAGE1001, MIRAGE1201. Warnings for class being sent over network, if the target object is a struct, we should check its field and apply these same rules. we should list the warning on the Syncvar/rpc/networkMessage if its field or subfields break the rule.
