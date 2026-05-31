# MIRAGE1003: Direct Mutation of SyncCollection Elements

## The Problem
A member or element within a `SyncList` or `SyncDictionary` is mutated directly without assigning it back to the collection or calling a change notification method.

Because C# structs are value types, modifying an element's member directly after retrieving it from a collection (e.g. `mySyncList[i].health = 10;`) only modifies a local copy of that struct. Even for reference types (classes), mutating the object directly does not call the indexer setter on the collection. In both cases, the collection cannot detect that a nested property has changed, preventing the dirty flag from being set and keeping the modifications from synchronizing to clients.

---

## Example of Triggering Code
```csharp
using Mirage;
using Mirage.Collections;

public struct PlayerData
{
    public int health;
}

public class Player : NetworkBehaviour
{
    public readonly SyncList<PlayerData> playerList = new SyncList<PlayerData>();

    public void DamagePlayer(int index, int damage)
    {
        // Warning: Direct mutation of elements inside playerList is not supported because changes cannot be tracked.
        playerList[index].health -= damage;
    }
}
```

---

## How to Resolve

Retrieve the element, perform the modification, and then assign the modified element back to the collection via the indexer. This ensures the collection's change tracking is triggered.

```csharp
using Mirage;
using Mirage.Collections;

public struct PlayerData
{
    public int health;
}

public class Player : NetworkBehaviour
{
    public readonly SyncList<PlayerData> playerList = new SyncList<PlayerData>();

    public void DamagePlayer(int index, int damage)
    {
        // Correct: Modifying the element and setting it back, triggering the index setter
        var data = playerList[index];
        data.health -= damage;
        playerList[index] = data;
    }
}
```
