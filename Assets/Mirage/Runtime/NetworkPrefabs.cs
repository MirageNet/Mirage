using System.Collections.Generic;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    ///     A scriptable object that contains a list of prefabs that can be spawned on the network.
    /// </summary>
    public sealed class NetworkPrefabs : ScriptableObject
    {
        public List<NetworkIdentity> Prefabs = new List<NetworkIdentity>();
    }
}
