using System.Collections.Generic;

namespace Mirage.Tests
{

    public static class LocalConnections
    {
        public static (NetworkPlayer serverPlayer, NetworkPlayer clientPlayer) PipedConnections()
        {
            var serverDictionary = new Dictionary<SocketLayer.IConnection, INetworkPlayer>();
            var clientDictionary = new Dictionary<SocketLayer.IConnection, INetworkPlayer>();
            var serverHandler = new DataHandler(serverDictionary);
            var clientHandler = new DataHandler(clientDictionary);

            (SocketLayer.IConnection clientConn, SocketLayer.IConnection serverConn) = PipePeerConnection.Create(clientHandler, serverHandler);

            var serverPlayer = new NetworkPlayer(serverConn);
            var clientPlayer = new NetworkPlayer(clientConn);

            serverDictionary.Add(clientConn, clientPlayer);
            clientDictionary.Add(serverConn, serverPlayer);


            return (serverPlayer, clientPlayer);
        }

    }
}
