using System.Collections.Generic;
using Mirage;
using UnityEngine;

namespace Mirage.Snippets.Analyzers
{
    namespace M1404.Triggering
    {
        // CodeEmbed-Start: mirage1404-triggering
        public class Explosion : NetworkBehaviour
        {
            [Server]
            public void Explode(List<Collider> hits)
            {
                // Warning: HashSet is not cleared when returned to the pool,
                // so it may still contain objects from a previous explosion
                using var wrapper = AutoPool<HashSet<NetworkIdentity>>.Take();
                var damaged = wrapper.Item;

                foreach (var hit in hits)
                {
                    var identity = hit.GetComponentInParent<NetworkIdentity>();

                    // stale items in the set cause objects to be skipped
                    if (identity != null && damaged.Add(identity))
                        ApplyDamage(identity);
                }
            }

            private void ApplyDamage(NetworkIdentity identity)
            {
                // damage logic...
            }
        }
        // CodeEmbed-End: mirage1404-triggering
    }

    namespace M1404.Recommended
    {
        // CodeEmbed-Start: mirage1404-recommended
        public class Explosion : NetworkBehaviour
        {
            [Server]
            public void Explode(List<Collider> hits)
            {
                // Correct: helper clears the HashSet when it is returned to the pool
                using var _ = AutoPool.TakeHashSet<NetworkIdentity>(out var damaged);

                foreach (var hit in hits)
                {
                    var identity = hit.GetComponentInParent<NetworkIdentity>();
                    if (identity != null && damaged.Add(identity))
                        ApplyDamage(identity);
                }
            }

            private void ApplyDamage(NetworkIdentity identity)
            {
                // damage logic...
            }
        }
        // CodeEmbed-End: mirage1404-recommended
    }

    namespace M1404.ClearAction
    {
        // CodeEmbed-Start: mirage1404-clear-action
        public class TurnOrder : NetworkBehaviour
        {
            static TurnOrder()
            {
                // SortedSet has no AutoPool helper, so tell the pool how to clear it
                AutoPool<SortedSet<uint>>.ClearAction = set => set.Clear();
            }

            [Server]
            public void StartRound(List<NetworkIdentity> players)
            {
#pragma warning disable MIRAGE1404 // cleared by ClearAction set in static constructor
                using var _ = AutoPool<SortedSet<uint>>.Take(out var order);
#pragma warning restore MIRAGE1404

                foreach (var player in players)
                    order.Add(player.NetId);

                // turn logic...
            }
        }
        // CodeEmbed-End: mirage1404-clear-action
    }
}
