# Clock Synchronization

For many features you need the clock to be synchronized between the client and
the server. Mirage does that automatically for you.

To get the current time use this code:

```cs
double now = NetworkTime.Time;
```

It will return the same value in the client and the servers. It starts at 0 when
the server starts. 

>[!NOTE]
> the time is a double and should never be casted to a
float. Casting this down to a float means the clock will lose precision after
some time:
> -   after 1 day, accuracy goes down to 8 ms
> -   after 10 days, accuracy is 62 ms
> -   after 30 days , accuracy is 250 ms
> -   after 60 days, accuracy is 500 ms

Mirage will also calculate the **Return Trip Time** as seen by the application:

```cs
double rtt = NetworkTime.Rtt;
```

>[!NOTE]
> Return RTT will also be effected by frame rate. higher frame rate will mean less delay before server reads ping message and replies. 

You can check the precision using:

```cs
double time_standard_deviation = NetworkTime.TimeSd;
```

for example, if this returns 0.2, it means the time measurements swing up and
down roughly 0.2 s

Network time is smoothing out the values using [Exponential moving average](https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average). 
You can configure how often you want the client to send pings using:

```cs
NetworkTime.PingInterval = 2.0f;
```

You can configure how quickly results will change using:

```cs
NetworkTime.PingWindowSize = 10;
```

Higher number will result in smoother results, but longer time to adjust to changes.
