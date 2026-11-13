---
id: IAddLateEventUnity-2
title: IAddLateEventUnity<T0, T1>
---

# Interface IAddLateEventUnity&lt;T0, T1&gt;


Version of  with 2 arguments




##### Syntax

```cs
public interface IAddLateEventUnity<T0, T1> : IAddLateEvent<T0, T1>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 |  |
| T1 |  |

### Methods
#### AddListener(UnityAction&lt;T0, T1&gt;)



##### Declaration

```cs
void AddListener(UnityAction<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0, T1&gt; | handler |  |


#### RemoveListener(UnityAction&lt;T0, T1&gt;)



##### Declaration

```cs
void RemoveListener(UnityAction<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0, T1&gt; | handler |  |


