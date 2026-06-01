---
sidebar_position: 2
---

# Data Types

The client and server can pass data to each other via [RPC Methods](/docs/guides/remote-actions/), [State Synchronization](/docs/guides/sync/), or [Network Messages](/docs/guides/remote-actions/network-messages).

Mirage supports a number of data types you can use with these, including:
- Basic c# types (byte, int, char, uint, UInt64, float, string, etc)
- Built-in Unity math type (Vector3, Quaternion, Rect, Plane, Vector3Int, etc)
- NetworkIdentity
- Game Object with a NetworkIdentity component attached 
    - See important details in [Game Objects](#game-objects) section below.
- Structures with any of the above  
    - It's recommended to implement [`IEquatable<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1) to avoid boxing and to have the struct `readonly` because modifying one of the fields doesn't cause a resync.
- Classes as long as each field has a supported data type 
    - These will allocate garbage and will be instantiated new on the receiver every time they're sent.
- ScriptableObject as long as each field has a supported data type 
    - These will allocate garbage and will be instantiated new on the receiver every time they're sent.
- Arrays of any of the above 
    - Not supported with SyncVars or SyncLists.
- ArraySegments of any of the above 
    - Not supported with SyncVars or SyncLists.

## Game Objects

Game Objects in SyncVars, SyncLists, and SyncDictionaries are fragile in some cases and should be used with caution.

- As long as the game object *already exists* on both the server and the client, the reference should be fine.

When the sync data arrives at the client, the referenced game object may not yet exist on that client, resulting in null values in the sync data. This is because internally Mirage passes the `NetId` from the `NetworkIdentity` and tries to look it up on the client's `NetworkIdentity.World.Spawned` dictionary.

If the object hasn't been spawned on the client yet, no match will be found. It could be in the same payload, especially for joining clients, but after the sync data from another object.  
It could also be null because the game object is excluded from a client due to network visibility, e.g. `NetworkProximityChecker`.  

You may find that it's more robust to sync the `NetworkIdentity.NetID` (`uint`) instead, and do your own lookup in 
`NetworkIdentity.World.Spawned` to get the object, perhaps in a coroutine:

{{{ Path:'Snippets/Serialization/DataTypesSnippets.cs' Name:'game-object-lookup' }}}

## Custom Data Types

Sometimes you don't want Mirage to generate serialization for your own types. For example, instead of serializing quest data, you may want to serialize just the quest id, and the receiver can look up the quest by id in a predefined list.

Sometimes you may want to serialize data that uses a different type not supported by Mirage, such as `DateTime` or `System.Uri`.

You can add support for any type by adding extension methods to `NetworkWriter` and `NetworkReader`. For example, to add support for `DateTime`, add this somewhere in your project:

{{{ Path:'Snippets/Serialization/DataTypesSnippets.cs' Name:'datetime-serializer' }}}

...then you can use `DateTime` in your `[ServerRpc]` or `SyncList`

## Inheritance and Polymorphism

Sometimes you might want to send a polymorphic data type to your commands. Mirage does not serialize the type name to keep messages small and for security reasons, therefore Mirage cannot figure out the type of object it received by looking at the message.

:::caution
This code does not work out of the box.
:::

{{{ Path:'Snippets/Serialization/DataTypesSnippets.cs' Name:'polymorphic-equip' }}}

`ServerRpcEquip` will work if you provide a custom serializer for the `Item` type. For example:

{{{ Path:'Snippets/Serialization/DataTypesSnippets.cs' Name:'polymorphic-serializer' }}}

## Scriptable Objects

People often want to send scriptable objects from the client or server. For example, you may have a bunch of swords created as scriptable objects and you want to put the equipped sword in a [SyncVar](/docs/guides/sync/sync-var). This will work fine, Mirage will generate a reader and writer for scriptable objects by calling `ScriptableObject.CreateInstance` and copy all the data. 

However, the generated reader and writer are not suitable for every occasion. Scriptable objects often reference other assets such as textures, prefabs, or other types that can't be serialized. Scriptable objects are often saved in the Resources folder or they can sometimes have a large amount of data in them. The generated reader and writers may not work or may be inefficient for these situations.

Instead of passing the scriptable object data, you can pass the name and the other side can look up the same object by name. This way you can have any kind of data in your scriptable object. You can do that by providing a custom reader and writer.  
Here is an example:

{{{ Path:'Snippets/Serialization/DataTypesSnippets.cs' Name:'scriptable-object-serializer' }}}

