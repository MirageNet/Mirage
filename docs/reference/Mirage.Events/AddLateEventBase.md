---
id: AddLateEventBase
title: AddLateEventBase
---

# Class AddLateEventBase



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
public abstract class AddLateEventBase
```


### Fields

#### HasInvoked

##### Declaration

```cs
protected bool HasInvoked
```
### Methods
#### MarkInvoked()



##### Declaration

```cs
protected void MarkInvoked()
```


#### Reset()


Resets invoked flag, meaning new handles wont be invoked untill invoke is called again
Reset does not remove listeners



##### Declaration

```cs
public void Reset()
```


