# Mirage Roslyn Analyzers

Mirage analyzers highlight networking mistakes in your editor. Use each rule page to understand the diagnostic, its exceptions, and how to fix your code.

These pages are the authoritative rule reference. The table lists default severities; rule availability depends on your analyzer version. [Message size estimation](MIRAGE1501.md) is proposed.

Rules apply to Mirage's types, attributes, and members, including supported derived types. Some rules impose additional checks beyond the Weaver; their pages explain these requirements.

## Rules

| Rule | Name | Severity |
| --- | --- | --- |
| [MIRAGE1001](MIRAGE1001.md) | SyncVar Class Warning | Warning |
| [MIRAGE1002](MIRAGE1002.md) | Direct Mutation of SyncCollection Elements | Warning |
| [MIRAGE1003](MIRAGE1003.md) | SyncObject fields must be marked as readonly | Error |
| [MIRAGE1004](MIRAGE1004.md) | Invalid SyncVar Hook Method | Error |
| [MIRAGE1005](MIRAGE1005.md) | Readonly SyncVar Field | Error |
| [MIRAGE1101](MIRAGE1101.md) | Misplaced Network Attribute | Error |
| [MIRAGE1102](MIRAGE1102.md) | Redundant Attribute on RPC | Warning |
| [MIRAGE1201](MIRAGE1201.md) | NetworkMessage/RPC Class Warning | Warning |
| [MIRAGE1202](MIRAGE1202.md) | RPC Signature Error | Error |
| [MIRAGE1203](MIRAGE1203.md) | Pass-by-Reference Modifiers in RPCs | Error |
| [MIRAGE1204](MIRAGE1204.md) | Static RPC Methods | Error |
| [MIRAGE1205](MIRAGE1205.md) | Invalid ClientRpc Target Configurations | Error |
| [MIRAGE1206](MIRAGE1206.md) | Invalid RateLimit Attribute Settings | Error |
| [MIRAGE1207](MIRAGE1207.md) | Missing RateLimit on ServerRpc | Warning |
| [MIRAGE1301](MIRAGE1301.md) | Field Type Serialization Validation | Error |
| [MIRAGE1302](MIRAGE1302.md) | Unserialized Member Warning | Warning |
| [MIRAGE1303](MIRAGE1303.md) | Mismatched Custom Serialization Methods | Error |
| [MIRAGE1304](MIRAGE1304.md) | Non-Serializable MonoBehaviour Parameter | Error |
| [MIRAGE1305](MIRAGE1305.md) | Missing NetworkMessage Attribute | Warning |
| [MIRAGE1401](MIRAGE1401.md) | Accessing Network State in Awake/Start | Warning |
| [MIRAGE1402](MIRAGE1402.md) | Missing base Call in OnSerialize/OnDeserialize | Warning |
| [MIRAGE1403](MIRAGE1403.md) | Enabled property check on NetworkServer/Client/NetworkIdentity | Warning |
| [MIRAGE1501](MIRAGE1501.md) | Network Message Serialized Size Estimation | Info |
