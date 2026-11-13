---
id: AddLateEventUnity-2
title: AddLateEventUnity<T0, TEvent>
---

# Class AddLateEventUnity&lt;T0, TEvent&gt;


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
<div class="level" style={{"--data-index": 2}}>
Mirage.Events.AddLateEvent&lt;T0&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Events.AddLateEvent&lt;T0&gt;._arg0


Mirage.Events.AddLateEvent&lt;T0&gt;.AddListener(System.Action&lt;T0&gt;)


Mirage.Events.AddLateEvent&lt;T0&gt;.RemoveListener(System.Action&lt;T0&gt;)


Mirage.Events.AddLateEvent&lt;T0&gt;.OnDestroyCleanup()


Mirage.Events.AddLateEventBase.HasInvoked


Mirage.Events.AddLateEventBase.MarkInvoked()


Mirage.Events.AddLateEventBase.Reset()

</details>

##### Syntax

```cs
[Serializable]
public abstract class AddLateEventUnity<T0, TEvent> : AddLateEvent<T0>, IAddLateEventUnity<T0>, IAddLateEvent<T0> where TEvent : UnityEvent<T0>, new()
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 | argument 0 |
| TEvent | UnityEvent |

### Methods
#### AddListener(UnityAction&lt;T0&gt;)



##### Declaration

```cs
public void AddListener(UnityAction<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0&gt; | handler |  |


#### RemoveListener(UnityAction&lt;T0&gt;)



##### Declaration

```cs
public void RemoveListener(UnityAction<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0&gt; | handler |  |


#### Invoke(T0)



##### Declaration

```cs
public override void Invoke(T0 arg0)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T0 | arg0 |  |


#### RemoveAllListeners()


Remove all non-persisent (ie created from script) listeners from the event.



##### Declaration

```cs
public void RemoveAllListeners()
```


