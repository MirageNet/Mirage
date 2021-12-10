# Getting Started

This document describes steps to creating a multiplayer game with Mirage. The process described here is a simplified, higher level version of the actual process for a real game; it doesn’t always work exactly like this, but it provides a basic recipe for the process.

## Video tutorials

Currently there are no mirage specific videos. You can use mirror existing videos along side our [Mirror to Mirage Conversion](../Guides/MirrorMigration.md) guide.

## Networking set-up
-   Right click in the Scene, select *Network* > *NetworkManager*. This will create a new GameObject with all the necessary networking components set up for you.
-   Add the NetworkManagerHUD component to the game object. This provides an out of the box canvas ui system that you can use and create your own ui with. Assign the variables
to properly setup the ui so everything will interact with network code. In future we will be providing out of the box ui that auto set's up.

See [Using the NetworkManager](../Components/NetworkManager.md).

## Player Prefab
- Create a new prefab in your project.
- Add a NetworkIdentity component to the player Prefab
- Set the `playerPrefab` field on the `CharacterSpawner` component  to the player Prefab. You can find this component on the GameObject you created in the first setup.

See [character objects](../Guides/GameObjects/SpawnPlayer.md) for more information.

## Player Movements
>[!NOTE]
> Attach the correct type of movement script you intended to use. NetworkTransform will move the transform component without physics calculations. If you
>require physics to be also part of your normal movement calculations suggestion would be to attach a NetworkRigidBody component instead.

>[!WARNING]
> Currently network transform and network rigid body components cannot handle various lag and drop of packets. Jitter or lag behind may occur. We are working
>to improve these components and have a better lag and jitter control in the future.
- Add the correct movement component you require.
- Check box client authority under the component if you want the player to be able to fully control movement without server controlling it.
>[!WARNING]
> If client authority is checked then hacking can occur due to trusting clients and accepting there movement. if you do not want this do not check client authority
> under the component and you will require to send commands to server to allow movement to occur.
- Update input and control scripts to reflect what type of control you want. See below for different examples.

>[!NOTE]
> If you require a camera to run on player prefab subscribe to `Identity.OnStartLocalPlayer` event to take control of the Main Camera in the scene for the player. You do not need a camera on every
>player prefab.

Example if client authority has been checked and you trust clients. Never trust clients tho.

```cs
using UnityEngine;
using Mirage;

public class Controls : NetworkBehaviour
{
    void Update()
    {
        if (!IsLocalPlayer)
        {
            // exit from update if this is not the local player
            return;
        }

        // handle player input for movement
    }
}
```

Example if server authority is going to be used.

```cs
using UnityEngine;
using Mirage;

public class Controls : NetworkBehaviour
{
    void Update()
    {
        if (!IsLocalPlayer)
        {
            // exit from update if this is not the local player
            return;
        }

        // handle player input for movement

       // You would call this command after handling input or you can send inputs directly to
       // server and let server buffer inputs up and do movements based on the buffered inputs.
       MovePlayer();
    }

    [ServerRpc]
    void MovePlayer()
    {
        // We are now firing off some kind of movement all done by server.
    }
}
```

## Basic player game state
-   Make scripts that contain important data into NetworkBehaviours instead of MonoBehaviours
-   You can sync different types of data depending on what you need to sync.

See [State Synchronization](../Guides/Sync/index.md).

## Remote calls
-   Make scripts that perform important actions into NetworkBehaviours instead of MonoBehaviours
-   Update functions that perform important player actions to have an attribute of `[ServerRpc]`

See [Remote actions](../Guides/RemoteCalls/index.md).

## Non-player game objects

Fix non-player prefabs such as enemies:
-   Add the `NetworkIdentify` component
-   Add the correct component needed to sync movement as state above with player movement.
-   Once all objects have a network identity. Find ClientObjectManager under the `networkmanager` game object in scene and hit register prefabs.
-   Update scripts with game state and actions

## Spawners
-   Potentially change spawner scripts to be NetworkBehaviours
-   Modify spawners to only run on the server (use `IsServer` property or the `OnStartServer` callback)
-   Call `ServerObjectManager.Spawn()` for created game objects

## Spawn positions for players
-   Add a new game object and place it at player’s start location
-   Add this GameObject to the `CharacterSpawner`'s `Positions` list.
