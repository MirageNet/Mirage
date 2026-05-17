---
title: Player Proxy Pattern
---
# Player Proxy Pattern

A common pattern in multiplayer games is to separate the **player's persistent identity** from the **gameplay character** they control. This is done by using a lightweight "proxy" object as the player's main `NetworkIdentity`, while the actual gameplay character is a separate spawned object with authority granted to that player.

## Why Use a Proxy?

When using `AddCharacter` directly with a gameplay prefab, the player's `NetworkIdentity` is tied to that character. This can cause problems when:

- **Changing characters**: Swapping characters mid-game (e.g. class selection, vehicle entry) requires `ReplaceCharacter` and careful state migration
- **Respawning**: Destroying and re-creating the character loses all state on the `NetworkIdentity`
- **Scene transitions**: The player's persistent data (name, team, score) needs to survive scene loads
- **Lobby to gameplay**: You need a different object in the lobby vs in-game

With a proxy object, the player's persistent state lives on a simple `NetworkBehaviour` that is never destroyed. The gameplay character is a separate object that can be spawned, destroyed, or swapped freely.

## Setup

### 1. Create the Proxy Prefab

Create a prefab with just a `NetworkIdentity` and your `PlayerContext` script. This prefab does not need any visual representation.

```cs
public class PlayerContext : NetworkBehaviour
{
    [SyncVar] public string PlayerName;
    [SyncVar] public string Team;

    // easy access to the gameplay character via NetworkPlayer.Identity
    [SyncVar] public PlayerCharacter ActiveCharacter;
}
```

### 2. Create the Gameplay Character Prefab

This is your normal player character with movement, visuals, etc. It does not use `IsLocalPlayer` — instead it uses `HasAuthority` to check for local control.

```cs
public class PlayerCharacter : NetworkBehaviour
{
    void Update()
    {
        // use HasAuthority, not IsLocalPlayer
        if (!HasAuthority)
            return;

        // handle input and movement
    }
}
```

### 3. Spawn the Proxy on Connect

When a player connects, spawn the proxy as their character using `AddCharacter`. This proxy persists for the entire session.

```cs
private void OnServerAuthenticated(INetworkPlayer player)
{
    var proxy = Instantiate(PlayerProxyPrefab);
    ServerObjectManager.AddCharacter(player, proxy.gameObject);
}
```

### 4. Spawn the Gameplay Character Separately

When entering gameplay (e.g. after scene load, match start), spawn the gameplay character and grant authority to the player.

```cs
private void SpawnGameplayCharacter(INetworkPlayer player)
{
    var proxy = player.Identity.GetComponent<PlayerContext>();

    var character = Instantiate(CharacterPrefab, spawnPoint.position, spawnPoint.rotation);
    ServerObjectManager.Spawn(character.Identity, player);

    proxy.ActiveCharacter = character;
}
```

:::note
The gameplay character is spawned with `Spawn(identity, owner)` which grants authority to the player, rather than `AddCharacter` which would replace the proxy.
:::

### 5. Destroy and Respawn Freely

Because the proxy is the player's `Identity`, you can destroy and re-create gameplay characters without affecting the player's connection or persistent state.

```cs
private void RespawnCharacter(INetworkPlayer player)
{
    var proxy = player.Identity.GetComponent<PlayerContext>();

    if (proxy.ActiveCharacter != null)
    {
        Server.Destroy(proxy.ActiveCharacter.gameObject);
        proxy.ActiveCharacter = null;
    }

    SpawnGameplayCharacter(player);
}
```
