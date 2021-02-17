using System;
using System.Collections;
using System.Linq;
using System.Net;
using Cysharp.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

using static Mirage.Tests.AsyncUtil;
using TaskChannel = Cysharp.Threading.Tasks.Channel;

namespace Mirage
{
    public class AsyncPipeConnectionTest
    {

        IConnection c1;
        IConnection c2;

        Channel<byte[]> c1Messages;
        Channel<byte[]> c2Messages;

        [SetUp]
        public void Setup()
        {
            (c1, c2) = PipeConnection.CreatePipe();

            c1Messages = TaskChannel.CreateSingleConsumerUnbounded<byte[]>();
            c2Messages = TaskChannel.CreateSingleConsumerUnbounded<byte[]>();

            c1.MessageReceived += (data, channel) =>
            {
                c1Messages.Writer.TryWrite(data.ToArray());
            };
            c2.MessageReceived += (data, channel) =>
            {
                c2Messages.Writer.TryWrite(data.ToArray());
            };
        }

        private static void SendData(IConnection c, byte[] data)
        {
            c.Send(new ArraySegment<byte>(data));
        }


        private static async UniTask ExpectData(Channel<byte[]> c, byte[] expected)
        {
            byte[] received = await c.Reader.ReadAsync();

            Assert.That(received, Is.EquivalentTo(expected));
        }

        [UnityTest]
        public IEnumerator TestSendAndReceive() => RunAsync(async () =>
        {
            SendData(c1, new byte[] { 1, 2, 3, 4 });

            await ExpectData(c2Messages, new byte[] { 1, 2, 3, 4 });
        });

        [UnityTest]
        public IEnumerator TestSendAndReceiveMultiple() => RunAsync(async () =>
        {
            SendData(c1, new byte[] { 1, 2, 3, 4 });
            SendData(c1, new byte[] { 5, 6, 7, 8 });

            await ExpectData(c2Messages, new byte[] { 1, 2, 3, 4 });
            await ExpectData(c2Messages, new byte[] { 5, 6, 7, 8 });
        });

        [UnityTest]
        public IEnumerator TestDisconnectC1() => RunAsync(async () =>
        {
            Action disconnectMock = Substitute.For<Action>();

            c1.Disconnected += disconnectMock;
            // disconnecting c1 should disconnect c1
            c1.Disconnect();

            disconnectMock.Received().Invoke();
        });

        [UnityTest]
        public IEnumerator TestDisconnectC2() => RunAsync(async () =>
        {
            Action disconnectMock = Substitute.For<Action>();

            c2.Disconnected += disconnectMock;
            // disconnecting c1 should disconnect c2
            c1.Disconnect();

            disconnectMock.Received().Invoke();
        });

        [Test]
        public void TestAddressC1()
        {
            Assert.That(c1.GetEndPointAddress(), Is.EqualTo(new IPEndPoint(IPAddress.Loopback, 0)));
        }

        [Test]
        public void TestAddressC2()
        {
            Assert.That(c2.GetEndPointAddress(), Is.EqualTo(new IPEndPoint(IPAddress.Loopback, 0)));
        }

    }
}
