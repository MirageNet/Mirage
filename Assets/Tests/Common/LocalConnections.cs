namespace Mirage.Tests
{

    public static class LocalConnections
    {
        public static (NetworkPlayer serverPlayer, NetworkPlayer clientPlayer) PipedConnections()
        {
            // we can re-use networkclient's handlers here as it just needs connection and player
            var clientHandler = new NetworkClient.DataHandler();
            var serverHandler = new NetworkClient.DataHandler();

            (SocketLayer.IConnection clientConn, SocketLayer.IConnection serverConn) = PipePeerConnection.Create(clientHandler, serverHandler);

            var clientPlayer = new NetworkPlayer(clientConn);
            var serverPlayer = new NetworkPlayer(serverConn);

            // give connections to each other so they can invoke handlers
            clientHandler.SetConnection(clientConn, clientPlayer);
            serverHandler.SetConnection(serverConn, serverPlayer);

            return (serverPlayer, clientPlayer);
        }
    }
}
