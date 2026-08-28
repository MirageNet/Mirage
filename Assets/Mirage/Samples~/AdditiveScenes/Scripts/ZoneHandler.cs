using Mirage.Logging;
using UnityEngine;

namespace Mirage.Examples.Additive
{
    // This script is attached to a scene object called Zone that is on the Player layer and has:
    // - Sphere Collider with isTrigger = true
    // - Network Identity with Server Only checked
    // These OnTrigger events only run on the server and tell SubSceneLoader to load or unload
    // the sub scene for the player that entered or left the Zone.
    public class ZoneHandler : NetworkBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ZoneHandler));

        [RequiredField]
        public SubSceneLoader SubSceneLoader;

        [Server]
        private void OnTriggerEnter(Collider other)
        {
            var owner = other.gameObject.GetComponent<NetworkIdentity>().Owner;
            if (logger.LogEnabled()) logger.Log($"Loading sub scene for {owner}");

            SubSceneLoader.ServerShowSubScene(owner);
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            var owner = other.gameObject.GetComponent<NetworkIdentity>().Owner;
            if (logger.LogEnabled()) logger.Log($"Unloading sub scene for {owner}");

            SubSceneLoader.ServerHideSubScene(owner);
        }
    }
}
