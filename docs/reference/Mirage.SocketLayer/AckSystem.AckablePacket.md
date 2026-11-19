---
id: AckSystem.AckablePacket
title: AckSystem.AckablePacket
---

# Struct AckSystem.AckablePacket




##### Syntax

```cs
public struct AckablePacket : IEquatable<AckSystem.AckablePacket>
```

### Constructors

#### AckablePacket(INotifyCallBack)



##### Declaration

```cs
public AckablePacket(INotifyCallBack token)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.INotifyCallBack | token |  |

#### AckablePacket(AckSystem.ReliablePacket)



##### Declaration

```cs
public AckablePacket(AckSystem.ReliablePacket reliablePacket)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.AckSystem.ReliablePacket | reliablePacket |  |

### Fields

#### Token

##### Declaration

```cs
public INotifyCallBack Token
```
#### ReliablePacket

##### Declaration

```cs
public AckSystem.ReliablePacket ReliablePacket
```

### Properties

#### IsNotify

##### Declaration

```cs
public bool IsNotify { get; }
```
#### IsReliable

##### Declaration

```cs
public bool IsReliable { get; }
```
### Methods
#### Equals(AckSystem.AckablePacket)



##### Declaration

```cs
public bool Equals(AckSystem.AckablePacket other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.AckSystem.AckablePacket | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsValid()



##### Declaration

```cs
public bool IsValid()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

