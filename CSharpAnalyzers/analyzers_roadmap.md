# Mirage Roslyn Analyzers Roadmap & Checklist (Updated)

This document tracks the implementation status of the proposed Roslyn Analyzers for Mirage, updated with user feedback to avoid gaps and adjust severities/rules.

---

## Group 1: SyncVars & SyncObjects (`MIRAGE1000 – MIRAGE1099`)

* [ ] **`MIRAGE1001`**: Warning: SyncVar Class Warning (Warns on class-based SyncVars / SyncObject type arguments, including types like List<T>, T[], Dictionary<T>).
* [ ] **`MIRAGE1002`**: Error: SyncVar Auto-Property Error (Requires SyncVar properties to be non-static auto-properties with both get/set).
* [ ] **`MIRAGE1003`**: Warning: Direct Mutation of SyncCollection Elements (Warning when modifying fields of class elements inside a `SyncList`/`SyncDictionary` without calling `SetItemDirty`). (ignore sync-by-reference like NetworkIdentity/Behaviour)
* [ ] **`MIRAGE1004`**: Error: Reassignment of SyncObject Fields, error when not assigned and not readonly (Error when `ISyncObject` fields like `SyncList` are reassigned or not marked `readonly`).
* [ ] **`MIRAGE1005`**: Error: Invalid SyncVar Hook Method (Error when a SyncVar hook method does not exist in the class, is static, or does not match the signature `void Hook(T oldValue, T newValue)`). **IMPORTANT TODO: check real syncvar hook rules from weaver code, they can be methods or events**

---

## Group 2: Network Behaviour & Attribute Placement (`MIRAGE1100 – MIRAGE1199`)

* [ ] **`MIRAGE1101`**: Error: Misplaced Network Attribute Error (Verifies network attributes are only on classes inheriting from `NetworkBehaviour`).
* [ ] **`MIRAGE1102`**: Warning: Redundant Server/Client Attribute on RPC (Warning if a method has both `[ServerRpc]` and `[Server]` or `[ClientRpc]` and `[Client]`).

---

## Group 3: Remote Procedure Calls (`MIRAGE1200 – MIRAGE1299`)

* [ ] **`MIRAGE1201`**: Warning: NetworkMessage/RPC Class Warning (Warns on class-based fields/parameters, ignoring types like List<T>, T[], Dictionary<T>).
* [ ] **`MIRAGE1202`**: Error: RPC Signature Error, generic allowed, void allowed, `UniTask<T>` (with return value) allowed, `UniTask` (not return value) not allowed.
* [ ] **`MIRAGE1203`**: Error: Pass-by-Reference Modifiers in RPCs (Error if parameters use `ref`, `in`, or `out`).
* [ ] **`MIRAGE1204`**: Error: Static RPC Methods (Error if an RPC method is marked `static`).
* [ ] **`MIRAGE1205`**: Error: Invalid ClientRpc Target Configurations, if using RpcTarget.Player, first Argument should be INetworkPlayer.
* [ ] **`MIRAGE1206`**: Error: Invalid RateLimit Attribute Settings (Error on zero/negative values inside `[RateLimit]`).
* [ ] **`MIRAGE1207`**: Warning: Missing RateLimit on ServerRpc (Warning if a `[ServerRpc]` method is missing the `[RateLimit]` attribute, as all client->server messages should be limited).

---

## Group 4: Serialization (`MIRAGE1300 – MIRAGE1399`)

* [ ] **`MIRAGE1301`**: Error: Field Type Serialization Validation (Error if type inside `[NetworkMessage]` or RPC is not serializable and has no custom serializer).
* [ ] **`MIRAGE1302`**: (SKIP: this will be added later when we have source gen instead of Weaver dll edit) Warning: Field Type Serialization Validation, private fields or properties that will not be serialized (eg, structs sent over the network should be clean and minimal to avoid developer confusion).
* [ ] **`MIRAGE1303`**: Error: Mismatched Custom Serialization Methods (Error if custom writer `Write` exists without matching reader `Read` or vice-versa - implemented as Error to prevent Weaver compile errors).
* [ ] **`MIRAGE1304`**: Error: Non-Serializable MonoBehaviour Parameter (Error if RPC parameter or NetworkMessage field is a `MonoBehaviour` type that does not inherit from `NetworkBehaviour`).
* [ ] **`MIRAGE1305`**: Warning: Missing `[NetworkMessage]` Attribute (Warning if a type is sent or registered as a message, but is missing the `[NetworkMessage]` attribute).

---

## Group 5: General API & Lifecycles (`MIRAGE1400 – MIRAGE1499`)

* [ ] **`MIRAGE1401`**: Accessing Network State in Awake/Start (Warning if checking `IsServer`/`IsClient`, accessing network behavior/identity properties, or calling RPCs inside `Awake()` or `Start()`).
* [ ] **`MIRAGE1402`**: Missing base Call in OnSerialize/OnDeserialize (Warning if overrides do not call base implementations in components containing SyncVars or SyncObjects).

---

## Group 6: Network Performance & Size Estimation (`MIRAGE1500 – MIRAGE1599`)
*Note: These rules are designated as `DiagnosticSeverity.Warning` to warn about security/performance issues, and output a parser-friendly format (e.g. JSON-like summary of size calculations) to facilitate future CodeLens or editor tooling integration.*

* [ ] **`MIRAGE1501`**: Network Message Exceeds Safe MTU (Warning if estimated serialization size exceeds 1200 bytes).
* [ ] **`MIRAGE1502`**: Unbounded String or Collection (Warning if string/collection has no defined size bounds).
* [ ] **`MIRAGE1503`**: High Bit-Over-Head Primitive Type (Warning on uncompressed float/vector transfers).

---

## Extra notes
### Classes for network syncing
note on MIRAGE1001, MIRAGE1201. Warnings for class being sent over network, if the target object is a struct, we should check its field and apply these same rules. we should list the warning on the Syncvar/rpc/networkMessage if its field or subfields break the rule.
