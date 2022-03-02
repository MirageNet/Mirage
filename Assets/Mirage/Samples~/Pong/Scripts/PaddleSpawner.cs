using UnityEngine;

namespace Mirage.Examples.Pong
{
    public class PaddleSpawner : CharacterSpawner
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public GameObject ballPrefab;

        GameObject ball;

        public override void Awake()
        {
            base.Awake();

            if (Server != null)
            {
                // add disconnect event so that OnServerDisconnect will be called when player disconnects
                Server.Disconnected.AddListener(OnServerDisconnect);
            }
        }

        // override OnServerAddPlayer so to do custom spawn location for character
        // this method will be called by base class when player sends `AddCharacterMessage`
        public override void OnServerAddPlayer(INetworkPlayer player)
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

        public void OnServerDisconnect(INetworkPlayer _)
        {
            // after 1 player disconnects then destroy the balll
            if (ball != null)
                ServerObjectManager.Destroy(ball);
        }
    }
}
