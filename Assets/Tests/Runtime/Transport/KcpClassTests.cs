using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mirror.KCP;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{

    [TestFixture]
    public class KcpClassTests : KcpSetup
    {
        [Test]
        public void SendExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                server.Send(null, 0, 0);
            });
        }

        [Test]
        public void SetMtuExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                server.SetMtu(0);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                server.SetMtu(uint.MaxValue);
            });
        }

        [Test]
        public void ReserveExceptionTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                server.ReserveBytes(uint.MaxValue);
            });
        }
    }
}
