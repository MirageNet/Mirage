namespace Mirage.Tests
{

    public static class LocalConnections
    {
        public static (NetworkPlayer serverPlayer, NetworkPlayer clientPlayer) PipedConnections()
        {
            // we can re-use networkclient's handlers here as it just needs connection and player
            var serverHandler = new NetworkClient.DataHandler();
            var clientHandler = new NetworkClient.DataHandler();

            (SocketLayer.IConnection clientConn, SocketLayer.IConnection serverConn) = PipePeerConnection.Create(clientHandler, serverHandler);

            var serverPlayer = new NetworkPlayer(serverConn);
            var clientPlayer = new NetworkPlayer(clientConn);

            // give connections to each other so they can invoke handlers
            serverHandler.SetConnection(clientConn, clientPlayer);
            clientHandler.SetConnection(serverConn, serverPlayer);

            return (serverPlayer, clientPlayer);
        }
    }
}
