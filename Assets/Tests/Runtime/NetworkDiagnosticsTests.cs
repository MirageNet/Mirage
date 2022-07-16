using System;
using Mirage.Tests.Runtime.Serialization;
using NSubstitute;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    [TestFixture(Category = "NetworkDiagnostics")]
    public class NetworkDiagnosticsTests
    {
        [Test]
        public void TestOnSendEvent()
        {
            var outMessageCallback = Substitute.For<Action<NetworkDiagnostics.MessageInfo>>();
            NetworkDiagnostics.OutMessageEvent += outMessageCallback;

            var message = new TestMessage();
            NetworkDiagnostics.OnSend(message, 10, 5);
            var expected = new NetworkDiagnostics.MessageInfo(message, 10, 5);
            outMessageCallback.Received(1).Invoke(Arg.Is(expected));

            NetworkDiagnostics.OutMessageEvent -= outMessageCallback;
        }

        [Test]
        public void TestOnSendZeroCountEvent()
        {
            var outMessageCallback = Substitute.For<Action<NetworkDiagnostics.MessageInfo>>();
            NetworkDiagnostics.OutMessageEvent += outMessageCallback;

            var message = new TestMessage();
            NetworkDiagnostics.OnSend(message, 10, 0);
            outMessageCallback.DidNotReceive();

            NetworkDiagnostics.OutMessageEvent -= outMessageCallback;
        }

        [Test]
        public void TestOnReceiveEvent()
        {
            var outMessageCallback = Substitute.For<Action<NetworkDiagnostics.MessageInfo>>();
            NetworkDiagnostics.InMessageEvent += outMessageCallback;

            var message = new TestMessage();
            NetworkDiagnostics.OnReceive(message, 10);
            var expected = new NetworkDiagnostics.MessageInfo(message, 10, 1);
            outMessageCallback.Received(1).Invoke(Arg.Is(expected));

            NetworkDiagnostics.InMessageEvent -= outMessageCallback;
        }
    }
}
