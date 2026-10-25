---
id: NetworkBehaviorSerializers
title: NetworkBehaviorSerializers
---

# Class NetworkBehaviorSerializers



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
public static class NetworkBehaviorSerializers
```

### Methods
#### WriteNetworkBehaviorSyncVar(NetworkWriter, NetworkBehaviorSyncvar)



##### Declaration

```cs
public static void WriteNetworkBehaviorSyncVar(this NetworkWriter writer, NetworkBehaviorSyncvar id)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.NetworkBehaviorSyncvar | id |  |


#### ReadNetworkBehaviourSyncVar(NetworkReader)



##### Declaration

```cs
public static NetworkBehaviorSyncvar ReadNetworkBehaviourSyncVar(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkBehaviorSyncvar |  |

#### WriteGenericNetworkBehaviorSyncVar&lt;T&gt;(NetworkWriter, NetworkBehaviorSyncvar&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteGenericNetworkBehaviorSyncVar<T>(this NetworkWriter writer, NetworkBehaviorSyncvar<T> id)
    where T : NetworkBehaviour
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.NetworkBehaviorSyncvar&lt;T&gt; | id |  |


#### ReadGenericNetworkBehaviourSyncVar&lt;T&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static NetworkBehaviorSyncvar<T> ReadGenericNetworkBehaviourSyncVar<T>(this NetworkReader reader)
    where T : NetworkBehaviour
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkBehaviorSyncvar&lt;T&gt; |  |

