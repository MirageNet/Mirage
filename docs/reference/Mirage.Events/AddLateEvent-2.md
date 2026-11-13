---
id: AddLateEvent-2
title: AddLateEvent<T0, T1>
---

# Class AddLateEvent&lt;T0, T1&gt;


Version of  with 2 arguments
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
public class AddLateEvent<T0, T1> : AddLateEventBase, IAddLateEvent<T0, T1>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 |  |
| T1 |  |


### Fields

#### _arg0

##### Declaration

```cs
protected T0 _arg0
```
#### _arg1

##### Declaration

```cs
protected T1 _arg1
```
### Methods
#### AddListener(Action&lt;T0, T1&gt;)



##### Declaration

```cs
public void AddListener(Action<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0, T1&gt; | handler |  |


#### RemoveListener(Action&lt;T0, T1&gt;)



##### Declaration

```cs
public void RemoveListener(Action<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0, T1&gt; | handler |  |


#### Invoke(T0, T1)



##### Declaration

```cs
public virtual void Invoke(T0 arg0, T1 arg1)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T0 | arg0 |  |
| T1 | arg1 |  |


#### OnDestroyCleanup()


Clears listeners, should be called from OnDestroy



##### Declaration

```cs
public void OnDestroyCleanup()
```


