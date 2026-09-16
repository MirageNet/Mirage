---
sidebar_position: 5
title: Spawn Object
---
# Spawning Game Objects

In Unity, you usually “spawn” (that is, create) new game objects with `Instantiate`. However, in Mirage, the word “spawn” means something more specific. In the server-authoritative model of the Mirage, to “spawn” a game object on the server means that the game object is created on clients connected to the server, and is managed by the spawning system.

To spawn an object on the server you need to `Instantiate` the prefab and then call `Spawn` on the new object. This will assign a `NetId` to the object and send a `SpawnMessage` to clients.
```cs
var boxGo = Instantiate(boxPrefab);
 ServerObjectManager.Spawn(boxGo);
```

Once the game object is spawned using this system, state updates are sent to clients whenever the game object changes on the server. When Mirage destroys the game object on the server, it also destroys it on the clients. The server manages spawned game objects alongside all other networked game objects so that if another client joins the game later, the server can spawn the game objects on that client. These spawned game objects have a unique network instance ID called `NetId` that is the same on the server and clients for each game object. The unique network instance ID is used to route messages sent across the network to game objects and to identify game objects.

When the server spawns a game object with a Network Identity component, the game object spawned on the client has the same “state”. This means it is identical to the game object on the server; it has the same Transform, movement state, and (if Network Transform and SyncVars are used) synchronized variables. Therefore, client game objects are always up-to-date when Mirage creates them. This avoids issues such as game objects spawning at the wrong initial location, then reappearing at their correct position when a state update arrives.

A game object Prefab must have a Network Identity component before trying to register it with the Network Manager.

To register a Prefab with the Client Object Manager in the Editor, select the Network Manager game object, and in the Inspector, navigate to the Client Object Manager component. Click the triangle next to Spawn Prefabs to open the settings, and click the plus (+) button. Drag and drop Prefabs into the empty field to assign them to the list. 
For automatic registering click the 'Register All Prefabs' button.

:::note
This searches the entire project for prefabs/objects that have a network identity component on them and register it for you.
:::

![Registered Spawnable Prefabs](/img/guides/game-objects/spawn-objects.png)

## Spawning Without Network Manager

For more advanced users, you may find that you want to register Prefabs and spawn game objects without using the Network Manager component.

To spawn game objects without using the Network Manager, you can handle the Prefab registration yourself via script. Add the Prefab to the `ClientObjectManager.spawnPrefabs` list, or register it using the `ClientObjectManager.RegisterPrefab` method.

```cs
[NetworkMessage]
 public struct SpawnTreeMessage {}

 public class MyNetworkManager : MonoBehaviour
 {
     // Assign values in inspector
     public NetworkIdentity treePrefab;
     public ClientObjectManager ClientObjectManager;
     public NetworkClient Client;
     public NetworkServer Server;
     public ServerObjectManager ServerObjectManager;

     private void Awake()
     {
         // it is best to add events once in awake
         // this avoids the need to remove or clean them up,
         // which is ok for Manager classes that live as long as the NetworkServer/Client themselves

         Server.Started.AddListener(OnServerStarted);
         // use Authenticated instead of Connected to ensure that player is fully setup
         Server.Authenticated.AddListener(OnServerConnect);
         Client.Authenticated.AddListener(OnClientConnect);
     }

     // Register prefab and connect to the server
     // Call this from your UI or other code
     public void StartClient(string address)
     {
         if (!ClientObjectManager.spawnPrefabs.Contains(treePrefab))
             ClientObjectManager.spawnPrefabs.Add(treePrefab);

         Client.Connect(address);
     }

     private void OnClientConnect(INetworkPlayer player)
     {
         Debug.Log("Connected to server: " + player);
         // Request the server to spawn trees
         player.Send(new SpawnTreeMessage());
     }
```

In this example, you create an empty game object to act as the Network Manager, then create and attach the `MyNetworkManager` script (above) to that game object. Create a prefab that has a Network Identity component attached to it, and drag that onto the `treePrefab` slot on the `MyNetworkManager` component in the Inspector. This ensures that when the server spawns the tree game object, it also creates the same kind of game object on the clients.

Registering prefabs ensures that there is no stalling or loading time for creating the Asset.

For the script to work, you also need to add code for the server. Add this to the `MyNetworkManager` script:

```cs
public void StartServer()
 {
     // start listening
     Server.StartServer();
 }

 private void OnServerStarted()
 {
     // it is best to register message from .Started event
     // this means they will be added early enough for host player to use them
     Server.MessageHandler.RegisterHandler<SpawnTreeMessage>(HandleSpawnTreeMessage);
 }

 // When client requests to spawn trees
 private void HandleSpawnTreeMessage(INetworkPlayer player, SpawnTreeMessage msg)
 {
     Debug.Log("Client requested to spawn trees: " + player);
     SpawnTrees();
 }

 private void SpawnTrees()
 {
     var x = 0;
     for (var i = 0; i < 5; ++i)
     {
         var treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
         ServerObjectManager.Spawn(treeGo);
     }
 }

 private void OnServerConnect(INetworkPlayer player)
 {
     Debug.Log("New client connected: " + player);
 }
```

The server does not need to register anything, as it knows what game object is being spawned (and the asset ID is sent in the spawn message). The client needs to be able to look up the game object, so it must be registered on the client.

When writing your own network manager, it’s important to make the client ready to receive state updates before calling the spawn command on the server, otherwise, they won’t be sent. If you’re using Mirage’s built-in Network Manager component, this happens automatically.

For more advanced uses, such as object pools or dynamically created Assets, you can use the `ClientObjectManager.RegisterSpawnHandler` method, which allows callback functions to be registered for client-side spawning. See the documentation on [Custom Spawn Functions](/docs/guides/game-objects/spawn-object-custom) for an example of this.

