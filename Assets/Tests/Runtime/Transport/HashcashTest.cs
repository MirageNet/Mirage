using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using System;
using System.IO;

using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using Mirror.KCP;

namespace Mirror.Tests
{

    public class HashcashTest : MonoBehaviour
    {
        HashCash hashCash;

        [SetUp]
        public void Setup()
        {
            hashCash = new HashCash
            {
                dt = DateTime.UtcNow,
                resource = "yomama".GetStableHashCode(),
                salt = 123123,
                counter = 10,
            };
        }

        [Test]
        public void EncodingDecoding()
        {
            byte[] buffer = new byte[1000];

            int encodeLength = HashCashEncoding.Encode(buffer, 0, hashCash);

            (int offset, HashCash decoded) = HashCashEncoding.Decode(buffer, 0);

            Assert.That(offset, Is.EqualTo(encodeLength));
            Assert.That(decoded, Is.EqualTo(hashCash));
        }

        [Test]
        public void TestShaMatch()
        {
            HashCash hashCash2 = hashCash;

            Assert.That(hashCash2.Sha1(), Is.EqualTo(hashCash.Sha1()));
        }

        [Test]
        public void TestShaDiff()
        {
            HashCash hashCash2 = hashCash;
            hashCash2.resource++;

            Assert.That(hashCash2.Sha1(), Is.Not.EqualTo(hashCash.Sha1()));
        }

        [Test]
        public void TestMining()
        {
            HashCash mined = HashCash.Mine("yomama".GetStableHashCode());

        }

    }
}