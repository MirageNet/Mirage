---
id: StringStore
title: StringStore
---

# Class StringStore



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
public class StringStore
```


### Fields

#### WriteLookup
Fast lookup to get index from an existing string

##### Declaration

```cs
public Dictionary<string, int> WriteLookup
```
#### Strings

##### Declaration

```cs
public List<string> Strings
```
### Methods
#### GetKey(String)



##### Declaration

```cs
public int GetKey(string value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### WriteString(NetworkWriter, String)



##### Declaration

```cs
public void WriteString(NetworkWriter writer, string value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.String | value |  |


#### ReadString(NetworkReader)



##### Declaration

```cs
public string ReadString(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

