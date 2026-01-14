using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    public class RecentlyDestroyed
    {
        private static readonly ILogger logger = LogFactory.GetLogger<RecentlyDestroyed>();

        private readonly Dictionary<uint, double> _recentlyDestroyed = new Dictionary<uint, double>();
        private double _lastCleanTime;

        public void Add(uint netId)
        {
            _recentlyDestroyed[netId] = Time.unscaledTimeAsDouble;
            if (logger.LogEnabled()) logger.Log($"Added netId {netId} to recently destroyed list.");
        }

        public bool WasRecentlyDestroyed(uint netId, out double destroyTime)
        {
            return _recentlyDestroyed.TryGetValue(netId, out destroyTime);
        }

        public void CleanUp(double gracePeriod)
        {
            var now = Time.unscaledTimeAsDouble;
            // clean at most once per 10 seconds
            if (now < _lastCleanTime + 10)
                return;

            _lastCleanTime = now;
            if (_recentlyDestroyed.Count == 0)
                return;

            var toRemove = new List<uint>();
            foreach (var kvp in _recentlyDestroyed)
            {
                var destroyTime = kvp.Value;
                if (now >= destroyTime + gracePeriod)
                    toRemove.Add(kvp.Key);
            }

            foreach (var netId in toRemove)
                _recentlyDestroyed.Remove(netId);

            if (toRemove.Count > 0 && logger.LogEnabled())
                logger.Log($"Cleaned up {toRemove.Count} netIds from recently destroyed list.");
        }
    }
}
