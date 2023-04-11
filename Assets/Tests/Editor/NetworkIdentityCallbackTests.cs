using System;
using System.Collections.Generic;
using Mirage.Serialization;
using Mirage.Tests.EnterRuntime;
using Mirage.Tests.Runtime;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using static Mirage.Tests.LocalConnections;

namespace Mirage.Tests
{
    public class NetworkIdentityCallbackTests : ClientServerSetup_EditorModeTest<MockComponent>
    {
        #region test components

        private class CheckObserverExceptionNetworkBehaviour : NetworkVisibility
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

        private class CheckObserverTrueNetworkBehaviour : NetworkVisibility
        {
            public int called;
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
            public override bool OnCheckObserver(INetworkPlayer player)
            {
                ++called;
                return true;
            }
        }

        private class CheckObserverFalseNetworkBehaviour : NetworkVisibility
        {
            public int called;
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
            public override bool OnCheckObserver(INetworkPlayer player)
            {
                ++called;
                return false;
            }
        }

        private class SerializeTest1NetworkBehaviour : NetworkBehaviour
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

        private class SerializeTest2NetworkBehaviour : NetworkBehaviour
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

        private class SerializeExceptionNetworkBehaviour : NetworkBehaviour
        {
            public const string MESSAGE = "some unique exception";
            public override bool OnSerialize(NetworkWriter writer, bool initialState)
            {
                throw new Exception(MESSAGE);
            }
            public override void OnDeserialize(NetworkReader reader, bool initialState)
            {
                throw new Exception(MESSAGE);
            }
        }

        private class SerializeMismatchNetworkBehaviour : NetworkBehaviour
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

        private class RebuildObserversNetworkBehaviour : NetworkVisibility
        {
            public INetworkPlayer observer;
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
            {
                observers.Add(observer);
            }
        }

        private class RebuildEmptyObserversNetworkBehaviour : NetworkVisibility
        {
            public override bool OnCheckObserver(INetworkPlayer player) { return true; }
            public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize) { }
        }

        #endregion

        private GameObject gameObject;
        private NetworkIdentity identity;
        private INetworkPlayer player1;
        private INetworkPlayer player2;

        public override void ExtraSetup()
        {
            identity = CreateNetworkIdentity();
            gameObject = identity.gameObject;
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
        }

