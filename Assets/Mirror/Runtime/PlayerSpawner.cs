using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mirror
{

    /// <summary>
    /// Spawns a player as soon  as the connection is authenticated
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {

        public NetworkClient client;
        public NetworkServer server;

        public NetworkIdentity playerPrefab;

        // Start is called before the first frame update
        public virtual void Start()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Assign a player in the PlayerSpawner");
                return;
            }

            if (client == null)
                client = GetComponent<NetworkClient>();
            if (server == null)
                server = GetComponent<NetworkServer>();

            client.Authenticated.AddListener(OnClientAuthenticated);
            server.Authenticated.AddListener(OnServerAuthenticated);
            client.RegisterPrefab(playerPrefab.gameObject);

            startPositions =
                FindObjectsOfType<NetworkStartPosition>()
                .Select(pos => pos.transform)
                .OrderBy(transform => transform.GetSiblingIndex())
                .ToList(); ;
        }

        private void OnServerAuthenticated(NetworkConnectionToClient connection)
        {
            // wait for client to send us an AddPlayerMessage
            connection.RegisterHandler<NetworkConnectionToClient, AddPlayerMessage>(OnServerAddPlayerInternal);
        }

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        private void OnClientAuthenticated(NetworkConnectionToServer connection)
        {
                // OnClientConnect by default calls AddPlayer but it should not do
                // that when we have online/offline scenes. so we need the
                // clientLoadedScene flag to prevent it.
                    // Ready/AddPlayer is usually triggered by a scene load completing. if no scene was loaded, then Ready/AddPlayer it here instead.
            if (!client.ready)
                client.Ready(connection);

            client.Send(new AddPlayerMessage());
        }

        void OnServerAddPlayerInternal(NetworkConnection conn, AddPlayerMessage msg)
        {
            if (LogFilter.Debug) Debug.Log("NetworkManager.OnServerAddPlayer");


            if (conn.identity != null)
            {
                Debug.LogError("There is already a player for this connection.");
                return;
            }

            OnServerAddPlayer(conn);
        }


        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public virtual void OnServerAddPlayer(NetworkConnection conn)
        {
            Transform startPos = GetStartPosition();
            NetworkIdentity player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            server.AddPlayerForConnection(conn, player.gameObject);
        }

        /// <summary>
        /// This finds a spawn position based on NetworkStartPosition objects in the scene.
        /// <para>This is used by the default implementation of OnServerAddPlayer.</para>
        /// </summary>
        /// <returns>Returns the transform to spawn a player at, or null.</returns>
        public Transform GetStartPosition()
        {
            // first remove any dead transforms
            startPositions.RemoveAll(t => t == null);

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
        /// List of transforms populted by NetworkStartPosition components found in the scene.
        /// </summary>
        public List<Transform> startPositions = new List<Transform>();

        /// <summary>
        /// Enumeration of methods of where to spawn player objects in multiplayer games.
        /// </summary>
        public enum PlayerSpawnMethod { Random, RoundRobin }

        /// <summary>
        /// The current method of spawning players used by the NetworkManager.
        /// </summary>
        [Tooltip("Round Robin or Random order of Start Position selection")]
        public PlayerSpawnMethod playerSpawnMethod;


        /// <summary>
        /// Registers the transform of a game object as a player spawn location.
        /// <para>This is done automatically by NetworkStartPosition components, but can be done manually from user script code.</para>
        /// </summary>
        /// <param name="start">Transform to register.</param>
        public void RegisterStartPosition(Transform start)
        {
            if (LogFilter.Debug) Debug.Log("RegisterStartPosition: (" + start.gameObject.name + ") " + start.position);
            startPositions.Add(start);
        }

        /// <summary>
        /// Unregisters the transform of a game object as a player spawn location.
        /// <para>This is done automatically by the <see cref="NetworkStartPosition">NetworkStartPosition</see> component, but can be done manually from user code.</para>
        /// </summary>
        /// <param name="start">Transform to unregister.</param>
        public void UnRegisterStartPosition(Transform start)
        {
            if (LogFilter.Debug) Debug.Log("UnRegisterStartPosition: (" + start.gameObject.name + ") " + start.position);
            startPositions.Remove(start);
        }

    }

}
