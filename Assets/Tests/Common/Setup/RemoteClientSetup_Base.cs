using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Tests.BaseSetups
{
    public class RemoteClientSetup_Base : ServerSetup_Base
    {
        // server properties to make it easier to use in tests
        protected GameObject serverPlayerGO => _serverInstance.Players[0].GameObject;
        protected NetworkIdentity serverIdentity => _serverInstance.Players[0].Identity;
        protected INetworkPlayer serverPlayer => _serverInstance.Players[0].Player;

        // client properties to make it easier to use in tests
        protected GameObject clientGo => _remoteClients[0].GameObject;
        protected NetworkClient client => _remoteClients[0].Client;
        protected ClientObjectManager clientObjectManager => _remoteClients[0].ClientObjectManager;
        protected GameObject clientPlayerGO => _remoteClients[0].character;
        protected NetworkIdentity clientIdentity => _remoteClients[0].identity;
        protected INetworkPlayer clientPlayer => _remoteClients[0].player;
        protected MessageHandler ClientMessageHandler => _remoteClients[0].Client.MessageHandler;

        protected override UniTask ExtraSetup()
        {
            return AddClient();
        }
    }

    public class RemoteClientSetup_Base<T> : RemoteClientSetup_Base where T : NetworkBehaviour
    {
        protected T ServerComponent(int i) => ServerGameObject(i).GetComponent<T>();

        // server properties to make it easier to use in tests
        protected T serverComponent => ServerComponent(0);

        // client properties to make it easier to use in tests
        protected T clientComponent => clientPlayerGO.GetComponent<T>();

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            prefab.gameObject.AddComponent<T>();
        }
    }

    public class RemoteClientSetup_Base<T1, T2> : RemoteClientSetup_Base<T1> where T1 : NetworkBehaviour where T2 : NetworkBehaviour
    {
        protected T2 ServerExtraComponent(int i) => ServerGameObject(i).GetComponent<T2>();

        // server properties to make it easier to use in tests
        protected T2 serverExtraComponent => ServerExtraComponent(0);

        // client properties to make it easier to use in tests
        protected T2 clientExtraComponent => clientPlayerGO.GetComponent<T2>();

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);
            prefab.gameObject.AddComponent<T2>();
        }
    }

    public class MultiRemoteClientSetup_Base : ServerSetup_Base
    {
        protected virtual int RemoteClientCount => 1;

        protected GameObject ClientGo(int i) => _remoteClients[i].GameObject;
        protected NetworkClient Client(int i) => _remoteClients[i].Client;
        protected ClientObjectManager ClientObjectManager(int i) => _remoteClients[i].ClientObjectManager;
        protected GameObject ClientPlayerGO(int i) => _remoteClients[i].character;
        protected NetworkIdentity ClientIdentity(int i) => _remoteClients[i].identity;
        protected INetworkPlayer ClientPlayer(int i) => _remoteClients[i].player;
        protected MessageHandler ClientMessageHandler(int i) => _remoteClients[i].Client.MessageHandler;

        protected override async UniTask ExtraSetup()
        {
            for (var i = 0; i < RemoteClientCount; i++)
            {
                await AddClient();
            }
        }
    }

    public class MultiRemoteClientSetup_Base<T> : MultiRemoteClientSetup_Base where T : NetworkBehaviour
    {
        protected T ServerComponent(int i) => ServerGameObject(i).GetComponent<T>();
        protected T ClientComponent(int i) => ClientPlayerGO(i).GetComponent<T>();

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            prefab.gameObject.AddComponent<T>();
        }
    }
}