        [Test]
        public void OnStartServerTest()
        {
            // lets add a component to check OnStartserver
            var func1 = Substitute.For<UnityAction>();
            var func2 = Substitute.For<UnityAction>();

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
            var hash = 123456789;
            identity.PrefabHash = hash;

            // did it work?
            Assert.That(identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void CanReplacePrefabHash()
        {
            var hash1 = 123;
            identity.PrefabHash = hash1;

            // assign a guid
            var hash2 = 1234;
            identity.PrefabHash = hash2;

            Assert.That(identity.PrefabHash, Is.EqualTo(hash2));
        }

        [Test]
        public void ThrowsIfSettingZero()
        {
            var hash = 123;
            identity.PrefabHash = hash;

            var exception = Assert.Throws<ArgumentException>(() =>
            {
                identity.PrefabHash = 0;
            });

            Assert.That(exception.Message, Is.EqualTo($"Cannot set PrefabHash to 0 on '{identity.name}'. Old PrefabHash '{hash}'."));

            // guid was NOT changed
            Assert.That(identity.PrefabHash, Is.EqualTo(hash));
        }

        [Test]
        public void SetClientOwner()
        {
            // SetClientOwner
            (_, var original) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            identity.SetClientOwner(original);
            Assert.That(identity.Owner, Is.EqualTo(original));
        }

        [Test]
        public void SetOverrideClientOwner()
        {
            // SetClientOwner
            (_, var original) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
            identity.SetClientOwner(original);

            // setting it when it's already set shouldn't overwrite the original
            (_, var overwrite) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
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
            var player = Substitute.For<INetworkPlayer>();
            identity.observers.Add(player);

            var player2 = Substitute.For<INetworkPlayer>();
            // RemoveObserverInternal with invalid connection should do nothing
            identity.RemoveObserverInternal(player2);
            Assert.That(identity.observers, Is.EquivalentTo(new[] { player }));

            // RemoveObserverInternal with existing connection should remove it
            identity.RemoveObserverInternal(player);
            Assert.That(identity.observers, Is.Empty);
        }

        [Test]
        public void OnStartServerCallsComponentsAndCatchesExceptions()
        {
            // make a mock delegate
            var func = Substitute.For<UnityAction>();

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
            var func = Substitute.For<UnityAction>();
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
            var func = Substitute.For<UnityAction<bool>>();
            identity.OnAuthorityChanged.AddListener(func);

            func
                .When(f => f.Invoke(Arg.Any<bool>()))
                .Do(f => { throw new Exception("Some exception"); });

            // make sure exceptions are not swallowed
            Assert.Throws<Exception>(() =>
            {
                identity.CallStartAuthority();
            });
            func.Received(1).Invoke(Arg.Any<bool>());
        }

        [Test]
        public void NotifyAuthorityCallsOnStartStopAuthority()
        {
            var startAuth = 0;
            var stopAuth = 0;
            // NotifyAuthority needs world to be set. this different than the owner case because world will always be set in the client's case
            identity.World = new NetworkWorld();
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

            var compTrue = CreateBehaviour<CheckObserverTrueNetworkBehaviour>();
            Assert.That(compTrue.Identity.OnCheckObserver(player1), Is.True);
            Assert.That(compTrue.called, Is.EqualTo(1));
        }

        [Test]
        public void OnCheckObserverFalse()
        {
            // create a networkidentity with a component that returns true and
            // one component that returns false.
            // result should still be false if any one returns false.
            var compFalse = CreateBehaviour<CheckObserverFalseNetworkBehaviour>();
            Assert.That(compFalse.Identity.OnCheckObserver(player1), Is.False);
            Assert.That(compFalse.called, Is.EqualTo(1));
        }

        [Test]
        public void OnSerializeAllShouldPropagateExceptions()
        {
            // create a networkidentity with our test components
            var comp1 = gameObject.AddComponent<SerializeTest1NetworkBehaviour>();
            var compExc = gameObject.AddComponent<SerializeExceptionNetworkBehaviour>();
            var comp2 = gameObject.AddComponent<SerializeTest2NetworkBehaviour>();

            // object must be spawned for OnSerializeAll to work
            // it will check for if server is active when using From.Server

            // serialize should propagate exceptions
            var exception = Assert.Throws<Exception>(() =>
            {
                // we can't call identity.OnSerializeAll(true,...) manually because server needs to be set for it to sync,
                // but calling spawn will invoke OnSerializeAll(true,...), so we can call that to check for Exception
                serverObjectManager.Spawn(identity);
            });

            // check that it was the exception from out bad test component 
            Assert.That(exception, Has.Message.EqualTo(SerializeExceptionNetworkBehaviour.MESSAGE));
        }

        // OnSerializeAllSafely supports at max 64 components, because our
        // dirty mask is ulong and can only handle so many bits.
        [Test]
        public void NoMoreThan64Components()
        {
            // add byte.MaxValue+1 components
            for (var i = 0; i < byte.MaxValue + 1; ++i)
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
            var comp1 = gameObject.AddComponent<SerializeTest1NetworkBehaviour>();
            gameObject.AddComponent<SerializeMismatchNetworkBehaviour>();
            var comp2 = gameObject.AddComponent<SerializeTest2NetworkBehaviour>();

            // object must be spawned for OnSerializeAll to work
            // it will check for if server is active when using From.Server
            serverObjectManager.Spawn(identity);

            // set some unique values to serialize
            comp1.value = 12345;
            comp2.value = "67890";

            // serialize
            var ownerWriter = new NetworkWriter(1300);
            var observersWriter = new NetworkWriter(1300);
            var (ownerWritten, observersWritten) = identity.OnSerializeAll(true, ownerWriter, observersWriter);

            Assert.That(ownerWritten, Is.EqualTo(0), "no owner, should have only written to observersWriter");
            Assert.That(observersWritten, Is.GreaterThanOrEqualTo(1), "should have written to observer writer");

            // reset component values
            comp1.value = 0;
            comp2.value = null;

            // deserialize all
            var reader = new NetworkReader();
            reader.Reset(observersWriter.ToArraySegment());
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
            var funcEx = Substitute.For<UnityAction>();
            var func = Substitute.For<UnityAction>();

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
            var mockCallback = Substitute.For<UnityAction>();
            identity.OnStopClient.AddListener(mockCallback);

            identity.StopClient();

            mockCallback.Received().Invoke();
        }

        [Test]
        public void OnStopServer()
        {
            var mockCallback = Substitute.For<UnityAction>();
            identity.OnStopServer.AddListener(mockCallback);

            identity.StopServer();

            mockCallback.Received().Invoke();
        }

        [Test]
        public void OnStopServerEx()
        {
            var mockCallback = Substitute.For<UnityAction>();
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
            var comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
            comp.observer = player1;

            // get new observers
            var observers = new HashSet<INetworkPlayer>();
            identity.GetNewObservers(observers, true);
            Assert.That(observers.Count, Is.EqualTo(1));
            Assert.That(observers.Contains(comp.observer), Is.True);
        }

        [Test]
        public void GetNewObserversClearsHashSet()
        {
            var sub1 = Substitute.For<INetworkPlayer>();
            var sub2 = Substitute.For<INetworkPlayer>();

            // get new observers. no observer components so it should just clear
            // it and not do anything else
            var observers = new HashSet<INetworkPlayer>
            {
                // add values that GetNewObservers wont add itself
               sub1, sub2
            };

            identity.GetNewObservers(observers, true);
            Assert.That(observers, Does.Not.Contains(sub1));
            Assert.That(observers, Does.Not.Contains(sub2));
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
            (_, var connection) = PipedConnections(Substitute.For<IMessageReceiver>(), Substitute.For<IMessageReceiver>());
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
            var comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
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
            var comp = gameObject.AddComponent<RebuildObserversNetworkBehaviour>();
            comp.observer = Substitute.For<INetworkPlayer>();
            comp.observer.SceneIsReady.Returns(false);

            // rebuild observers should add all component's ready observers
            identity.RebuildObservers(true);
            Assert.That(identity.observers, Is.Empty);
        }

        [Test]
        public void RebuildObserversAddsReadyServerConnectionsIfNotImplemented()
        {
            var readyConnection = Substitute.For<INetworkPlayer>();
            readyConnection.SceneIsReady.Returns(true);
            var notReadyConnection = Substitute.For<INetworkPlayer>();
            notReadyConnection.SceneIsReady.Returns(false);

            // add some server connections
            server.AddTestPlayer(readyConnection);
            server.AddTestPlayer(notReadyConnection);

            // rebuild observers should add all ready server connections
            // because no component implements OnRebuildObservers
            identity.RebuildObservers(true);
            // should also include the serverPlayer that was added by test base
            Assert.That(identity.observers, Is.EquivalentTo(new[] { serverPlayer, readyConnection }));
        }

    }
}
