using System.Linq;
using UnityEngine;

namespace Mirage.Examples.Pong
{
    public class PaddleSpawner : CharacterSpawner
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;
        public GameObject ballPrefab;
        private GameObject ball;

        protected override void Awake()
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
            var start = Server.AllPlayers.Count(x => x.HasCharacter) == 0 ? leftRacketSpawn : rightRacketSpawn;
            var character = Instantiate(PlayerPrefab, start.position, start.rotation);
            ServerObjectManager.AddCharacter(player, character.gameObject);

            // spawn ball if two players
            if (Server.AllPlayers.Count(x => x.HasCharacter) == 2)
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
