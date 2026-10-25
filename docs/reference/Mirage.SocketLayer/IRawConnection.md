---
id: IRawConnection
title: IRawConnection
---

# Interface IRawConnection


A connection that can send data directly to sockets
Only things inside socket layer should be sending raw packets. Others should use the methods inside 




##### Syntax

```cs
public interface IRawConnection
```

### Methods
#### SendRaw(Byte[], Int32)


Sends directly to socket without adding header
packet given to this function as assumed to already have a header



##### Declaration

```cs
void SendRaw(byte[] packet, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | packet | header and messages |
| System.Int32 | length |  |


