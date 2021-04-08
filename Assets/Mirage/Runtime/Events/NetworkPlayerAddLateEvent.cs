using System;
using UnityEngine.Events;

namespace Mirage.Events
{
    /// <summary>
    /// Event fires from a <see cref="NetworkClient">NetworkClient</see> or <see cref="NetworkServer">NetworkServer</see> during a new connection, a new authentication, or a disconnection.
    /// <para>INetworkConnection - connection creating the event</para>
    /// </summary>
    [Serializable] public class NetworkPlayerEvent : UnityEvent<INetworkPlayer> { }

    [Serializable]
    public class NetworkPlayerAddLateEvent : AddLateEvent<INetworkPlayer, NetworkPlayerEvent> { }
}
