using UnityEngine;
using UnityEngine.Serialization;

namespace Mirage
{

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Guides/Communications/NetworkManager.html")]
    [RequireComponent(typeof(NetworkServer))]
    [RequireComponent(typeof(NetworkClient))]
    [DisallowMultipleComponent]
    public class NetworkManager : MonoBehaviour
    {
        [FormerlySerializedAs("server")]
        public NetworkServer Server;
        [FormerlySerializedAs("client")]
        public NetworkClient Client;
        [FormerlySerializedAs("sceneManager")]
        [FormerlySerializedAs("SceneManager")]
        public NetworkSceneManager NetworkSceneManager;
        [FormerlySerializedAs("serverObjectManager")]
        public ServerObjectManager ServerObjectManager;
        [FormerlySerializedAs("clientObjectManager")]
        public ClientObjectManager ClientObjectManager;

        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        public bool IsNetworkActive => Server.Active || Client.Active;
    }
}
