using UnityEngine;

namespace Mirage
{
    public abstract class ServerIdGenerator : MonoBehaviour
    {
        /// <summary>
        ///     Generate your own specific server id to be used specifically for linking
        ///     to of the same server so that information can be passed between the 2 same servers.
        /// </summary>
        public abstract byte GenerateServerId();
    }
}
