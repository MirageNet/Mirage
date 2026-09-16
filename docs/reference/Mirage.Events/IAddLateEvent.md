---
id: IAddLateEvent
title: IAddLateEvent
---

# Interface IAddLateEvent


Event that can only run once, adding handler late will it invoke right away




##### Syntax

```cs
public interface IAddLateEvent
```

### Methods
#### AddListener(Action)



##### Declaration

```cs
void AddListener(Action handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action | handler |  |


#### RemoveListener(Action)



##### Declaration

```cs
void RemoveListener(Action handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action | handler |  |


