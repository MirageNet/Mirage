---
id: AddLateEvent
title: AddLateEvent
---

# Class AddLateEvent


An event that will invoke handlers immediately if they are added after  has been called



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
public class AddLateEvent : AddLateEventBase, IAddLateEvent
```

### Methods
#### AddListener(Action)



##### Declaration

```cs
public void AddListener(Action handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action | handler |  |


#### RemoveListener(Action)



##### Declaration

```cs
public void RemoveListener(Action handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action | handler |  |


#### Invoke()



##### Declaration

```cs
public virtual void Invoke()
```


#### OnDestroyCleanup()


Clears listeners, should be called from OnDestroy



##### Declaration

```cs
public void OnDestroyCleanup()
```


