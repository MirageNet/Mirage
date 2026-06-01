---
sidebar_position: 6
title: Spawn Object - Custom
---
# Custom Spawn Functions

You can use spawn handler functions to customize the default behavior when creating spawned game objects on the client. Spawn handler functions ensure you have full control of how you spawn the game object, as well as how you destroy it.

Use `ClientObjectManager.RegisterSpawnHandler` or `ClientObjectManager.RegisterPrefab` to register functions to spawn and destroy client game objects. The server creates game objects directly and then spawns them on the clients through this functionality. This function takes either the asset ID or a prefab and two function delegates: one to handle creating game objects on the client, and one to handle destroying game objects on the client. The asset ID can be a dynamic one, or just the asset ID found on the prefab game object you want to spawn.

The spawn/unspawn delegates will look something like this:

**Spawn Handler**
{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'spawn-handler-delegate' }}}

**UnSpawn Handler**
{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'unspawn-handler-delegate' }}}

When a prefab is saved its `PrefabHash` field will be automatically set. If you want to create prefabs at runtime you will have to generate a new Hash instead.

**Generate prefab at runtime**
{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'generate-prefab-runtime' }}}

:::note
The unspawn function may be left as `null`, Mirage will then call `GameObject.Destroy` when the destroy message is received.
:::

**Use existing prefab**
{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'use-existing-prefab' }}}

**Spawn on Server**
{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'spawn-on-server' }}}

The spawn functions themselves are implemented with the delegate signature. Here is the coin spawner. The `SpawnCoin` would look the same, but have different spawn logic:

{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'spawn-coin-methods' }}}

When using custom spawn functions, it is sometimes useful to be able to unspawn game objects without destroying them. This can be done by calling `NetworkServer.Destroy(identity, destroyServerObject: false)`, making sure that the 2nd argument is false. This causes the object to be `Reset` on the server and sends a `ObjectDestroyMessage` to clients. The `ObjectDestroyMessage` will cause the custom unspawn function to be called on the clients. If there is no unspawn function the object will instead be `Destroy`

Note that on the host, game objects are not spawned for the local client, because they already exist on the server. This also means that no spawn or unspawn handler functions are called.

## Setting Up a Game Object Pool with Custom Spawn Handlers

you can use custom spawn handlers in order set up object pooling so you dont need to instantiate and destroy objects each time you use them. 

A full guide on pooling can be found here: [Spawn Object Pooling](./spawn-object-pooling)

{{{ Path:'Snippets/GameObjects/CustomSpawnExample.cs' Name:'pool-spawn-handlers' }}}

## Dynamic spawning 

Some times you may want to create objects at runtime and you might not know the prefab hash ahead of time. For this you can use Dynamic Spawn Handlers to return a spawn handler for a prefab hash.

Below is an example where client pre-spawns objects while loading, and then network spawns them when receiving a `SpawnMessage` from server.

Dynamic Handler avoid the need to add 1 spawn handler for each prefab hash. Instead you can just add a single dynamic handler that can then be used to find and return objects.

{{{ Path:'Snippets/Spawning/DynamicSpawning.cs' Name:'dynamic-spawning' }}}