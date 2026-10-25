---
id: RateLimitBucket.RefillConfig
title: RateLimitBucket.RefillConfig
---

# Struct RateLimitBucket.RefillConfig




##### Syntax

```cs
[Serializable]
public struct RefillConfig
```


### Fields

#### Interval
Seconds

##### Declaration

```cs
public float Interval
```
#### Refill
How many tokens refilled each interval

##### Declaration

```cs
public int Refill
```
#### MaxTokens
Max number of tokens in bucket. Set this higher than &apos;Refill&apos; to allow for bursts of usage.

##### Declaration

```cs
public int MaxTokens
```
