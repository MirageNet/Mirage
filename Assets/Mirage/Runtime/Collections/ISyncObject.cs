using System;
using Mirage.Serialization;

namespace Mirage.Collections
{
    /// <summary>
    /// A sync object is an object that can synchronize it's state
    /// between server and client, such as a SyncList
    /// </summary>
    public interface ISyncObject
    {
        /// <summary>
        /// Raised after the list has been updated
        /// <para><b>Sender:</b> This will invoke for every change</para>
        /// <para><b>Receiver:</b> This will invoke after OnDeserialize</para>
        /// <para>note: by default the Server will be the sender, but that can be changed with syncDirection</para>
        /// </summary>
        event Action OnChange;

        /// <summary>
        /// true if there are changes since the last flush
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Are we sending or receiving data from this instance. This is used to determine if we should throw if a change is made on the wrong instance
        /// </summary>
        /// <param name="shouldSync"></param>
        void SetShouldSyncFrom(bool shouldSync);

        /// <summary>
        /// Discard all the queued changes
        /// <para>Consider the object fully synchronized with clients</para>
        /// </summary>
        void Flush();

        /// <summary>
        /// Write a full copy of the object
        /// </summary>
        /// <param name="writer"></param>
        void OnSerializeAll(NetworkWriter writer);

        /// <summary>
        /// Write the changes made to the object since last sync
        /// </summary>
        /// <param name="writer"></param>
        void OnSerializeDelta(NetworkWriter writer);

        /// <summary>
        /// Reads a full copy of the object
        /// </summary>
        /// <param name="reader"></param>
        void OnDeserializeAll(NetworkReader reader);

        /// <summary>
        /// Reads the changes made to the object since last sync
        /// </summary>
        /// <param name="reader"></param>
        void OnDeserializeDelta(NetworkReader reader);

        /// <summary>
        /// Resets the SyncObject so that it can be re-used
        /// </summary>
        void Reset();

        /// <summary>
        /// Sets the NetworkBehaviour that owns this SyncObject
        /// <para>This can be used by custom syncObjects to listen to events on NetworkBehaviour</para>
        /// <para>This will only be called once, the first time the object is spawned (similar to unity's awake call)</para>
        /// </summary>
        /// <param name="networkBehaviour"></param>
        void SetNetworkBehaviour(NetworkBehaviour networkBehaviour);
    }
}
