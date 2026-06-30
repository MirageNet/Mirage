using Mirage;

namespace Mirage.Snippets.Analyzers
{
    namespace M1305.Triggering
    {
        // CodeEmbed-Start: mirage1305-triggering
        // Error: Lacks [NetworkMessage] attribute, but is sent/registered as a message
        public struct PlayerScoreMessage
        {
            public int score;
        }

        public class GameClient : NetworkBehaviour
        {
            public void NotifyScore(INetworkPlayer player, PlayerScoreMessage msg)
            {
                player.Send(msg);
            }
        }
        // CodeEmbed-End: mirage1305-triggering
    }

    namespace M1305.Resolved
    {
        // CodeEmbed-Start: mirage1305-resolved
        // Correct: Message is marked with [NetworkMessage]
        [NetworkMessage]
        public struct PlayerScoreMessage
        {
            public int score;
        }

        public class GameClient : NetworkBehaviour
        {
            public void NotifyScore(INetworkPlayer player, PlayerScoreMessage msg)
            {
                player.Send(msg);
            }
        }
        // CodeEmbed-End: mirage1305-resolved
    }
}
