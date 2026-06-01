---
sidebar_position: 4
---
# Generics

Mirage supports generic types for [SyncVar](/docs/guides/sync/sync-var), [Rpcs](/docs/guides/remote-actions/), and fields in [NetworkMessages](/docs/guides/remote-actions/network-messages).

## NetworkBehaviour

By making a [NetworkBehaviour](/docs/guides/game-objects/network-behaviour) generic you can then use generic SyncVar fields or use the generic in an RPC.

{{{ Path:'Snippets/Serialization/GenericsSnippets.cs' Name:'generic-behaviour' }}}

:::warning
Making the RPC itself generic does not work. For example, `MyRpc<T>(T value)` will not work. This is because the receiver will have no idea what generic to invoke the type as.
:::

## Ensure Type has Write and Read functions

For a type to work as a generic, it must have a write and read that Mirage can find. For built-in types, this is done automatically (see [Serialization](/docs/guides/serialization/advanced)).

For custom types Mirage will try to automatically find them and generate functions, however, this does not always work. Adding `[NetworkMessage]` to the type will tell Mirage to generate functions for it.

{{{ Path:'Snippets/Serialization/GenericsSnippets.cs' Name:'custom-type' }}}

Alternatively, you can manually create Write and Read functions for your type

{{{ Path:'Snippets/Serialization/GenericsSnippets.cs' Name:'custom-type-extensions' }}}

## Network Messages and other types

Generic messages are partly supported. Generic instances can be used as messages, For example, using `MyMessage<int>` in the example below.

This also includes using generic types in RPC or inside other types as long they are generic instances.

{{{ Path:'Snippets/Serialization/GenericsSnippets.cs' Name:'generic-message' }}}

:::note
Generic message should not have `[NetworkMessage]` because this cause Mirage to try to make a writer for the generic itself. Only generic instances (eg `MyMessage<int>`) can have serialize functions 
:::

## SyncList, SyncDictionary, SyncSet

SyncList, SyncDictionary, and SyncSet can have generic types as their element type as long as it is a generic instance (eg `MyType<int>` not `MyType<T>`).

{{{ Path:'Snippets/Serialization/GenericsSnippets.cs' Name:'generic-collections' }}}