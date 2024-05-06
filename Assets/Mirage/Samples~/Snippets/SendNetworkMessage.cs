using UnityEngine;

namespace Mirage.Snippets.SendNetworkMessages
{
    // CodeEmbed-Start: send-score


    // optional: add NetworkMessage attribute so that it is easier for Mirage to find
    [NetworkMessage]
    // using structs to prevent GC allocations
    public struct ScoreMessage
    {
        public int score;
        public Vector3 scorePos;
        public int lives;
    }

    public class Scores : MonoBehaviour
    {
        // attach these in the inspector
        public NetworkServer Server;
        public NetworkClient Client;

        private void Awake()
        {
            Client.Started.AddListener(ClientStarted);
        }

        private void ClientStarted()
        {
            // Register Client to listen for the ScoreMessage
            Client.MessageHandler.RegisterHandler<ScoreMessage>(OnScore);
        }


        private void OnScore(INetworkPlayer player, ScoreMessage msg)
        {
            Debug.Log("ScoreMessage received on client with score " + msg.score);
        }

        // Send from server
        public void SendScore(int score, Vector3 scorePos, int lives)
        {
            var msg = new ScoreMessage()
            {
                score = score,
                scorePos = scorePos,
                lives = lives
            };

            // also send to host player so we can update ui
            Server.SendToAll(msg, authenticatedOnly: true, excludeLocalPlayer: false);
        }
    }
    // CodeEmbed-End: send-score
}
