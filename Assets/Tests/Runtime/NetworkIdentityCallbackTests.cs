using System.Collections.Generic;
using Mirage.Tests.Runtime.ClientServer;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static Mirage.Tests.LocalConnections;
using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime
{
    public class NetworkIdentityCallbackTests : ClientServerSetup<MockComponent>
    {
        GameObject gameObject;
        NetworkIdentity identity;

        INetworkPlayer player1;
        INetworkPlayer player2;

        [SetUp]
        public override void ExtraSetup()
        {
            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.Server = server;
            identity.ServerObjectManager = serverObjectManager;

            player1 = Substitute.For<INetworkPlayer>();
            player2 = Substitute.For<INetworkPlayer>();
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
