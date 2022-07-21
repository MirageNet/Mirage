---
sidebar_position: 4
---
# Network Messages
For the most part, we recommend the high-level [ServerRpc](/docs/guides/remote-actions/server-rpc)/[ClientRpc](/docs/guides/remote-actions/client-rpc) calls and [SyncVar](/docs/guides/sync/sync-var), but you can also send low-level network messages. This can be useful if you want clients to send messages that are not tied to game objects, such as logging, analytics, or profiling information.

## Usage
1. Define a new struct (rather than a class to prevent GC allocations) that will represent your message.
2. Add any [supported Mirage types](/docs/guides/data-types) as public fields of that struct. This will be the data you want to send.
3. Register a handler for that message on the [NetworkServer](/docs/reference/Mirage/NetworkServer) and/or [NetworkClient](/docs/reference/Mirage/NetworkClient)'s `MessageHandler` depending on where you want to listen for that message being received.
4. Use the `Send()` method on the [NetworkClient](/docs/reference/Mirage/NetworkClient), [NetworkServer](/docs/reference/Mirage/NetworkServer), or [NetworkPlayer](/docs/reference/Mirage/NetworkPlayer) classes depending on which way you want to send the message.

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

    private void Awake() 
    {
        Client.MessageHandler.RegisterHandler<ScoreMessage>(OnScore); // Register Client to listen for the ScoreMessage
    }
    
    public void SendScore(int score, Vector3 scorePos, int lives)
    {
        ScoreMessage msg = new ScoreMessage()
        {
            score = score,
            scorePos = scorePos,
            lives = lives
        };

        Server.SendToAll(msg);
    }

    private void OnScore(INetworkPlayer player, ScoreMessage msg)
    {
        Debug.Log("ScoreMessage received on client with score " + msg.score);
    }
}
```

Note that there is no serialization code for the `ScoreMessage` struct in this source code example. Mirage will generate a reader and writer for ScoreMessage when it sees that it is being sent.
