using System;
using System.Linq;
using Mirage.SocketLayer;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests
{
    public class PipePeerConnectionTest
    {
        private class ConnectionHandler
        {
            public IDataHandler handler;
            public IConnection connection;

            public ConnectionHandler(IDataHandler handler, IConnection connection)
            {
                this.handler = handler;
                this.connection = connection;
            }

            public void SendUnreliable(byte[] data)
            {
                connection.SendUnreliable(data);
            }
            public void SendReliable(byte[] data)
            {
                connection.SendReliable(data);
            }
            public INotifyToken SendNotify(byte[] data)
            {
                return connection.SendNotify(data);
            }
            public void SendNotify(byte[] data, INotifyCallBack callbacks)
            {
                connection.SendNotify(data, callbacks);
            }
            public void ExpectData(byte[] expected)
            {
                handler.Received(1).ReceiveMessage(connection, Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(expected)));
            }
            public void ExpectNoData()
            {
                handler.DidNotReceiveWithAnyArgs().ReceiveMessage(default, default);
            }
        }

        private ConnectionHandler conn1;
        private ConnectionHandler conn2;
        private Action disconnect1;
        private Action disconnect2;

        [SetUp]
        public void Setup()
        {
            var handler1 = Substitute.For<IDataHandler>();
            var handler2 = Substitute.For<IDataHandler>();
            disconnect1 = Substitute.For<Action>();
            disconnect2 = Substitute.For<Action>();
            (var connection1, var connection2) = PipePeerConnection.Create(handler1, handler2, disconnect1, disconnect2);

            conn1 = new ConnectionHandler(handler1, connection1);
            conn2 = new ConnectionHandler(handler2, connection2);
        }

        [Test]
        public void ReceivesUnreliableSentData()
        {
            conn1.SendUnreliable(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
        }

        [Test]
        public void ReceivesUnreliableSentDataMultiple()
        {
            conn1.SendUnreliable(new byte[] { 1, 2, 3, 4 });
            conn1.SendUnreliable(new byte[] { 5, 6, 7, 8 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
            conn2.ExpectData(new byte[] { 5, 6, 7, 8 });
        }

        [Test]
        public void ReceivesReliableSentData()
        {
            conn1.SendReliable(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
        }

        [Test]
        public void ReceivesReliableSentDataMultiple()
        {
            conn1.SendReliable(new byte[] { 1, 2, 3, 4 });
            conn1.SendReliable(new byte[] { 5, 6, 7, 8 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
            conn2.ExpectData(new byte[] { 5, 6, 7, 8 });
        }

        [Test]
        public void ReceivesNotifySentData()
        {
            conn1.SendNotify(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
        }

        [Test]
        public void ReceivesNotifySentDataMultiple()
        {
            conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            conn1.SendNotify(new byte[] { 5, 6, 7, 8 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
            conn2.ExpectData(new byte[] { 5, 6, 7, 8 });
        }

        [Test]
        public void ReceivesNotifyCallbacksSentData()
        {
            conn1.SendNotify(new byte[] { 1, 2, 3, 4 }, Substitute.For<INotifyCallBack>());

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
        }

        [Test]
        public void ReceivesNotifyCallbacksSentDataMultiple()
        {
            conn1.SendNotify(new byte[] { 1, 2, 3, 4 }, Substitute.For<INotifyCallBack>());
            conn1.SendNotify(new byte[] { 5, 6, 7, 8 }, Substitute.For<INotifyCallBack>());

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
            conn2.ExpectData(new byte[] { 5, 6, 7, 8 });
        }

        [Test]
        public void DisconnectShouldBeCalledOnBothConnections()
        {
            conn1.connection.Disconnect();

            Assert.That(conn1.connection.State, Is.EqualTo(ConnectionState.Disconnected));
            Assert.That(conn2.connection.State, Is.EqualTo(ConnectionState.Disconnected));

            disconnect1.Received(1).Invoke();
            disconnect2.Received(1).Invoke();
        }

        [Test]
        public void NoReceivesUnreliableAfterDisconnect()
        {
            conn1.connection.Disconnect();
            conn1.SendUnreliable(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectNoData();
        }

        [Test]
        public void NoReceivesReliableAfterDisconnect()
        {
            conn1.connection.Disconnect();
            conn1.SendReliable(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectNoData();
        }

        [Test]
        public void NoReceivesNotifyAfterDisconnect()
        {
            conn1.connection.Disconnect();
            _ = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectNoData();
        }

        [Test]
        public void EndpointIsPipeEndPoint()
        {
            Assert.That(conn1.connection.Handle, Is.TypeOf<PipePeerConnection.PipeConnectionHandle>());
        }

        [Test]
        public void NotifyTokenShouldInvokeHandlerImmediately()
        {
            var token = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var invoked = 0;
            token.Delivered += () => invoked++;
            Assert.That(invoked, Is.EqualTo(1), "Delivered should be invoked 1 time Immediately when handler");
        }

        [Test]
        public void NotifyTokenShouldNotSavePreviousHandler()
        {
            var token = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var invoked1 = 0;
            token.Delivered += () => invoked1++;
            Assert.That(invoked1, Is.EqualTo(1), "Delivered should be invoked 1 time Immediately when handler");

            var invoked2 = 0;
            token.Delivered += () => invoked2++;
            Assert.That(invoked1, Is.EqualTo(1), "invoked1 handler should not be called a second time");
            Assert.That(invoked2, Is.EqualTo(1));
        }

        [Test]
        public void NotifyTokenRemoveDeliveredHandlerDoesNothing()
        {
            var token = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var invoked1 = 0;
            token.Delivered -= () => invoked1++;
            Assert.That(invoked1, Is.EqualTo(0), "Does nothing");
        }

        [Test]
        public void NotifyTokenAddLostHandlerDoesNothing()
        {
            var token = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var invoked1 = 0;
            token.Lost += () => invoked1++;
            Assert.That(invoked1, Is.EqualTo(0), "Does nothing");
        }

        [Test]
        public void NotifyTokenRemoveLostHandlerDoesNothing()
        {
            var token = conn1.SendNotify(new byte[] { 1, 2, 3, 4 });
            Assert.That(token, Is.TypeOf<AutoCompleteToken>());

            var invoked1 = 0;
            token.Lost -= () => invoked1++;
            Assert.That(invoked1, Is.EqualTo(0), "Does nothing");
        }

        [Test]
        public void NotifyCallbackShouldBeInvokedImmediately()
        {
            var callbacks = Substitute.For<INotifyCallBack>();
            conn1.SendNotify(new byte[] { 1, 2, 3, 4 }, callbacks);

            callbacks.Received(1).OnDelivered();
            callbacks.DidNotReceive().OnLost();
        }
    }
}
