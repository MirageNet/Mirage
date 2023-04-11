using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Serialization;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Syncing
{
    public class MockPlayerHook : NetworkBehaviour
    {
        [SyncVar(hook = nameof(MyNumberEventChanged), invokeHookOnServer = true)]
        public int myNumberEvent;

        public event Action<int> MyNumberEventChanged;


        [SyncVar(hook = nameof(MyNumberMethodChanged), invokeHookOnServer = true)]
        public int myNumberMethod;

        public event Action<int> MyNumberMethodChangedCalled;
        public void MyNumberMethodChanged(int newValue)
        {
            MyNumberMethodChangedCalled?.Invoke(newValue);
        }
    }

    // todo find way to avoid having a full copy of this class
    public class SyncDirectionTestBase_Host : HostSetup<MockPlayer>
    {
        protected static readonly MockPlayer.Guild guild = new MockPlayer.Guild("Fun");
        protected static readonly MockPlayer.Guild guild2 = new MockPlayer.Guild("Other");

        protected readonly NetworkWriter _ownerWriter = new NetworkWriter(1300);
        protected readonly NetworkWriter _observersWriter = new NetworkWriter(1300);
        protected readonly MirageNetworkReader _reader = new MirageNetworkReader();

        protected ClientInstance<MockPlayer> _client2;

        /// <summary>
        /// Object that client1 Owns on client2
        /// </summary>
        protected MockPlayer ObserverComponent { get; private set; }

        protected NetworkIdentity ServerExtraIdentity { get; private set; }
        protected MockPlayer ServerExtraComponent { get; private set; }
        /// <summary>
        /// Object on the owner's instance, but is not owned by them
        /// </summary>
        protected NetworkIdentity OwnerExtraIdentity { get; private set; }
        protected MockPlayer OwnerExtraComponent { get; private set; }


        protected int _onSerializeCalled;
        protected int _onDeserializeCalled;
        protected INetworkPlayer _serverPlayer2;

        protected void ResetCounters()
        {
            _onSerializeCalled = 0;
            _onDeserializeCalled = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _ownerWriter.Reset();
            _observersWriter.Reset();
            _reader.Dispose();
        }

        public override async UniTask LateSetup()
        {
            _client2 = new ClientInstance<MockPlayer>(ClientConfig, (TestSocketFactory)server.SocketFactory);
            _client2.client.Connect("localhost");

            await AsyncUtil.WaitUntilWithTimeout(() => server.Players.Count > 1);

            // get new player
            _serverPlayer2 = server.Players.First(x => x != server.LocalPlayer);

            _client2.clientObjectManager.RegisterPrefab(playerPrefab);

            // wait for client and server to initialize themselves
            await UniTask.Yield();

            var serverCharacter2 = InstantiateForTest(playerPrefab);
            serverObjectManager.AddCharacter(_serverPlayer2, serverCharacter2);

            await AsyncUtil.WaitUntilWithTimeout(() => _client2.client.Player.HasCharacter);

            _client2.SetupCharacter();

            var found = _client2.client.World.TryGetIdentity(playerComponent.NetId, out var player1Character);
            if (!found)
                Debug.LogError("Could not find instance of player 1's character on client 2");
            ObserverComponent = player1Character.GetComponent<MockPlayer>();
            Debug.Assert(ObserverComponent != null);

            ServerExtraIdentity = InstantiateForTest(playerPrefab);
            ServerExtraComponent = ServerExtraIdentity.GetComponent<MockPlayer>();
            Debug.Assert(ServerExtraIdentity != null);
            serverObjectManager.Spawn(ServerExtraIdentity);

            await UniTask.Yield();

            if (client.World.TryGetIdentity(ServerExtraIdentity.NetId, out var ownerExtra))
            {
                OwnerExtraIdentity = ownerExtra;
                OwnerExtraComponent = ownerExtra.GetComponent<MockPlayer>();
            }

            playerComponent.OnSerializeCalled += () => _onSerializeCalled++;
            playerComponent.OnDeserializeCalled += () => _onDeserializeCalled++;
        }

        protected static void SetDirection(NetworkBehaviour behaviour, SyncFrom from, SyncTo to)
        {
            Debug.Assert(SyncSettings.IsValidDirection(from, to));

            behaviour.SyncSettings.From = from;
            behaviour.SyncSettings.To = to;
            behaviour._nextSyncTime = Time.time;
        }
        protected void SetDirection(SyncFrom from, SyncTo to)
        {
            SetDirection(playerComponent, from, to);
            SetDirection(ObserverComponent, from, to);
        }
    }

    public class SyncDirectionFromServer_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToOwner()
        {
            SetDirection(SyncFrom.Server, SyncTo.Owner);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            // Should not have serialized anything
            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.ObserversOnly);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }

        [UnityTest]
        public IEnumerator ToOwnerAndObserver()
        {
            SetDirection(SyncFrom.Server, SyncTo.OwnerAndObservers);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            // Should not have serialized anything
            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }
    }

    public class SyncDirectionFromOwner_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToServer()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = OwnerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = OwnerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }
    }

    public class SyncDirectionFromServerAndOwner_Host : SyncDirectionTestBase_Host
    {
        [UnityTest]
        public IEnumerator ToServerAndOwner()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.Zero);
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.Null.Or.Empty);
            Assert.That(ObserverComponent.target, Is.Null);
        }

        [UnityTest]
        public IEnumerator ToServerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.ObserversOnly);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;


            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }

        [UnityTest]
        public IEnumerator ToServerOwnerAndObservers()
        {
            SetDirection(SyncFrom.Server | SyncFrom.Owner, SyncTo.Server | SyncTo.Owner | SyncTo.ObserversOnly);

            ResetCounters();
            playerComponent.guild = guild;
            playerComponent.target = ServerExtraIdentity;

            // wait for sync
            yield return null;
            yield return null;

            Assert.That(_onSerializeCalled, Is.EqualTo(1));
            Assert.That(_onDeserializeCalled, Is.Zero);

            Assert.That(playerComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(playerComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));

            Assert.That(ObserverComponent.guild.name, Is.EqualTo(guild.name));
            Assert.That(ObserverComponent.target.NetId, Is.EqualTo(ServerExtraIdentity.NetId));
        }
    }

    public class SyncDirectionFromServerHostToOwner : SyncDirectionTestBase_Host
    {
        /// <summary>
        /// object on host, owned by client 2
        /// </summary>
        private MockPlayerHook hostWithHook;
        /// <summary>
        /// object on client 2, owned by client 2
        /// </summary>
        private MockPlayerHook ownerWithHook;

        public override async UniTask LateSetup()
        {
            await base.LateSetup();

            var prefab = CreateBehaviour<MockPlayerHook>(true).Identity;
            prefab.PrefabHash = 456;

            _client2.clientObjectManager.RegisterPrefab(prefab);
            clientObjectManager.RegisterPrefab(prefab);

            var clone = InstantiateForTest(prefab);
            clone.gameObject.SetActive(true);
            hostWithHook = clone.GetComponent<MockPlayerHook>();
            serverObjectManager.Spawn(clone, owner: _serverPlayer2);

            await UniTask.DelayFrame(2);

            if (_client2.client.World.TryGetIdentity(hostWithHook.NetId, out var ownerIdentity))
            {
                ownerWithHook = ownerIdentity.GetComponent<MockPlayerHook>();
            }
        }

        [UnityTest]
        public IEnumerator SettingValueOnHostInvokesHookEventForBoth()
        {
            var hostHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();
            hostWithHook.MyNumberEventChanged += (n) => hostHookInvoked.Add(n);
            ownerWithHook.MyNumberEventChanged += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            hostWithHook.myNumberEvent = Value1;


            Assert.That(hostHookInvoked, Has.Count.EqualTo(1), "hook not called on host");
            Assert.That(hostHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "hook not called on server");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(hostWithHook.myNumberEvent, Is.EqualTo(Value1));
            Assert.That(ownerWithHook.myNumberEvent, Is.EqualTo(Value1));
        }

        [UnityTest]
        public IEnumerator SettingValueOnHostInvokesHookMethodForBoth()
        {
            var hostHookInvoked = new List<int>();
            var ownerHookInvoked = new List<int>();
            hostWithHook.MyNumberMethodChangedCalled += (n) => hostHookInvoked.Add(n);
            ownerWithHook.MyNumberMethodChangedCalled += (n) => ownerHookInvoked.Add(n);

            const int Value1 = 10;

            hostWithHook.myNumberMethod = Value1;


            Assert.That(hostHookInvoked, Has.Count.EqualTo(1), "hook not called on host");
            Assert.That(hostHookInvoked[0], Is.EqualTo(Value1));

            yield return null;
            yield return null;


            Assert.That(ownerHookInvoked, Has.Count.EqualTo(1), "hook not called on server");
            Assert.That(ownerHookInvoked[0], Is.EqualTo(Value1));

            Assert.That(hostWithHook.myNumberMethod, Is.EqualTo(Value1));
            Assert.That(ownerWithHook.myNumberMethod, Is.EqualTo(Value1));
        }
    }
}
