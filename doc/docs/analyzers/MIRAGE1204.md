# MIRAGE1204: Static RPC Methods

## When this appears

A `[ServerRpc]` or `[ClientRpc]` method is `static`.

RPCs need a specific `NetworkBehaviour` instance so the message can be routed to its network identity. A static method has no instance to identify the receiving object.

{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-triggering' }}}

## How to fix

Remove `static` from the RPC method:

{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-resolved' }}}
