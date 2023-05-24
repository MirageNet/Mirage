using System;
using Mirage.Authenticators.SessionId;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Authentication
{
    public class SessionKeyTest : TestBase
    {
        [Test]
        public void SameHashWithArray()
        {
            var array = CreateArray(0, 4);

            var key1 = new SessionIdAuthenticator.SessionKey(array);
            var key2 = new SessionIdAuthenticator.SessionKey(array);

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
        }

        [Test]
        public void SameHashWithArraySegment()
        {
            var array1 = CreateArray(0, 4);
            var array2 = new ArraySegment<byte>(CreateArray(2, 4), 2, 4);

            var key1 = new SessionIdAuthenticator.SessionKey(array1);
            var key2 = new SessionIdAuthenticator.SessionKey(array2);

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
        }

        [Test]
        public void KeysAreEqualWithSegment()
        {
            var array1 = CreateArray(0, 4);
            var array2 = new ArraySegment<byte>(CreateArray(2, 4), 2, 4);

            var key1 = new SessionIdAuthenticator.SessionKey(array1);
            var key2 = new SessionIdAuthenticator.SessionKey(array2);

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
            Assert.That(key1.Equals(key2));
        }

        private static byte[] CreateArray(int offset, int size)
        {
            var array = new byte[size + offset];
            for (var i = 0; i < size; i++)
            {
                array[i + offset] = (byte)(i + 1);
            }

            return array;
        }
    }
}
