using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class PaddleSpawner : PlayerSpawner
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        GameObject ball;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = server.NumPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            NetworkIdentity player = Instantiate(playerPrefab, start.position, start.rotation);
            server.AddPlayerForConnection(conn, player.gameObject);

            // spawn ball if two players
            if (server.NumPlayers == 2)
            {
                ball = Instantiate(client.spawnPrefabs.Find(prefab => prefab.name == "Ball"));
                server.Spawn(ball);
            }
        }


        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // destroy ball
            if (ball != null)
                server.Destroy(ball);

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }


    }
}