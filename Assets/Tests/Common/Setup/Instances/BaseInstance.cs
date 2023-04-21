using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Tests
{
    public abstract class BaseInstance
    {
        public GameObject GameObject;
        public abstract NetworkWorld World { get; }

        public void AddCleanupObjects(List<Object> toDestroy)
        {
            toDestroy.Add(GameObject);
            toDestroy.AddRange(World.SpawnedIdentities);
        }

        public NetworkIdentity Get(uint netId)
        {
            if (World.TryGetIdentity(netId, out var identity))
                return identity;

            throw new KeyNotFoundException($"No NetworkIdentity found on {GameObject} with netId={netId}");
        }

        /// <summary>
        /// Uses a NetworkIdentity from another instance, to find one in this instance
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public NetworkIdentity Get(NetworkIdentity other)
        {
            if (World.TryGetIdentity(other.NetId, out var identity))
                return identity;

            throw new KeyNotFoundException($"No NetworkIdentity found on {GameObject} with netId={other.NetId}");
        }

        public T Get<T>(T other) where T : NetworkBehaviour
        {
            if (World.TryGetIdentity(other.NetId, out var identity))
                return (T)identity.NetworkBehaviours[other.ComponentIndex];

            throw new KeyNotFoundException($"No NetworkIdentity found on {GameObject} with netId={other.NetId}");
        }
    }
}
