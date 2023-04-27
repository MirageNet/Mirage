using System;
using UnityEngine;

namespace Mirage
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets <see cref="NetworkIdentity"/> on a <see cref="GameObject"/> and throws <see cref="InvalidOperationException"/> if the GameObject does not have one.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>attached NetworkIdentity</returns>
        /// <exception cref="InvalidOperationException">Throws when <paramref name="gameObject"/> does not have a NetworkIdentity attached</exception>
        public static NetworkIdentity GetNetworkIdentity(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(out NetworkIdentity identity))
            {
                throw new InvalidOperationException($"Gameobject {gameObject.name} doesn't have NetworkIdentity.");
            }
            return identity;
        }
    }
}
