---
id: RpcRateLimitConfig
title: RpcRateLimitConfig
---

# Struct RpcRateLimitConfig


Rate limit configuration for an RPC method.
Use  for no rate limiting, or  to configure a token bucket.




##### Syntax

```cs
public struct RpcRateLimitConfig
```


### Fields

#### IsEnabled

##### Declaration

```cs
public readonly bool IsEnabled
```
#### BucketConfig

##### Declaration

```cs
public readonly RateLimitBucket.RefillConfig BucketConfig
```
#### Penalty

##### Declaration

```cs
public readonly int Penalty
```
### Methods
#### Disabled()


No rate limiting for this RPC



##### Declaration

```cs
public static RpcRateLimitConfig Disabled()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.RemoteCalls.RpcRateLimitConfig |  |

#### Enabled(Single, Int32, Int32, Int32)


Enables rate limiting with the specified token bucket configuration



##### Declaration

```cs
public static RpcRateLimitConfig Enabled(float interval, int refill, int maxTokens, int penalty)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | interval | Seconds between refills |
| System.Int32 | refill | How many tokens refilled each interval |
| System.Int32 | maxTokens | Max number of tokens in bucket |
| System.Int32 | penalty | Error cost applied to NetworkPlayer&apos;s ErrorRateLimit when limit is exceeded |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.RemoteCalls.RpcRateLimitConfig |  |

