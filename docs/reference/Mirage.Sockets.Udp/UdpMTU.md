---
id: UdpMTU
title: UdpMTU
---

# Class UdpMTU



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
public class UdpMTU
```


### Properties

#### MaxPacketSize

Max size of array that will be sent to or can be received from 
This will also be the size of all buffers used by 
This is not max message size because this size includes packets header added by 


##### Declaration

```cs
public static int MaxPacketSize { get; }
```
