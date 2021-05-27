using System;
using UnityEngine.Events;

namespace Mirage.Events
{
    [Serializable] public class DisconnectEvent : UnityEvent<ClientStoppedReason> { }

    /// <summary>
    /// Event fires from a <see cref="NetworkClient">NetworkClient</see> when it fails to connect to the server
    /// </summary>
    [Serializable] public class DisconnectAddLateEvent : AddLateEvent<ClientStoppedReason, DisconnectEvent> { }
}
