using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests
{
    public class ClientServerRPCTestsMultipleComponents : ClientServerSetup
    {
        private const int COMPONENT_COUNT = 4;
        private MockRpcComponent[] clientComponent;
        private MockRpcComponent[] serverComponent;

        protected override void ExtraPrefabSetup(NetworkIdentity prefab)
        {
            base.ExtraPrefabSetup(prefab);

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                prefab.gameObject.AddComponent<MockRpcComponent>();
            }
        }

        protected override UniTask LateSetup()
        {
            clientComponent = new MockRpcComponent[COMPONENT_COUNT];
            serverComponent = new MockRpcComponent[COMPONENT_COUNT];

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                serverComponent[i] = (MockRpcComponent)serverIdentity.NetworkBehaviours[i];
                clientComponent[i] = (MockRpcComponent)clientIdentity.NetworkBehaviours[i];
            }

            return base.LateSetup();
        }


        [UnityTest]
        public IEnumerator ServerRpc([Range(0, 3)] int index)
        {
            clientComponent[index].Server2Args(1, "hello");

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(serverComponent[i].Server2ArgsCalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(serverComponent[i].Server2ArgsCalls[0].arg1, Is.EqualTo(1));
                    Assert.That(serverComponent[i].Server2ArgsCalls[0].arg2, Is.EqualTo("hello"));
                }
                else
                {
                    Assert.That(serverComponent[i].Server2ArgsCalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ServerRpcWithSenderOnClient([Range(0, 3)] int index)
        {
            clientComponent[index].ServerWithSender(1);

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(serverComponent[i].ServerWithSenderCalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(serverComponent[i].ServerWithSenderCalls[0].arg1, Is.EqualTo(1));
                    Assert.That(serverComponent[i].ServerWithSenderCalls[0].sender, Is.EqualTo(serverPlayer));
                }
                else
                {
                    Assert.That(serverComponent[i].ServerWithSenderCalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity([Range(0, 3)] int index)
        {
            clientComponent[index].ServerWithNI(clientIdentity);

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(serverComponent[i].ServerWithNICalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(serverComponent[i].ServerWithNICalls[0], Is.SameAs(serverIdentity));
                }
                else
                {
                    Assert.That(serverComponent[i].ServerWithNICalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ClientRpc([Range(0, 3)] int index)
        {
            serverComponent[index].Client2Args(1, "hello");

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(clientComponent[i].Client2ArgsCalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(clientComponent[i].Client2ArgsCalls[0].arg1, Is.EqualTo(1));
                    Assert.That(clientComponent[i].Client2ArgsCalls[0].arg2, Is.EqualTo("hello"));
                }
                else
                {
                    Assert.That(clientComponent[i].Client2ArgsCalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ClientConnRpc([Range(0, 3)] int index)
        {
            serverComponent[index].ClientTarget(serverPlayer, 1, "hello");

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(clientComponent[i].ClientTargetCalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(clientComponent[i].ClientTargetCalls[0].player, Is.EqualTo(clientPlayer));
                    Assert.That(clientComponent[i].ClientTargetCalls[0].arg1, Is.EqualTo(1));
                    Assert.That(clientComponent[i].ClientTargetCalls[0].arg2, Is.EqualTo("hello"));
                }
                else
                {
                    Assert.That(clientComponent[i].ClientTargetCalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ClientOwnerRpc([Range(0, 3)] int index)
        {
            serverComponent[index].ClientOwner(1, "hello");

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                if (i == index)
                {
                    Assert.That(clientComponent[i].ClientOwnerCalls.Count, Is.EqualTo(1), $"index {i}");
                    Assert.That(clientComponent[i].ClientOwnerCalls[0].arg1, Is.EqualTo(1));
                    Assert.That(clientComponent[i].ClientOwnerCalls[0].arg2, Is.EqualTo("hello"));
                }
                else
                {
                    Assert.That(clientComponent[i].ClientOwnerCalls.Count, Is.EqualTo(0), $"index {i}");
                }
            }
        }

        [UnityTest]
        public IEnumerator ClientExcludeOwner([Range(0, 3)] int index)
        {
            // all components on same gameobject, so they are all owner
            serverComponent[index].ClientExcludeOwner(1, "hello");

            yield return null;
            yield return null;

            for (var i = 0; i < COMPONENT_COUNT; i++)
            {
                Assert.That(clientComponent[i].ClientExcludeOwnerCalls.Count, Is.EqualTo(0), $"index {i}");
            }
        }
    }
}
