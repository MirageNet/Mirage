using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.BaseSetups
{
    /// <summary>
    /// Base class that will setup a serer, Use override HostMode to set host, and Method AddClient For remote clients
    ///
    /// <para>
    /// IMPORTANT: name of this should have _Base this is to imply it should not be used by tests directly
    ///            instead tests should use the ServerSetup class because that has runtime or edit time setup
    /// </para>
    /// </summary>
    public class ServerSetup_Base : TestBase
    {
        protected ServerInstance _serverInstance;
        protected List<ClientInstance> _remoteClients = new List<ClientInstance>();
        private int _clientNameIndex;
        protected NetworkIdentity _characterPrefab;

        // properties to make it easier to use in tests
        protected GameObject serverGo => _serverInstance.GameObject;
        protected NetworkServer server => _serverInstance.Server;
        protected ServerObjectManager serverObjectManager => _serverInstance.ServerObjectManager;
        protected MessageHandler ServerMessageHandler => server.MessageHandler;

        protected GameObject ServerGameObject(int i) => _serverInstance.Players[i].GameObject;
        protected NetworkIdentity ServerIdentity(int i) => _serverInstance.Players[i].Identity;
        protected INetworkPlayer ServerPlayer(int i) => _serverInstance.Players[i].Player;

        protected GameObject _characterPrefabGo => _characterPrefab.gameObject;

        protected virtual bool HostMode => false;
        protected virtual bool StartServer => true;
        protected virtual bool SpawnCharacterOnConnect => true;
        protected virtual Config ServerConfig => null;
        protected virtual Config ClientConfig => null;

        protected async UniTask ServerSeutup()
        {
            Console.WriteLine($"[MirageTest] UnitySetUp class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName}");

            _clientNameIndex = 0;
            if (HostMode)
            {
                var hostInstance = new HostInstance(ServerConfig);
                _serverInstance = hostInstance;
                ExtraServerSetup();
                ExtraClientSetup(hostInstance);
            }
            else
            {
                _serverInstance = new ServerInstance(ServerConfig);
                ExtraServerSetup();
            }

            // create prefab
            _characterPrefab = CreateNetworkIdentity(disable: true);
            _characterPrefab.name = "player (unspawned)";
            // DontDestroyOnLoad so that "prefab" wont be destroyed by scene loading
            // also means that NetworkScenePostProcess will skip this unspawned object
            Object.DontDestroyOnLoad(_characterPrefab);
            _characterPrefab.PrefabHash = Guid.NewGuid().GetHashCode();

            // create prefab before Setup, so i can be added to inside extra setup
            ExtraPrefabSetup(_characterPrefab);

            // wait a frame for start to be called
            await UniTask.DelayFrame(1);

            // start via instance, incase it is HostInstance
            if (StartServer)
                _serverInstance.StartServer();

            await ExtraSetup();

            if (_serverInstance is HostInstance host)
                ExtraClientLateSetup(host);

            await LateSetup();
        }

        /// <summary>
        /// called before start on server objects
        /// </summary>
        protected virtual void ExtraServerSetup() { }

        /// <summary>
        /// called on prefab before it is fully setup
        /// </summary>
        protected virtual void ExtraPrefabSetup(NetworkIdentity prefab) { }

        /// <summary>
        /// called before Start() after client objects is setup
        /// <para>Called on client and host</para>
        /// </summary>
        protected virtual void ExtraClientSetup(IClientInstance instance) { }

        /// <summary>
        /// Called after client is connected and spawned character
        /// <para>Called on client and host</para>
        /// </summary>
        /// <param name="instance"></param>
        protected virtual void ExtraClientLateSetup(IClientInstance instance) { }

        /// <summary>
        /// Called after server is ready, before LateSetup
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask ExtraSetup() => UniTask.CompletedTask;

        /// <summary>
        /// Called after all other setup methods
        /// </summary>
        /// <returns></returns>
        protected virtual UniTask LateSetup() => UniTask.CompletedTask;

        /// <summary>
        /// Use this to create a client instance that doesn't automatically connect
        /// </summary>
        /// <returns></returns>
        public ClientInstance CreateClientInstance(bool extraSetup = true)
        {
            var instance = new ClientInstance(ClientConfig, _serverInstance.SocketFactory, _clientNameIndex.ToString());
            // make sure to add to destory, just incase AddClient is not called after this
            toDestroy.Add(instance.GameObject);

            _clientNameIndex++;
            if (extraSetup)
                ExtraClientSetup(instance);

            return instance;
        }

        /// <summary>
        /// Creates and adds a client instance
        /// </summary>
        /// <returns></returns>
        public async UniTask<ClientInstance> AddClient()
        {
            var instance = CreateClientInstance();
            await AddClient(instance, SpawnCharacterOnConnect);
            return instance;
        }

        /// <summary>
        /// adds 
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public async UniTask AddClient(ClientInstance instance, bool spawnCharacter)
        {
            if (_remoteClients.Contains(instance))
                throw new ArgumentException("instance already added");

            var serverStartCount = server.Players.Count;
            instance.Client.Connect("localhost");

            // wait for new connections
            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > serverStartCount);

            await SetupPlayer(instance, spawnCharacter);

            _remoteClients.Add(instance);
            ExtraClientLateSetup(instance);
        }

        // used by host and client
        protected async UniTask SetupPlayer(IClientInstance instance, bool spawnCharacter)
        {
            if (spawnCharacter)
            {
                instance.ClientObjectManager.RegisterPrefab(_characterPrefab);

                _serverInstance.SpawnCharacterForNew(_characterPrefab);
                // wait for client to spawn it
                await AsyncUtil.WaitUntilWithTimeout(() => instance.Client.Player.HasCharacter);
            }
            else
            {
                _serverInstance.AddNewPlayer();
            }

            instance.SetupPlayer(spawnCharacter);
        }

        public virtual void ExtraTearDown() { }
        public virtual UniTask ExtraTearDownAsync() => UniTask.CompletedTask;

        public async UniTask TearDownAsync()
        {
            // make sure all gameobject are added to base.toDestroy (duplicates are fine)

            foreach (var instance in _remoteClients)
            {
                instance.AddCleanupObjects(toDestroy);

                if (instance.Client.Active)
                    instance.Client.Disconnect();
            }

            // check active, it might have been stopped by tests
            _serverInstance.AddCleanupObjects(toDestroy);
            if (server.Active)
                server.Stop();

            foreach (var client in _remoteClients)
            {
                await AsyncUtil.WaitUntilWithTimeout(() => !client.Client.Active);
                Object.DestroyImmediate(client.GameObject);
            }
            await AsyncUtil.WaitUntilWithTimeout(() => !server.Active);


            TearDownTestObjects();

            ExtraTearDown();
            await ExtraTearDownAsync();

            // clear all references so that next test doesn't use them by mistake
            _serverInstance = null;
            _remoteClients.Clear();
            _clientNameIndex = 0;
            _characterPrefab = null;

            Console.WriteLine($"[MirageTest] UnityTearDown class:{TestContext.CurrentContext.Test.ClassName} method:{TestContext.CurrentContext.Test.MethodName}");
        }

        protected void RunOnAll(NetworkIdentity identity, Action<NetworkIdentity> change)
        {
            change.Invoke(_serverInstance.Get(identity));
            foreach (var remote in _remoteClients)
            {
                change.Invoke(remote.Get(identity));
            }
        }

        protected void RunOnAll<T>(T comp, Action<T> action) where T : NetworkBehaviour
        {
            action.Invoke(_serverInstance.Get(comp));
            foreach (var remote in _remoteClients)
            {
                action.Invoke(remote.Get(comp));
            }
        }

        protected void RunOnAllClients(Action<ClientInstance> action)
        {
            foreach (var remote in _remoteClients)
            {
                action.Invoke(remote);
            }
        }

        protected void RunOnAllClients(Action<ClientInstance, int> action)
        {
            for (var i = 0; i < _remoteClients.Count; i++)
            {
                var remote = _remoteClients[i];
                action.Invoke(remote, i);
            }
        }
    }
}
