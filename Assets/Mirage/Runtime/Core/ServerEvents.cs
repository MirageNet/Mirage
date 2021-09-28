using System;
using Mirage.Events;

namespace Mirage.Core
{
    [Serializable]
    public class ServerEvents
    {
        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// </summary>
        //[Header("Events")]
        //[SerializeField]
        public AddLateEvent Started = new AddLateEvent();

        /// <summary>
        /// Event fires once a new Client has connect to the Server.
        /// </summary>
        //[FoldoutEvent, SerializeField, FormerlySerializedAs("Connected")]
        public NetworkPlayerEvent Connected = new NetworkPlayerEvent();

        /// <summary>
        /// Event fires once a new Client has passed Authentication to the Server.
        /// </summary>
        //[FoldoutEvent, SerializeField, FormerlySerializedAs("Authenticated")]
        public NetworkPlayerEvent Authenticated = new NetworkPlayerEvent();

        /// <summary>
        /// Event fires once a Client has Disconnected from the Server.
        /// </summary>
        //[FoldoutEvent, SerializeField, FormerlySerializedAs("Disconnected")]
        public NetworkPlayerEvent Disconnected = new NetworkPlayerEvent();

        //[SerializeField]
        public AddLateEvent Stopped = new AddLateEvent();

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        //[SerializeField]
        public AddLateEvent OnStartHost = new AddLateEvent();

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        //[SerializeField]
        public AddLateEvent OnStopHost = new AddLateEvent();
    }
}
