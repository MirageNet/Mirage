---
id: IDataHandler
title: IDataHandler
---

# Interface IDataHandler


Handles data from SocketLayer
A high level script should implement this interface give it to Peer when it is created




##### Syntax

```cs
public interface IDataHandler
```

### Methods
#### ReceiveMessage(IConnection, ArraySegment&lt;Byte&gt;)


Receives a new Packet from low level

IMPORTANT: message is a shared/pooled buffer and is only valid during this call. 
Copy the data if it needs to be retained.




##### Declaration

```cs
void ReceiveMessage(IConnection connection, ArraySegment<byte> message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | connection | connection that sent data |
| System.ArraySegment&lt;System.Byte&gt; | message | Single message received by peer |


