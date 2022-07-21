---
sidebar_position: 5
---
# Clock Synchronization

For many features, you need the clock to be synchronized between the client and the server. Mirage does that automatically for you.

To get the current time use this code:
```cs
double now = NetworkTime.Time;
```

It will return the same value on the client and the server. It starts at 0 when the server starts. 

:::note
The time is a double and should never be cast to a float. Casting this down to a float means the clock will lose precision after some time:

-   after 1 day, the accuracy goes down to 8 ms
-   after 10 days, the accuracy is 62 ms
-   after 30 days, the accuracy is 250 m
-   after 60 days, the accuracy is 500 ms
:::

Mirage will also calculate the **Return Trip Time** as seen by the application:

```cs
double rtt = NetworkTime.Rtt;
```

:::note
Return RTT will also be affected by the frame rate. A higher frame rate will mean less delay before the server reads the ping message and replies. 
:::

You can check the precision using:

```cs
double timeStandardDeviation = NetworkTime.TimeSd;
```

For example, if this returns 0.2, it means the time measurements swing up and down roughly 0.2 seconds.

Network time is smoothing out the values using [Exponential moving average](https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average). 
You can configure how often you want the client to send pings using:

```cs
NetworkTime.PingInterval = 2.0f;
```

You can configure how quickly results will change using:

```cs
NetworkTime.PingWindowSize = 10;
```

A higher number will result in smoother results, but a longer time to adjust to changes.
