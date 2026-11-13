---
id: SyncSettings
title: SyncSettings
---

# Struct SyncSettings




##### Syntax

```cs
[Serializable]
public struct SyncSettings
```


### Fields

#### INTERVAL_TOOLTIP

##### Declaration

```cs
public const string INTERVAL_TOOLTIP = "Time in seconds until next change is synchronized to the client. '0' means send immediately if changed. '0.5' means only send changes every 500ms.\n(This is for state synchronization like SyncVars, SyncLists, OnSerialize. Not for Cmds, Rpcs, etc.)"
```
#### From

##### Declaration

```cs
public SyncFrom From
```
#### To

##### Declaration

```cs
public SyncTo To
```
#### Timing

##### Declaration

```cs
public SyncTiming Timing
```
#### Interval

##### Declaration

```cs
public float Interval
```
#### Default

##### Declaration

```cs
public static readonly SyncSettings Default
```
### Methods
#### UpdateTime(ref Double, Double)



##### Declaration

```cs
public void UpdateTime(ref double nextSyncTime, double now)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Double | nextSyncTime |  |
| System.Double | now |  |


#### UpdateTime(Single, SyncTiming, ref Double, Double)



##### Declaration

```cs
public static void UpdateTime(float interval, SyncTiming timing, ref double nextSyncTime, double now)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Single | interval |  |
| Mirage.SyncTiming | timing |  |
| System.Double | nextSyncTime |  |
| System.Double | now |  |


#### ShouldSyncFrom(NetworkIdentity, Boolean)



##### Declaration

```cs
public bool ShouldSyncFrom(NetworkIdentity identity, bool syncInHostMode)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |
| System.Boolean | syncInHostMode |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### ToObserverWriterOnly(NetworkIdentity)



##### Declaration

```cs
public bool ToObserverWriterOnly(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CopyToObservers(NetworkIdentity)



##### Declaration

```cs
public bool CopyToObservers(NetworkIdentity identity)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkIdentity | identity |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsValidDirection(SyncFrom, SyncTo)



##### Declaration

```cs
public static bool IsValidDirection(SyncFrom from, SyncTo to)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SyncFrom | from |  |
| Mirage.SyncTo | to |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### InvalidReason(SyncFrom, SyncTo)



##### Declaration

```cs
public static string InvalidReason(SyncFrom from, SyncTo to)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SyncFrom | from |  |
| Mirage.SyncTo | to |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

