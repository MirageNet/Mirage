using System.Collections.Generic;

namespace Mirage
{
    /// <summary>
    /// Class that Syncs syncvar and other <see cref="NetworkIdentity"/> State
    /// </summary>
    public class SyncVarSender
    {
        private readonly HashSet<NetworkIdentity> DirtyObjects = new HashSet<NetworkIdentity>();
        private readonly List<NetworkIdentity> DirtyObjectsTmp = new List<NetworkIdentity>();

        public void AddDirtyObject(NetworkIdentity dirty)
        {
            DirtyObjects.Add(dirty);
        }


        internal void Update()
        {
            DirtyObjectsTmp.Clear();

            foreach (NetworkIdentity identity in DirtyObjects)
            {
                if (identity != null)
                {
                    identity.UpdateVars();

                    if (identity.StillDirty())
                        DirtyObjectsTmp.Add(identity);
                }
            }

            DirtyObjects.Clear();

            foreach (NetworkIdentity obj in DirtyObjectsTmp)
                DirtyObjects.Add(obj);
        }
    }
}
