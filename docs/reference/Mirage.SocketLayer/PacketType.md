---
id: PacketType
title: PacketType
---

# Enum PacketType




##### Syntax

```cs
public enum PacketType
```


### Fields

#### Command

see 


##### Declaration

```cs
Command = 1
```
#### Unreliable

data packet sent with no guarantee for order or reliability
used for data that is fire and forget


##### Declaration

```cs
Unreliable = 2
```
#### Notify

data packet sent with ack header so sender knows if packet gets delivered or lost


##### Declaration

```cs
Notify = 3
```
#### Reliable

data packet that are guarantee to be in order, and not lost.
contains ack header
If a package is lost then other Reliable packets will be held until the lost packet is resent


##### Declaration

```cs
Reliable = 4
```
#### ReliableFragment

part of a Reliable message. same as Reliable but only part of a message


##### Declaration

```cs
ReliableFragment = 6
```
#### Ack

packet with just ack header
only sent if no other packets with ack header were sent recently


##### Declaration

```cs
Ack = 5
```
#### KeepAlive

Used to keep connection alive.
Similar to ping/pong


##### Declaration

```cs
KeepAlive = 10
```
