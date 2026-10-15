---
id: HeadlessFrameLimiter
title: HeadlessFrameLimiter
---

# Class HeadlessFrameLimiter



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public class HeadlessFrameLimiter : MonoBehaviour
```


### Fields

#### serverTickRate

Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.


##### Declaration

```cs
public int serverTickRate
```
### Methods
#### Start()


Set the frame rate for a headless server.



##### Declaration

```cs
public void Start()
```


