---
id: RateLimitBucket
title: RateLimitBucket
---

# Class RateLimitBucket



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public class RateLimitBucket
```

### Constructors

#### RateLimitBucket(Double, RateLimitBucket.RefillConfig)



##### Declaration

```cs
public RateLimitBucket(double now, RateLimitBucket.RefillConfig config)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | now |  |
| Mirage.SocketLayer.RateLimitBucket.RefillConfig | config |  |

### Fields

#### Config

##### Declaration

```cs
public readonly RateLimitBucket.RefillConfig Config
```

### Properties

#### Tokens

##### Declaration

```cs
public float Tokens { get; }
```
### Methods
#### UseTokens(Double, Int32)


Refills the bucket based on time passed and then consumes tokens.
Useful for rarely used buckets that need to be up-to-date at the moment of a call.



##### Declaration

```cs
public bool UseTokens(double now, int amount)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | now |  |
| System.Int32 | amount |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CheckRefill(Double)


Refills tokens based on config. If the bucket is full, it will reset the refill timer to &apos;now&apos;.



##### Declaration

```cs
public void CheckRefill(double now)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | now | Current time in seconds |


#### UseTokens(Int32)


Subtracts cost from token count. 
Tokens can go negative to penalize burst spam.



##### Declaration

```cs
public bool UseTokens(int cost)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | cost |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | True if the bucket is empty (negative), indicating the rate limit was exceeded. |

#### IsEmpty()


Returns true if tokens is negative (indicating we ran out and are currently in debt).



##### Declaration

```cs
public bool IsEmpty()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

