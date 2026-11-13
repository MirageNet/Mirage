---
id: DeserializeFailedException
title: DeserializeFailedException
---

# Class DeserializeFailedException



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
System.Exception
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
[Serializable]
public class DeserializeFailedException : Exception, _Exception, ISerializable
```

### Constructors

#### DeserializeFailedException(String)



##### Declaration

```cs
public DeserializeFailedException(string message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | message |  |

#### DeserializeFailedException(SerializationInfo, StreamingContext)



##### Declaration

```cs
protected DeserializeFailedException(SerializationInfo info, StreamingContext context)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Runtime.Serialization.SerializationInfo | info |  |
| System.Runtime.Serialization.StreamingContext | context |  |
