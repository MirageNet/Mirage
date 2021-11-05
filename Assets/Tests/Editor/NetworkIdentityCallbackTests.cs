using System;
using System.Collections.Generic;
using Mirage.Serialization;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using static Mirage.Tests.LocalConnections;
using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    public class NetworkIdentityCallbackTests
    {
        #region test components

        class CheckObserverExceptionNetworkBehaviour : NetworkVisibility
        {
            public int called;
            public INetworkPlayer valuePassed;
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
            public override bool OnCheckObserver(INetworkPlayer player)
            {
                ++called;
                valuePassed = player;
                throw new Exception("some exception");
            }
        }

        class CheckObserverTrueNetworkBehaviour : NetworkVisibility
        {
            public int called;
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
            public override bool OnCheckObserver(INetworkPlayer player)
            {
                ++called;
                return true;
            }
        }

        class CheckObserverFalseNetworkBehaviour : NetworkVisibility
        {
            public int called;
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
            public override bool OnCheckObserver(INetworkPlayer player)
            {
                ++called;
                return false;
            }
        }

        class SerializeTest1NetworkBehaviour : NetworkBehaviour
        {
            public int value;
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                writer.WriteInt32(value);
                return true;
            }
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                value = reader.ReadInt32();
            }
        }

        class SerializeTest2NetworkBehaviour : NetworkBehaviour
        {
            public string value;
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                writer.WriteString(value);
                return true;
            }
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                value = reader.ReadString();
            }
        }

        class SerializeExceptionNetworkBehaviour : NetworkBehaviour
        {
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                throw new Exception("some exception");
            }
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                throw new Exception("some exception");
            }
        }

        class SerializeMismatchNetworkBehaviour : NetworkBehaviour
        {
            public int value;
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                writer.WriteInt32(value);
                // one too many
                writer.WriteInt32(value);
                return true;
            }
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                value = reader.ReadInt32();
            }
        }

        class RebuildObserversNetworkBehaviour : NetworkVisibility
        {
            public INetworkPlayer observer;
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
            {
                observers.Add(observer);
            }
        }

        class RebuildEmptyObserversNetworkBehaviour : NetworkVisibility
        {
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
        }

        #endregion

        GameObject gameObject;
        NetworkIdentity identity;
        private NetworkServer server;
        private ServerObjectManager serverObjectManager;
        private GameObject networkServerGameObject;

        INetworkPlayer player1;
        INetworkPlayer player2;

        [SetUp]
        public void SetUp()
        {
            networkServerGameObject = new GameObject();
            server = networkServerGameObject.AddComponent<NetworkServer>();
            serverObjectManager = networkServerGameObject.AddComponent<ServerObjectManager>();
            serverObjectManager.Server = server;
            networkServerGameObject.AddComponent<NetworkClient>();

            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }

        [TearDown]
        public void TearDown()
        {
            // set isServer is false. otherwise Destroy instead of
            // DestroyImmediate is called internally, giving an error in Editor
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(networkServerGameObject);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void OnStartServerTest()
        {
            // lets add a component to check OnStartserver
            UnityAction func1 = Substitute.For<UnityAction>();
            UnityAction func2 = Substitute.For<UnityAction>();

            identity.OnStartServer.AddListener(func1);
            identity.OnStartServer.AddListener(func2);

            identity.StartServer();

            func1.Received(1).Invoke();
            func2.Received(1).Invoke();
        }

        [Test]
        public void GetSetPrefabHash()
        {
            // assign a guid
            int hash = 123456789;
            identity.PrefabHash = hash;

            // did it work?
            Assert.That(identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SetPrefabHash_GivesErrorIfOneExists()
        {
            int hash1 = "Assets/Prefab/myPrefab.asset".GetStableHashCode();
            identity.PrefabHash = hash1;

            // assign a guid
            int hash2 = "Assets/Prefab/myPrefab2.asset".GetStableHashCode();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                identity.PrefabHash = hash2;
            });

            Assert.That(exception.Message, Is.EqualTo($"Can not Set PrefabHash on NetworkIdentity '{identity.name}' because it already had an PrefabHash, current PrefabHash '{hash1}', attempted new PrefabHash '{hash2}'"));
            // guid was changed
            Assert.That(identity.PrefabHash, Is.EqualTo(hash1));
        }

        [Test]
        public void SetPrefabHash_GivesErrorForEmptyGuid()
        {
            int hash1 = "Assets/Prefab/myPrefab.asset".GetStableHashCode();
            identity.PrefabHash = hash1;

            // assign a guid
            int hash2 = 0;
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                identity.PrefabHash = hash2;
            });

            Assert.That(exception.Message, Is.EqualTo($"Can not set PrefabHash to empty guid on NetworkIdentity '{identity.name}', old PrefabHash '{hash1}'"));
            // guid was NOT changed
            Assert.That(identity.PrefabHash, Is.EqualTo(hash1));
        }
        [Test]
        public void SetPrefabHash_DoesNotGiveErrorIfBothOldAndNewAreEmpty()
        {
            Debug.Assert(identity.PrefabHash == 0, "PrefabHash needs to be empty at the start of this test");
            // assign a guid
            int hash2 = 0;
            // expect no errors
            identity.PrefabHash = hash2;

            // guid was still empty
            Assert.That(identity.PrefabHash, Is.EqualTo(0));
        }

        [Test]
        public void SetClientOwner()
        {
            // SetClientOwner
            (_, NetworkPlayer original) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            identity.SetClientOwner(original);
            Assert.That(identity.Owner, Is.EqualTo(original));
        }

        [Test]
        public void SetOverrideClientOwner()
        {
            // SetClientOwner
            (_, NetworkPlayer original) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            identity.SetClientOwner(original);

            // setting it when it's already set shouldn't overwrite the original
            (_, NetworkPlayer overwrite) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            // will log a warning
            Assert.Throws<InvalidOperationException>(() =>
            {
                identity.SetClientOwner(overwrite);
            });

            Assert.That(identity.Owner, Is.EqualTo(original));
        }

        [Test]
        public void RemoveObserverInternal()
        {
            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // add an observer connection
            INetworkPlayer player = Substitute.For<INetworkPlayer>();
            identity.observers.Add(player);

            INetworkPlayer player2 = Substitute.For<INetworkPlayer>();
            // RemoveObserverInternal with invalid connection should do nothing
            identity.RemoveObserverInternal(player2);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player }));

            // RemoveObserverInternal with existing connection should remove it
            identity.RemoveObserverInternal(player);
            Assert.That(identity.observers, Is.Empty);
        }

        [Test]
        public void AssignSceneID()
        {
            // OnValidate will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.SceneId, Is.Not.Zero);
            Assert.That(identity.SceneId & 0xFFFFFFFF00000000ul, Is.Zero);

            // make sure that OnValidate added it to sceneIds dict
            Assert.That(NetworkIdentityIdGenerator.sceneIds[(int)(identity.SceneId & 0x00000000FFFFFFFFul)], Is.Not.Null);
        }

        [Test]
        public void SetSceneIdSceneHashPartInternal()
        {
            // Awake will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.SceneId, Is.Not.Zero);
            Assert.That(identity.SceneId & 0xFFFFFFFF00000000, Is.Zero, "scene hash should start empty");
            ulong originalId = identity.SceneId;

            // set scene hash
            NetworkIdentityIdGenerator.SetSceneHash(identity);

            ulong newSceneId = identity.SceneId;
            ulong newID = newSceneId & 0x00000000FFFFFFFF;
            ulong newHash = newSceneId & 0xFFFFFFFF00000000;

            // make sure that the right part is still the random sceneid
            Assert.That(newID, Is.EqualTo(originalId));

            // make sure that the left part is a scene hash now
            Assert.That(newHash, Is.Not.Zero);

            // calling it again should said the exact same hash again
            NetworkIdentityIdGenerator.SetSceneHash(identity);
            Assert.That(identity.SceneId, Is.EqualTo(newSceneId), "should be same value as first time it was called");
        }

        [Test]
        public void OnValidateSetupIDsSetsEmptyPrefabHashForSceneObject()
        {
            // OnValidate will have been called. make sure that PrefabHash was set
            // to 0 empty and not anything else, because this is a scene object
            Assert.That(identity.PrefabHash, Is.EqualTo(0));
        }

        [Test]
        public void OnStartServerCallsComponentsAndCatchesExceptions()
        {
            // make a mock delegate
            UnityAction func = Substitute.For<UnityAction>();

            // add it to the listener
            identity.OnStartServer.AddListener(func);

            // Since we are testing that exceptions are not swallowed,
            // when the mock is invoked, throw an exception 
            func
                .When(f => f.Invoke())
                .Do(f => { throw new Exception("Some exception"); });

            // Make sure that the exception is not swallowed
            Assert.Throws<Exception>(() =>
            {
                identity.StartServer();
            });

            // ask the mock if it got invoked
            // if the mock is not invoked,  then this fails
            // This is a type of assert
            func.Received().Invoke();
        }

        [Test]
        public void OnStartClientCallsComponentsAndCatchesExceptions()
        {
            // add component
            UnityAction func = Substitute.For<UnityAction>();
            identity.OnStartClient.AddListener(func);

            func
                .When(f => f.Invoke())
                .Do(f => { throw new Exception("Some exception"); });

            // make sure exceptions are not swallowed
            Assert.Throws<Exception>(() =>
            {
                identity.StartClient();
            });
            func.Received().Invoke();

            // we have checks to make sure that it's only called once.
            Assert.DoesNotThrow(() =>
            {
                identity.StartClient();
            });
            func.Received(1).Invoke();
        }

        [Test]
        public void OnAuthorityChangedCallsComponentsAndCatchesExceptions()
        {
            // add component
            UnityAction<bool> func = Substitute.For<UnityAction<bool>>();
            identity.OnAuthorityChanged.AddListener(func);

            func
                .When(f => f.Invoke(Arg.Any<bool>()))
                .Do(f => { throw new Exception("Some exception"); });

            // make sure exceptions are not swallowed
            Assert.Throws<Exception>(() =>
            {
                identity.StartAuthority();
            });
            func.Received(1).Invoke(Arg.Any<bool>());
        }

        [Test]
        public void NotifyAuthorityCallsOnStartStopAuthority()
        {
            int startAuth = 0;
            int stopAuth = 0;
            identity.OnAuthorityChanged.AddListener(auth =>
            {
                if (auth) startAuth++;
                else stopAuth++;
            });

            // set authority from false to true, which should call OnStartAuthority
            identity.HasAuthority = true;
            identity.NotifyAuthority();
            // shouldn't be touched
            Assert.That(identity.HasAuthority, Is.True);
            // start should be called
            Assert.That(startAuth, Is.EqualTo(1));
            Assert.That(stopAuth, Is.EqualTo(0));

            // set it to true again, should do nothing because already true
            identity.HasAuthority = true;
            identity.NotifyAuthority();
            // shouldn't be touched
            Assert.That(identity.HasAuthority, Is.True);
            // same as before
            Assert.That(startAuth, Is.EqualTo(1));
            Assert.That(stopAuth, Is.EqualTo(0));

            // set it to false, should call OnStopAuthority
            identity.HasAuthority = false;
            identity.NotifyAuthority();
            // shouldn't be touched
            Assert.That(identity.HasAuthority, Is.False);
            // same as before
            Assert.That(startAuth, Is.EqualTo(1));
            Assert.That(stopAuth, Is.EqualTo(1));

            // set it to false again, should do nothing because already false
            identity.HasAuthority = false;
            identity.NotifyAuthority();
            // shouldn't be touched
            Assert.That(identity.HasAuthority, Is.False);
            // same as before
            Assert.That(startAuth, Is.EqualTo(1));
            Assert.That(stopAuth, Is.EqualTo(1));
        }

        [Test]
        public void OnCheckObserverCatchesException()
        {
            // add component
            gameObject.AddComponent<CheckObserverExceptionNetworkBehaviour>();

            // should catch the exception internally and not throw it
            Assert.Throws<Exception>(() =>
            {
                identity.OnCheckObserver(player1);
            });
        }

        [Test]
        public void OnCheckObserverTrue()
        {
            // create a networkidentity with a component that returns true
            // result should still be true.
            var gameObjectTrue = new GameObject();
            NetworkIdentity identityTrue = gameObjectTrue.AddComponent<NetworkIdentity>();
            CheckObserverTrueNetworkBehaviour compTrue = gameObjectTrue.AddComponent<CheckObserverTrueNetworkBehaviour>();
            Assert.That(identityTrue.OnCheckObserver(player1), Is.True);
            Assert.That(compTrue.called, Is.EqualTo(1));
        }

        [Test]
        public void OnCheckObserverFalse()
        {
            // create a networkidentity with a component that returns true and
            // one component that returns false.
            // result should still be false if any one returns false.
            var gameObjectFalse = new GameObject();
            NetworkIdentity identityFalse = gameObjectFalse.AddComponent<NetworkIdentity>();
            CheckObserverFalseNetworkBehaviour compFalse = gameObjectFalse.AddComponent<CheckObserverFalseNetworkBehaviour>();
            Assert.That(identityFalse.OnCheckObserver(player1), Is.False);
            Assert.That(compFalse.called, Is.EqualTo(1));
        }

        [Test]
        public void OnSerializeAllSafely()
        {
            // create a networkidentity with our test components
            SerializeTest1NetworkBehaviour comp1 = gameObject.AddComponent<SerializeTest1NetworkBehaviour>();
            SerializeExceptionNetworkBehaviour compExc = gameObject.AddComponent<SerializeExceptionNetworkBehaviour>();
            SerializeTest2NetworkBehaviour comp2 = gameObject.AddComponent<SerializeTest2NetworkBehaviour>();

            // set some unique values to serialize
            comp1.value = 12345;
            comp1.syncMode = SyncMode.Observers;
            compExc.syncMode = SyncMode.Observers;
            comp2.value = "67890";
            comp2.syncMode = SyncMode.Owner;

            // serialize all
            var ownerWriter = new NetworkWriter(1300);
            var observersWriter = new NetworkWriter(1300);

            // serialize should propagate exceptions
            Assert.Throws<Exception>(() =>
            {
                identity.OnSerializeAll(true, ownerWriter, observersWriter);
            });
        }

        // OnSerializeAllSafely supports at max 64 components, because our
        // dirty mask is ulong and can only handle so many bits.
        [Test]
        public void NoMoreThan64Components()
        {
            // add byte.MaxValue+1 components
            for (int i = 0; i < byte.MaxValue + 1; ++i)
            {
                gameObject.AddComponent<SerializeTest1NetworkBehaviour>();
            }
            // ingore error from creating cache (has its own test)
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = identity.NetworkBehaviours;
            });
        }

        // OnDeserializeSafely should be able to detect and handle serialization
        // mismatches (= if compA writes 10 bytes but only reads 8 or 12, it
        // shouldn't break compB's serialization. otherwise we end up with
        // insane runtime errors like monsters that look like npcs. that's what
        // happened back in the day with UNET).
        [Test]
        public void OnDeserializeSafelyShouldDetectAndHandleDeSerializationMismatch()
        {
            // add components
            SerializeTest1NetworkBehaviour comp1 = gameObject.AddComponent<SerializeTest1NetworkBehaviour>();
            gameObject.AddComponent<SerializeMismatchNetworkBehaviour>();
            SerializeTest2NetworkBehaviour comp2 = gameObject.AddComponent<SerializeTest2NetworkBehaviour>();

            // set some unique values to serialize
            comp1.value = 12345;
            comp2.value = "67890";

            // serialize
            var ownerWriter = new NetworkWriter(1300);
            var observersWriter = new NetworkWriter(1300);
            identity.OnSerializeAll(true, ownerWriter, observersWriter);

            // reset component values
            comp1.value = 0;
            comp2.value = null;

            // deserialize all
            var reader = new NetworkReader();
            reader.Reset(ownerWriter.ToArraySegment());
            Assert.Throws<DeserializeFailedException>(() =>
            {
                identity.OnDeserializeAll(reader, true);
            });
            reader.Dispose();
        }

        [Test]
        public void OnStartLocalPlayer()
        {
            // add components
            UnityAction funcEx = Substitute.For<UnityAction>();
            UnityAction func = Substitute.For<UnityAction>();

            identity.OnStartLocalPlayer.AddListener(funcEx);
            identity.OnStartLocalPlayer.AddListener(func);

            funcEx
                .When(f => f.Invoke())
                .Do(f => { throw new Exception("Some exception"); });


            // make sure that comp.OnStartServer was called
            // the exception was caught and not thrown in here.
            Assert.Throws<Exception>(() =>
            {
                identity.StartLocalPlayer();
            });

            funcEx.Received(1).Invoke();
            //Due to the order the listeners are added the one without exception is never called
            func.Received(0).Invoke();

            // we have checks to make sure that it's only called once.
            // let's see if they work.
            Assert.DoesNotThrow(() =>
            {
                identity.StartLocalPlayer();
            });
            // same as before?
            funcEx.Received(1).Invoke();
            //Due to the order the listeners are added the one without exception is never called
            func.Received(0).Invoke();
        }

        [Test]
        public void OnStopClient()
        {
            UnityAction mockCallback = Substitute.For<UnityAction>();
            identity.OnStopClient.AddListener(mockCallback);

            identity.StopClient();

            mockCallback.Received().Invoke();
        }

        [Test]
        public void OnStopServer()
        {
            UnityAction mockCallback = Substitute.For<UnityAction>();
            identity.OnStopServer.AddListener(mockCallback);

            identity.StopServer();

            mockCallback.Received().Invoke();
        }

        [Test]
        public void OnStopServerEx()
        {
            UnityAction mockCallback = Substitute.For<UnityAction>();
            mockCallback
                .When(f => f.Invoke())
                .Do(f => { throw new Exception("Some exception"); });

            identity.OnStopServer.AddListener(mockCallback);

            Assert.Throws<Exception>(() =>
            {
                identity.StopServer();
            });
        }

        [Test]
        public void AddObserver()
        {
            identity.Server = server;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // call AddObservers
            identity.AddObserver(player1);
            identity.AddObserver(player2);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player1, player2 }));

            // adding a duplicate connectionId shouldn't overwrite the original
            identity.AddObserver(player1);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player1, player2 }));
        }

        [Test]
        public void ClearObservers()
        {
            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // add some observers
            identity.observers.Add(player1);
            identity.observers.Add(player2);

            // call ClearObservers
            identity.ClearObservers();
            Assert.That(identity.observers.Count, Is.EqualTo(0));
        }


        [Test]
        public void Reset()
        {
            // creates .observers and generates a netId
            identity.StartServer();
            identity.Owner = player1;
            identity.observers.Add(player1);

            // mark for reset and reset
            identity.NetworkReset();
            Assert.That(identity.NetId, Is.EqualTo(0));
            Assert.That(identity.Owner, Is.Null);
        }

        [Test]
        public void GetNewObservers()
        {
            // add components
            RebuildObserversNetworkBehaviour comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
            comp.observer = player1;

            // get new observers
            var observers = new HashSet<INetworkPlayer>();
            bool result = identity.GetNewObservers(observers, true);
            Assert.That(result, Is.True);
            Assert.That(observers.Count, Is.EqualTo(1));
            Assert.That(observers.Contains(comp.observer), Is.True);
        }

        [Test]
        public void GetNewObserversClearsHashSet()
        {
            // get new observers. no observer components so it should just clear
            // it and not do anything else
            var observers = new HashSet<INetworkPlayer>
            {
                player1
            };
            identity.GetNewObservers(observers, true);
            Assert.That(observers.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNewObserversFalseIfNoComponents()
        {
            // get new observers. no observer components so it should be false
            var observers = new HashSet<INetworkPlayer>();
            bool result = identity.GetNewObservers(observers, true);
            Assert.That(result, Is.False);
        }

        // RebuildObservers should always add the own ready connection
        // (if any). fixes https://github.com/vis2k/Mirror/issues/692
        [Test]
        public void RebuildObserversDoesNotAddPlayerIfNotReady()
        {
            // add at least one observers component, otherwise it will just add
            // all server connections
            gameObject.AddComponent<RebuildEmptyObserversNetworkBehaviour>();

            // add own player connection that isn't ready
            (_, NetworkPlayer connection) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            // set not ready (ready is default true now)
            connection.SceneIsReady = false;

            identity.Owner = connection;

            // call OnStartServer so that observers dict is created
            identity.StartServer();

            // rebuild shouldn't add own player because conn wasn't set ready
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Does.Not.Contains(identity.Owner));
        }

        [Test]
        public void RebuildObserversAddsReadyConnectionsIfImplemented()
        {

            // add a proximity checker
            // one with a ready connection, one with no ready connection, one with null connection
            RebuildObserversNetworkBehaviour comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
            comp.observer = Substitute.For<INetworkPlayer>();
            comp.observer.SceneIsReady.Returns(true);

            // rebuild observers should add all component's ready observers
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { comp.observer }));
        }


        [Test]
        public void RebuildObserversDoesntAddNotReadyConnectionsIfImplemented()
        {
            // add a proximity checker
            // one with a ready connection, one with no ready connection, one with null connection
            RebuildObserversNetworkBehaviour comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
            comp.observer = Substitute.For<INetworkPlayer>();
            comp.observer.SceneIsReady.Returns(false);

            // rebuild observers should add all component's ready observers
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Is.Empty);
        }

        [Test]
        public void RebuildObserversAddsReadyServerConnectionsIfNotImplemented()
        {
            INetworkPlayer readyConnection = Substitute.For<INetworkPlayer>();
            readyConnection.SceneIsReady.Returns(true);
            INetworkPlayer notReadyConnection = Substitute.For<INetworkPlayer>();
            notReadyConnection.SceneIsReady.Returns(false);

            // add some server connections
            server.Players.Add(readyConnection);
            server.Players.Add(notReadyConnection);

            // rebuild observers should add all ready server connections
            // because no component implements OnRebuildObservers
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { readyConnection }));
        }

    }
}
