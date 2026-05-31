# MIRAGE1004: Reassignment of SyncObject Fields

## The Problem
A field implementing `ISyncObject` (such as `SyncList`, `SyncDictionary`, `SyncHashSet`, etc.) is reassigned, or is not marked `readonly`.

Fields implementing `ISyncObject` must be initialized once when the class is constructed and cannot be reassigned during the lifecycle of the object. Reassigning these fields breaks Mirage's post-processing code weaving and prevents proper dirty-tracking and delta serialization.

---

## Example of Triggering Code
```csharp
using Mirage;
using Mirage.Collections;

public class Player : NetworkBehaviour
{
    // Error: ISyncObject field 'playerList' must be marked readonly and cannot be reassigned
    public SyncList<int> playerList = new SyncList<int>();

    public void ResetList()
    {
        playerList = new SyncList<int>();
    }
}
```

---

## How to Resolve

Mark the field as `readonly` to ensure it cannot be reassigned. To clear or reset the collection, use the collection's `.Clear()` method instead of instantiating a new object.

```csharp
using Mirage;
using Mirage.Collections;

public class Player : NetworkBehaviour
{
    // Correct: Marked as readonly
    public readonly SyncList<int> playerList = new SyncList<int>();

    public void ResetList()
    {
        // Correct: Clear the list instead of reassigning it
        playerList.Clear();
    }
}
```
