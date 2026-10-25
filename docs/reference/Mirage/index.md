---
id: Mirage
title: Mirage
---

# Mirage

## Classes

#### [AutoPool&lt;T&gt;](./AutoPool-1)
> 
Pool class that will create a Disposable wrapper around T so it can be used with any class automatically without additional setup

#### [AutoPool&lt;T&gt;.Wrapper](./AutoPool-1.Wrapper)
#### [ClientAttribute](./ClientAttribute)
> 
Prevents this method from running if client is not active.
Can only be used inside a NetworkBehaviour

#### [ClientObjectManager](./ClientObjectManager)
#### [ClientRpcAttribute](./ClientRpcAttribute)
> 
The server uses a Remote Procedure Call (RPC) to run this function on specific clients.
Note that if you set the target as Connection, you need to pass a specific connection as a parameter of your method

#### [DefaultSpawnValuesHandler](./DefaultSpawnValuesHandler)
#### [DeserializeFailedException](./DeserializeFailedException)
#### [ExponentialMovingAverage](./ExponentialMovingAverage)
#### [FoldoutEventAttribute](./FoldoutEventAttribute)
> 
Draws UnityEvent as a foldout

#### [GameObjectExtensions](./GameObjectExtensions)
#### [GameObjectSerializers](./GameObjectSerializers)
#### [HasAuthorityAttribute](./HasAuthorityAttribute)
> 
Prevents players without authority from running this method.
Can only be used inside a NetworkBehaviour

#### [HeadlessAutoStart](./HeadlessAutoStart)
#### [HeadlessFrameLimiter](./HeadlessFrameLimiter)
#### [HostRendererVisibility](./HostRendererVisibility)
> 
Disables all Renders on GameObject when the NetworkIdentity is not visible too the host player because of a 

#### [LocalPlayerAttribute](./LocalPlayerAttribute)
> 
Prevents nonlocal players from running this method.
Can only be used inside a NetworkBehaviour

#### [MaxLengthAttribute](./MaxLengthAttribute)
> 
Restricts the serialization and deserialization size of strings, collections (arrays, lists), 
or any custom type that has read/write overloads accepting an integer limit.
This will use the Write/Read with length functions and will work on any type that has writers/readers for those.

#### [MessageHandler](./MessageHandler)
#### [MessageReceiverExtensions](./MessageReceiverExtensions)
#### [MessageWaiter&lt;T&gt;](./MessageWaiter-1)
> 
Register handler just for 1 message
Useful on client when you want too receive a single auth message

#### [MethodInvocationException](./MethodInvocationException)
> 
Exception thrown if a guarded method is invoked incorrectly

#### [NetworkBehaviorSerializers](./NetworkBehaviorSerializers)
#### [NetworkBehaviour](./NetworkBehaviour)
> 
Base class which should be inherited by scripts which contain networking functionality.


#### [NetworkClient](./NetworkClient)
> 
This is a network client class used by the networking system. It contains a NetworkConnection that is used to connect to a network server.
The  handle connection state, messages handlers, and connection configuration. There can be many  instances in a process at a time, but only one that is connected to a game server () that uses spawned objects.
 has an internal update function where it handles events from the transport layer. This includes asynchronous connect events, disconnect events and incoming data from a server.

#### [NetworkDiagnostics](./NetworkDiagnostics)
> 
Provides profiling information from mirror
A profiler can subscribe to these events and
present the data in a friendly way to the user

#### [NetworkExtensions](./NetworkExtensions)
#### [NetworkIdentity](./NetworkIdentity)
> 
The NetworkIdentity identifies objects across the network, between server and clients.
Its primary data is a NetworkInstanceId which is allocated by the server and then set on clients.
This is used in network communications to be able to lookup game objects on different machines.

#### [NetworkIdentitySerializers](./NetworkIdentitySerializers)
#### [NetworkInspectorCallbacks](./NetworkInspectorCallbacks)
> 
Callbacks for 

#### [NetworkManager](./NetworkManager)
#### [NetworkManagerGUI](./NetworkManagerGUI)
#### [NetworkManagerHud](./NetworkManagerHud)
#### [NetworkMatchChecker](./NetworkMatchChecker)
> 
Component that controls visibility of networked objects based on match id.
Any object with this component on it will only be visible to other objects in the same match.
This would be used to isolate players to their respective matches within a single game server instance. 

#### [NetworkMessageAttribute](./NetworkMessageAttribute)
> 
Tell the weaver to generate  reader and writer for a class

#### [NetworkMethodAttribute](./NetworkMethodAttribute)
> 
Prevents this method from running unless the NetworkFlags match the current state
Can only be used inside a NetworkBehaviour

#### [NetworkPingDisplay](./NetworkPingDisplay)
> 
Component that will display the clients ping in milliseconds

