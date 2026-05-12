using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            // you may need to check if owner is the host player.
            var networkIdentity = other.gameObject.GetComponent<NetworkIdentity>();
            var owner = networkIdentity.Owner;

            // skip if host, server will already have scenes loaded
            if (owner.IsHost)
                return;

            // we dont need to send the scene in this example, because the client also has the inspector field
            TargetRpcLoadSubScene(networkIdentity.Owner);
        }

        [Server]
        private void OnTriggerExit(Collider other)
        {
            if (logger.LogEnabled()) logger.LogFormat(LogType.Log, "Unloading {0}", subScene);

            // this is just the opposite of OnTriggerEnter

            var networkIdentity = other.gameObject.GetComponent<NetworkIdentity>();
            var owner = networkIdentity.Owner;
            if (owner.IsHost)
                return;
            TargetRpcUnloadSubScene(networkIdentity.Owner);
        }

        // target rpc needs the INetworkPlayer parameter so server knows where to sent it,
        // but we can use `INetworkPlayer _` because client does not need to use it
        [ClientRpc(target = RpcTarget.Player)]
        public void TargetRpcLoadSubScene(INetworkPlayer _)
        {
            SceneManager.LoadSceneAsync(subScene, LoadSceneMode.Additive);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public void TargetRpcUnloadSubScene(INetworkPlayer _)
        {
            SceneManager.UnloadSceneAsync(subScene);
        }
    }
}
