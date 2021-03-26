using UnityEngine;

namespace Mirage.Examples.Pong
{
    public class PaddleSpawner : CharacterSpawner
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public GameObject ballPrefab;

        GameObject ball;

        public override void OnServerAddPlayer(NetworkPlayer player)
        {
            // add player at correct spawn position
            Transform start = Server.NumberOfPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            NetworkIdentity character = Instantiate(PlayerPrefab, start.position, start.rotation);
            ServerObjectManager.AddCharacter(player, character.gameObject);

            // spawn ball if two players
            if (Server.NumberOfPlayers == 2)
            {
                ball = Instantiate(ballPrefab);
                ServerObjectManager.Spawn(ball);
            }
        }


        public void OnServerDisconnect(NetworkPlayer player)
        {
            // destroy ball
            if (ball != null)
                ServerObjectManager.Destroy(ball);
        }
    }
}
