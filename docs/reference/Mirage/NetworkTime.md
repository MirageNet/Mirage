---
id: NetworkTime
title: NetworkTime
---

# Class NetworkTime


Synchronize time between the server and the clients



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
public class NetworkTime
```

### Constructors

#### NetworkTime()



##### Declaration

```cs
public NetworkTime()
```

### Fields

#### PingInterval

how often are we sending ping messages
used to calculate network time and RTT


##### Declaration

```cs
public float PingInterval
```
#### PingWindowSize

average out the last few results from Ping


##### Declaration

```cs
public int PingWindowSize
```

### Properties

#### Time

The time in seconds since the server started.


##### Declaration

```cs
public double Time { get; }
```
#### TimeVar

Measurement of the variance of time.
The higher the variance, the less accurate the time is


##### Declaration

```cs
public double TimeVar { get; }
```
#### TimeSd

standard deviation of time.
The higher the variance, the less accurate the time is


##### Declaration

```cs
public double TimeSd { get; }
```
#### Offset

Clock difference in seconds between the client and the server


##### Declaration

```cs
public double Offset { get; }
```
#### Rtt

how long in seconds does it take for a message to go
to the server and come back


##### Declaration

```cs
public double Rtt { get; }
```
#### RttVar

measure variance of rtt
the higher the number,  the less accurate rtt is


##### Declaration

```cs
public double RttVar { get; }
```
#### RttSd

Measure the standard deviation of rtt
the higher the number,  the less accurate rtt is


##### Declaration

```cs
public double RttSd { get; }
```
### Methods
#### Reset()



##### Declaration

```cs
public void Reset()
```


#### PingNow(IMessageSender)


Sends  right away ignoring lastPingTime



##### Declaration

```cs
public void PingNow(IMessageSender client)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageSender | client |  |


