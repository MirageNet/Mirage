---
id: AddLateEventUnity
title: AddLateEventUnity
---

# Class AddLateEventUnity


An event that will invoke handlers immediately if they are added after  has been called



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Events.AddLateEventBase
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Events.AddLateEvent
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Events.AddLateEvent.OnDestroyCleanup()


Mirage.Events.AddLateEventBase.HasInvoked


Mirage.Events.AddLateEventBase.MarkInvoked()


Mirage.Events.AddLateEventBase.Reset()

</details>

##### Syntax

```cs
[Serializable]
public sealed class AddLateEventUnity : AddLateEvent, IAddLateEventUnity, IAddLateEvent
```

### Methods
#### AddListener(UnityAction)



##### Declaration

```cs
public void AddListener(UnityAction handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction | handler |  |


#### RemoveListener(UnityAction)



##### Declaration

```cs
public void RemoveListener(UnityAction handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction | handler |  |


#### Invoke()



##### Declaration

```cs
public override void Invoke()
```


#### RemoveAllListeners()


Remove all non-persisent (ie created from script) listeners from the event.



##### Declaration

```cs
public void RemoveAllListeners()
```


