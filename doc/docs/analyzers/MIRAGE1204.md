# MIRAGE1204: Static RPC Methods

## When this appears

A method decorated with `[ServerRpc]` or `[ClientRpc]` is `static`. RPCs need a specific `NetworkBehaviour` instance so Mirage can route the message to its network identity.

{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-triggering' }}}

## How to fix

Remove `static` from the RPC method:

{{{ Path:'Snippets/Analyzers/Mirage1204.cs' Name:'mirage1204-resolved' }}}
