---
id: AckSystem.ReliableReceived
title: AckSystem.ReliableReceived
---

# Struct AckSystem.ReliableReceived




##### Syntax

```cs
public struct ReliableReceived : IEquatable<AckSystem.ReliableReceived>
```

### Constructors

#### ReliableReceived(ByteBuffer, Int32, Boolean)



##### Declaration

```cs
public ReliableReceived(ByteBuffer buffer, int length, bool isFragment)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.ByteBuffer | buffer |  |
| System.Int32 | length |  |
| System.Boolean | isFragment |  |

### Fields

#### Buffer

##### Declaration

```cs
public readonly ByteBuffer Buffer
```
#### Length

##### Declaration

```cs
public readonly int Length
```
#### IsFragment

##### Declaration

```cs
public readonly bool IsFragment
```

### Properties

#### FragmentIndex

##### Declaration

```cs
public int FragmentIndex { get; }
```
### Methods
#### Equals(AckSystem.ReliableReceived)



##### Declaration

```cs
public bool Equals(AckSystem.ReliableReceived other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.AckSystem.ReliableReceived | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

