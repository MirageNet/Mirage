---
id: IPeer
title: IPeer
---

# Interface IPeer




##### Syntax

```cs
public interface IPeer
```

### Methods
#### Bind(IBindEndPoint)



##### Declaration

```cs
void Bind(IBindEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IBindEndPoint | endPoint |  |


#### Connect(IConnectEndPoint)



##### Declaration

```cs
IConnection Connect(IConnectEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectEndPoint | endPoint |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnection |  |

#### Close()



##### Declaration

```cs
void Close()
```


#### UpdateReceive()


Call this at the start of the frame to receive new messages



##### Declaration

```cs
void UpdateReceive()
```


#### UpdateSent()


Call this at end of frame to send new batches



##### Declaration

```cs
void UpdateSent()
```


