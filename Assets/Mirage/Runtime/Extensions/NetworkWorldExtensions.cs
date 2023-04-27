using System;

namespace Mirage
{
    public static class NetworkWorldExtensions
    {
        /// <summary>
        /// adds an event handler, and invokes it on current objects in world
        /// </summary>
        /// <param name="action"></param>
        public static void AddAndInvokeOnSpawn(this NetworkWorld world, Action<NetworkIdentity> action)
        {
            world.onSpawn += action;
            foreach (var identity in world.SpawnedIdentities)
            {
                action.Invoke(identity);
            }
        }

        /// <summary>
        /// adds an event handler, and invokes it on current objects in world
        /// </summary>
        /// <param name="action"></param>
        public static void AddAndInvokeOnAuthorityChanged(this NetworkWorld world, AuthorityChanged action)
        {
            world.OnAuthorityChanged += action;
            foreach (var identity in world.SpawnedIdentities)
            {
                if (identity.HasAuthority)
                {
                    // owner might be null, but that is fine
                    action.Invoke(identity, true, identity.Owner);
                }
            }
        }
    }
}
