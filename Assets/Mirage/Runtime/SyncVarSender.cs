using System.Collections.Generic;

namespace Mirage
{
    /// <summary>
    /// Class that Syncs syncvar and other <see cref="NetworkIdentity"/> State
    /// </summary>
    public class SyncVarSender
    {
        private readonly HashSet<NetworkIdentity> _dirtyObjects = new HashSet<NetworkIdentity>();
        private readonly List<NetworkIdentity> _dirtyObjectsTmp = new List<NetworkIdentity>();

        public SyncVarSender(NetworkWorld world)
        {
            world.onSpawn += World_onSpawn;
            foreach (var obj in world.SpawnedIdentities)
                World_onSpawn(obj);
        }

        private void World_onSpawn(NetworkIdentity obj)
        {
            // todo find way remove action when de-spawned
            obj.OnDirty += () => {
                AddDirtyObject(obj);
            };
        }

        public void AddDirtyObject(NetworkIdentity dirty)
        {
            _dirtyObjects.Add(dirty);
        }

        internal void Update()
        {
            _dirtyObjectsTmp.Clear();

            foreach (var identity in _dirtyObjects)
            {
                if (identity != null)
                {
                    identity.UpdateVars();

                    if (identity.StillDirty())
                        _dirtyObjectsTmp.Add(identity);
                }
            }

            _dirtyObjects.Clear();

            foreach (var obj in _dirtyObjectsTmp)
                _dirtyObjects.Add(obj);
        }
    }
}
