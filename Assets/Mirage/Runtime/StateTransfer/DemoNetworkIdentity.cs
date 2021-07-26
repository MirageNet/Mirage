using UnityEngine;

namespace Mirage.Experimental
{
    public class DemoNetworkIdentity : MonoBehaviour
    {
        private uint _id;
        public uint Id => _id;

        public uint SpawnId { get; private set; }

        public DemoNetworkTransform networkTransform;
        public DemoHealth health;
        public DemoPlayer player;
        public DemoMonster monster;

        internal void Init(uint netid, uint spawnId)
        {
            _id = netid;
            SpawnId = spawnId;
        }
    }
}
