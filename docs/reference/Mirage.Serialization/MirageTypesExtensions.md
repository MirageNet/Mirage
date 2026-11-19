---
id: MirageTypesExtensions
title: MirageTypesExtensions
---

# Class MirageTypesExtensions



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
public static class MirageTypesExtensions
```

### Methods
#### WriteNetworkIdentity(NetworkWriter, NetworkIdentity)



##### Declaration

```cs
public static void WriteNetworkIdentity(this NetworkWriter writer, NetworkIdentity value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.NetworkIdentity | value |  |


#### WriteNetworkBehaviour(NetworkWriter, NetworkBehaviour)



##### Declaration

```cs
public static void WriteNetworkBehaviour(this NetworkWriter writer, NetworkBehaviour value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.NetworkBehaviour | value |  |


#### WriteGameObject(NetworkWriter, GameObject)



##### Declaration

```cs
public static void WriteGameObject(this NetworkWriter writer, GameObject value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| GameObject | value |  |


#### ToMirageReader(NetworkReader)


Casts reader to , throw if cast is invalid



##### Declaration

```cs
public static MirageNetworkReader ToMirageReader(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.MirageNetworkReader |  |

#### ReadNetworkIdentity(NetworkReader)



##### Declaration

```cs
public static NetworkIdentity ReadNetworkIdentity(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentity |  |

#### ReadNetworkBehaviour(NetworkReader)



##### Declaration

```cs
public static NetworkBehaviour ReadNetworkBehaviour(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkBehaviour |  |

#### ReadNetworkBehaviour&lt;T&gt;(NetworkReader)



##### Declaration

```cs
public static T ReadNetworkBehaviour<T>(this NetworkReader reader)
    where T : NetworkBehaviour
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### ReadGameObject(NetworkReader)



##### Declaration

```cs
public static GameObject ReadGameObject(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| GameObject |  |

