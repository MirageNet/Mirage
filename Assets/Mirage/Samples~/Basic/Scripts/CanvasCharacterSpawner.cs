using UnityEngine;

namespace Mirage.Examples.Basic
{
    public class CanvasCharacterSpawner : CharacterSpawner
    {
        [Header("Character Parent")]
        public Transform Parent;
        private int _playerCounter;

        protected override void Awake()
        {
            base.Awake();
            Server.Started.AddListener(ServerStarted);
        }

        private void ServerStarted()
        {
            // reset counter to 1 when starting server
            _playerCounter = 0;
        }

        private int GetNextPlayerId()
        {
            _playerCounter++;
            return _playerCounter;
        }

        public override void OnServerAddPlayer(INetworkPlayer player)
        {
            var character = Instantiate(PlayerPrefab);

            // We can set SyncVar here or in OnStartServer and they will be sent with first spawn message from AddCharacter
            var basicPlayer = character.GetComponent<BasicPlayer>();
            basicPlayer.playerNo = GetNextPlayerId();


            SetCharacterName(player, character);
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }
    }
}
