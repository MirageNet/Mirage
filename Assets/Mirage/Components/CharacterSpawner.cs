using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage
{
    public class ReconnectManager : MonoBehaviour
    {
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        private Dictionary<string, OwnedObjects> _objects;


        public void Awake()
        {
            Server.Authenticated.AddListener(OnServerAuthenticated);
            Server.Disconnected.AddListener(OnServerDisconnected);
        }

        private void OnServerAuthenticated(INetworkPlayer player)
        {
            if (!(player.Authentication.Data is IReconnectKey reconnect))
                return;

            var key = reconnect.GetReconnectKey();
            if (_objects.TryGetValue(key, out var ownedObjects))
            {
                ServerObjectManager.AddCharacter(player, ownedObjects.Character);
                foreach (var otherOwnedObject in ownedObjects.OtherOwnedObjects)
                {
                    ServerObjectManager.Spawn(otherOwnedObject, player);
                }
            }
            else
            {
                // todo spawn new character here because they are new player
            }
        }

        private void OnServerDisconnected(INetworkPlayer player)
        {
            if (!(player.Authentication.Data is IReconnectKey reconnect))
                return;

            var key = reconnect.GetReconnectKey();
            if (!_objects.TryGetValue(key, out var ownedObjects))
            {
                ownedObjects = new OwnedObjects();
                _objects.Add(key, ownedObjects);
            }

            ownedObjects.Character = player.Identity;
            // todo add way to get owned objects from player
        }

        public interface IReconnectKey
        {
            public string GetReconnectKey();
        }
        private class OwnedObjects
        {
            public NetworkIdentity Character;
            public readonly List<NetworkIdentity> OtherOwnedObjects = new List<NetworkIdentity>();
        }
    }

    public class MatchSceneLoader : MonoBehaviour
    {
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;
        public NetworkClient Client;

        public NetworkIdentity CharacterPrefab;

        public async UniTask LoadScene(string sceneName, IEnumerable<INetworkPlayer> players)
        {
            // mark all players are not ready
            foreach (var player in players)
            {
                player.SceneIsReady = false;
            }
            NetworkServer.SendToMany(players, new SceneMessage { MainActivateScene = sceneName });

            // wait until all players are ready
            await UniTask.WaitUntil(() => players.All(player => player.SceneIsReady));

            // spawn characters for all players
            foreach (var player in players)
            {
                var clone = Instantiate(CharacterPrefab);
                ServerObjectManager.AddCharacter(player, clone);
            }
        }
    }

    public class SimpleCharacterSpawner : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger<CharacterSpawner>();

        [Header("References")]
        public NetworkClient Client;
        public NetworkServer Server;
        public NetworkSceneManager SceneManager;
        public ClientObjectManager ClientObjectManager;
        public ServerObjectManager ServerObjectManager;

        [Header("Spawn")]
        public NetworkIdentity PlayerPrefab;

        [Tooltip("Whether to span the player upon connection automatically")]
        public bool AutoSpawn = true;

        [Tooltip("Should the characters gameObject name be set when it is spawned")]
        public bool SetName = true;
    }

    public class SpawnPositionList : ScriptableObject
    {
        private List<SpawnPosition> _spawnPositions = new List<SpawnPosition>();

        public void Add(SpawnPosition spawnPosition)
        {
            _spawnPositions.Add(spawnPosition);
        }

        public void Remove(SpawnPosition spawnPosition)
        {
            _spawnPositions.Remove(spawnPosition);
        }

        public Transform Get(int index)
        {
            return _spawnPositions[index].transform;
        }

        public Transform GetRandom()
        {
            var index = UnityEngine.Random.Range(0, _spawnPositions.Count);
            return _spawnPositions[index].transform;
        }
    }
    public class SpawnPosition : MonoBehaviour
    {
        [SerializeField] private SpawnPositionList _spawnPositionList;

        private void OnEnable()
        {
            _spawnPositionList.Add(this);
        }
        private void OnDisable()
        {
            _spawnPositionList.Remove(this);
        }
    }



    /// <summary>
    /// Spawns a player as soon as the connection is authenticated
    /// </summary>
    public class CharacterSpawner : MonoBehaviour
    {
        private static readonly ILogger logger = LogFactory.GetLogger<CharacterSpawner>();

        [Header("References")]
        public NetworkClient Client;
        public NetworkServer Server;
        public NetworkSceneManager SceneManager;
        public ClientObjectManager ClientObjectManager;
        public ServerObjectManager ServerObjectManager;

        [Header("Spawn")]
        public NetworkIdentity PlayerPrefab;

        [Tooltip("Whether to span the player upon connection automatically")]
        public bool AutoSpawn = true;

        [Tooltip("Should the characters gameObject name be set when it is spawned")]
        public bool SetName = true;

        [Header("Location")]
        public int startPositionIndex;

        /// <summary>
        /// List of transforms where players can be spawned
        /// </summary>
        public List<Transform> startPositions = new List<Transform>();

        /// <summary>
        /// Enumeration of methods of where to spawn player objects in multiplayer games.
        /// </summary>
        public enum PlayerSpawnMethod { Random, RoundRobin }

        /// <summary>
        /// The current method of spawning players used by the CharacterSpawner.
        /// </summary>
        [Tooltip("Round Robin or Random order of Start Position selection")]
        public PlayerSpawnMethod playerSpawnMethod;

        private void ThrowIfNotSet(Object field, string name)
        {
            if (field == null)
                throw new InvalidOperationException($"{name} not assigned");
        }

        // Start is called before the first frame update
        protected internal virtual void Awake()
        {
            ThrowIfNotSet(PlayerPrefab, nameof(PlayerPrefab));
            ThrowIfNotSet(Client, nameof(Client));
            ThrowIfNotSet(ClientObjectManager, nameof(ClientObjectManager));
            ThrowIfNotSet(Server, nameof(Server));
            ThrowIfNotSet(ServerObjectManager, nameof(ServerObjectManager));

            Client.Started.AddListener(ClientSetup);

            Server.Started.AddListener(ServerSetup);

            //if (Client != null)
            //{
            //    if (SceneManager != null)
            //    {
            //        SceneManager.OnClientFinishedSceneChange.AddListener(OnClientFinishedSceneChange);
            //    }
            //    else
            //    {
            //        Client.Authenticated.AddListener(OnClientAuthenticated);
            //        Client.Connected.AddListener(OnClientConnected);
            //    }
            //}
            //if (Server != null)
            //{
            //    Server.Started.AddListener(OnServerStarted);
            //    if (ServerObjectManager == null)
            //    {
            //        throw new InvalidOperationException("Assign a ServerObjectManager");
            //    }
            //}
        }

        private void ClientSetup()
        {
            ClientObjectManager.RegisterPrefab(PlayerPrefab);
        }

        private void ServerSetup()
        {
            Server.Authenticated.AddListener(ServerAuthenticated);
            if (SceneManager != null)
                SceneManager.OnPlayerSceneReady.AddListener(ServerPlayerSceneReady);
        }

        private void ServerAuthenticated(INetworkPlayer player)
        {
            // if loading scene, do nothing
            if (!player.SceneIsReady)
                return;

            AddCharacter(player);
        }

        public void ServerPlayerSceneReady(INetworkPlayer player)
        {
            AddCharacter(player);
        }

        private void AddCharacter(INetworkPlayer player)
        {
            logger.Log("CharacterSpawner.AddCharacter");

            if (player.HasCharacter)
            {
                // player already has character on server, but client asked for it
                // so we respawn it here so that client recieves it again
                // this can happen when client loads normally, but server addititively
                ServerObjectManager.Spawn(player.Identity);
            }
            else
            {
                OnServerAddPlayer(player);
            }
        }

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="player">Connection from client.</param>
        public virtual void OnServerAddPlayer(INetworkPlayer player)
        {
            var startPos = GetStartPosition();
            var character = startPos != null
                ? Instantiate(PlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(PlayerPrefab);

            if (SetName)
                SetCharacterName(player, character);
            ServerObjectManager.AddCharacter(player, character.gameObject);
        }

        protected virtual void SetCharacterName(INetworkPlayer player, NetworkIdentity character)
        {
            // When spawning a player game object, Unity defaults to something like "MyPlayerObject(clone)"
            // which sucks... So let's override it and make it easier to debug. Credit to Mirror for the nice touch.
            character.name = $"{PlayerPrefab.name} {player.Address}";
        }

        /// <summary>
        /// This finds a spawn position based on start position objects in the scene.
        /// <para>This is used by the default implementation of OnServerAddPlayer.</para>
        /// </summary>
        /// <returns>Returns the transform to spawn a player at, or null.</returns>
        public virtual Transform GetStartPosition()
        {
            if (startPositions.Count == 0)
                return null;

            if (playerSpawnMethod == PlayerSpawnMethod.Random)
            {
                return startPositions[UnityEngine.Random.Range(0, startPositions.Count)];
            }
            else
            {
                var startPosition = startPositions[startPositionIndex];
                startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
                return startPosition;
            }
        }
    }
}
