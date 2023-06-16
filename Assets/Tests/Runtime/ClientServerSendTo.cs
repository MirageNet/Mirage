using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine.TestTools;
using Is = NUnit.Framework.Is;

namespace Mirage.Tests.Runtime.ServerSendTo
{
    public class ClientServerSendTo : MultiRemoteClientSetup
    {
        protected override int RemoteClientCount => 4;

        private int[] _receives;

        protected override UniTask LateSetup()
        {
            _receives = new int[RemoteClientCount];
            for (var i = 0; i < RemoteClientCount; i++)
            {
                // use local variable to avoid access to modified closure
                var index = i;
                _remoteClients[i].Client.MessageHandler.RegisterHandler<TestMessage>(msg => { _receives[index]++; });
            }

            return base.LateSetup();
        }

        [UnityTest]
        public IEnumerator SendToAll([Values(true, false)] bool excludeLocal)
        {
            server.SendToAll(new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < RemoteClientCount; i++)
            {
                Assert.That(_receives[i], Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator SendToMany_List([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new List<INetworkPlayer>();
            // skipping first 2 players
            hashSet.Add(ServerPlayer(3));
            hashSet.Add(ServerPlayer(2));

            server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < RemoteClientCount; i++)
            {
                var expected = i >= 2 ? 1 : 0;
                Assert.That(_receives[i], Is.EqualTo(expected));
            }
        }

        [UnityTest]
        public IEnumerator SendToMany_Enumerable([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new HashSet<INetworkPlayer>();
            for (var i = 0; i < RemoteClientCount; i++)
            {
                hashSet.Add(ServerPlayer(i));
            }

            var colletion = hashSet.Skip(0).Take(2);
            server.SendToMany(colletion, new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < RemoteClientCount; i++)
            {
                var expected = i < 2 ? 1 : 0;
                Assert.That(_receives[i], Is.EqualTo(expected));
            }
        }

        [UnityTest]
        public IEnumerator SendToMany_Enumerator([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new HashSet<INetworkPlayer>();
            // skipping last 2 players
            hashSet.Add(ServerPlayer(0));
            hashSet.Add(ServerPlayer(1));

            server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < RemoteClientCount; i++)
            {
                var expected = i < 2 ? 1 : 0;
                Assert.That(_receives[i], Is.EqualTo(expected));
            }
        }

        [UnityTest]
        public IEnumerator SendToObservers([Values(true, false)] bool excludeLocal, [Values(true, false)] bool excludeOwner)
        {
            for (var idIndex = 0; idIndex < RemoteClientCount; idIndex++)
            {
                // repeat for each identity
                server.SendToObservers(ServerIdentity(idIndex), new TestMessage(), excludeLocal, excludeOwner);
                yield return null;

                for (var i = 0; i < RemoteClientCount; i++)
                {
                    // expected receive
                    var expected = (excludeOwner && i == idIndex) ? 0 : 1;
                    Assert.That(_receives[i], Is.EqualTo(expected), $"Did not receive message on Client {i} when sending from {idIndex}");
                    // clear after so we can run next receive
                    _receives[i] = 0;
                }
            }
        }
    }
}
