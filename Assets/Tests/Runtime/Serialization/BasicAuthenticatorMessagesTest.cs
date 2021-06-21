using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Serialization
{
    // todo what is this testing? do we need it?
    [TestFixture]
    public class BasicAuthenticatorMessagesTest
    {
        public class AuthRequestMessage
        {
            public string serverCode;
        }

        public class AuthResponseMessage
        {
            public bool success;
            public string message;
        }

        [Test]
        public void AuthRequestMessageTest()
        {
            // try setting value with constructor
            var message = new AuthRequestMessage
            {
                serverCode = "abc",
            };
            Assert.That(message.serverCode, Is.EqualTo("abc"));

            // serialize
            var writer = new NetworkWriter(1300);
            writer.Write(message);

            // try deserialize
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            AuthRequestMessage fresh = reader.Read<AuthRequestMessage>();
            Assert.That(fresh.serverCode, Is.EqualTo("abc"));
            reader.Dispose();
        }

        [Test]
        public void AuthResponseMessageTest()
        {
            // try setting value with constructor
            var message = new AuthResponseMessage
            {
                success = true,
                message = "abc"
            };
            Assert.That(message.success, Is.EqualTo(true));
            Assert.That(message.message, Is.EqualTo("abc"));

            // serialize
            var writer = new NetworkWriter(1300);
            writer.Write(message);

            // try deserialize
            var reader = new NetworkReader();
            reader.Reset(writer.ToArraySegment());
            AuthResponseMessage fresh = reader.Read<AuthResponseMessage>();
            Assert.That(fresh.success, Is.EqualTo(true));
            Assert.That(fresh.message, Is.EqualTo("abc"));
            reader.Dispose();
        }
    }
}
