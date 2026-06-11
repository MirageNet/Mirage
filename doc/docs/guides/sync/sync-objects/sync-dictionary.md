---
sidebar_position: 2
---
# Sync Dictionary
[`SyncDictionary`](/docs/reference/Mirage.Collections/SyncDictionary-2) is an associative array containing an unordered list of key, value pairs. Keys and values can be any of [Mirage supported types](/docs/guides/serialization/data-types).

SyncDictionary works much like [SyncLists](/docs/guides/sync/sync-objects/sync-list): when you make a change on the server, the change is propagated to all clients and the appropriate callback is called.

## Usage
Add a field of type [SyncDictionary](/docs/reference/Mirage.Collections/SyncDictionary-2) on any [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) where `TKey` and `TValue` can be any supported Mirage type and initialize it.

:::caution IMPORTANT
You need to initialize the SyncDictionary immediately after the definition for them to work. You can mark them as `readonly` to enforce proper usage.
:::

### Basic example
{{{ Path:'Snippets/Sync/SyncDictionaryExamples.cs' Name:'SyncDictionaryBasicExample' }}}

## Callbacks
You can detect when a SyncDictionary changes on the client and/or server. This is especially useful for refreshing your UI, character appearance, etc.

There are different callbacks for different operations, such as `OnChange` (any change to the dictionary), `OnInsert` (adding a new element), etc. Please check the [SyncDictionary API reference](/docs/reference/Mirage.Collections/SyncDictionary-2) for the complete list of callbacks.

Depending on where you want to invoke the callbacks, you can use these methods to register them:
- `Awake` for both client and server
- `Identity.OnStartServer` event for server-only
- `Identity.OnStartClient` event for client-only

:::note
By the time you subscribe, the dictionary will already be initialized, so you will not get a call for the initial data, only updates.
:::

### Example
{{{ Path:'Snippets/Sync/SyncDictionaryExamples.cs' Name:'SyncDictionaryCallbackExample' }}}

By default, `SyncDictionary` uses a [`Dictionary`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=netstandard-2.0) to store its data. If you want to use a different dictionary implementation, add a constructor and pass the dictionary implementation to the parent constructor. For example:

{{{ Path:'Snippets/Sync/SyncDictionaryExamples.cs' Name:'SyncDictionaryCustomImplementation' }}}