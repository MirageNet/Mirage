# MIRAGE1204: Static RPC Methods

## The Problem
An RPC method decorated with `[ServerRpc]` or `[ClientRpc]` is `static`.

RPC methods must execute on a specific `NetworkBehaviour` instance so Mirage can route the message to the correct network identity. Static methods lack this instance context (`this`).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-triggering' }}}

---

## How to Resolve
Remove the `static` modifier so the method executes within the instance context of the `NetworkBehaviour`.

{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-resolved' }}}
