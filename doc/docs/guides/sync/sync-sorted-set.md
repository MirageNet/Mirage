---
sidebar_position: 6
---
# Sync Sorted Set

[`SyncSortedSet`](/docs/reference/Mirage.Collections/SyncSortedSet-1) is a set similar to C\# [SortedSet<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedset-1) that synchronizes its contents from the server to the clients.

Unlike SyncHashSets, all elements in a SyncSortedSet are sorted when they are inserted. Please note this has some performance implications.

A SyncSortedSet can contain any [supported Mirage type](/docs/guides/data-types) 

## Usage

Create a class that derives from SyncSortedSet for your specific type. This is necessary because Mirage will add methods to that class with the weaver. Then add a SyncSortedSet field to your NetworkBehaviour class. For example:

:::caution IMPORTANT
You need to initialize the SyncSortedSet immediately after the definition for them to work. You can mark them as `readonly` to enforce proper usage.
:::

```cs
class Player : NetworkBehaviour {

    class SyncSkillSet : SyncSortedSet<string> {}

    readonly SyncSkillSet skills = new SyncSkillSet();

    int skillPoints = 10;

    [Command]
    public void CmdLearnSkill(string skillName)
    {
        if (skillPoints > 1)
        {
            skillPoints--;

            skills.Add(skillName);
        }
    }
}
```

You can also detect when a SyncSortedSet changes. This is useful for refreshing your character in the client or determining when you need to update your database. Subscribe to the Callback event typically during `Start`, `OnClientStart`, or `OnServerStart` for that. 

:::note
That by the time you subscribe, the set will already be initialized, so you will not get a call for the initial data, only updates.
:::

```cs
public class Player : NetworkBehaviour
{
    private class SyncSetBuffs : SyncSortedSet<string> {};

    private readonly SyncSetBuffs buffs = new SyncSetBuffs();

    // This will add the delegate on the client.
    // Use OnStartServer instead if you want it on the server
    public override void OnStartClient()
    {
        buffs.Callback += OnBuffsChanged;
    }

    private void OnBuffsChanged(SyncSetBuffs.Operation op, string buff)
    {
        switch (op) 
        {
            case SyncSetBuffs.Operation.OP_ADD:
                // we added a buff, draw an icon on the character
                break;
            case SyncSetBuffs.Operation.OP_CLEAR:
                // clear all buffs from the character
                break;
            case SyncSetBuffs.Operation.OP_REMOVE:
                // We removed a buff from the character
                break;
        }
    }
}
```
