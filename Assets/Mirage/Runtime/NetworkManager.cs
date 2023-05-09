using System;
using UnityEngine;

namespace Mirage
{
    [Flags]
    public enum NetworkManagerMode
    {
        None = 0,
        Server = 1,
        Client = 2,
        Host = Server | Client
    }

    [AddComponentMenu("Network/NetworkManager")]
    [HelpURL("https://miragenet.github.io/Mirage/docs/guides/callbacks/network-manager")]
    [DisallowMultipleComponent]
    public class NetworkManager : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;
        public NetworkSceneManager NetworkSceneManager;
        public ServerObjectManager ServerObjectManager;
        public ClientObjectManager ClientObjectManager;

        [Tooltip("Will setup reference automatically")]
        public bool ValidateReferences = true;


        /// <summary>
        /// True if the server or client is started and running
        /// <para>This is set True in StartServer / StartClient, and set False in StopServer / StopClient</para>
        /// </summary>
        public bool IsNetworkActive => Server.Active || Client.Active;

        /// <summary>
        /// helper enum to know if we started the NetworkManager as server/client/host.
        /// </summary>
        public NetworkManagerMode NetworkMode
        {
            get
            {
                if (!Server.Active && !Client.Active)
                    return NetworkManagerMode.None;
                else if (Server.Active && Client.Active)
                    return NetworkManagerMode.Host;
                else if (Server.Active)
                    return NetworkManagerMode.Server;
                else
                    return NetworkManagerMode.Client;
            }
        }


        private void OnValidate()
        {
            if (!ValidateReferences)
                return;

            FindIfNull(ref Server);
            FindIfNull(ref Client);
            FindIfNull(ref NetworkSceneManager);
            FindIfNull(ref ServerObjectManager);
            FindIfNull(ref ClientObjectManager);

            if (Server != null)
            {
                SetIfNull(ref Server.ObjectManager, ServerObjectManager);
            }

            if (Client != null)
            {
                SetIfNull(ref Client.ObjectManager, ClientObjectManager);
            }

            if (NetworkSceneManager != null)
            {
                SetIfNull(ref NetworkSceneManager.Server, Server);
                SetIfNull(ref NetworkSceneManager.Client, Client);
                SetIfNull(ref NetworkSceneManager.ServerObjectManager, ServerObjectManager);
                SetIfNull(ref NetworkSceneManager.ClientObjectManager, ClientObjectManager);
            }
        }

        private void FindIfNull<T>(ref T field) where T : class
        {
            if (field == null && gameObject.TryGetComponent<T>(out var value))
            {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Setting Reference on NetworkManager");
#endif
                field = value;
            }
        }
        private void SetIfNull<T>(ref T field, T value) where T : class
        {
            if (field == null && value != null)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Setting Reference on NetworkManager");
#endif
                field = value;
            }
        }
    }
}
