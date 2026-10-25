---
id: Config
title: Config
---

# Class Config



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public class Config
```


### Fields

#### MaxConnections

Max concurrent connections server will accept


##### Declaration

```cs
public int MaxConnections
```
#### SendRejectIfUnconnectedPacketIsInvalid

Send reject if a new (unconnected) endPoint sends an invalid packet. If this is false then invalid packets will be ignored
Setting this to true is useful if the EndPoint/Socket has a stateful connection and can listen for reject to close the connection


##### Declaration

```cs
public bool SendRejectIfUnconnectedPacketIsInvalid
```
#### ConnectAttemptInterval

How often connect attempt message will be re-sent if server does not reply


##### Declaration

```cs
public float ConnectAttemptInterval
```
#### MaxConnectAttempts

How many times attempt to connect before giving up


##### Declaration

```cs
public int MaxConnectAttempts
```
#### KeepAliveInterval

how long after previous send before sending keep alive message
Keep alive is to stop connection from timing out
keep alive is sent over unreliable so this interval should be low enough so that  does not timeout if some unreliable packets are missed 


##### Declaration

```cs
public float KeepAliveInterval
```
#### TimeoutDuration

how long without a message before disconnecting connection


##### Declaration

```cs
public float TimeoutDuration
```
#### key

Key sent with connection message (defaults to Major version of assmebly)
Used to validate that server and client are same application/version
NOTE: key will be ASCII encoded


##### Declaration

```cs
public string key
```
#### DisconnectDuration

How long after disconnect before connection is fully removed from Peer


##### Declaration

```cs
public float DisconnectDuration
```
#### BufferPoolStartSize

How many buffers to create at start


##### Declaration

```cs
public int BufferPoolStartSize
```
#### BufferPoolMaxSize

max number of buffers allowed to be stored in pool
buffers over this limit will be left for GC


##### Declaration

```cs
public int BufferPoolMaxSize
```
#### TimeBeforeEmptyAck

how long after last send to send ack without a message


##### Declaration

```cs
public float TimeBeforeEmptyAck
```
#### ReceivesBeforeEmptyAck

How many receives before sending an empty ack
this is so that acks are still sent even if receives many message before replying


##### Declaration

```cs
public int ReceivesBeforeEmptyAck
```
#### EmptyAckLimit

How many empty acks to send via 
Send enough acks that there is a high chances that 1 of them reaches other size
Empty Ack count resets after receives new message


##### Declaration

```cs
public int EmptyAckLimit
```
#### MaxReliablePacketsInSendBufferPerConnection

How many packets can exist it ring buffers for Ack and Reliable system
This value wont count null packets so can be set lower than &apos;s value to limit actual number of packets waiting to be acked
Example: (max=2000) * (MTU=1200) * (connections=100) => 240MB


##### Declaration

```cs
public int MaxReliablePacketsInSendBufferPerConnection
```
#### SequenceSize

Bit size of sequence used for AckSystem
this value also determines the size of ring buffers for Ack and Reliable system
Max of 16


##### Declaration

```cs
public int SequenceSize
```
#### MaxReliableFragments

How many fragments large reliable message can be split into
if set to 0 then messages over  will not be allowed to be sent
max value is 255


##### Declaration

```cs
public int MaxReliableFragments
```
#### DisableReliableLayer

Enable if the Socket you are using has its own Reliable layer. For example using Websocket, which is TCP.


##### Declaration

```cs
public bool DisableReliableLayer
```
### Methods
#### Create(PeerConfigProfile)



##### Declaration

```cs
public static Config Create(PeerConfigProfile profile)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.PeerConfigProfile | profile |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.Config |  |

#### RawUdp()



##### Declaration

```cs
public static Config RawUdp()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.Config |  |

#### WebSocket()



##### Declaration

```cs
public static Config WebSocket()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.Config |  |

#### StatefulUdp()



##### Declaration

```cs
public static Config StatefulUdp()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.Config |  |

