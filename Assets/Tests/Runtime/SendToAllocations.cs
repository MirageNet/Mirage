using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Mirage.Tests.Runtime.ServerSendTo
{
    public class SendToAllocations : MultiRemoteClientSetup
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

        private static void TestAllocation<T>(TestDelegate testDelegate, bool shouldAllocate = false)
        {
            using (new LogOverride<T>())
            {
                // call first for jit
                testDelegate.Invoke();

                var contraint = shouldAllocate ? Is.AllocatingGCMemory() : Is.Not.AllocatingGCMemory();

                Assert.That(() =>
                {
                    testDelegate.Invoke();
                }, contraint);
            }
        }

        [Test]
        public void ShouldNotAllocate([Values(true, false)] bool excludeLocal)
        {
            TestAllocation<NetworkServer>(() =>
            {
                server.SendToAll(new TestMessage(), excludeLocal);
            });
        }

        [Test]
        public void SendToMany_List([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new List<INetworkPlayer>();
            // skipping first 2 players
            hashSet.Add(ServerPlayer(3));
            hashSet.Add(ServerPlayer(2));

            TestAllocation<NetworkServer>(() =>
            {
                server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            });
        }

        [Test]
        public void SendToMany_Enumerable([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new HashSet<INetworkPlayer>();
            for (var i = 0; i < RemoteClientCount; i++)
            {
                hashSet.Add(ServerPlayer(i));
            }

            var colletion = hashSet.Skip(0).Take(2);
            TestAllocation<NetworkServer>(() =>
            {
                server.SendToMany(colletion, new TestMessage(), excludeLocal);

            },
            // we want to check if this method allocates so that we know that the TestAllocation is actually working
            shouldAllocate: true);
        }

        [Test]
        public void SendToMany_Enumerator([Values(true, false)] bool excludeLocal)
        {
            var hashSet = new HashSet<INetworkPlayer>();
            // skipping last 2 players
            hashSet.Add(ServerPlayer(0));
            hashSet.Add(ServerPlayer(1));

            TestAllocation<NetworkServer>(() =>
            {
                server.SendToMany(hashSet.GetEnumerator(), new TestMessage(), excludeLocal);
            });
        }

        [Test]
        public void SendToObservers([Values(true, false)] bool excludeLocal, [Values(true, false)] bool excludeOwner)
        {
            TestAllocation<NetworkServer>(() =>
            {
                server.SendToObservers(ServerIdentity(0), new TestMessage(), excludeLocal, excludeOwner);
            });
        }
    }
}
