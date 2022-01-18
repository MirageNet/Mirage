using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Mirage
{

    /// <summary>
    /// Spawns a player as soon as the connection is authenticated
    /// </summary>
    public class CharacterSpawner : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(CharacterSpawner));

        [FormerlySerializedAs("client")]
        public NetworkClient Client;
        [FormerlySerializedAs("server")]
        public NetworkServer Server;
        [FormerlySerializedAs("sceneManager")]
        public NetworkSceneManager SceneManager;
        [FormerlySerializedAs("clientObjectManager")]
        public ClientObjectManager ClientObjectManager;
        [FormerlySerializedAs("serverObjectManager")]
        public ServerObjectManager ServerObjectManager;
        [FormerlySerializedAs("playerPrefab")]
        public NetworkIdentity PlayerPrefab;

        /// <summary>
        /// Whether to span the player upon connection automatically
        /// </summary>
        public bool AutoSpawn = true;

        // Start is called before the first frame update
        public virtual void Awake()
        {
            if (PlayerPrefab == null)
            {
                throw new InvalidOperationException("Assign a player in the CharacterSpawner");
            }
            if (Client != null)
            {
                if (SceneManager != null)
                {
                    SceneManager.OnClientFinishedSceneChange.AddListener(OnClientFinishedSceneChange);
                }
                else
                {
                    Client.Authenticated.AddListener(OnClientAuthenticated);
                    Client.Connected.AddListener(OnClientConnected);
                }
            }
            if (Server != null)
            {
                Server.Started.AddListener(OnServerStarted);
                if (ServerObjectManager == null)
                {
                    throw new InvalidOperationException("Assign a ServerObjectManager");
                }
            }
        }

        void OnDestroy()
        {
            if (Client != null && SceneManager != null)
            {
                SceneManager.OnClientFinishedSceneChange.RemoveListener(OnClientFinishedSceneChange);
                Client.Authenticated.RemoveListener(OnClientAuthenticated);
            }
            if (Server != null)
            {
                Server.Started.RemoveListener(OnServerStarted);
            }
        }

        internal void OnClientConnected(INetworkPlayer player)
        {
            if (ClientObjectManager != null)
            {
                ClientObjectManager.RegisterPrefab(PlayerPrefab);
            }
            else
            {
                throw new InvalidOperationException("Assign a ClientObjectManager");
            }
        }

        private void OnClientAuthenticated(INetworkPlayer _)
        {
            Client.Send(new AddCharacterMessage());
        }

        private void OnServerStarted()
        {
            Server.MessageHandler.RegisterHandler<AddCharacterMessage>(OnServerAddPlayerInternal);
        }

        /// <summary>
        /// Called on the client when a normal scene change happens.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneOperation">The type of scene load that happened.</param>
        public virtual void OnClientFinishedSceneChange(Scene scene, SceneOperation sceneOperation)
        {
            if (AutoSpawn && sceneOperation == SceneOperation.Normal)
                RequestServerSpawnPlayer();
        }

        public virtual void RequestServerSpawnPlayer()
        {
            Client.Send(new AddCharacterMessage());
        }

        void OnServerAddPlayerInternal(INetworkPlayer player, AddCharacterMessage msg)
        {
            logger.Log("CharacterSpawner.OnServerAddPlayer");

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
            Transform startPos = GetStartPosition();
            NetworkIdentity character = startPos != null
                ? Instantiate(PlayerPrefab, startPos.position, startPos.rotation)
                : Instantiate(PlayerPrefab);

            ServerObjectManager.AddCharacter(player, character.gameObject);
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
                Transform startPosition = startPositions[startPositionIndex];
                startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
                return startPosition;
            }
        }

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
    }
}
