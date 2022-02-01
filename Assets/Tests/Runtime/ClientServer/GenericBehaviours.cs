using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// extreme example/edge case to make sure generic really work
// These check the Weaver methods: GetMethodInBaseType and MatchGenericParameters
namespace Mirage.Tests.Runtime.ClientServer.GenericBehaviours.NoMiddle
{
    public class With3<A, B, C> : NetworkBehaviour
    {
        [SyncVar] public int a;
    }

    public class With2<D, E> : With3<int, E, D>
    {
    }

    public class With0 : With2<Vector3, float>
    {
        [SyncVar] public float b;
    }

    public class GenericNetworkBehaviorSyncvarNoMiddleTest : ClientServerSetup<With0>
    {
        [UnityTest]
        public IEnumerator CanSyncValues()
        {
            serverComponent.a = 10;
            serverComponent.b = 12.5f;
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.a, Is.EqualTo(10));
            Assert.That(clientComponent.b, Is.EqualTo(12.5f));
        }
    }
}
namespace Mirage.Tests.Runtime.ClientServer.GenericBehaviours.SyncVarMiddle
{
    public class With3<A, B, C> : NetworkBehaviour
    {
        [SyncVar] public int a;
    }

    public class With2<D, E> : With3<int, E, D>
    {
        [SyncVar] public int b;
    }

    public class With0 : With2<Vector3, float>
    {
        [SyncVar] public float c;
    }

    public class GenericNetworkBehaviorSyncvarNoMiddleTest : ClientServerSetup<With0>
    {
        [UnityTest]
        public IEnumerator CanSyncValues()
        {
            serverComponent.a = 10;
            serverComponent.b = 20;
            serverComponent.c = 12.5f;
            yield return new WaitForSeconds(0.2f);

            Assert.That(clientComponent.a, Is.EqualTo(10));
            Assert.That(clientComponent.b, Is.EqualTo(20));
            Assert.That(clientComponent.c, Is.EqualTo(12.5f));
        }
    }
}
