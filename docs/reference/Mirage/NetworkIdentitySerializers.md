---
id: NetworkIdentitySerializers
title: NetworkIdentitySerializers
---

# Class NetworkIdentitySerializers



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
public static class NetworkIdentitySerializers
```

### Methods
#### WriteNetworkIdentitySyncVar(NetworkWriter, NetworkIdentitySyncvar)



##### Declaration

```cs
public static void WriteNetworkIdentitySyncVar(this NetworkWriter writer, NetworkIdentitySyncvar id)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.NetworkIdentitySyncvar | id |  |


#### ReadNetworkIdentitySyncVar(NetworkReader)



##### Declaration

```cs
public static NetworkIdentitySyncvar ReadNetworkIdentitySyncVar(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.NetworkIdentitySyncvar |  |

