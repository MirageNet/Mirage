# MIRAGE1403: Enabled property check on NetworkServer/Client/NetworkIdentity

## The Problem
Accessing or modifying `.enabled` on `NetworkServer`, `NetworkClient`, or `NetworkIdentity` only checks whether the underlying `MonoBehaviour` component is enabled in the Inspector. This does not indicate if the server or client is actively running, or if the network identity has been spawned.

Using `.enabled` leads to incorrect logic because the component can remain enabled even when the networking systems are inactive.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

---

## How to Resolve
Use `.Active` on `NetworkServer` or `NetworkClient` to check their status, and `.IsSpawned` on `NetworkIdentity` to check if it is active on the network.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}
