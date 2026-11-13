---
id: NetworkClient
title: NetworkClient
---

# Class NetworkClient


This is a network client class used by the networking system. It contains a NetworkConnection that is used to connect to a network server.
The  handle connection state, messages handlers, and connection configuration. There can be many  instances in a process at a time, but only one that is connected to a game server () that uses spawned objects.
 has an internal update function where it handles events from the transport layer. This includes asynchronous connect events, disconnect events and incoming data from a server.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class NetworkClient : MonoBehaviour, IMessageSender
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
#### SocketFactory

##### Declaration

```cs
public SocketFactory SocketFactory
```
#### ObjectManager

##### Declaration

```cs
public ClientObjectManager ObjectManager
```
#### DisconnectOnException

##### Declaration

```cs
public bool DisconnectOnException
```
#### RethrowException

##### Declaration

```cs
public bool RethrowException
```
#### RunInBackground

##### Declaration

```cs
public bool RunInBackground
```
#### Authenticator

##### Declaration

```cs
public AuthenticatorSettings Authenticator
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

Event fires when the client starts, before it has connected to the Server.


##### Declaration

```cs
public IAddLateEventUnity Started { get; }
```
#### Connected

Event fires once the Client has connected its Server.


##### Declaration

```cs
public IAddLateEventUnity<INetworkPlayer> Connected { get; }
```
#### Authenticated

Event fires after the Client connection has successfully been authenticated with its Server.


##### Declaration

```cs
public IAddLateEventUnity<INetworkPlayer> Authenticated { get; }
```
#### Disconnected

Event fires after the Client has disconnected from its Server and Cleanup has been called.


##### Declaration

```cs
public IAddLateEventUnity<ClientStoppedReason> Disconnected { get; }
```
#### Player

The NetworkConnection object this client is using.


##### Declaration

```cs
public INetworkPlayer Player { get; }
```
#### Active

active is true while a client is connecting/connected
(= while the network is active)


##### Declaration

```cs
public bool Active { get; }
```
#### IsConnected

This gives the current connection status of the client.


##### Declaration

```cs
public bool IsConnected { get; }
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
#### IsLocalClient

Is this NetworkClient connected to a local server in host mode


##### Declaration

```cs
[Obsolete("use IsHost instead")]
public bool IsLocalClient { get; }
```
#### IsHost

Is this NetworkClient connected to a local server in host mode


##### Declaration

```cs
public bool IsHost { get; }
```
### Methods
#### Connect(String, Nullable&lt;UInt16&gt;)


Connect client to a NetworkServer instance.



##### Declaration

```cs
public void Connect(string address = null, ushort? port = default(ushort? ))
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | address |  |
| System.Nullable&lt;System.UInt16&gt; | port |  |


#### Disconnect()


Disconnect from server.
The disconnect message will be invoked.



##### Declaration

```cs
public void Disconnect()
```


#### Send&lt;T&gt;(T, Channel)


This sends a network message with a message Id to the server. This message is sent on channel zero, which by default is the reliable channel.
The message must be an instance of a class derived from MessageBase.
The message id passed to Send() is used to identify the handler function to invoke on the server when the message is received.



##### Declaration

```cs
public void Send<T>(T message, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.Channel | channelId |  |


#### Send(ArraySegment&lt;Byte&gt;, Channel)



##### Declaration

```cs
public void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | segment |  |
| Mirage.Channel | channelId |  |


#### Send&lt;T&gt;(T, INotifyCallBack)



##### Declaration

```cs
public void Send<T>(T message, INotifyCallBack notifyCallBack)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.SocketLayer.INotifyCallBack | notifyCallBack |  |


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


