# MIRAGE1403: Enabled property check on NetworkServer/Client

## The Problem

Checking or setting the `.enabled` property on a `NetworkServer` or `NetworkClient` reference:

*   `NetworkServer.enabled`
*   `NetworkClient.enabled`

`NetworkServer` and `NetworkClient` both inherit from `MonoBehaviour`, which inherits the `.enabled` property from Unity's `Behaviour` class. Checking `.enabled` only checks if the MonoBehaviour component is enabled or disabled in the Unity Inspector. It does **not** reflect whether the server or client is actively running or connected.

Accessing `.enabled` to check server or client status will lead to incorrect state logic (e.g. returning `true` even if the server is stopped but the component remains enabled).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

---

## How to Resolve

Use the `.Active` property on `NetworkServer` or `NetworkClient` to check whether the server is listening for connections or if the client is connected to a server.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}
