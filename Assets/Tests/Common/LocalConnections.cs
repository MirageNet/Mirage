using System;

namespace Mirage.Tests
{
    public static class LocalConnections
    {
        public static (NetworkPlayer, NetworkPlayer) PipedConnections(IMessageSender serverHandler, IMessageSender clientHandler)
        {
            // todo fix pipe
            throw new NotImplementedException();
            //(IConnection c1, IConnection c2) = PipeConnection.CreatePipe();
            //var toServer = new NetworkPlayer(c2, clientHandler);
            //var toClient = new NetworkPlayer(c1, serverHandler);

            //return (toServer, toClient);
        }

        public static (NetworkPlayer, NetworkPlayer) PipedConnections()
        {
            // todo fix pipe
            throw new NotImplementedException();
            //return PipedConnections(Substitute.For<IMessageHandler>(), Substitute.For<IMessageHandler>());
        }
    }
}
