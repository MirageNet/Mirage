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

*   **For SyncVars**: Weaver uses `NetworkIdentitySyncvar.cs` internally to manage object references automatically. Therefore, you should sync `GameObject` or `NetworkIdentity` references directly rather than manually syncing `uint` (NetId).
*   **For RPCs**: RPCs are executed instantly. If the referenced game object does not yet exist on the client when the RPC payload is received (e.g. if the object hasn't spawned on the client yet or is excluded due to network visibility), passing a `GameObject` or `NetworkIdentity` reference directly will result in a null value.

For RPCs, it is more robust to pass the `NetworkIdentity.NetID` (`uint`) and perform your own lookup on the client, using `UniTask` to wait for the object to spawn:

```cs
public GameObject target;

 [ClientRpc]
 public void SetTargetRpc(uint targetID)
 {
     SetTargetAsync(targetID).Forget();
 }

 private async UniTaskVoid SetTargetAsync(uint targetID)
 {
     var token = destroyCancellationToken;

     // Wait until the target object is spawned on the client
     while (!token.IsCancellationRequested)
     {
         if (Identity.World.TryGetIdentity(targetID, out var identity))
         {
             target = identity.gameObject;
             return;
         }
         await UniTask.Yield();
     }
 }
```

## Custom Data Types

Sometimes you don't want Mirage to generate serialization for your own types. For example, instead of serializing quest data, you may want to serialize just the quest id, and the receiver can look up the quest by id in a predefined list.

Sometimes you may want to serialize data that uses a different type not supported by Mirage, such as `DateTime` or `System.Uri`.

You can add support for any type by adding extension methods to `NetworkWriter` and `NetworkReader`. For example, to add support for `DateTime`, add this somewhere in your project:

```cs
public static class DateTimeReaderWriter
 {
     public static void WriteDateTime(this NetworkWriter writer, DateTime dateTime)
     {
         writer.WriteInt64(dateTime.Ticks);
     }

     public static DateTime ReadDateTime(this NetworkReader reader)
     {
         return new DateTime(reader.ReadInt64());
     }
 }
```

...then you can use `DateTime` in your `[ServerRpc]` or `SyncList`

## Inheritance and Polymorphism

Sometimes you might want to send a polymorphic data type to your commands. Mirage does not serialize the type name to keep messages small and for security reasons, therefore Mirage cannot figure out the type of object it received by looking at the message.

:::caution
This code does not work out of the box.
:::

```cs
public class Item
 {
     public string name;
 }

 public class Weapon : Item
 {
     public int hitPoints;
 }

 public class Armor : Item
 {
     public int hitPoints;
     public int level;
 }

 public class Player : NetworkBehaviour
 {
     [ServerRpc]
     private void ServerRpcEquip(Item item)
     {
         // IMPORTANT: this does not work. Mirage will pass you an object of type item
         // even if you pass a weapon or an armor.
         if (item is Weapon weapon)
         {
             // The item is a weapon, 
             // maybe you need to equip it in the hand
         }
         else if (item is Armor armor)
         {
             // you might want to equip armor in the body
         }
     }

     [ServerRpc]
     private void ServerEquipArmor(Armor armor)
     {
         // IMPORTANT: this does not work either, you will receive an armor, but 
         // the armor will not have a valid Item.name, even if you passed an armor with name
     }
 }
```

`ServerRpcEquip` will work if you provide a custom serializer for the `Item` type. For example:

```cs
public static class ItemSerializer
 {
     private const byte WEAPON = 1;
     private const byte ARMOR = 2;

     public static void WriteItem(this NetworkWriter writer, Item item)
     {
         if (item is Weapon weapon)
         {
             writer.WriteByte(WEAPON);
             writer.WriteString(weapon.name);
             writer.WritePackedInt32(weapon.hitPoints);
         }
         else if (item is Armor armor)
         {
             writer.WriteByte(ARMOR);
             writer.WriteString(armor.name);
             writer.WritePackedInt32(armor.hitPoints);
             writer.WritePackedInt32(armor.level);
         }
     }

     public static Item ReadItem(this NetworkReader reader)
     {
         byte type = reader.ReadByte();
         switch (type)
         {
             case WEAPON:
                 return new Weapon
                 {
                     name = reader.ReadString(),
                     hitPoints = reader.ReadPackedInt32()
                 };
             case ARMOR:
                 return new Armor
                 {
                     name = reader.ReadString(),
                     hitPoints = reader.ReadPackedInt32(),
                     level = reader.ReadPackedInt32()
                 };
             default:
                 throw new Exception($"Invalid weapon type {type}");
         }
     }
 }
```

## Scriptable Objects

People often want to send scriptable objects from the client or server. For example, you may have a bunch of swords created as scriptable objects and you want to put the equipped sword in a [SyncVar](/docs/guides/sync/sync-var). This will work fine, Mirage will generate a reader and writer for scriptable objects by calling `ScriptableObject.CreateInstance` and copy all the data. 

However, the generated reader and writer are not suitable for every occasion. Scriptable objects often reference other assets such as textures, prefabs, or other types that can't be serialized. Scriptable objects are often saved in the Resources folder or they can sometimes have a large amount of data in them. The generated reader and writers may not work or may be inefficient for these situations.

Instead of passing the scriptable object data, you can pass the name and the other side can look up the same object by name. This way you can have any kind of data in your scriptable object. You can do that by providing a custom reader and writer.  
Here is an example:

```cs
[CreateAssetMenu(fileName = "New Armor", menuName = "Armor Data")]
 public class Armor : ScriptableObject
 {
     public int Hitpoints;
     public int Weight;
     public string Description;
     public Texture2D Icon;
     // ...
 }

 public static class ArmorSerializer
 {
     public static void WriteArmor(this NetworkWriter writer, Armor armor)
     {
         // No need to serialize the data, just the name of the armor.
         writer.WriteString(armor.name);
     }

     public static Armor ReadArmor(this NetworkReader reader)
     {
         // Load the same armor by name. The data will come from the asset in Resources folder.
         return Resources.Load<Armor>(reader.ReadString());
     }
 }
```

