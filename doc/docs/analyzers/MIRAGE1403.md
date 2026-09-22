# MIRAGE1403: Enabled property check on NetworkServer/Client/NetworkIdentity

## The Problem
Reading or writing Unity's inherited `Behaviour.enabled` property on `NetworkServer`, `NetworkClient`, or `NetworkIdentity` concerns Unity component enablement. It does not report or control the connection or spawn state.

This warning flags a likely confusion between component and network state. Intentional component enablement is legal and may be suppressed. A different property named `enabled` is outside the rule.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

---

## How to Resolve
Use `.Active` on `NetworkServer` or `NetworkClient` to check their status, and `.IsSpawned` on `NetworkIdentity` to check whether it has a network ID. `NetworkClient.Active` includes both connecting and connected states; use `.IsConnected` when a completed connection is required.

These state properties are read-only. To change network state, use the server/client lifecycle or object manager spawn/despawn APIs; do not mechanically replace an assignment to `.enabled` with an assignment to `.Active` or `.IsSpawned`.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}
