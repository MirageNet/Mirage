---
id: IAddLateEvent-2
title: IAddLateEvent<T0, T1>
---

# Interface IAddLateEvent&lt;T0, T1&gt;


Version of  with 2 arguments




##### Syntax

```cs
public interface IAddLateEvent<T0, T1>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 |  |
| T1 |  |

### Methods
#### AddListener(Action&lt;T0, T1&gt;)



##### Declaration

```cs
void AddListener(Action<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0, T1&gt; | handler |  |


#### RemoveListener(Action&lt;T0, T1&gt;)



##### Declaration

```cs
void RemoveListener(Action<T0, T1> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0, T1&gt; | handler |  |


