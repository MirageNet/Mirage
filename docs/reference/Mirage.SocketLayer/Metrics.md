---
id: Metrics
title: Metrics
---

# Class Metrics



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
public class Metrics
```

### Constructors

#### Metrics(Int32)



##### Declaration

```cs
public Metrics(int bitSize = 10)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bitSize |  |

### Fields

#### Sequencer

##### Declaration

```cs
public readonly Sequencer Sequencer
```
#### buffer

##### Declaration

```cs
public readonly Metrics.Frame[] buffer
```
#### tick

##### Declaration

```cs
public uint tick
```
### Methods
#### OnTick(Int32)



##### Declaration

```cs
public void OnTick(int connectionCount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | connectionCount |  |


#### OnSend(Int32)



##### Declaration

```cs
public void OnSend(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnResend(Int32)



##### Declaration

```cs
public void OnResend(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceive(Int32)



##### Declaration

```cs
public void OnReceive(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceiveUnconnected(Int32)



##### Declaration

```cs
public void OnReceiveUnconnected(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnSendMessageUnreliable(Int32)



##### Declaration

```cs
public void OnSendMessageUnreliable(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceiveMessageUnreliable(Int32)



##### Declaration

```cs
public void OnReceiveMessageUnreliable(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnSendMessageReliable(Int32)



##### Declaration

```cs
public void OnSendMessageReliable(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceiveMessageReliable(Int32)



##### Declaration

```cs
public void OnReceiveMessageReliable(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnSendMessageNotify(Int32)



##### Declaration

```cs
public void OnSendMessageNotify(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceiveMessageNotify(Int32)



##### Declaration

```cs
public void OnReceiveMessageNotify(int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | length |  |


#### OnReceiveMessage(PacketType, Int32)



##### Declaration

```cs
public void OnReceiveMessage(PacketType packetType, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.PacketType | packetType |  |
| System.Int32 | length |  |


