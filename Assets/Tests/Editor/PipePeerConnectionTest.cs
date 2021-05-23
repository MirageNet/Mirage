using System;
using System.Linq;
using Mirage.SocketLayer;
using NSubstitute;
using NUnit.Framework;

namespace Mirage
{
    public class PipePeerConnectionTest
    {
        class ConnectionHandler
        {
            public IDataHandler handler;
            public SocketLayer.IConnection connection;

            public ConnectionHandler(IDataHandler handler, SocketLayer.IConnection connection)
            {
                this.handler = handler;
                this.connection = connection;
            }

            public void Send(byte[] data)
            {
                connection.SendUnreliable(data);
            }
            public void ExpectData(byte[] expected)
            {
                handler.Received(1).ReceiveMessage(connection, Arg.Is<ArraySegment<byte>>(x => x.SequenceEqual(expected)));
            }
        }

        ConnectionHandler conn1;
        ConnectionHandler conn2;


        [SetUp]
        public void Setup()
        {
            IDataHandler handler1 = Substitute.For<IDataHandler>();
            IDataHandler handler2 = Substitute.For<IDataHandler>();
            (SocketLayer.IConnection connection1, SocketLayer.IConnection connection2) = PipePeerConnection.Create(handler1, handler2);

            conn1 = new ConnectionHandler(handler1, connection1);
            conn2 = new ConnectionHandler(handler2, connection2);
        }

        [Test]
        public void ReceivesSentData()
        {
            conn1.Send(new byte[] { 1, 2, 3, 4 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
        }

        [Test]
        public void ReceivesSentDataMultiple()
        {
            conn1.Send(new byte[] { 1, 2, 3, 4 });
            conn1.Send(new byte[] { 5, 6, 7, 8 });

            conn2.ExpectData(new byte[] { 1, 2, 3, 4 });
            conn2.ExpectData(new byte[] { 5, 6, 7, 8 });
        }

        [Test]
        public void DisconnectShouldDoStuff()
        {
            Assert.Ignore("NotImplemented");
            // todo add disconnect actions
            /*
            When a pipe connection is disconnected it should:
            - send state to disconnected
            - tell its pair to disconnect
            - invoke peer callbacks via an action so that server/client know about the disconnect
             */

            //// disconnecting c1 should disconnect both
            //conn1.Disconnect();

            //var memoryStream = new MemoryStream();
            //try
            //{
            //    await conn1.ReceiveAsync(memoryStream);
            //    Assert.Fail("Recive Async should have thrown EndOfStreamException");
            //}
            //catch (EndOfStreamException)
            //{
            //    // good to go
            //}

            //try
            //{
            //    await conn2.ReceiveAsync(memoryStream);
            //    Assert.Fail("Recive Async should have thrown EndOfStreamException");
            //}
            //catch (EndOfStreamException)
            //{
            //    // good to go
            //}
        }

        [Test]
        public void EndpointIsPipeEndPoint()
        {
            Assert.That(conn1.connection.EndPoint, Is.TypeOf<PipePeerConnection.PipeEndPoint>());
        }
    }
}
