---
sidebar_position: 4
---
# Attributes

Networking attributes are added to members of [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) scripts to tell Mirage to do different things.

There are 4 types of attributes that Mirage has:
- **[RPC Attributes](#rpc-attributes)**: Cause a method to send a network message so that the body of the method is invoked on either the server or client.
- **[Block methods invokes](#block-methods-invokes)**: Attributes used to restrict method invocation to specific contexts.
- **[SyncVar](/docs/guides/sync/sync-var)**: Add to Fields to cause their value to be automatically synced to clients.
- **[Bit Packing](/docs/guides/bit-packing)**: These attributes modify how values are written, providing an easy way to compress values before they are sent over the network. They can be applied to Fields and method Parameters.