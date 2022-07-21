---
sidebar_position: 5
---
# Sync Hash Set

[`SyncHashSet`](/docs/reference/Mirage.Collections/SyncHashSet-1) is a set similar to C\# [HashSet<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1) that synchronizes its contents from the server to the clients.

A SyncHashSet can contain any [supported Mirage type](/docs/guides/data-types) 

## Usage

Create a class that derives from SyncHashSet for your specific type. This is necessary because Mirage will add methods to that class with the weaver. Then add a SyncHashSet field to your NetworkBehaviour class. For example:

:::caution IMPORTANT
You need to initialize the SyncHashSet immediately after the definition in order for them to work. You can mark them as `readonly` to enforce proper usage.
:::

### Basic example
```cs
[System.Serializable]
public class SyncSkillSet : SyncHashSet<string> {}

public class Player : NetworkBehaviour {

    [SerializeField]
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

# Callbacks
You can detect when a SyncHashSet changes on the client and/or the server. This is especially useful for refreshing your UI, character appearance, etc. 

Subscribe to the Callback event typically during `Start`, `OnClientStart`, or `OnServerStart` for that. 

:::note
Note that by the time you subscribe, the set will already be initialized, so you will not get a call for the initial data, only updates.
:::

```cs
[System.Serializable]
public class SyncSetBuffs : SyncHashSet<string> {};

public class Player : NetworkBehaviour
{
    [SerializeField]
    public readonly SyncSetBuffs buffs = new SyncSetBuffs();

    // this will add the delegate on the client.
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
