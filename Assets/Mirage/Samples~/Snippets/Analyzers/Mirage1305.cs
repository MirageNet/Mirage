using System.Collections.Generic;
using Mirage;
using Mirage.Serialization;

namespace Mirage.Snippets.Analyzers
{
    namespace M1305.Triggering
    {
        // CodeEmbed-Start: mirage1305-triggering
        // MIRAGE1305 convention: explicitly mark this message declaration.
        public struct PlayerScoreMessage
        {
            public int score;
        }

        public class GameExample : NetworkBehaviour
        {
            [Server]
            public void ServerSend(INetworkPlayer player)
            {
                // MIRAGE1305 on each of the calls below
                player.Send(new PlayerScoreMessage { score = 10 });
                Server.SendToAll(new PlayerScoreMessage(), authenticatedOnly: false, excludeLocalPlayer: false);
                Server.SendToMany(new List<INetworkPlayer>(), new PlayerScoreMessage(), excludeLocalPlayer: false);
                var bytes = MessagePacker.Pack(new PlayerScoreMessage());
                MessagePacker.Unpack<PlayerScoreMessage>(bytes, null);
                MessagePacker.GetId<PlayerScoreMessage>();
            }

            [Client]
            public void ClientRegister()
            {
                // MIRAGE1305 on each of the calls below
                Client.MessageHandler.RegisterHandler<PlayerScoreMessage>(OnScore);
                Client.MessageHandler.UnregisterHandler<PlayerScoreMessage>();
            }

            private void OnScore(INetworkPlayer player, PlayerScoreMessage msg) { }
        }
        // CodeEmbed-End: mirage1305-triggering
    }

    namespace M1305.Resolved
    {
        // CodeEmbed-Start: mirage1305-resolved
        // Explicitly requests serialization discovery in this assembly.
        [NetworkMessage]
        public struct PlayerScoreMessage
        {
            public int score;
        }

        public class GameExample : NetworkBehaviour
        {
            [Server]
            public void ServerSend(INetworkPlayer player)
            {
                player.Send(new PlayerScoreMessage { score = 10 });
                Server.SendToAll(new PlayerScoreMessage(), authenticatedOnly: false, excludeLocalPlayer: false);
                Server.SendToMany(new List<INetworkPlayer>(), new PlayerScoreMessage(), excludeLocalPlayer: false);
                var bytes = MessagePacker.Pack(new PlayerScoreMessage());
                MessagePacker.Unpack<PlayerScoreMessage>(bytes, null);
                MessagePacker.GetId<PlayerScoreMessage>();
            }

            [Client]
            public void ClientRegister()
            {
                Client.MessageHandler.RegisterHandler<PlayerScoreMessage>(OnScore);
                Client.MessageHandler.UnregisterHandler<PlayerScoreMessage>();
            }

            private void OnScore(INetworkPlayer player, PlayerScoreMessage msg) { }
        }
        // CodeEmbed-End: mirage1305-resolved
    }
}
