---
id: SyncTiming
title: SyncTiming
---

# Enum SyncTiming




##### Syntax

```cs
public enum SyncTiming : byte
```


### Fields

#### Variable

Will wait for atleast  after last sync before sending again.

Best used when values dont change often, or for non-time-critical data.


Will send less often than  for the same .



##### Declaration

```cs
Variable = 0
```
#### Fixed

Will ensure data is sent every  if changed.

Best used for data that changes often and you want (1/) updates per second



##### Declaration

```cs
Fixed = 1
```
#### NoInterval

Ignores Interval and will send changes in next update


##### Declaration

```cs
NoInterval = 2
```
