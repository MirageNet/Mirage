using Mirage.Logging;
using UnityEngine;

namespace Mirage.Examples.Additive
{
    // This script is attached to a scene object called Zone that is on the Player layer and has:
    // - Sphere Collider with isTrigger = true
    // - Network Identity with Server Only checked
    // These OnTrigger events only run on the server and will only send a message to the player
    // that entered the Zone to load the subscene assigned to the subscene property.
    public class ZoneHandler : NetworkBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(ZoneHandler));

        [Scene]
        [Tooltip("Assign the sub-scene to load for this zone")]
        public string subScene;

        [Server]
        private void OnTriggerEnter(Collider other)
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Loading {0}", subScene);

            var networkIdentity = other.gameObject.GetComponent<NetworkIdentity>();
            // note: you may need to check if owner is the host player.
            //       if they are the host, the scene will already be loaded on servere so does not need to be loaded
            networkIdentity.Owner.Send(new SceneMessage { MainActivateScene = subScene, SceneOperation = SceneOperation.LoadAdditive });
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Unloading {0}", subScene);

            var networkIdentity = other.gameObject.GetComponent<NetworkIdentity>();
            // note: you may need to check if owner is the host player.
            //       if they are the host, you do not want to be unloading scene on server
            networkIdentity.Owner.Send(new SceneMessage { MainActivateScene = subScene, SceneOperation = SceneOperation.UnloadAdditive });
        }
    }
}