If the game object has a network state like synchronized variables, then that state is synchronized with the spawn message. In the following example, this script is attached to the tree Prefab:

```cs
public class Tree : NetworkBehaviour
 {
     [SyncVar]
     public int numLeaves;

     void Start()
     {
         Identity.OnStartClient.AddListener(OnStartClient);
     }

     public void OnStartClient()
     {
         Debug.Log("Tree spawned with leaf count " + numLeaves);
     }
 }
```

With this script attached, you can change the `numLeaves` variable and modify the `SpawnTrees` function to see it accurately reflected on the client:

```cs
void SpawnTrees()
 {
     int x = 0;
     for (int i = 0; i < 5; ++i)
     {
         GameObject treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
         Tree tree = treeGo.GetComponent<Tree>();
         tree.numLeaves = Random.Range(10, 200);
         Debug.Log("Spawning leaf with leaf count " + tree.numLeaves);
         ServerObjectManager.Spawn(treeGo);
     }
 }
```

Attach the `Tree` script to the `treePrefab` script created earlier to see this in action.

### Constraints
-   A NetworkIdentity must be on the root game object of a spawnable Prefab. Without this, the Network Manager can’t register the Prefab.

## Game Object Creation Flow

The actual flow of internal operations that takes place for spawning game objects is:
-   Prefab with Network Identity component is registered as spawnable.
-   A game object is instantiated from the Prefab on the server.
-   The game code sets initial values on the instance (note that 3D physics forces applied here do not take effect immediately).
-   `ServerObjectManager.Spawn` is called with the instance.
-   The state of the SyncVars on the instance on the server is collected by calling `OnSerialize` on [Network Behaviour] components.
-   A network message of the type `ObjectSpawn` is sent to connected clients that include the SyncVar data.
-   `OnStartServer` is called on the instance on the server, and `IsServer` is set to `true`
-   Clients receive the `ObjectSpawn` message and create a new instance from the registered Prefab.
-   The SyncVar data is applied to the new instance on the client by calling OnDeserialize on Network Behaviour components.
-   `OnStartClient` is called on the instance on each client, and `IsClient` is set to `true`
-   As game-play progresses, changes to SyncVar values are automatically synchronized to clients. This continues until the game ends.
-   `ServerObjectManager.Destroy` is called on the instance on the server.
-   A network message of the type `ObjectDestroy` is sent to clients.
-   `OnNetworkDestroy` is called on the instance on clients, then the instance is destroyed.

### Character Game Objects

Character game objects in Mirage represent the network identity associated with an active player connection. The flow for spawning character game objects is:
-   Prefab with `NetworkIdentity` is registered as the `PlayerPrefab` on `CharacterSpawner`.
-   The client connects and authenticates with the server.
-   Client sends an `AddCharacterMessage` to the server.
-   The server receives the message and calls `CharacterSpawner.OnServerAddPlayer` (or your custom spawn handler).
-   A game object is instantiated from the Player Prefab on the server.
-   `ServerObjectManager.AddCharacter` is called with the player and the new character instance on the server.
-   The character instance is spawned - you do not have to call `ServerObjectManager.Spawn` for the character instance. The spawn message is sent to all clients like on a normal spawn.
-   A message is sent to the client assigning ownership of the character.
-   The client receives the spawn/ownership message.
-   `OnStartLocalPlayer` is called on the character instance on the owning client, and `IsLocalPlayer` is set to `true`.

:::note
`OnStartLocalPlayer` is called after `OnStartClient`, because it only happens when the ownership message arrives from the server after the character game object is spawned, so `IsLocalPlayer` is not set in `OnStartClient`.  
Because `OnStartLocalPlayer` is only called for the client’s local character game object, it is a good place to perform initialization that should only be done for the local character. This could include enabling input processing and enabling camera tracking for the character game object.
:::

## Spawning Game Objects with Client Authority

To spawn game objects and assign authority of those game objects to a particular client, use `ServerObjectManager.Spawn`, which takes as an argument the `INetworkPlayer` of the client that is to be made the authority.

For these game objects, the property `HasAuthority` is true on the client with authority, and `OnStartAuthority` is called on the client with authority. That client can issue Server RPCs for that game object. On other clients (and on the host), `HasAuthority` is false.

For example, the tree spawn example above can be modified to allow the tree to have client authority like this (note that we now need to pass in the `INetworkPlayer` representing the owning client’s connection):

```cs
void SpawnTrees(INetworkPlayer player)
 {
     int x = 0;
     for (int i = 0; i < 5; ++i)
     {
         GameObject treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
         Tree tree = treeGo.GetComponent<Tree>();
         tree.numLeaves = Random.Range(10, 200);
         Debug.Log("Spawning leaf with leaf count " + tree.numLeaves);
         ServerObjectManager.Spawn(treeGo, player);
     }
 }
```

The Tree script can now be modified to send a Server RPC Call to the server:

```cs
public class Tree : NetworkBehaviour
 {
     public int numLeaves;

     private void Awake()
     {
         // Register listener in Awake to catch any early authority changes.
         Identity.OnAuthorityChanged.AddListener(OnStartAuthority);
     }

     public void OnStartAuthority(bool hasAuthority)
     {
         // Only execute server command when we are given authority over the tree.
         if (hasAuthority)
             CmdMessageFromTree("Tree with " + numLeaves + " reporting in");
     }

     [ServerRpc]
     private void CmdMessageFromTree(string msg)
     {
         Debug.Log("Client sent a tree message: " + msg);
     }
 }
```

Note that you can’t just add the `CmdMessageFromTree` call into `OnStartClient`, because at that point the authority has not been set yet, so the call would fail.
