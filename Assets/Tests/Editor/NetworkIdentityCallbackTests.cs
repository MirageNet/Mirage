using System;
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

        #endregion

        GameObject gameObject;
        NetworkIdentity identity;
        private ServerObjectManager serverObjectManager;
        private GameObject networkServerGameObject;

        [SetUp]
        public void SetUp()
        {
            networkServerGameObject = new GameObject();
            var server = networkServerGameObject.AddComponent<NetworkServer>();
            serverObjectManager = networkServerGameObject.AddComponent<ServerObjectManager>();
            serverObjectManager.Server = server;
            networkServerGameObject.AddComponent<NetworkClient>();

            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            _ = Substitute.For<INetworkPlayer>();
            _ = Substitute.For<INetworkPlayer>();
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
        public void GetSetAssetId()
        {
            // assign a guid
            int hash = 123456789;
            identity.PrefabHash = hash;

            // did it work?
            Assert.That(identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SetAssetId_GivesErrorIfOneExists()
        {
            int hash1 = "Assets/Prefab/myPrefab.asset".GetStableHashCode();
            identity.PrefabHash = hash1;

            // assign a guid
            int hash2 = "Assets/Prefab/myPrefab2.asset".GetStableHashCode();
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                identity.PrefabHash = hash2;
            });

            Assert.That(exception.Message, Is.EqualTo($"Can not Set AssetId on NetworkIdentity '{identity.name}' because it already had an assetId, current assetId '{hash1}', attempted new assetId '{hash2}'"));
            // guid was changed
            Assert.That(identity.PrefabHash, Is.EqualTo(hash1));
        }

        [Test]
        public void SetAssetId_GivesErrorForEmptyGuid()
        {
            int hash1 = "Assets/Prefab/myPrefab.asset".GetStableHashCode();
            identity.PrefabHash = hash1;

            // assign a guid
            int hash2 = 0;
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            {
                identity.PrefabHash = hash2;
            });

            Assert.That(exception.Message, Is.EqualTo($"Can not set AssetId to empty guid on NetworkIdentity '{identity.name}', old assetId '{hash1}'"));
            // guid was NOT changed
            Assert.That(identity.PrefabHash, Is.EqualTo(hash1));
        }
        [Test]
        public void SetAssetId_DoesNotGiveErrorIfBothOldAndNewAreEmpty()
        {
            Debug.Assert(identity.PrefabHash == 0, "assetId needs to be empty at the start of this test");
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
        public void AssignSceneID()
        {
            // OnValidate will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.sceneId, Is.Not.Zero);
            Assert.That(identity.sceneId & 0xFFFFFFFF00000000, Is.EqualTo(0x0000000000000000));

            // make sure that OnValidate added it to sceneIds dict
            Assert.That(NetworkIdentity.sceneIds[identity.sceneId], Is.Not.Null);
        }

        [Test]
        public void SetSceneIdSceneHashPartInternal()
        {
            // Awake will have assigned a random sceneId of format 0x00000000FFFFFFFF
            // -> make sure that one was assigned, and that the left part was
            //    left empty for scene hash
            Assert.That(identity.sceneId, !Is.Zero);
            Assert.That(identity.sceneId & 0xFFFFFFFF00000000, Is.EqualTo(0x0000000000000000));
            ulong rightPart = identity.sceneId;

            // set scene hash
            identity.SetSceneIdSceneHashPartInternal();

            // make sure that the right part is still the random sceneid
            Assert.That(identity.sceneId & 0x00000000FFFFFFFF, Is.EqualTo(rightPart));

            // make sure that the left part is a scene hash now
            Assert.That(identity.sceneId & 0xFFFFFFFF00000000, !Is.Zero);
            ulong finished = identity.sceneId;

            // calling it again should said the exact same hash again
            identity.SetSceneIdSceneHashPartInternal();
            Assert.That(identity.sceneId, Is.EqualTo(finished));
        }

        [Test]
        public void OnValidateSetupIDsSetsEmptyAssetIDForSceneObject()
        {
            // OnValidate will have been called. make sure that assetId was set
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
    }
}
