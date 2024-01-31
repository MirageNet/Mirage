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
        public static void AddTestPlayer(this NetworkServer server, INetworkPlayer player, bool authenticated = true)
        {
            var info = typeof(NetworkServer).GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic);
            var connections = (Dictionary<IConnection, INetworkPlayer>)info.GetValue(server);

            var connection = Substitute.For<IConnection>();
            player.Connection.Returns(connection);
            if (authenticated)
            {
                player.IsAuthenticated.Returns(true);
                var auth = new Authentication.PlayerAuthentication(null, null);
                player.Authentication.Returns(auth);
            }

            connections.Add(connection, player);
        }
    }
}

