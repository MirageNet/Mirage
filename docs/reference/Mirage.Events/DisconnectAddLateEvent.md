---
id: DisconnectAddLateEvent
title: DisconnectAddLateEvent
---

# Class DisconnectAddLateEvent


Event fires from a  when it fails to connect to the server



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Events.AddLateEventBase
</div>
<div class="level" style={{"--data-index": 2}}>
Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;
</div>
<div class="level" style={{"--data-index": 3}}>
Mirage.Events.AddLateEventUnity&lt;Mirage.ClientStoppedReason, Mirage.Events.DisconnectEvent&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Events.AddLateEventUnity&lt;Mirage.ClientStoppedReason, Mirage.Events.DisconnectEvent&gt;.AddListener(UnityAction&lt;Mirage.ClientStoppedReason&gt;)


Mirage.Events.AddLateEventUnity&lt;Mirage.ClientStoppedReason, Mirage.Events.DisconnectEvent&gt;.RemoveListener(UnityAction&lt;Mirage.ClientStoppedReason&gt;)


Mirage.Events.AddLateEventUnity&lt;Mirage.ClientStoppedReason, Mirage.Events.DisconnectEvent&gt;.Invoke(Mirage.ClientStoppedReason)


Mirage.Events.AddLateEventUnity&lt;Mirage.ClientStoppedReason, Mirage.Events.DisconnectEvent&gt;.RemoveAllListeners()


Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;._arg0


Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;.AddListener(System.Action&lt;Mirage.ClientStoppedReason&gt;)


Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;.RemoveListener(System.Action&lt;Mirage.ClientStoppedReason&gt;)


Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;.Invoke(Mirage.ClientStoppedReason)


Mirage.Events.AddLateEvent&lt;Mirage.ClientStoppedReason&gt;.OnDestroyCleanup()


Mirage.Events.AddLateEventBase.HasInvoked


Mirage.Events.AddLateEventBase.MarkInvoked()


Mirage.Events.AddLateEventBase.Reset()

</details>

##### Syntax

```cs
[Serializable]
public class DisconnectAddLateEvent : AddLateEventUnity<ClientStoppedReason, DisconnectEvent>, IAddLateEventUnity<ClientStoppedReason>, IAddLateEvent<ClientStoppedReason>
```

