using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Mirror.Tests.AsyncTests;

namespace Mirror.Tests
{
    struct TestMessage : IMessageBase
    {
        public int IntValue;
        public string StringValue;
        public double DoubleValue;

        public TestMessage(int i, string s, double d)
        {
            IntValue = i;
            StringValue = s;
            DoubleValue = d;
        }

        public void Deserialize(NetworkReader reader)
        {
            IntValue = reader.ReadInt32();
            StringValue = reader.ReadString();
            DoubleValue = reader.ReadDouble();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.WriteInt32(IntValue);
            writer.WriteString(StringValue);
            writer.WriteDouble(DoubleValue);
        }
    }

    struct WovenTestMessage : IMessageBase
    {
        public int IntValue;
        public string StringValue;
        public double DoubleValue;

        public void Deserialize(NetworkReader reader) { }
        public void Serialize(NetworkWriter writer) { }
    }

    public class OnStartClientTestNetworkBehaviour : NetworkBehaviour
    {
        // counter to make sure that it's called exactly once
        public int called;
        public override void OnStartClient() { ++called; }
    }

    public class OnNetworkDestroyTestNetworkBehaviour : NetworkBehaviour
    {
        // counter to make sure that it's called exactly once
        public int called;
        public override void OnNetworkDestroy() { ++called; }
    }

    [TestFixture]
    public class NetworkServerTest 
    {
        NetworkServer server;
        GameObject serverGO;
        NetworkClient client;
        GameObject clientGO;

        GameObject gameObject;
        NetworkIdentity identity;

        NetworkConnectionToClient conn42;
        NetworkConnectionToClient conn43;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            serverGO = new GameObject();
            Tcp2.Tcp2Transport transport = serverGO.AddComponent<Tcp2.Tcp2Transport>();
            server = serverGO.AddComponent<NetworkServer>();
            server.Transport2 = transport;

            clientGO = new GameObject();
            client = clientGO.AddComponent<NetworkClient>();

            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();

            conn42 = new NetworkConnectionToClient(Substitute.For<IConnection>());
            conn43 = new NetworkConnectionToClient(Substitute.For<IConnection>());

            return RunAsync(async () =>
           {
               await server.ListenAsync();
           });

        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);

