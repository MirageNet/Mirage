---
id: NetworkServer
title: NetworkServer
---

# Class NetworkServer


The NetworkServer.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class NetworkServer : MonoBehaviour
```


### Fields

#### EnablePeerMetrics

##### Declaration

```cs
public bool EnablePeerMetrics
```
#### MetricsSize

##### Declaration

```cs
public int MetricsSize
```
#### PeerConfigProfile

##### Declaration

```cs
public PeerConfigProfile PeerConfigProfile
```
#### MaxConnections

The maximum number of concurrent network connections to support. Excluding the host player.
This field is only used if the  property is null


##### Declaration

```cs
public int MaxConnections
```
#### RunInBackground

##### Declaration

```cs
public bool RunInBackground
```
#### Listening

##### Declaration

```cs
public bool Listening
```
#### SocketFactory

##### Declaration

```cs
public SocketFactory SocketFactory
```
#### ObjectManager

##### Declaration

```cs
public ServerObjectManager ObjectManager
```
#### Authenticator

##### Declaration

```cs
public AuthenticatorSettings Authenticator
```
#### DisconnectOnException

##### Declaration

```cs
[Obsolete("Use RpcErrorConfig and RateLimitCallback", true)]
public bool DisconnectOnException
```
#### RethrowException

##### Declaration

```cs
public bool RethrowException
```
#### ErrorRateLimitEnabled

Will kick players or run callback if players hit Error limit set by . If changed at runtime, new value will only apply to new connections


##### Declaration

```cs
public bool ErrorRateLimitEnabled
```
#### ErrorRateLimitConfig

##### Declaration

```cs
public RateLimitBucket.RefillConfig ErrorRateLimitConfig
```
#### ManualUpdate

Set to true if you want to manually call  and  and stop mirage from automatically calling them


##### Declaration

```cs
public bool ManualUpdate
```

### Properties

#### Metrics

##### Declaration

```cs
public Metrics Metrics { get; }
```
#### PeerConfig

Config for peer, if not set will use default settings


##### Declaration

```cs
public Config PeerConfig { get; set; }
```
#### PeerPoolMetrics

##### Declaration

```cs
public PoolMetrics? PeerPoolMetrics { get; }
```
#### Started

This is invoked when a server is started - including when a host is started.


##### Declaration

```cs
public IAddLateEventUnity Started { get; }
```
#### Connected

##### Declaration

```cs
public NetworkPlayerEvent Connected { get; }
```
#### Authenticated

##### Declaration

```cs
public NetworkPlayerEvent Authenticated { get; }
```
#### Disconnected

##### Declaration

```cs
public NetworkPlayerEvent Disconnected { get; }
```
#### Stopped

##### Declaration

```cs
public IAddLateEventUnity Stopped { get; }
```
#### OnStartHost

##### Declaration

```cs
public IAddLateEventUnity OnStartHost { get; }
```
#### OnStopHost

##### Declaration

```cs
public IAddLateEventUnity OnStopHost { get; }
```
#### LocalPlayer

The connection to the host mode client (if any).


##### Declaration

```cs
public INetworkPlayer LocalPlayer { get; }
```
#### LocalClient

The host client for this server 


##### Declaration

```cs
public NetworkClient LocalClient { get; }
```
#### LocalClientActive

True if there is a local client connected to this server (host mode)


##### Declaration

```cs
[Obsolete("use IsHost instead")]
public bool LocalClientActive { get; }
```
#### IsHost

True if there is a local client connected to this server (host mode)


##### Declaration

```cs
public bool IsHost { get; }
```
#### AllPlayers

All players on server (including unauthenticated players)


##### Declaration

```cs
public IReadOnlyCollection<INetworkPlayer> AllPlayers { get; }
```
#### Players

##### Declaration

```cs
[Obsolete("Use AllPlayers or AuthenticatedPlayers instead")]
public IReadOnlyCollection<INetworkPlayer> Players { get; }
```
#### AuthenticatedPlayers

List of players that have Authenticated with server


##### Declaration

```cs
public IReadOnlyList<INetworkPlayer> AuthenticatedPlayers { get; }
```
#### Active

Checks if the server has been started.
This will be true after NetworkServer.Listen() has been called.


##### Declaration

```cs
public bool Active { get; }
```
#### World

##### Declaration

```cs
public NetworkWorld World { get; }
```
#### SyncVarSender

##### Declaration

```cs
public SyncVarSender SyncVarSender { get; }
```
#### MessageHandler

##### Declaration

```cs
public MessageHandler MessageHandler { get; }
```
### Methods
#### ErrorRateLimitReached(INetworkPlayer)



##### Declaration

```cs
public void ErrorRateLimitReached(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |


#### SetErrorRateLimitReachedCallback(NetworkServer.RateLimitCallback)



##### Declaration

```cs
public void SetErrorRateLimitReachedCallback(NetworkServer.RateLimitCallback callback)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkServer.RateLimitCallback | callback |  |


#### Stop()


This shuts down the server and disconnects all clients.
If In host mode, this will also stop the local client



##### Declaration

```cs
public void Stop()
```


#### StartServer(NetworkClient)


Start the server
If localClient is given then will start in host mode



##### Declaration

```cs
public void StartServer(NetworkClient localClient = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkClient | localClient | if not null then start the server and client in hostmode |


#### UpdateReceive()



##### Declaration

```cs
public void UpdateReceive()
```


#### UpdateSent()



##### Declaration

```cs
public void UpdateSent()
```


#### SetAuthenticationFailedCallback(NetworkServer.AuthFailCallback)



##### Declaration

```cs
public void SetAuthenticationFailedCallback(NetworkServer.AuthFailCallback callback)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkServer.AuthFailCallback | callback |  |


#### SendToAll&lt;T&gt;(T, Boolean, Channel)



##### Declaration

```cs
[Obsolete("Use SendToAll(msg, authenticatedOnly, excludeLocalPlayer, channelId) instead")]
public void SendToAll<T>(T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | msg |  |
| System.Boolean | excludeLocalPlayer |  |
| Mirage.Channel | channelId |  |


#### SendToAll&lt;T&gt;(T, Boolean, Boolean, Channel)



##### Declaration

```cs
public void SendToAll<T>(T msg, bool authenticatedOnly, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | msg |  |
| System.Boolean | authenticatedOnly |  |
| System.Boolean | excludeLocalPlayer |  |
| Mirage.Channel | channelId |  |


#### SendToMany&lt;T&gt;(IReadOnlyList&lt;INetworkPlayer&gt;, T, Boolean, Channel)



##### Declaration

```cs
public void SendToMany<T>(IReadOnlyList<INetworkPlayer> players, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IReadOnlyList&lt;Mirage.INetworkPlayer&gt; | players |  |
| T | msg |  |
| System.Boolean | excludeLocalPlayer |  |
| Mirage.Channel | channelId |  |


#### SendToMany&lt;T&gt;(IEnumerable&lt;INetworkPlayer&gt;, T, Boolean, Channel)


Warning: this will allocate, Use  or  instead



##### Declaration

```cs
public void SendToMany<T>(IEnumerable<INetworkPlayer> players, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;Mirage.INetworkPlayer&gt; | players |  |
| T | msg |  |
| System.Boolean | excludeLocalPlayer |  |
| Mirage.Channel | channelId |  |


#### SendToMany&lt;T, TEnumerator&gt;(TEnumerator, T, Boolean, Channel)


use to avoid allocation of IEnumerator



##### Declaration

```cs
public void SendToMany<T, TEnumerator>(TEnumerator playerEnumerator, T msg, bool excludeLocalPlayer, Channel channelId = Channel.Reliable)
    where TEnumerator : struct, IEnumerator<INetworkPlayer>
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| TEnumerator | playerEnumerator |  |
| T | msg |  |
| System.Boolean | excludeLocalPlayer |  |
| Mirage.Channel | channelId |  |


#### SendToObservers&lt;T&gt;(NetworkIdentity, T, Boolean, Boolean, Channel)



##### Declaration

```cs
public void SendToObservers<T>(NetworkIdentity identity, T msg, bool excludeLocalPlayer, bool excludeOwner, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| T | msg |  |
| System.Boolean | excludeLocalPlayer |  |
| System.Boolean | excludeOwner |  |
| Mirage.Channel | channelId |  |


#### SendToMany&lt;T&gt;(List&lt;INetworkPlayer&gt;, T, Channel)


Sends to list of players.
All other SendTo... functions call this, it dooes not do any extra checks, just serializes message if not empty, then sends it



##### Declaration

```cs
public static void SendToMany<T>(List<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.List&lt;Mirage.INetworkPlayer&gt; | players |  |
| T | msg |  |
| Mirage.Channel | channelId |  |


#### SendToMany&lt;T&gt;(IReadOnlyList&lt;INetworkPlayer&gt;, T, Channel)


Sends to list of players.
All other SendTo... functions call this, it dooes not do any extra checks, just serializes message if not empty, then sends it



##### Declaration

```cs
public static void SendToMany<T>(IReadOnlyList<INetworkPlayer> players, T msg, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IReadOnlyList&lt;Mirage.INetworkPlayer&gt; | players |  |
| T | msg |  |
| Mirage.Channel | channelId |  |


