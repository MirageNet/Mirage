---
id: IConnectionHandle
title: IConnectionHandle
---

# Interface IConnectionHandle


Object that can be used as an endPoint or handle for  and 

Implementation of this should override  and  so that 2 instance wil be equal if they have the same address internally


When a new connection is received by Peer a copy of this endPoint will be created and given to that connection.
On future received the incoming endPoint will be compared to active connections inside a dictionary





##### Syntax

```cs
public interface IConnectionHandle
```


### Properties

#### IsStateful

##### Declaration

```cs
bool IsStateful { get; }
```
#### SocketLayerConnection

Used by stateful connections, stores a direct reference to avoid lookup


##### Declaration

```cs
ISocketLayerConnection SocketLayerConnection { get; set; }
```
#### SupportsGracefulDisconnect

Can gracefulDisconnectReason be used with  or should  handle Linger and disconnect itself


##### Declaration

```cs
bool SupportsGracefulDisconnect { get; }
```
### Methods
#### Disconnect(String)


disconnect for stateful connections. Should be made safe to call multiple times



##### Declaration

```cs
void Disconnect(string gracefulDisconnectReason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | gracefulDisconnectReason |  |


#### CreateCopy()


Used by stateless connection, copy will be stored and used later to sending messages
Creates a new instance of  with same connection data.
this is called when a new connection is created by 



##### Declaration

```cs
IConnectionHandle CreateCopy()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle |  |

#### GetHashCode()



##### Declaration

```cs
int GetHashCode()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### Equals(Object)



##### Declaration

```cs
bool Equals(object obj)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Object | obj |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