            // reset all state
            server.Shutdown();
            UnityEngine.Object.DestroyImmediate(serverGO);
            UnityEngine.Object.DestroyImmediate(clientGO);
        }

        [Test]
        public void NoConnections()
        {
            Assert.That(server.connections, Is.Empty);
        }

        [Test]
        public void IsActiveTest()
        {
            Assert.That(server.active, Is.True);
            server.Shutdown();
            Assert.That(server.active, Is.False);
        }

        [Test]
        public void MaxConnectionsTest()
        {
            server.MaxConnections = 1;
            // connect first: should work
            Transport.activeTransport.OnServerConnected.Invoke(42);
            Assert.That(server.connections.Count, Is.EqualTo(1));

            // connect second: should fail
            Transport.activeTransport.OnServerConnected.Invoke(43);
            Assert.That(server.connections.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConnectionsDictTest()
        {
            // connect first
            Transport.activeTransport.OnServerConnected.Invoke(42);
            Assert.That(server.connections, Has.Count.EqualTo(1));

            // connect second
            Transport.activeTransport.OnServerConnected.Invoke(43);
            Assert.That(server.connections, Has.Count.EqualTo(2));

            // disconnect second
            Transport.activeTransport.OnServerDisconnected.Invoke(43);
            Assert.That(server.connections, Has.Count.EqualTo(1));

            // disconnect first
            Transport.activeTransport.OnServerDisconnected.Invoke(42);
            Assert.That(server.connections, Is.Empty);
        }

        [Test]
        public void OnConnectedOnlyAllowsGreaterZeroConnectionIdsTest()
        {
            // connect 0
            // (it will show an error message, which is expected)
            LogAssert.ignoreFailingMessages = true;
            Transport.activeTransport.OnServerConnected.Invoke(0);
            Assert.That(server.connections, Is.Empty);

            // connect <0
            Transport.activeTransport.OnServerConnected.Invoke(-1);
            Assert.That(server.connections, Is.Empty);
            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void ConnectDuplicateConnectionIdsTest()
        {
            // connect first
            Transport.activeTransport.OnServerConnected.Invoke(42);
            Assert.That(server.connections, Has.Count.EqualTo(1));
            NetworkConnectionToClient original = server.connections.First();

            // connect duplicate - shouldn't overwrite first one
            Transport.activeTransport.OnServerConnected.Invoke(42);
            Assert.That(server.connections, Is.EquivalentTo(new[] { original }));
        }

        [Test]
        public void LocalClientActiveTest()
        {
            Assert.That(server.LocalClientActive, Is.False);

            client.ConnectHost(server);

            Assert.That(server.LocalClientActive, Is.True);

            client.Disconnect();
        }

        [Test]
        public void AddConnectionTest()
        {
            // add first connection
            server.AddConnection(conn42);
            Assert.That(server.connections, Is.EquivalentTo ( new[] { conn42 }));

            // add second connection
            server.AddConnection(conn43);
            Assert.That(server.connections, Is.EquivalentTo(new[] { conn42, conn43 }));

            // add duplicate connectionId
            server.AddConnection(conn43);
            Assert.That(server.connections, Is.EquivalentTo(new[] { conn42, conn43 }));
        }

        [Test]
        public void DisconnectAllConnectionsTest()
        {
            // add connection
            server.AddConnection(conn42);

            // disconnect all connections
            server.DisconnectAllConnections();
            Assert.That(server.connections, Is.Empty);
        }

        [Test]
        public void DisconnectAllTest()
        {
            client.ConnectHost(server);
            Assert.That(server.localConnection, Is.Not.Null);

            // add connection
            server.AddConnection(conn42);

            // disconnect all connections and local connection
            server.DisconnectAll();
            Assert.That(server.connections, Is.Empty);
            Assert.That(server.localConnection, Is.Null);
        }

        [Test]
        public void OnDataReceivedTest()
        {
            // add one custom message handler
            bool wasReceived = false;
            NetworkConnection connectionReceived = null;

            // add a connection
            var connection = new NetworkConnectionToClient(null);

            var messageReceived = new TestMessage();
            connection.RegisterHandler<NetworkConnectionToClient, TestMessage>((conn, msg) => {
                wasReceived = true;
                connectionReceived = conn;
                messageReceived = msg;
            }, false);


            server.AddConnection(connection);
            Assert.That(server.connections.Count, Is.EqualTo(1));

            // serialize a test message into an arraysegment
            var testMessage = new TestMessage{IntValue = 13, DoubleValue = 14, StringValue = "15"};
            var writer = new NetworkWriter();
            MessagePacker.Pack(testMessage, writer);
            var segment = writer.ToArraySegment();

            // call transport.OnDataReceived
            // -> should call server.OnDataReceived
            //    -> conn.TransportReceive
            //       -> Handler(CommandMessage)
            Transport.activeTransport.OnServerDataReceived.Invoke(42, segment, 0);

            // was our message handler called now?
            Assert.That(wasReceived, Is.True);
            Assert.That(connectionReceived, Is.EqualTo(connection));
            Assert.That(messageReceived, Is.EqualTo(testMessage));
        }

        [Test]
        public void OnDataReceivedInvalidConnectionIdTest()
        {

            // add one custom message handler
            bool wasReceived = false;
            NetworkConnection connectionReceived = null;
            var messageReceived = new TestMessage();
            /*
            server.RegisterHandler<TestMessage>((conn, msg) => {
                wasReceived = true;
                connectionReceived = conn;
                messageReceived = msg;
            }, false);
            */

            // serialize a test message into an arraysegment
            var testMessage = new TestMessage{IntValue = 13, DoubleValue = 14, StringValue = "15"};
            var writer = new NetworkWriter();
            MessagePacker.Pack(testMessage, writer);
            var segment = writer.ToArraySegment();

            // call transport.OnDataReceived with an invalid connectionId
            // an error log is expected.
            LogAssert.ignoreFailingMessages = true;
            Transport.activeTransport.OnServerDataReceived.Invoke(42, segment, 0);
            LogAssert.ignoreFailingMessages = false;

            // message handler should never be called
            Assert.That(wasReceived, Is.False);
            Assert.That(connectionReceived, Is.Null);
        }

        [Test]
        public void SetClientReadyAndNotReadyTest()
        {
            var connection = new ULocalConnectionToClient
            {
                connectionToServer = new ULocalConnectionToServer()
            };
            Assert.That(connection.isReady, Is.False);

            server.SetClientReady(connection);
            Assert.That(connection.isReady, Is.True);

            server.SetClientNotReady(connection);
            Assert.That(connection.isReady, Is.False);
        }

        [Test]
        public void SetAllClientsNotReadyTest()
        {
            // add first ready client
            var first = new ULocalConnectionToClient
            {
                connectionToServer = new ULocalConnectionToServer(),
                isReady = true
            };
            server.AddConnection(first);

            // add second ready client
            var second = new ULocalConnectionToClient
            {
                connectionToServer = new ULocalConnectionToServer(),
                isReady = true
            };
            server.AddConnection(second);

            // set all not ready
            server.SetAllClientsNotReady();
            Assert.That(first.isReady, Is.False);
            Assert.That(second.isReady, Is.False);
        }

        [Test]
        public void ReadyMessageSetsClientReadyTest()
        {
            // add connection
            var connection = new ULocalConnectionToClient();
            connection.connectionToServer = new ULocalConnectionToServer();
            server.AddConnection(connection);

            // set as authenticated, otherwise readymessage is rejected
            connection.isAuthenticated = true;

            // serialize a ready message into an arraysegment
            var message = new ReadyMessage();
            var writer = new NetworkWriter();
            MessagePacker.Pack(message, writer);
            var segment = writer.ToArraySegment();

            // call transport.OnDataReceived with the message
            // -> calls server.OnClientReadyMessage
            //    -> calls SetClientReady(conn)
            Transport.activeTransport.OnServerDataReceived.Invoke(0, segment, 0);

            // ready?
            Assert.That(connection.isReady, Is.True);
        }

        [Test]
        public void ActivateHostSceneCallsOnStartClient()
        {
            // add an identity with a networkbehaviour to .spawned
            var go = new GameObject();
            NetworkIdentity identity = go.AddComponent<NetworkIdentity>();
            identity.netId = 42;
            //identity.connectionToClient = connection; // for authority check
            OnStartClientTestNetworkBehaviour comp = go.AddComponent<OnStartClientTestNetworkBehaviour>();
            Assert.That(comp.called, Is.EqualTo(0));
            //connection.identity = identity;
            server.spawned[identity.netId] = identity;

            // ActivateHostScene
            server.ActivateHostScene();

            // was OnStartClient called for all .spawned networkidentities?
            Assert.That(comp.called, Is.EqualTo(1));

            // clean up
            server.spawned.Clear();
            // destroy the test gameobject AFTER server was stopped.
            // otherwise isServer is true in OnDestroy, which means it would try
            // to call Destroy(go). but we need to use DestroyImmediate in
            // Editor
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void SendToAllTest()
        {
            // add connection
            var connection = new ULocalConnectionToClient();
            connection.connectionToServer = new ULocalConnectionToServer();
            // set a client handler
            int called = 0;
            connection.connectionToServer.RegisterHandler<TestMessage>(msg => ++called);
            server.AddConnection(connection);

            // create a message
            var message = new TestMessage{ IntValue = 1, DoubleValue = 2, StringValue = "3" };

            // send it to all
            server.SendToAll(message);
            // update local connection once so that the incoming queue is processed
            connection.connectionToServer.Update();

            // was it send to and handled by the connection?
            Assert.That(called, Is.EqualTo(1));
        }

        [Test]
        public void RegisterUnregisterClearHandlerTest()
        {

            // add a connection
            var connection = new NetworkConnectionToClient(null);

            // RegisterHandler(conn, msg) variant
            int variant1Called = 0;
            connection.RegisterHandler<TestMessage>( msg => { ++variant1Called; }, false);

            // RegisterHandler(msg) variant
            int variant2Called = 0;
            connection.RegisterHandler<WovenTestMessage>(msg => { ++variant2Called; }, false);

            server.AddConnection(connection);
            Assert.That(server.connections.Count, Is.EqualTo(1));

            // serialize first message, send it to server, check if it was handled
            var writer = new NetworkWriter();
            MessagePacker.Pack(new TestMessage(), writer);
            Transport.activeTransport.OnServerDataReceived.Invoke(42, writer.ToArraySegment(), 0);
            Assert.That(variant1Called, Is.EqualTo(1));

            // serialize second message, send it to server, check if it was handled
            writer = new NetworkWriter();
            var wovenMessage = new WovenTestMessage
            {
                IntValue = 1,
                DoubleValue = 1.0,
                StringValue = "hello"
            };

            MessagePacker.Pack(wovenMessage, writer);
            Transport.activeTransport.OnServerDataReceived.Invoke(42, writer.ToArraySegment(), 0);
            Assert.That(variant2Called, Is.EqualTo(1));

            // unregister first handler, send, should fail
            writer = new NetworkWriter();
            MessagePacker.Pack(new TestMessage(), writer);
            // log error messages are expected
            LogAssert.ignoreFailingMessages = true;
            Transport.activeTransport.OnServerDataReceived.Invoke(42, writer.ToArraySegment(), 0);
            LogAssert.ignoreFailingMessages = false;
            Assert.That(variant1Called, Is.EqualTo(1)); // still 1, not 2

            writer = new NetworkWriter();
            MessagePacker.Pack(new TestMessage(), writer);
            // log error messages are expected
            LogAssert.ignoreFailingMessages = true;
            Transport.activeTransport.OnServerDataReceived.Invoke(42, writer.ToArraySegment(), 0);
            LogAssert.ignoreFailingMessages = false;
            Assert.That(variant2Called, Is.EqualTo(1)); // still 1, not 2
        }

        [Test]
        public void SendToClientOfPlayer()
        {
            // add connection
            var connection = new ULocalConnectionToClient();
            connection.connectionToServer = new ULocalConnectionToServer();
            // set a client handler
            int called = 0;
            connection.connectionToServer.RegisterHandler<TestMessage>(msg => ++called);
            server.AddConnection(connection);

            // create a message
            var message = new TestMessage{ IntValue = 1, DoubleValue = 2, StringValue = "3" };

            // create a gameobject and networkidentity
            NetworkIdentity identity = new GameObject().AddComponent<NetworkIdentity>();
            identity.connectionToClient = connection;

            // send it to that player
            server.SendToClientOfPlayer(identity, message);

            // update local connection once so that the incoming queue is processed
            connection.connectionToServer.Update();

            // was it send to and handled by the connection?
            Assert.That(called, Is.EqualTo(1));
            // destroy GO after shutdown, otherwise isServer is true in OnDestroy and it tries to call
            // GameObject.Destroy (but we need DestroyImmediate in Editor)
            UnityEngine.Object.DestroyImmediate(identity.gameObject);
        }

        [Test]
        public void GetNetworkIdentity()
        {
            // create a GameObject with NetworkIdentity
            var go = new GameObject();
            NetworkIdentity identity = go.AddComponent<NetworkIdentity>();

            // GetNetworkIdentity
            bool result = server.GetNetworkIdentity(go, out NetworkIdentity value);
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(identity));

            // create a GameObject without NetworkIdentity
            var goWithout = new GameObject();

            // GetNetworkIdentity for GO without identity
            // (error log is expected)
            LogAssert.ignoreFailingMessages = true;
            result = server.GetNetworkIdentity(goWithout, out NetworkIdentity valueNull);
            Assert.That(result, Is.False);
            Assert.That(valueNull, Is.Null);
            LogAssert.ignoreFailingMessages = false;

            // clean up
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(goWithout);
        }

        [Test]
        public void ShowForConnection()
        {
            // add connection
            var connection = new ULocalConnectionToClient();
            connection.isReady = true; // required for ShowForConnection
            connection.connectionToServer = new ULocalConnectionToServer();
            // set a client handler
            int called = 0;
            connection.connectionToServer.RegisterHandler<SpawnMessage>(msg => ++called);
            server.AddConnection(connection);

            // create a gameobject and networkidentity and some unique values
            NetworkIdentity identity = new GameObject().AddComponent<NetworkIdentity>();
            identity.connectionToClient = connection;

            // call ShowForConnection
            server.ShowForConnection(identity, connection);

            // update local connection once so that the incoming queue is processed
            connection.connectionToServer.Update();

            // was it sent to and handled by the connection?
            Assert.That(called, Is.EqualTo(1));

            // it shouldn't send it if connection isn't ready, so try that too
            connection.isReady = false;
            server.ShowForConnection(identity, connection);
            connection.connectionToServer.Update();
            Assert.That(called, Is.EqualTo(1)); // not 2 but 1 like before?
            // destroy GO after shutdown, otherwise isServer is true in OnDestroy and it tries to call
            // GameObject.Destroy (but we need DestroyImmediate in Editor)
            UnityEngine.Object.DestroyImmediate(identity.gameObject);
        }

        [Test]
        public void HideForConnection()
        {
            // add connection
            var connection = new ULocalConnectionToClient
            {
                isReady = true, // required for ShowForConnection
                connectionToServer = new ULocalConnectionToServer()
            };
            // set a client handler
            int called = 0;
            connection.connectionToServer.RegisterHandler<ObjectHideMessage>(msg => ++called);
            server.AddConnection(connection);

            // create a gameobject and networkidentity
            NetworkIdentity identity = new GameObject().AddComponent<NetworkIdentity>();
            identity.connectionToClient = connection;

            // call HideForConnection
            server.HideForConnection(identity, connection);

            // update local connection once so that the incoming queue is processed
            connection.connectionToServer.Update();

            // was it sent to and handled by the connection?
            Assert.That(called, Is.EqualTo(1));
            // destroy GO after shutdown, otherwise isServer is true in OnDestroy and it tries to call
            // GameObject.Destroy (but we need DestroyImmediate in Editor)
            UnityEngine.Object.DestroyImmediate(identity.gameObject);
        }

        [Test]
        public void ValidateSceneObject()
        {
            // create a gameobject and networkidentity
            var go = new GameObject();
            NetworkIdentity identity = go.AddComponent<NetworkIdentity>();
            identity.sceneId = 42;

            // should be valid as long as it has a sceneId
            Assert.That(server.ValidateSceneObject(identity), Is.True);

            // shouldn't be valid with 0 sceneID
            identity.sceneId = 0;
            Assert.That(server.ValidateSceneObject(identity), Is.False);
            identity.sceneId = 42;

            // shouldn't be valid for certain hide flags
            go.hideFlags = HideFlags.NotEditable;
            Assert.That(server.ValidateSceneObject(identity), Is.False);
            go.hideFlags = HideFlags.HideAndDontSave;
            Assert.That(server.ValidateSceneObject(identity), Is.False);

            // clean up
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void SpawnObjects()
        {
            // create a gameobject and networkidentity that lives in the scene(=has sceneid)
            var go = new GameObject("Test");
            NetworkIdentity identity = go.AddComponent<NetworkIdentity>();
            identity.sceneId = 42; // lives in the scene from the start
            go.SetActive(false); // unspawned scene objects are set to inactive before spawning

            // create a gameobject that looks like it was instantiated and doesn't live in the scene
            var go2 = new GameObject("Test2");
            NetworkIdentity identity2 = go2.AddComponent<NetworkIdentity>();
            identity2.sceneId = 0; // not a scene object
            go2.SetActive(false); // unspawned scene objects are set to inactive before spawning

            // calling SpawnObjects while server isn't active should do nothing
            Assert.That(server.SpawnObjects(), Is.False);

            // calling SpawnObjects while server is active should succeed
            Assert.That(server.SpawnObjects(), Is.True);

            // was the scene object activated, and the runtime one wasn't?
            Assert.That(go.activeSelf, Is.True);
            Assert.That(go2.activeSelf, Is.False);
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(go2);
        }

        [Test]
        public void UnSpawn()
        {
            // create a gameobject and networkidentity that lives in the scene(=has sceneid)
            var go = new GameObject("Test");
            NetworkIdentity identity = go.AddComponent<NetworkIdentity>();
            OnNetworkDestroyTestNetworkBehaviour comp = go.AddComponent<OnNetworkDestroyTestNetworkBehaviour>();
            identity.sceneId = 42; // lives in the scene from the start
            go.SetActive(true); // spawned objects are active
            Assert.That(identity.IsMarkedForReset(), Is.False);

            // unspawn
            server.UnSpawn(go);

            // it should have been marked for reset now
            Assert.That(identity.IsMarkedForReset(), Is.True);

            // clean up
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ClearDirtyComponentsDirtyBits()
        {
            // create a networkidentity and add some components
            OnStartClientTestNetworkBehaviour compA = gameObject.AddComponent<OnStartClientTestNetworkBehaviour>();
            OnStartClientTestNetworkBehaviour compB = gameObject.AddComponent<OnStartClientTestNetworkBehaviour>();

            // set syncintervals so one is always dirty, one is never dirty
            compA.syncInterval = 0;
            compB.syncInterval = Mathf.Infinity;

            // set components dirty bits
            compA.SetDirtyBit(0x0001);
            compB.SetDirtyBit(0x1001);
            Assert.That(compA.IsDirty(), Is.True); // dirty because interval reached and mask != 0
            Assert.That(compB.IsDirty(), Is.False); // not dirty because syncinterval not reached

            // call identity.ClearDirtyComponentsDirtyBits
            identity.ClearDirtyComponentsDirtyBits();
            Assert.That(compA.IsDirty(), Is.False); // should be cleared now
            Assert.That(compB.IsDirty(), Is.False); // should be untouched

            // set compB syncinterval to 0 to check if the masks were untouched
            // (if they weren't, then it should be dirty now)
            compB.syncInterval = 0;
            Assert.That(compB.IsDirty(), Is.True);
        }

        [Test]
        public void ClearAllComponentsDirtyBits()
        {
            // create a networkidentity and add some components
            OnStartClientTestNetworkBehaviour compA = gameObject.AddComponent<OnStartClientTestNetworkBehaviour>();
            OnStartClientTestNetworkBehaviour compB = gameObject.AddComponent<OnStartClientTestNetworkBehaviour>();

            // set syncintervals so one is always dirty, one is never dirty
            compA.syncInterval = 0;
            compB.syncInterval = Mathf.Infinity;

            // set components dirty bits
            compA.SetDirtyBit(0x0001);
            compB.SetDirtyBit(0x1001);
            Assert.That(compA.IsDirty(), Is.True); // dirty because interval reached and mask != 0
            Assert.That(compB.IsDirty(), Is.False); // not dirty because syncinterval not reached

            // call identity.ClearAllComponentsDirtyBits
            identity.ClearAllComponentsDirtyBits();
            Assert.That(compA.IsDirty(), Is.False); // should be cleared now
            Assert.That(compB.IsDirty(), Is.False); // should be cleared now

            // set compB syncinterval to 0 to check if the masks were cleared
            // (if they weren't, then it would still be dirty now)
            compB.syncInterval = 0;
            Assert.That(compB.IsDirty(), Is.False);
        }

        [Test]
        public void ShutdownCleanupTest()
        {
            Assert.That(server.active, Is.True);

            client.ConnectHost(server);
            // set local connection
            Assert.That(server.LocalClientActive, Is.True);

            // connect
            Transport.activeTransport.OnServerConnected.Invoke(42);
            Assert.That(server.connections.Count, Is.EqualTo(1));

            server.DisconnectAll();
            server.Shutdown();

            // state cleared?
            Assert.That(server.connections, Is.Empty);
            Assert.That(server.active, Is.False);
            Assert.That(server.localConnection, Is.Null);
            Assert.That(server.LocalClientActive, Is.False);
        }
    }
}
