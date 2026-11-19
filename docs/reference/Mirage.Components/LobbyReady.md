---
id: LobbyReady
title: LobbyReady
---

# Class LobbyReady



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class LobbyReady : MonoBehaviour
```


### Fields

#### Server

##### Declaration

```cs
public NetworkServer Server
```
#### Players

##### Declaration

```cs
public Dictionary<NetworkIdentity, ReadyCheck> Players
```
### Methods
#### SetAllClientsNotReady()



##### Declaration

```cs
public void SetAllClientsNotReady()
```


#### SendToReady&lt;T&gt;(T, Boolean, NetworkIdentity, Channel)


Send a message to players that are ready on check, or not ready if sendToReady fakse



##### Declaration

```cs
public void SendToReady<T>(T msg, bool sendToReady = true, NetworkIdentity exclude = null, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | msg |  |
| System.Boolean | sendToReady | Use to send message no not ready players instead, not this doesn&apos;t check server for players with out character, only players with PlayerReadyCheck on their character |
| Mirage.NetworkIdentity | exclude | Add Identity to exclude here, useful when you want to send to all players except the owner |
| Mirage.Channel | channelId |  |


