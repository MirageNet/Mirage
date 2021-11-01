# Network Messages
For the most part we recommend the high level [RPC](RemoteActions.md) calls and [SyncVar](../Sync/index.md), but you can also send low level network messages. This can be useful if you want clients to send messages that are not tied to game objects, such as logging, analytics or profiling information.

## Usage
1. Define a new struct (rather than a class to prevent GC allocations) which will represent your message.
2. Add any [supported Mirage types](../DataTypes.md) as public fields of that struct. This will be the data you want to send.
3. Register a handler for that message on <xref:Mirage.NetworkServer> and/or <xref:Mirage.NetworkClient>'s `MessageHandler` depending on where you want to listen for that message being received.
4. Use the `Send()` method on the <xref:Mirage.NetworkClient>, <xref:Mirage.NetworkServer> or <xref:Mirage.NetworkPlayer> classes depending on which way you want to send the message.

## Example
``` cs
using UnityEngine;
using Mirage;

public class Scores : MonoBehaviour
{
    // attach these in the inspector
    public NetworkServer Server;
    public NetworkClient Client;

    // using structs to prevent GC allocations
    public struct ScoreMessage
    {
        public int score;
        public Vector3 scorePos;
        public int lives;
    }

    void Awake() {
        Client.MessageHandler.RegisterHandler<ScoreMessage>(OnScore); // register Client to listen for the ScoreMessage
    }
    
    public void SendScore(int score, Vector3 scorePos, int lives)
    {
        ScoreMessage msg = new ScoreMessage()
        {
            score = score,
            scorePos = scorePos,
            lives = lives
        };

        NetworkServer.SendToAll(msg);
    }

    void OnScore(INetworkPlayer player, ScoreMessage msg)
    {
        Debug.Log("ScoreMessage received on client with score " + msg.score);
    }
}
```

Note that there is no serialization code for the `ScoreMessage` struct in this source code example. Mirage will generate a reader and writer for ScoreMessage when it sees that it is being sent.
