---
id: IAddLateEventUnity-1
title: IAddLateEventUnity<T0>
---

# Interface IAddLateEventUnity&lt;T0&gt;


Version of  with 1 argument




##### Syntax

```cs
public interface IAddLateEventUnity<T0> : IAddLateEvent<T0>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 |  |

### Methods
#### AddListener(UnityAction&lt;T0&gt;)



##### Declaration

```cs
void AddListener(UnityAction<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0&gt; | handler |  |


#### RemoveListener(UnityAction&lt;T0&gt;)



##### Declaration

```cs
void RemoveListener(UnityAction<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| UnityAction&lt;T0&gt; | handler |  |


