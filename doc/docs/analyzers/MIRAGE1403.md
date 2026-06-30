# MIRAGE1403: Enabled property check on NetworkServer/Client/NetworkIdentity

## The Problem

Checking or setting the `.enabled` property on a `NetworkServer`, `NetworkClient`, or `NetworkIdentity` reference:

*   `NetworkServer.enabled`
*   `NetworkClient.enabled`
*   `NetworkIdentity.enabled`

`NetworkServer`, `NetworkClient`, and `NetworkIdentity` all inherit from `MonoBehaviour` (directly or indirectly), which inherits the `.enabled` property from Unity's `Behaviour` class. Checking `.enabled` only checks if the MonoBehaviour component is enabled or disabled in the Unity Inspector. It does **not** reflect whether the server or client is actively running, or if the network identity is active and spawned on the network.

Accessing `.enabled` to check server, client, or identity status will lead to incorrect state logic (e.g. returning `true` even if the server is stopped or if the identity has not been spawned, but the component remains enabled).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

---

## How to Resolve

Use the `.Active` property on `NetworkServer` or `NetworkClient` to check whether the server is listening for connections or if the client is connected to a server. For `NetworkIdentity`, use the `.IsSpawned` property to check if the network object is currently active and spawned.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}
