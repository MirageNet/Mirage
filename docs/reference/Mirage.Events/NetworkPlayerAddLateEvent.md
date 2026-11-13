---
id: NetworkPlayerAddLateEvent
title: NetworkPlayerAddLateEvent
---

# Class NetworkPlayerAddLateEvent


Event fires from a  or  during a new connection, a new authentication, or a disconnection.



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Events.AddLateEventBase
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;
</div>
<div class="level" style={{"--data-index": 3}}>
Mirage.Events.AddLateEventUnity&lt;Mirage.INetworkPlayer, Mirage.Events.NetworkPlayerEvent&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Events.AddLateEventUnity&lt;Mirage.INetworkPlayer, Mirage.Events.NetworkPlayerEvent&gt;.AddListener(UnityAction&lt;Mirage.INetworkPlayer&gt;)


Mirage.Events.AddLateEventUnity&lt;Mirage.INetworkPlayer, Mirage.Events.NetworkPlayerEvent&gt;.RemoveListener(UnityAction&lt;Mirage.INetworkPlayer&gt;)


Mirage.Events.AddLateEventUnity&lt;Mirage.INetworkPlayer, Mirage.Events.NetworkPlayerEvent&gt;.Invoke(Mirage.INetworkPlayer)


Mirage.Events.AddLateEventUnity&lt;Mirage.INetworkPlayer, Mirage.Events.NetworkPlayerEvent&gt;.RemoveAllListeners()


Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;._arg0


Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;.AddListener(System.Action&lt;Mirage.INetworkPlayer&gt;)


Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;.RemoveListener(System.Action&lt;Mirage.INetworkPlayer&gt;)


Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;.Invoke(Mirage.INetworkPlayer)


Mirage.Events.AddLateEvent&lt;Mirage.INetworkPlayer&gt;.OnDestroyCleanup()


Mirage.Events.AddLateEventBase.HasInvoked


Mirage.Events.AddLateEventBase.MarkInvoked()


Mirage.Events.AddLateEventBase.Reset()

</details>

##### Syntax

```cs
[Serializable]
public class NetworkPlayerAddLateEvent : AddLateEventUnity<INetworkPlayer, NetworkPlayerEvent>, IAddLateEventUnity<INetworkPlayer>, IAddLateEvent<INetworkPlayer>
```

