---
id: IAddLateEventUnity
title: IAddLateEventUnity
---

# Interface IAddLateEventUnity


Event that can only run once, adding handler late will it invoke right away




##### Syntax

```cs
public interface IAddLateEventUnity : IAddLateEvent
```

### Methods
#### AddListener(UnityAction)



##### Declaration

```cs
void AddListener(UnityAction handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction | handler |  |


#### RemoveListener(UnityAction)



##### Declaration

```cs
void RemoveListener(UnityAction handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction | handler |  |


