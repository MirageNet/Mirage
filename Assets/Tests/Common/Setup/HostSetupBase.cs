using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Tests.BaseSetups
{
    public class HostSetup_Base : ServerSetup_Base
    {
        protected new HostInstance _serverInstance => (HostInstance)base._serverInstance;
        protected override bool HostMode => true;

        protected override async UniTask ExtraSetup()
        {
            await SpawnCharacter(_serverInstance);
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

        protected T ServerComponent(int i) => ServerGameObject(i).GetComponent<T>();

        // host properties to make it easier to use in tests
        protected T hostComponent => ServerComponent(0);
    }
}
