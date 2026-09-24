---
id: IAuthenticationDataWrapper
title: IAuthenticationDataWrapper
---

# Interface IAuthenticationDataWrapper


Auth data might be a wrapper around another Authenticator&apos;s data.
In that case  should check if data is T or if it is IDataWrapper




##### Syntax

```cs
public interface IAuthenticationDataWrapper
```


### Properties

#### Inner

##### Declaration

```cs
object Inner { get; }
```
