using System;
using UnityEngine.Events;

namespace Mirage
{
    public delegate NetworkIdentity SpawnHandlerDelegate(SpawnMessage msg);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(NetworkIdentity spawned);

    [Serializable]
    public class SpawnEvent : UnityEvent<NetworkIdentity> { }
}
