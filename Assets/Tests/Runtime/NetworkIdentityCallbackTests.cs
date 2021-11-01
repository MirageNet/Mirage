using Mirage.Tests.Runtime.ClientServer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime
{
    public class NetworkIdentityCallbackTests : ClientServerSetup<MockComponent>
    {
        GameObject gameObject;

        [SetUp]
        public override void ExtraSetup()
        {
            gameObject = new GameObject();
            NetworkIdentity identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            _ = Substitute.For<INetworkPlayer>();
            _ = Substitute.For<INetworkPlayer>();
        }

        [TearDown]
        public override void ExtraTearDown()
        {
            // set isServer is false. otherwise Destroy instead of
            // DestroyImmediate is called internally, giving an error in Editor
            Object.DestroyImmediate(gameObject);
        }
    }
}
