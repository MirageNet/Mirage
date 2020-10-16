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
        [Test]
        public void TokenStructure (){
            var hashCash = new HashCash
            {
                dt = DateTime.UtcNow,
                resource = "yomama".GetStableHashCode(),
                salt = 123123,
                counter = 10,
            };

            Assert.That(hashCash.resource, Is.EqualTo("yomama".GetStableHashCode()));
        }

        [Test]
        public void EncodingDecoding()
        {
            byte[] buffer = new byte[1000];

            var hashCash = new HashCash
            {
                dt = DateTime.UtcNow,
                resource = "yomama".GetStableHashCode(),
                salt = 123123,
                counter = 10,
            };

            int encodeLength = HashCashEncoding.Encode(buffer, 0, hashCash);

            (int offset, HashCash decoded) = HashCashEncoding.Decode(buffer, 0);

            Assert.That(offset, Is.EqualTo(encodeLength));
            Assert.That(decoded, Is.EqualTo(hashCash));
        }
    }
}