#### [NetworkPlayer](./NetworkPlayer)
> 
A High level network connection. This is used for connections from client-to-server and for connection from server-to-client.

#### [NetworkPrefabs](./NetworkPrefabs)
> 
A scriptable object that contains a list of prefabs that can be spawned on the network.

#### [NetworkProximityChecker](./NetworkProximityChecker)
> 
Component that controls visibility of networked objects for players.
Any object with this component on it will not be visible to players more than a (configurable) distance away.

#### [NetworkSceneChecker](./NetworkSceneChecker)
> 
Component that controls visibility of networked objects between scenes.
Any object with this component on it will only be visible to other objects in the same scene
This would be used when the server has multiple additive subscenes loaded to isolate players to their respective subscenes

#### [NetworkServer](./NetworkServer)
> 
The NetworkServer.

#### [NetworkTime](./NetworkTime)
> 
Synchronize time between the server and the clients

#### [NetworkTransform](./NetworkTransform)
#### [NetworkTransformBase](./NetworkTransformBase)
#### [NetworkTransformBase.DataPoint](./NetworkTransformBase.DataPoint)
#### [NetworkTransformChild](./NetworkTransformChild)
> 
A component to synchronize the position of child transforms of networked objects.
There must be a NetworkTransform on the root object of the hierarchy. There can be multiple NetworkTransformChild components on an object. This does not use physics for synchronization, it simply synchronizes the localPosition and localRotation of the child transform and lerps towards the received values.

#### [NetworkVisibility](./NetworkVisibility)
> 
NetworkBehaviour that calculates if the gameObject should be visible to different players or not

#### [NetworkWorld](./NetworkWorld)
> 
Holds collection of spawned network objects
This class works on both server and client

#### [NetworkWorldExtensions](./NetworkWorldExtensions)
#### [NetworkedPrefabAttribute](./NetworkedPrefabAttribute)
> 
Forces the user to provide a prefab that has a NetworkIdentity component and is registered.
Also provides a fix button to fix the prefab if it hasn&apos;t been networked.

#### [PendingAsyncSpawn](./PendingAsyncSpawn)
#### [PipePeerConnection](./PipePeerConnection)
> 
A  that directly sends data to a 
bypassing the transport layer for local host/client communication.

#### [PipePeerConnection.PipeConnectionHandle](./PipePeerConnection.PipeConnectionHandle)
> 
Virtual connection handle for internal pipe connections.

#### [RateLimitAttribute](./RateLimitAttribute)
#### [ReadOnlyInspectorAttribute](./ReadOnlyInspectorAttribute)
> 
Makes field readonly in inspector.
This is useful for fields that are set by code, but are shown in inpector for debuggiing

#### [RecentlyDestroyed](./RecentlyDestroyed)
#### [SceneAttribute](./SceneAttribute)
> 
Converts a string property into a Scene property in the inspector

#### [SceneVisibilityChecker](./SceneVisibilityChecker)
#### [ServerAttribute](./ServerAttribute)
> 
Prevents a method from running if server is not active.
Can only be used inside a NetworkBehaviour

#### [ServerObjectManager](./ServerObjectManager)
> 
The ServerObjectManager.

#### [ServerObjectManagerExtensions](./ServerObjectManagerExtensions)
> 
Extra helper methods for  that dont add any extra logic

#### [ServerRpcAttribute](./ServerRpcAttribute)
> 
Call this from a client to run this function on the server.
Make sure to validate input etc. It&apos;s not possible to call this from a server.

#### [ShowInInspectorAttribute](./ShowInInspectorAttribute)
> 
Used to show private SyncList in the inspector,
 Use instead of SerializeField for non Serializable types 

#### [ShowSyncSettingsAttribute](./ShowSyncSettingsAttribute)
> 
Add to NetworkBehaviour to force SyncSettings to be drawn, even if there are no syncvars

#### [SpawnEvent](./SpawnEvent)
#### [SpawnHandler](./SpawnHandler)
#### [SpawnObjectException](./SpawnObjectException)
> 
Exception thrown when spawning fails

#### [StringHash](./StringHash)
#### [SyncPrefabSerialize](./SyncPrefabSerialize)
#### [SyncVarAttribute](./SyncVarAttribute)
> 
SyncVars are used to synchronize a variable from the server to all clients automatically.
Value must be changed on server, not directly by clients.  Hook parameter allows you to define a client-side method to be invoked when the client gets an update from the server.

#### [SyncVarReceiver](./SyncVarReceiver)
> 
Class that handles syncvar message and passes it to correct 

#### [SyncVarSender](./SyncVarSender)
> 
Class that Syncs syncvar and other  State

#### [Version](./Version)
## Structs

#### [AddCharacterMessage](./AddCharacterMessage)
#### [AuthorityTime](./AuthorityTime)
#### [GameObjectSyncvar](./GameObjectSyncvar)
> 
backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.

