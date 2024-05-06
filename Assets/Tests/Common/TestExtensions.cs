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
            var connections = server.Reflection_GetConnections();
            var authenticatedPlayers = server.Reflection_AuthenticatedPlayers();
            var connection = Substitute.For<IConnection>();
            player.Connection.Returns(connection);

            connections.Add(connection, player);
            if (authenticated)
            {
                player.IsAuthenticated.Returns(true);
                var auth = new Authentication.PlayerAuthentication(null, null);
                player.Authentication.Returns(auth);
                authenticatedPlayers.Add(player);
            }
        }

        public static Dictionary<IConnection, INetworkPlayer> Reflection_GetConnections(this NetworkServer server)
        {
            var connectionsInfo = typeof(NetworkServer).GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic);
            var connections = (Dictionary<IConnection, INetworkPlayer>)connectionsInfo.GetValue(server);
            return connections;
        }
        public static List<INetworkPlayer> Reflection_AuthenticatedPlayers(this NetworkServer server)
        {
            var authenticatedPlayersInfo = typeof(NetworkServer).GetField("_authenticatedPlayers", BindingFlags.Instance | BindingFlags.NonPublic);
            var authenticatedPlayers = (List<INetworkPlayer>)authenticatedPlayersInfo.GetValue(server);
            return authenticatedPlayers;
        }
    }
}

