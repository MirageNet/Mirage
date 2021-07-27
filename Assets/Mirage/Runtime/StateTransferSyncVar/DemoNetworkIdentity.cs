using UnityEngine;

namespace Mirage.Experimental.StateSyncVar
{
    public class DemoNetworkIdentity : MonoBehaviour
    {
        private uint _id;
        public uint Id => _id;

        public DemoNetworkTransform networkTransform;
        public DemoHealth health;
        public DemoPlayer player;
        public DemoMonster monster;

        internal void Init(uint netid)
        {
            _id = netid;
        }
    }
}
