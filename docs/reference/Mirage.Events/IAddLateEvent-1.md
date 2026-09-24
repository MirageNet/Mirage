---
id: IAddLateEvent-1
title: IAddLateEvent<T0>
---

# Interface IAddLateEvent&lt;T0&gt;


Version of  with 1 argument




##### Syntax

```cs
public interface IAddLateEvent<T0>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T0 |  |

### Methods
#### AddListener(Action&lt;T0&gt;)



##### Declaration

```cs
void AddListener(Action<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0&gt; | handler |  |


#### RemoveListener(Action&lt;T0&gt;)



##### Declaration

```cs
void RemoveListener(Action<T0> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T0&gt; | handler |  |


