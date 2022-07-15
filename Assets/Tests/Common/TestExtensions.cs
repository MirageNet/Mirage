using System.Collections.Generic;
using System.Reflection;
using Mirage.SocketLayer;
using NSubstitute;

namespace Mirage.Tests
{
    public static class NetworkIdentityTestExtensions
    {
        public static void SetSceneId(this NetworkIdentity identity, int id, int hash = 0)
        {
            var fieldInfo = typeof(NetworkIdentity).GetField("_sceneId", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(identity, (ulong)((((long)hash) << 32) | (long)id));
        }
    }

    public static class NetworkServerTestExtensions
    {
        public static void AddTestPlayer(this NetworkServer server, INetworkPlayer player)
        {
            var info = typeof(NetworkServer).GetField("connections", BindingFlags.Instance | BindingFlags.NonPublic);
            var connections = (Dictionary<IConnection, INetworkPlayer>)info.GetValue(server);

            var connectiion = Substitute.For<IConnection>();
            player.Connection.Returns(connectiion);
            connections.Add(connectiion, player);
        }
    }
}

