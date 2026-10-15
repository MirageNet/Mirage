---
id: RateLimitAttribute
title: RateLimitAttribute
---

# Class RateLimitAttribute



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
System.Attribute
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
[AttributeUsage(AttributeTargets.Method)]
public class RateLimitAttribute : Attribute, _Attribute
```


### Fields

#### DEFAULT_INTERVAL

##### Declaration

```cs
public const float DEFAULT_INTERVAL = 1F
```
#### DEFAULT_REFILL

##### Declaration

```cs
public const int DEFAULT_REFILL = 50
```
#### DEFAULT_MAX_TOKENS

##### Declaration

```cs
public const int DEFAULT_MAX_TOKENS = 200
```
#### DEFAULT_PENALTY

##### Declaration

```cs
public const int DEFAULT_PENALTY = 1
```
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
Max number of tokens in bucket. set this number higher than per seconds value to allow bursts of usage

##### Declaration

```cs
public int MaxTokens
```
#### Penalty

Amount of error cost added to NetworkPlayer&apos;s ErrorRateLimit when the player exceeds the allowed rate limit interval.


##### Declaration

```cs
public int Penalty
```
