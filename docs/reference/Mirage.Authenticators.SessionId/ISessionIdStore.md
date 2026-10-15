---
id: ISessionIdStore
title: ISessionIdStore
---

# Interface ISessionIdStore




##### Syntax

```cs
public interface ISessionIdStore
```

### Methods
#### TryGetSession(out ClientSession)



##### Declaration

```cs
bool TryGetSession(out ClientSession session)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authenticators.SessionId.ClientSession | session |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### StoreSession(ClientSession)



##### Declaration

```cs
void StoreSession(ClientSession session)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authenticators.SessionId.ClientSession | session |  |


