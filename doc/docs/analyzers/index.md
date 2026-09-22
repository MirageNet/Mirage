# Mirage Roslyn Analyzers

These pages define the intended contract for Mirage's Roslyn analyzers. The analyzer implementation and its tests may lag behind this contract; a documented diagnostic does not establish that a released analyzer already implements it.

## Source of truth

Each rule page is authoritative for its trigger, scope, exceptions, and remedies. The table below defines default severity and provides navigation; it does not replace the detailed rule. Update a rule page and its referenced example snippets together before changing analyzer behavior or tests.

Distinguish restrictions enforced by Mirage's runtime or Weaver from additional analyzer policy. A rule can deliberately reject or warn about a pattern Mirage accepts, but its page must say so. An Error severity below describes the intended analyzer diagnostic, not necessarily an existing Weaver error. MIRAGE1501 is a proposed informational contract, with explicit limits on what static analysis can determine.

Rules must identify Mirage types, attributes, members, and overridden methods by their resolved symbols, including supported derived types. Unrelated user APIs with the same simple names are outside their scope. Follow each rule's exclusions for built-in serializers, custom serializers, ignored fields, and RPC plumbing.

## Rule summary

| Rule ID | Name | Severity | Short Summary |
| --- | --- | --- | --- |
| [MIRAGE1001](MIRAGE1001.md) | SyncVar Class Warning | Warning | Warns about class payload allocations and SyncVar change tracking. |
| [MIRAGE1002](MIRAGE1002.md) | Direct Mutation of SyncCollection Elements | Warning | Warns about nested mutations that bypass collection change tracking. |
| [MIRAGE1003](MIRAGE1003.md) | SyncObject fields must be marked as readonly | Error | Requires readonly instance SyncObject fields on NetworkBehaviours as analyzer policy. |
| [MIRAGE1004](MIRAGE1004.md) | Invalid SyncVar Hook Method | Error | Validates hook lookup, signatures, and automatic selection. |
| [MIRAGE1005](MIRAGE1005.md) | Readonly SyncVar Field | Error | Rejects readonly SyncVar fields as analyzer policy. |
| [MIRAGE1101](MIRAGE1101.md) | Misplaced Network Attribute | Error | Checks placement of the listed NetworkBehaviour-specific attributes. |
| [MIRAGE1102](MIRAGE1102.md) | Redundant Attribute on RPC | Warning | Warns about a matching destination guard on an RPC. |
| [MIRAGE1201](MIRAGE1201.md) | NetworkMessage/RPC Class Warning | Warning | Warns about class payload allocations and declared-type serialization. |
| [MIRAGE1202](MIRAGE1202.md) | RPC Signature Error | Error | Validates RPC method shape and `void` / `UniTask<T>` returns. |
| [MIRAGE1203](MIRAGE1203.md) | Pass-by-Reference Modifiers in RPCs | Error | Rejects `ref`, `out`, and `in` RPC parameters. |
| [MIRAGE1204](MIRAGE1204.md) | Static RPC Methods | Error | Disallows declaring RPC methods as `static` to preserve the `NetworkBehaviour` instance context. |
| [MIRAGE1205](MIRAGE1205.md) | Invalid ClientRpc Target Configurations | Error | Validates `[ClientRpc]` target settings, ensuring correct return types and connection parameters. |
| [MIRAGE1206](MIRAGE1206.md) | Invalid RateLimit Attribute Settings | Error | Validates interval, refill, capacity, and penalty settings. |
| [MIRAGE1207](MIRAGE1207.md) | Missing RateLimit on ServerRpc | Warning | Recommends a server RPC rate limit as one layer of validation. |
| [MIRAGE1301](MIRAGE1301.md) | Field Type Serialization Validation | Error | Checks serialized payload types against available writer and reader paths. |
| [MIRAGE1302](MIRAGE1302.md) | Unserialized Member Warning | Warning | Warns about message members omitted by automatic serialization. |
| [MIRAGE1303](MIRAGE1303.md) | Mismatched Custom Serialization Methods | Error | Requires matching custom read/write registrations as analyzer policy. |
| [MIRAGE1304](MIRAGE1304.md) | Non-Serializable MonoBehaviour Parameter | Error | Rejects unsupported component payloads without a serializer. |
| [MIRAGE1305](MIRAGE1305.md) | Missing NetworkMessage Attribute | Warning | Recommends explicit message marking at recognized message API uses. |
| [MIRAGE1401](MIRAGE1401.md) | Accessing Network State in Awake/Start | Warning | Warns about network initialization tied to Unity Awake/Start ordering. |
| [MIRAGE1402](MIRAGE1402.md) | Missing base Call in OnSerialize/OnDeserialize | Warning | Warns when custom serialization omits the base contract. |
| [MIRAGE1403](MIRAGE1403.md) | Enabled property check on NetworkServer/Client/NetworkIdentity | Warning | Distinguishes Unity component enablement from network state. |
| [MIRAGE1501](MIRAGE1501.md) | Network Message Serialized Size Estimation | Info | Proposes exact, bounded, variable, or unknown payload-size reporting. |


## Maintaining examples

Examples are stored in `Assets/Mirage/Samples~/Snippets/Analyzers/` and embedded into the corresponding rule page by [EmbedCodeInMarkdown](https://github.com/James-Frowen/EmbedCodeInMarkdown). Review the expanded output so that prose and code are checked together. Keep one editable copy of each example in its snippet source; do not commit expanded copies beside the source pages.

From the repository root, run the embedding tool to a separate output directory:

```sh
EmbedCodeInMarkdown -docs="doc/docs/analyzers/" -code="Assets/Mirage/Samples~/" -out="analyzer-docs-review/"
```

The tool emits pages that contain embed tags; pages such as this index need to be copied separately when assembling a full preview. Triggering examples intentionally demonstrate invalid code. Resolved examples must satisfy the relevant contract, including matching serialization writes and reads. Analyzer tests should eventually cover both triggering and accepted cases from these pages.
