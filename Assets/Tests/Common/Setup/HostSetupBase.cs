using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Tests.BaseSetups
{
    public class HostSetup_Base : ServerSetup_Base
    {
        protected new HostInstance _serverInstance => (HostInstance)base._serverInstance;
        protected sealed override bool HostMode => true;

        protected override async UniTask ExtraSetup()
        {
            // dont spawn character if server is no auto started
            if (StartServer)
                await SetupPlayer(_serverInstance, SpawnCharacterOnConnect);
        }

        // host properties to make it easier to use in tests
        protected NetworkClient client => _serverInstance.Client;
        protected ClientObjectManager clientObjectManager => _serverInstance.ClientObjectManager;
        protected MessageHandler ClientMessageHandler => _serverInstance.Client.MessageHandler;

        protected GameObject hostPlayerGO => _serverInstance.HostPlayer.GameObject;
        protected NetworkIdentity hostIdentity => _serverInstance.HostPlayer.Identity;
        protected INetworkPlayer hostServerPlayer => _serverInstance.HostPlayer.Player;
        protected INetworkPlayer hostClientPlayer => _serverInstance.Client.Player;
    }

    public class HostSetup_Base<T> : HostSetup_Base where T : NetworkBehaviour
    {
        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            prefab.gameObject.AddComponent<T>();
        }

        /// <summary>Object on host/server that Remote client owns</summary>
        protected T ServerComponent(int i) => ServerGameObject(i).GetComponent<T>();

        // host properties to make it easier to use in tests
        protected T hostComponent => _serverInstance.HostPlayer.GameObject.GetComponent<T>();
    }
}
