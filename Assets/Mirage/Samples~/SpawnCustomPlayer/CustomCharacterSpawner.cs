using System.ComponentModel;
using Mirage;
using UnityEngine;

namespace Example.CustomCharacter
{
    public class CustomCharacterSpawner : MonoBehaviour
    {
        [Header("References")]
        public NetworkClient Client;
        public NetworkServer Server;
        public ClientObjectManager ClientObjectManager;
        public ServerObjectManager ServerObjectManager;

        [Header("Prefabs")]
        // different prefabs based on the Race the player picks
        public CustomCharacter HumanPrefab;
        public CustomCharacter ElvishPrefab;
        public CustomCharacter DwarvishPrefab;

        public void Start()
        {
            Client.Started.AddListener(OnClientStarted);
            Client.Authenticated.AddListener(OnClientAuthenticated);
            Server.Started.AddListener(OnServerStarted);
        }

        private void OnClientStarted()
        {
            // make sure all prefabs are Register so mirage can spawn the character for this client and for other players
            ClientObjectManager.RegisterPrefab(HumanPrefab.Identity);
            ClientObjectManager.RegisterPrefab(ElvishPrefab.Identity);
            ClientObjectManager.RegisterPrefab(DwarvishPrefab.Identity);
        }

        // you can send the message here if you already know
        // everything about the character at the time of player
        // or at a later time when the user submits his preferences
        private void OnClientAuthenticated(INetworkPlayer player)
        {
            var mmoCharacter = new CreateMMOCharacterMessage
            {
                // populate the message with your data
                name = "player name",
                race = Race.Human,
                eyeColor = Color.red,
                hairColor = Color.black,
            };
            player.Send(mmoCharacter);
        }

        private void OnServerStarted()
        {
            // wait for client to send us an AddPlayerMessage
            Server.MessageHandler.RegisterHandler<CreateMMOCharacterMessage>(OnCreateCharacter);
        }

        private void OnCreateCharacter(INetworkPlayer player, CreateMMOCharacterMessage msg)
        {
            var prefab = GetPrefab(msg);

            // create your character object
            // use the data in msg to configure it
            var character = Instantiate(prefab);

            // set syncVars before telling mirage to spawn character
            // this will cause them to be sent to client in the spawn message
            character.PlayerName = msg.name;
            character.hairColor = msg.hairColor;
            character.eyeColor = msg.eyeColor;

            // spawn it as the character object
            ServerObjectManager.AddCharacter(player, character.Identity);
        }

        private CustomCharacter GetPrefab(CreateMMOCharacterMessage msg)
        {
            // get prefab based on race
            CustomCharacter prefab;
            switch (msg.race)
            {
                case Race.Human: prefab = HumanPrefab; break;
                case Race.Elvish: prefab = ElvishPrefab; break;
                case Race.Dwarvish: prefab = DwarvishPrefab; break;
                // default case to check that client sent valid race.
                // the only reason it should be invalid is if the client's code was modified by an attacker
                // throw will cause the client to be kicked
                default: throw new InvalidEnumArgumentException("Invalid race given");
            }

            return prefab;
        }
    }
    public class CustomCharacter : NetworkBehaviour
    {
        [SyncVar] public string PlayerName;
        [SyncVar] public Color hairColor;
        [SyncVar] public Color eyeColor;

        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);

        }

        private void OnStartClient()
        {
            // use name and color syncvars to modify renderer settings
        }
    }
}
