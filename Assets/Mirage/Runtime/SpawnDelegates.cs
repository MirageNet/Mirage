using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace Mirage
{
    public delegate NetworkIdentity SpawnHandlerDelegate(SpawnMessage msg);
    public delegate UniTask<NetworkIdentity> SpawnHandlerAsyncDelegate(SpawnMessage msg);
    public delegate SpawnHandler DynamicSpawnHandlerDelegate(int prefabHash);

    // Handles requests to unspawn objects on the client
    public delegate void UnSpawnDelegate(NetworkIdentity spawned);

    [Serializable]
    public class SpawnEvent : UnityEvent<NetworkIdentity> { }
}
