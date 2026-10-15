---
id: MessagePacker
title: MessagePacker
---

# Class MessagePacker



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
public static class MessagePacker
```


### Fields

#### ID_BYTE_SIZE

##### Declaration

```cs
public const int ID_BYTE_SIZE = 2
```

### Properties

#### MessageTypes

Map of Message Id => Type
When we receive a message, we can lookup here to find out what type it was.
This is populated by the weaver.


##### Declaration

```cs
public static IReadOnlyDictionary<int, Type> MessageTypes { get; }
```
### Methods
#### RegisterMessage&lt;T&gt;()


Registers a message with its ID, Useful for debugging if a message handler is missing
Used by weaver



##### Declaration

```cs
public static void RegisterMessage<T>()
```


#### GetId&lt;T&gt;()


Gets the Id from  for T



##### Declaration

```cs
public static int GetId<T>()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### GetId(Type)


Used to calculate new hash for type



##### Declaration

```cs
public static int GetId(Type type)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Type | type |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### Pack&lt;T&gt;(T, NetworkWriter)



##### Declaration

```cs
public static void Pack<T>(T message, NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |
| Mirage.Serialization.NetworkWriter | writer |  |


#### Pack&lt;T&gt;(T)



##### Declaration

```cs
public static byte[] Pack<T>(T message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | message |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### Unpack&lt;T&gt;(Byte[], IObjectLocator)


unpack a message we received



##### Declaration

```cs
public static T Unpack<T>(byte[] data, IObjectLocator objectLocator)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | data |  |
| Mirage.IObjectLocator | objectLocator | Can be null, but must be set in order to read NetworkIdentity Values |

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### UnpackId(NetworkReader)



##### Declaration

```cs
public static int UnpackId(NetworkReader messageReader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | messageReader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

