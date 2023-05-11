using System.Collections.Generic;

namespace Mirage
{
    public class ReconnectCharacterSpawner : CharacterSpawner
    {
        public Dictionary<object, NetworkIdentity> Characters = new Dictionary<object, NetworkIdentity>();

        protected internal override void Awake()
        {
            base.Awake();
            if (Server != null)
            {
                Server.Disconnected.AddListener(Disconnected);
            }
        }

        private void Disconnected(INetworkPlayer player)
        {
            var authenticationData = player.AuthenticationData;
            if (!(authenticationData is IHasCharacterKey data))
                return;

            if (!player.HasCharacter)
                return;

            var character = player.Identity;
            player.RemoveOwnedObject(character);

            var key = data.GetCharacterKey();
            Characters[key] = character;
        }

        public override void OnServerAddPlayer(INetworkPlayer player)
        {
            var authenticationData = player.AuthenticationData;
            if (!(authenticationData is IHasCharacterKey data))
            {
                // if authenticationData doesn't use interface, just spawn normally
                base.OnServerAddPlayer(player);
                return;
            }


            var key = data.GetCharacterKey();
            if (Characters.TryGetValue(key, out var character))
            {
                // add existing character to new player

                if (SetName)
                    SetCharacterName(player, character);
                ServerObjectManager.AddCharacter(player, character);
            }
            else
            {
                base.OnServerAddPlayer(player);

                // get new character
                character = player.Identity;
                Characters.Add(key, character);
            }
        }

        public interface IHasCharacterKey
        {
            object GetCharacterKey();
        }
    }
}
