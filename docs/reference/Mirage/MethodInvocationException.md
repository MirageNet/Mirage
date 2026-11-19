---
id: MethodInvocationException
title: MethodInvocationException
---

# Class MethodInvocationException


Exception thrown if a guarded method is invoked incorrectly



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
public class MethodInvocationException : Exception, _Exception, ISerializable
```

### Constructors

#### MethodInvocationException()


Initializes a new instance of the  class



##### Declaration

```cs
public MethodInvocationException()
```

#### MethodInvocationException(String)


Initializes a new instance of the  class



##### Declaration

```cs
public MethodInvocationException(string message)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | message | A <xref href="System.String" data-throw-if-not-resolved="false"></xref> that describes the exception.  |

#### MethodInvocationException(SerializationInfo, StreamingContext)



##### Declaration

```cs
protected MethodInvocationException(SerializationInfo info, StreamingContext context)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Runtime.Serialization.SerializationInfo | info |  |
| System.Runtime.Serialization.StreamingContext | context |  |
