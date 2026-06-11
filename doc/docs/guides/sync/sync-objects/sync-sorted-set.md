---
sidebar_position: 6
---
# Sync Sorted Set

[`SyncSortedSet`](/docs/reference/Mirage.Collections/SyncSortedSet-1) is a set similar to C\# [SortedSet<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedset-1) that synchronizes its contents from the server to the clients.

Unlike SyncHashSets, all elements in a SyncSortedSet are sorted when they are inserted. Please note this has some performance implications.

A SyncSortedSet can contain any [supported Mirage type](/docs/guides/serialization/data-types) 

## Usage

Create a class that derives from SyncSortedSet for your specific type. This is necessary because Mirage will add methods to that class with the weaver. Then add a SyncSortedSet field to your NetworkBehaviour class. For example:

:::caution IMPORTANT
You need to initialize the SyncSortedSet immediately after the definition for them to work. You can mark them as `readonly` to enforce proper usage.
:::

{{{ Path:'Snippets/Sync/SyncSortedSetExamples.cs' Name:'SyncSortedSetBasicExample' }}}

You can also detect when a SyncSortedSet changes. This is useful for refreshing your character in the client or determining when you need to update your database. Subscribe to the Callback event typically during `Start`, `OnClientStart`, or `OnServerStart` for that. 

:::note
That by the time you subscribe, the set will already be initialized, so you will not get a call for the initial data, only updates.
:::

{{{ Path:'Snippets/Sync/SyncSortedSetExamples.cs' Name:'SyncSortedSetCallbackExample' }}}
