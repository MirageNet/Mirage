---
sidebar_position: 3
---
# Sync Hash Set

[`SyncHashSet`](/docs/reference/Mirage.Collections/SyncHashSet-1) is a set similar to C\# [HashSet<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1) that synchronizes its contents from the server to the clients.

A SyncHashSet can contain any [supported Mirage type](/docs/guides/serialization/data-types) 

## Usage

Create a class that derives from SyncHashSet for your specific type. This is necessary because Mirage will add methods to that class with the weaver. Then add a SyncHashSet field to your NetworkBehaviour class. For example:

:::caution IMPORTANT
You need to initialize the SyncHashSet immediately after the definition in order for them to work. You can mark them as `readonly` to enforce proper usage.
:::

### Basic example
{{{ Path:'Snippets/Sync/SyncHashSetExamples.cs' Name:'SyncHashSetBasicExample' }}}

# Callbacks
You can detect when a SyncHashSet changes on the client and/or the server. This is especially useful for refreshing your UI, character appearance, etc. 

Subscribe to the Callback event typically during `Start`, `OnClientStart`, or `OnServerStart` for that. 

:::note
Note that by the time you subscribe, the set will already be initialized, so you will not get a call for the initial data, only updates.
:::

{{{ Path:'Snippets/Sync/SyncHashSetExamples.cs' Name:'SyncHashSetCallbackExample' }}}
