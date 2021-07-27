using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Experimental.State2
{
    public class DemoHealth : MonoBehaviour
    {
        public struct Snapshot
        {
            public ushort previousSnapshot;
            public int health;
        }

        public void InitizeSnapshots(int snapshotCount, SnapshotState state)
        {
            // todo weaver generated body
            snapshots = new Snapshot[snapshotCount];
            this.state = state;
        }
        public void WriteWhole(NetworkWriter writer)
        {
            writer.Write(snapshots[state.Sequence].health);
        }
        public void WriteDelta(NetworkWriter writer, ulong previous)
        {
            writer.WriteDelta(snapshots[previous].health, snapshots[state.Sequence].health);
        }
        public void ReadWhole(NetworkReader reader)
        {
            snapshots[state.Sequence].health = reader.Read<int>();
        }
        public void ReadDelta(NetworkReader reader, ulong previous)
        {
            snapshots[state.Sequence].health = reader.ReadDelta<int>(snapshots[previous].health);
        }
        SnapshotState state;
        Snapshot[] snapshots;
        public ulong lastChanged;


        DemoNetworkIdentity _id;
        DemoNetworkIdentity Identity => _id ?? (_id = GetComponent<DemoNetworkIdentity>());

        public DemoMonster monster;

        public int Health
        {
            get => snapshots[state.Sequence].health;
            set
            {
                snapshots[state.Sequence].health = value;
                lastChanged = state.Sequence;
            }
        }

        public bool Harm(int damage)
        {
            Health = Mathf.Max(0, Health - damage);
            if (Health <= 0)
            {
                Death();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Death()
        {
            if (Identity.player != null)
            {
                Identity.player.OnDeath();
                Health = 20;
            }
            else if (Identity.monster != null)
            {
                // destroy if monster
                GameObject.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("unknown object");
            }
        }
    }
}
