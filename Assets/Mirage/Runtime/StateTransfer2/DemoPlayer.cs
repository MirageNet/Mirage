using Mirage.Serialization;
using UnityEngine;

namespace Mirage.Experimental.State2
{
    public class DemoPlayer : MonoBehaviour
    {
        public struct Snapshot
        {
            public ushort previousSnapshot;
            public int money;
            public int damage;
        }

        public void InitizeSnapshots(int snapshotCount, SnapshotState state)
        {
            // todo weaver generated body
            snapshots = new Snapshot[snapshotCount];
            this.state = state;
        }
        public void WriteWhole(NetworkWriter writer)
        {
            writer.Write(snapshots[state.Sequence].money);
            writer.Write(snapshots[state.Sequence].damage);
        }
        public void WriteDelta(NetworkWriter writer, ulong previous)
        {
            writer.WriteDelta(snapshots[previous].money, snapshots[state.Sequence].money);
            writer.WriteDelta(snapshots[previous].damage, snapshots[state.Sequence].damage);
        }
        public void ReadWhole(NetworkReader reader)
        {
            snapshots[state.Sequence].money = reader.Read<int>();
            snapshots[state.Sequence].damage = reader.Read<int>();
        }
        public void ReadDelta(NetworkReader reader, ulong previous)
        {
            snapshots[state.Sequence].money = reader.ReadDelta<int>(snapshots[previous].money);
            snapshots[state.Sequence].damage = reader.ReadDelta<int>(snapshots[previous].damage);
        }
        SnapshotState state;
        Snapshot[] snapshots;
        public ulong lastChanged;

        public int Money
        {
            get => snapshots[state.Sequence].money;
            set
            {
                snapshots[state.Sequence].money = value;
                lastChanged = state.Sequence;
            }
        }
        public int Damage
        {
            get => snapshots[state.Sequence].damage;
            set
            {
                snapshots[state.Sequence].damage = value;
                lastChanged = state.Sequence;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DemoHealth>(out DemoHealth health))
            {
                bool died = health.Harm(Damage);
                if (died)
                {
                    Money++;
                    if (Money > Damage)
                    {
                        Damage++;
                        Money -= Damage;
                    }
                }
            }
        }

        internal void OnDeath()
        {
            Money = 0;
            Damage = 1;
        }
    }
}
