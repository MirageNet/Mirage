using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Host;
using Mirage.Tests.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine.TestTools;
using Is = NUnit.Framework.Is;

namespace Mirage.Tests.Runtime.ServerSendTo
{
    public class HostSendTo : HostSetup
    {
        public const int RemoteClientCount = 3;

        public int ClientCount => RemoteClientCount + 1;

        private int[] _receives;

        protected override async UniTask ExtraSetup()
        {
            await base.ExtraSetup();

            for (var i = 0; i < RemoteClientCount; i++)
            {
                await AddClient();
            }
        }

        protected override UniTask LateSetup()
        {
            _receives = new int[ClientCount];

            // hostclient
            client.MessageHandler.RegisterHandler<TestMessage>(msg => { _receives[0]++; });

            for (var i = 0; i < RemoteClientCount; i++)
            {
                // use local variable to avoid access to modified closure
                var index = i + 1; // +1 because host is 0
                _remoteClients[i].Client.MessageHandler.RegisterHandler<TestMessage>(msg => { _receives[index]++; });
            }

            return base.LateSetup();
        }

        [UnityTest]
        public IEnumerator SendToAll([Values(true, false)] bool excludeLocal)
        {
            server.SendToAll(new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < ClientCount; i++)
            {
                var expected = 1;
                if (excludeLocal && i == 0)
                    expected = 0;
                Assert.That(_receives[i], Is.EqualTo(expected), $"receiveIndex {i}");
            }
        }

        [UnityTest]
        public IEnumerator SendToMany_List([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new List<INetworkPlayer>();
            // skipping first 2 players
            hashSet.Add(ServerPlayer(1));
            hashSet.Add(ServerPlayer(2));
            hashSet.Add(hostServerPlayer);

            server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < ClientCount; i++)
            {
                var expected = i != 1 ? 1 : 0;
                if (excludeLocal && i == 0)
                    expected = 0;
                Assert.That(_receives[i], Is.EqualTo(expected), $"receiveIndex {i}");
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
            // add host player, we need it for excludeLocal test
            colletion = colletion.Append(hostServerPlayer);
            server.SendToMany(colletion, new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < ClientCount; i++)
            {
                // we take first 2 players, so index 1 and 2, and also host player
                var expected = i <= 2 ? 1 : 0;
                if (excludeLocal && i == 0)
                    expected = 0;
                Assert.That(_receives[i], Is.EqualTo(expected), $"receiveIndex {i}");
            }
        }

        [UnityTest]
        public IEnumerator SendToMany_Enumerator([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new HashSet<INetworkPlayer>();
            // skipping last 2 players
            hashSet.Add(hostServerPlayer);
            hashSet.Add(ServerPlayer(0));
            hashSet.Add(ServerPlayer(1));

            server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            yield return null;

            for (var i = 0; i < ClientCount; i++)
            {
                var expected = i <= 2 ? 1 : 0;
                if (excludeLocal && i == 0)
                    expected = 0;
                Assert.That(_receives[i], Is.EqualTo(expected), $"receiveIndex {i}");
            }
        }

        [UnityTest]
        public IEnumerator SendToObservers([Values(true, false)] bool excludeLocal, [Values(true, false)] bool excludeOwner)
        {
            for (var idIndex = 0; idIndex < ClientCount; idIndex++)
            {
                var identity = idIndex == 0 ? hostIdentity : ServerIdentity(idIndex - 1);
                // repeat for each identity
                server.SendToObservers(identity, new TestMessage(), excludeLocal, excludeOwner);
                yield return null;

                for (var i = 0; i < ClientCount; i++)
                {
                    // expected receive
                    var expected = (excludeOwner && i == idIndex) ? 0 : 1;
                    if (excludeLocal && i == 0)
                        expected = 0;
                    Assert.That(_receives[i], Is.EqualTo(expected), $"Did not receive message on Client {i} when sending from {idIndex}");
                    // clear after so we can run next receive
                    _receives[i] = 0;
                }
            }
        }
    }
}
