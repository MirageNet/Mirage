using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Class that Syncs syncvar and other <see cref="NetworkIdentity"/> State
    /// </summary>
    public class SyncVarSender
    {
        private static readonly ILogger logger = LogFactory.GetLogger<SyncVarSender>();

        private readonly HashSet<NetworkIdentity> _dirtyObjects = new HashSet<NetworkIdentity>();
        private readonly List<NetworkIdentity> _dirtyObjectsTmp = new List<NetworkIdentity>();

        public void AddDirtyObject(NetworkIdentity dirty)
        {
            var added = _dirtyObjects.Add(dirty);
            if (added && logger.LogEnabled())
                logger.Log($"New Dirty Object [netId={dirty.NetId} name={dirty.name}]");
        }

        internal void Update()
        {
            if (_dirtyObjects.Count > 0 && logger.LogEnabled())
                logger.Log($"SyncVar Sender Update, {_dirtyObjects.Count} dirty objects");

            _dirtyObjectsTmp.Clear();

            foreach (var identity in _dirtyObjects)
            {
                if (identity == null)
                    continue;

                if (identity.observers.Count > 0)
                {
                    if (logger.LogEnabled()) logger.Log($"Sending syncvars to {identity.observers.Count} observers [netId={identity.NetId} name={identity.name}]");

                    identity.SendUpdateVarsMessage();

                    if (identity.StillDirty())
                        _dirtyObjectsTmp.Add(identity);
                }
                else
                {
                    if (logger.LogEnabled()) logger.Log($"No observers, Clearing dirty bits [netId={identity.NetId} name={identity.name}]");

                    // clear all component's dirty bits.
                    // it would be spawned on new observers anyway.
                    identity.ClearAllComponentsDirtyBits();
                }
            }

            _dirtyObjects.Clear();

            foreach (var obj in _dirtyObjectsTmp)
                _dirtyObjects.Add(obj);
        }
    }
}
