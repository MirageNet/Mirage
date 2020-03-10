using UnityEngine;

namespace Mirror.Examples.Pong
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class NetworkManagerPong : NetworkManager
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        GameObject ball;

        protected override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = NumberOfActivePlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            GameObject player = Instantiate(PlayerPrefab, start.position, start.rotation);
            Server.AddPlayerForConnection(conn, player);

            // spawn ball if two players
            if (NumberOfActivePlayers == 2)
            {
                ball = Instantiate(SpawnPrefabs.Find(prefab => prefab.name == "Ball"));
                Server.Spawn(ball);
            }
        }

        protected override void OnServerDisconnect(NetworkConnection conn)
        {
            // destroy ball
            if (ball != null)
                Server.Destroy(ball);

            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}
