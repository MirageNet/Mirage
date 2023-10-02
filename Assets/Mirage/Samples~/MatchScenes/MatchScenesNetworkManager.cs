using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Examples.MatchScenes
{
    internal class Match
    {
        public List<INetworkPlayer> Players = new List<INetworkPlayer>();
        public bool Full;
        public Scene scene;

        public event Action OnSceneLoad;

        public void SetScene(AsyncOperation _)
        {
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            // remove extra AudioListener so it doesn't spam unity console...
            // todo do this in a better way, like only spawning camera in scene for local player
            foreach (var listener in Resources.FindObjectsOfTypeAll<AudioListener>().Skip(1))
            {
                listener.enabled = false;
            }

            OnSceneLoad?.Invoke();

        }
    }
    /// <summary>
    /// Creates a new scene for each match, 2 players per match
    /// </summary>
    public class MatchScenesNetworkManager : NetworkManager
    {
        [Scene] public string offlineScene;
        [Scene] public string matchScene;

        [SerializeField, NetworkedPrefab] private NetworkIdentity _character;

        private List<Match> matchList = new List<Match>();
        private Dictionary<INetworkPlayer, Match> matchLookup = new Dictionary<INetworkPlayer, Match>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Server.Started.AddListener(ServerStarted);
            Server.Authenticated.AddListener(OnServerConnected);
            Server.Disconnected.AddListener(OnServerDisconnected);

            Client.Authenticated.AddListener(OnClientConnected);
            Client.Disconnected.AddListener(OnClientDisconnected);
        }

        private void ServerStarted()
        {
            Server.MessageHandler.RegisterHandler<SceneLoaded>(PlayerLoadedScene);
        }

        private void OnServerConnected(INetworkPlayer player)
        {
            Debug.Log("OnServerConnected");
            var match = GetNextMatch();

            match.Players.Add(player);
            if (match.Players.Count == 2)
                match.Full = true;
            matchLookup.Add(player, match);
        }

        private Match GetNextMatch()
        {
            // get a match that has 1 player, or create a new one
            var match = matchList.LastOrDefault();
            // no matches, or last is full
            if (match == null || match.Full)
            {
                // Create new match
                match = new Match();
                matchList.Add(match);

                var op = SceneManager.LoadSceneAsync(matchScene, new LoadSceneParameters
                {
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.Physics3D,
                });
                // when complete add scene to match, it will be the last scene in GetSceneAt
                op.completed += match.SetScene;
            }

            return match;
        }

        private void OnServerDisconnected(INetworkPlayer player)
        {
            Debug.Log("OnServerDisconnected");
            if (matchLookup.TryGetValue(player, out var match))
            {
                match.Players.Remove(player);
                matchLookup.Remove(player);

                // started match, that all players have disconnected from
                if (match.Full && match.Players.Count == 0)
                {
                    matchList.Remove(match);
                    SceneManager.UnloadSceneAsync(match.scene);
                }
            }
        }

        private void PlayerLoadedScene(INetworkPlayer player, SceneLoaded message)
        {
            Debug.Log("PlayerLoadedScene");
            var match = matchLookup[player];

            var playerIndex = match.Players.IndexOf(player);

            // add player now if scene is loaded
            if (match.scene.IsValid())
            {
                AddCharacter(match.scene, player, playerIndex);
            }
            // or add player after scene has finished loading on server
            else
            {
                match.OnSceneLoad += () =>
                {
                    AddCharacter(match.scene, player, playerIndex);
                };
            }
        }

        private void AddCharacter(Scene scene, INetworkPlayer player, int v)
        {
            Debug.Log("AddCharacter");
            var clone = Instantiate(_character);

            // make it easier to tell players apart
            clone.transform.position = new Vector3(-2 + (4 * v), 0, 0);
            clone.GetComponentInChildren<Renderer>().material.color = v == 0 ? Color.red : Color.green;
            clone.GetComponent<MatchScenesPlayer>().color = v == 0 ? Color.red : Color.green;

            var sceneChecker = (SceneVisibilityChecker)clone.Visibility;
            sceneChecker.MoveToScene(scene);
            ServerObjectManager.AddCharacter(player, clone);
        }

        private void OnClientConnected(INetworkPlayer player)
        {
            Debug.Log("OnClientConnected");
            UniTask.Void(async () =>
            {
                await SceneManager.LoadSceneAsync(matchScene).ToUniTask();
                Debug.Log("OnClientConnected.LoadedScene");
                player.Send(new SceneLoaded());
            });
        }
        [NetworkMessage]
        public struct SceneLoaded { }

        private void OnClientDisconnected(ClientStoppedReason arg0)
        {
            Debug.Log("OnClientDisconnected");
            _ = SceneManager.LoadSceneAsync(offlineScene);
            // destory this object, because we are going back to offline scene where another instance will exist
            Destroy(gameObject);
        }
    }
}
