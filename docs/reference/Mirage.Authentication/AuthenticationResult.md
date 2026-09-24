---
id: AuthenticationResult
title: AuthenticationResult
---

# Struct AuthenticationResult


Result from Authentication, Use static methods to create new instance




##### Syntax

```cs
public struct AuthenticationResult
```


### Properties

#### Success

##### Declaration

```cs
public bool Success { get; }
```
#### Authenticator

Which Authenticator gave success 


##### Declaration

```cs
public INetworkAuthenticator Authenticator { get; }
```
#### Data

Auth data from Success, will be set on INetworkPlayer


##### Declaration

```cs
public object Data { get; }
```
#### Reason

Can be reason for Success of fail


##### Declaration

```cs
public string Reason { get; }
```
### Methods
#### CreateSuccess(String)



##### Declaration

```cs
public static AuthenticationResult CreateSuccess(string reason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | reason |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### CreateSuccess(INetworkAuthenticator, Object)



##### Declaration

```cs
public static AuthenticationResult CreateSuccess(INetworkAuthenticator authenticator, object data)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Authentication.INetworkAuthenticator | authenticator |  |
| System.Object | data |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### CreateSuccess(String, INetworkAuthenticator, Object)



##### Declaration

```cs
public static AuthenticationResult CreateSuccess(string reason, INetworkAuthenticator authenticator, object data)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | reason |  |
| Mirage.Authentication.INetworkAuthenticator | authenticator |  |
| System.Object | data |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### CreateFail(String)



##### Declaration

```cs
public static AuthenticationResult CreateFail(string reason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | reason |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

#### CreateFail(String, INetworkAuthenticator)



##### Declaration

```cs
public static AuthenticationResult CreateFail(string reason, INetworkAuthenticator authenticator)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | reason |  |
| Mirage.Authentication.INetworkAuthenticator | authenticator |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Authentication.AuthenticationResult |  |