#### [NetworkBehaviorSyncvar](./NetworkBehaviorSyncvar)
> 
backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.

#### [NetworkBehaviorSyncvar&lt;T&gt;](./NetworkBehaviorSyncvar-1)
#### [NetworkBehaviour.Id](./NetworkBehaviour.Id)
#### [NetworkDiagnostics.MessageInfo](./NetworkDiagnostics.MessageInfo)
> 
Describes an outgoing message

#### [NetworkIdentitySyncvar](./NetworkIdentitySyncvar)
> 
backing struct for a NetworkIdentity when used as a syncvar
the weaver will replace the syncvar with this struct.

#### [NetworkPingMessage](./NetworkPingMessage)
#### [NetworkPongMessage](./NetworkPongMessage)
#### [NetworkSpawnSettings](./NetworkSpawnSettings)
> 
Spawn Settings for 

#### [ObjectDestroyMessage](./ObjectDestroyMessage)
#### [ObjectHideMessage](./ObjectHideMessage)
#### [RemoveAuthorityMessage](./RemoveAuthorityMessage)
#### [RemoveCharacterMessage](./RemoveCharacterMessage)
#### [SceneMessage](./SceneMessage)
#### [SceneNotReadyMessage](./SceneNotReadyMessage)
> 
Sent to client to mark their scene as not ready
Client can sent  once its scene is ready again

#### [SceneReadyMessage](./SceneReadyMessage)
> 
Sent to indicate the scene is finished loading

#### [SpawnMessage](./SpawnMessage)
#### [SpawnValues](./SpawnValues)
#### [SyncPrefab](./SyncPrefab)
#### [SyncSettings](./SyncSettings)
#### [UpdateVarsMessage](./UpdateVarsMessage)
## Interfaces

#### [IMessageReceiver](./IMessageReceiver)
> 
An object that can receive messages

#### [IMessageSender](./IMessageSender)
> 
An object that can send messages

#### [INetIdGenerator](./INetIdGenerator)
#### [INetworkPlayer](./INetworkPlayer)
> 
An object owned by a player that can: send/receive messages, have network visibility, be an object owner, authenticated permissions, and load scenes.
May be from the server to client or from client to server

#### [INetworkVisibility](./INetworkVisibility)
#### [IObjectLocator](./IObjectLocator)
> 
An object that implements this interface can find objects by their net id
This is used by readers when trying to deserialize gameobjects

#### [IObjectOwner](./IObjectOwner)
> 
An object that can own networked objects

#### [ISceneLoader](./ISceneLoader)
#### [ISpawnValuesHandler](./ISpawnValuesHandler)
#### [IVisibilityTracker](./IVisibilityTracker)
> 
An object that can observe NetworkIdentities.
this is useful for interest management

## Enums

#### [Channel](./Channel)
#### [ClientStoppedReason](./ClientStoppedReason)
> 
Reason why Client was stopped or disconnected

#### [ConnectState](./ConnectState)
#### [NetworkFlags](./NetworkFlags)
#### [NetworkManagerMode](./NetworkManagerMode)
#### [PendingAsyncSpawn.MessageType](./PendingAsyncSpawn.MessageType)
#### [PlayerErrorFlags](./PlayerErrorFlags)
#### [RpcTarget](./RpcTarget)
> 
Used by ClientRpc to tell mirage who to send remote call to

#### [SyncActiveOption](./SyncActiveOption)
#### [SyncFrom](./SyncFrom)
#### [SyncHookType](./SyncHookType)
#### [SyncTiming](./SyncTiming)
#### [SyncTo](./SyncTo)
## Delegates

#### [AuthorityChanged](./AuthorityChanged)
> 
Event that can be used to check authority

#### [DynamicSpawnHandlerDelegate](./DynamicSpawnHandlerDelegate)
#### [MessageDelegate&lt;T&gt;](./MessageDelegate-1)
#### [MessageDelegateAsync&lt;T&gt;](./MessageDelegateAsync-1)
#### [MessageDelegateWithPlayer&lt;T&gt;](./MessageDelegateWithPlayer-1)
#### [MessageDelegateWithPlayerAsync&lt;T&gt;](./MessageDelegateWithPlayerAsync-1)
#### [NetworkServer.AuthFailCallback](./NetworkServer.AuthFailCallback)
#### [NetworkServer.RateLimitCallback](./NetworkServer.RateLimitCallback)
#### [NetworkVisibility.VisibilityChanged](./NetworkVisibility.VisibilityChanged)
#### [NetworkWorld.UnspawnHandler](./NetworkWorld.UnspawnHandler)
#### [SpawnHandlerAsyncDelegate](./SpawnHandlerAsyncDelegate)
#### [SpawnHandlerDelegate](./SpawnHandlerDelegate)
#### [UnSpawnDelegate](./UnSpawnDelegate)
