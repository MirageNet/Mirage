---
id: StringStoreExtensions
title: StringStoreExtensions
---

# Class StringStoreExtensions

Default write/read methods for , using 


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
public static class StringStoreExtensions
```

### Methods
#### WriteStringStore(NetworkWriter, StringStore)



##### Declaration

```cs
public static void WriteStringStore(this NetworkWriter writer, StringStore store)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Serialization.StringStore | store |  |


#### ReadStringStore(NetworkReader)

Default read method for , using 


##### Declaration

```cs
public static StringStore ReadStringStore(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.StringStore |  |

