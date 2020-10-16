using NUnit.Framework;
using System;

using UnityEngine;
using Mirror.KCP;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Mirror.Tests
{

    public class HashcashTest : MonoBehaviour
    {
        HashCash hashCash;

        [SetUp]
        public void Setup()
        {
            hashCash = new HashCash(
                DateTime.UtcNow,
                "yomama",
                123123,
                10
            );
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
            var hashCash2 = new HashCash(hashCash.dt, hashCash.resource + 1, hashCash.salt, hashCash.counter);

            Assert.That(hashCash2.Sha1(), Is.Not.EqualTo(hashCash.Sha1()));
        }

        [Test]
        public void TestMining()
        {
            var mined = HashCash.Mine("yomama", 10);
            Assert.That(mined.ValidateHash(10), Is.True);
        }

        [Test]
        public void TestMiningNotGoodEnough()
        {
            var mined = HashCash.Mine("yomama", 10);
            Assert.That(mined.ValidateHash(11), Is.False);
        }

        [Test]
        public void TestNotMined()
        {
            // we didn't mine this one,  so it should not validate
            Assert.That(hashCash.ValidateHash(10), Is.False);
        }

        [Test]
        public void InvalidHash()
        {
            byte[] hash = new byte[20] {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
            Assert.That(HashCash.Validate(hash, 16), Is.False);
        }
        [Test]
        public void ValidHash()
        {
            byte[] hash = new byte[20] {
                0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
            Assert.That(HashCash.Validate(hash, 15), Is.True);
        }

        [Test]
        public void InvalidResource()
        {
            var mined = HashCash.Mine("yomama", 10);
            // token is for wrong resource
            Assert.That(mined.Validate("filomon", 10), Is.False);
        }

        [Test]
        public void ValidToken()
        {
            var mined = HashCash.Mine("yomama", 10);
            // token is for wrong resource
            Assert.That(mined.Validate("yomama", 10), Is.True);
        }

        [Test]
        public void ValidToken2()
        {
            var stopWatch = new Stopwatch();
            for (int i = 0; i < 10; i++)
            {
                stopWatch.Restart();
                var mined = HashCash.Mine("yomama", 18);
                // token is for wrong resource
                Assert.That(mined.Validate("yomama", 18), Is.True);

                stopWatch.Stop();
                Debug.Log($"It took {stopWatch.ElapsedMilliseconds} to mine");
            }
        }
    }
}