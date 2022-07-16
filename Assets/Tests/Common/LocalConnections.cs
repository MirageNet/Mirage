namespace Mirage.Tests
{

    public static class LocalConnections
    {
        public static (NetworkPlayer serverPlayer, NetworkPlayer clientPlayer) PipedConnections(IMessageReceiver clientMessages, IMessageReceiver serverMessages)
        {
            // we can re-use networkclient's handlers here as it just needs connection and player
            var clientHandler = new NetworkClient.DataHandler(clientMessages);
            var serverHandler = new NetworkClient.DataHandler(serverMessages);

            (var clientConn, var serverConn) = PipePeerConnection.Create(clientHandler, serverHandler, null, null);

            var clientPlayer = new NetworkPlayer(clientConn);
            var serverPlayer = new NetworkPlayer(serverConn);

            // give connections to each other so they can invoke handlers
            clientHandler.SetConnection(clientConn, clientPlayer);
            serverHandler.SetConnection(serverConn, serverPlayer);

            return (serverPlayer, clientPlayer);
        }
    }
}
