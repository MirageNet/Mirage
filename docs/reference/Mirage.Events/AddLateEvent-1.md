---
id: AddLateEvent-1
title: AddLateEvent<T0>
---

# Class AddLateEvent&lt;T0&gt;


Version of  with 1 argument
Create a non-generic class inheriting from this to use in inspector. Same rules as 



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Events.AddLateEventBase
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Events.AddLateEventBase.HasInvoked


Mirage.Events.AddLateEventBase.MarkInvoked()


Mirage.Events.AddLateEventBase.Reset()

</details>

##### Syntax

```cs
[Serializable]
public class AddLateEvent<T0> : AddLateEventBase, IAddLateEvent<T0>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 | argument 0 |


### Fields

#### _arg0

##### Declaration

```cs
protected T0 _arg0
```
### Methods
#### AddListener(Action&lt;T0&gt;)



##### Declaration

```cs
public void AddListener(Action<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0&gt; | handler |  |


#### RemoveListener(Action&lt;T0&gt;)



##### Declaration

```cs
public void RemoveListener(Action<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0&gt; | handler |  |


#### Invoke(T0)



##### Declaration

```cs
public virtual void Invoke(T0 arg0)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T0 | arg0 |  |


#### OnDestroyCleanup()


Clears listeners, should be called from OnDestroy



##### Declaration

```cs
public void OnDestroyCleanup()
```


