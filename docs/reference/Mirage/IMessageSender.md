---
id: IMessageSender
title: IMessageSender
---

# Interface IMessageSender


An object that can send messages




##### Syntax

```cs
public interface IMessageSender
```

### Methods
#### Send&lt;T&gt;(T, Channel)



##### Declaration

```cs
void Send<T>(T message, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.Channel | channelId |  |


#### Send(ArraySegment&lt;Byte&gt;, Channel)



##### Declaration

```cs
void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | segment |  |
| Mirage.Channel | channelId |  |


#### Send&lt;T&gt;(T, INotifyCallBack)



##### Declaration

```cs
void Send<T>(T message, INotifyCallBack notifyCallBack)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.SocketLayer.INotifyCallBack | notifyCallBack |  |


