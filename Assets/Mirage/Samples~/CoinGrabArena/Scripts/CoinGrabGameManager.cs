using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mirage.Examples.CoinGrabArena
{
    public enum RoundState
    {
        Waiting,
        Countdown,
        Playing,
        Results,
    }

    /// <summary>
    /// Server-driven round state machine on a scene <see cref="NetworkIdentity"/>.
    /// <para>
    /// Rather than syncing a countdown float every frame (which wastes bandwidth), the server syncs
    /// <see cref="RoundEndTime"/> once per state transition using <see cref="NetworkBehaviour.NetworkTime"/>.
    /// Each client then computes the remaining time locally from <c>RoundEndTime - NetworkTime.Time</c>.
    /// </para>
    /// </summary>
    public class CoinGrabGameManager : NetworkBehaviour
    {
        [Header("References")]
        [RequiredField]
        public CoinGrabSpawner PlayerSpawner;
        [RequiredField]
        public CoinSpawner CoinSpawner;

        [Header("Round Settings")]
        [Tooltip("Minimum players required to start a round. Set to 1 in the Inspector to test alone in Host mode.")]
        public int MinPlayers = 2;
        public int TargetScore = 30;
        public float CountdownDuration = 3f;
        public float RoundDuration = 120f;
        public float ResultsDuration = 5f;

        [Header("Synced State")]
        [SyncVar(hook = nameof(OnStateChanged), invokeHookOnServer = true)]
        public RoundState State = RoundState.Waiting;

        [SyncVar]
        public double RoundEndTime;

        [SyncVar(hook = nameof(OnWinnerTextChanged), invokeHookOnServer = true), MaxLength(256)]
        public string WinnerText;

        /// <summary>
        /// Fired when <see cref="State"/> or <see cref="WinnerText"/> changes so the HUD can update.
        /// </summary>
        public event Action OnGameStateChanged;

        private readonly List<CoinGrabPlayer> _serverPlayers = new List<CoinGrabPlayer>();
        private readonly StringBuilder _winnerBuilder = new StringBuilder(256);

        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartServer()
        {
            _serverPlayers.Clear();

            // Track players joining and leaving via NetworkWorld events instead of polling FindObjectsOfType
            Server.World.onSpawn += OnServerObjectSpawned;
            Server.World.onUnspawn += OnServerObjectUnspawned;

            foreach (var identity in Server.World.SpawnedIdentities)
                OnServerObjectSpawned(identity);

            EnterWaitingState();
        }

        private void OnStopServer()
        {
            if (Server != null && Server.World != null)
            {
                Server.World.onSpawn -= OnServerObjectSpawned;
                Server.World.onUnspawn -= OnServerObjectUnspawned;
            }

            _serverPlayers.Clear();
        }

        private void OnStartClient()
        {
            OnGameStateChanged?.Invoke();
        }

        private void Update()
        {
            if (!IsServer)
                return;

            switch (State)
            {
                case RoundState.Waiting:
                    if (_serverPlayers.Count >= MinPlayers)
                        EnterCountdownState();
                    break;

                case RoundState.Countdown:
                    if (_serverPlayers.Count < MinPlayers)
                        EnterWaitingState();
                    else if (NetworkTime.Time >= RoundEndTime)
                        EnterPlayingState();
                    break;

                case RoundState.Playing:
                    if (_serverPlayers.Count == 0)
                        EnterWaitingState();
                    else if (_serverPlayers.Count < MinPlayers || NetworkTime.Time >= RoundEndTime)
                        EndRound();
                    break;

                case RoundState.Results:
                    if (NetworkTime.Time >= RoundEndTime)
                    {
                        if (_serverPlayers.Count >= MinPlayers)
                            EnterCountdownState();
                        else
                            EnterWaitingState();
                    }
                    break;
            }
        }

        [Server]
        public void OnPlayerScored(CoinGrabPlayer player)
        {
            if (State == RoundState.Playing && player.Score >= TargetScore)
                EndRound();
        }

        /// <summary>
        /// Computes the remaining time in the current state on either client or server using synced NetworkTime.
        /// </summary>
        public float GetRemainingTime()
        {
            if (!IsServer && !IsClient)
                return 0f;

            return Mathf.Max(0f, (float)(RoundEndTime - NetworkTime.Time));
        }

        private void EnterWaitingState()
        {
            CoinSpawner.ClearAllCoins();
            WinnerText = string.Empty;
            RoundEndTime = 0;
            State = RoundState.Waiting;
        }

        private void EnterCountdownState()
        {
            CoinSpawner.ClearAllCoins();
            WinnerText = string.Empty;

            // Reset every player's score and teleport them back to a spawn point before the round starts
            for (var i = 0; i < _serverPlayers.Count; i++)
            {
                var player = _serverPlayers[i];
                PlayerSpawner.GetNextSpawnPoint(out var pos, out var rot);
                player.ServerResetForRound(pos, rot);
            }

            // Set RoundEndTime before changing State so the State hook on clients sees the updated timestamp
            RoundEndTime = NetworkTime.Time + CountdownDuration;
            State = RoundState.Countdown;
        }

        private void EnterPlayingState()
        {
            CoinSpawner.ResetSpawnTimer();
            RoundEndTime = NetworkTime.Time + RoundDuration;
            State = RoundState.Playing;
        }

        private void EndRound()
        {
            CoinSpawner.ClearAllCoins();

            WinnerText = BuildWinnerAnnouncement();
            RoundEndTime = NetworkTime.Time + ResultsDuration;
            State = RoundState.Results;
        }

        private string BuildWinnerAnnouncement()
        {
            if (_serverPlayers.Count == 0)
                return "No players remaining";

            var highestScore = -1;
            var winnerCount = 0;

            for (var i = 0; i < _serverPlayers.Count; i++)
            {
                var score = _serverPlayers[i].Score;
                if (score > highestScore)
                {
                    highestScore = score;
                    winnerCount = 1;
                }
                else if (score == highestScore)
                {
                    winnerCount++;
                }
            }

            _winnerBuilder.Clear();
            if (winnerCount == 1)
            {
                for (var i = 0; i < _serverPlayers.Count; i++)
                {
                    if (_serverPlayers[i].Score == highestScore)
                    {
                        _winnerBuilder.Append(_serverPlayers[i].PlayerName);
                        _winnerBuilder.Append(" wins with ");
                        _winnerBuilder.Append(highestScore);
                        _winnerBuilder.Append(" pts!");
                        break;
                    }
                }
            }
            else
            {
                _winnerBuilder.Append("Tie (");
                _winnerBuilder.Append(highestScore);
                _winnerBuilder.Append(" pts): ");

                var first = true;
                for (var i = 0; i < _serverPlayers.Count; i++)
                {
                    if (_serverPlayers[i].Score != highestScore)
                        continue;

                    if (!first)
                        _winnerBuilder.Append(", ");

                    _winnerBuilder.Append(_serverPlayers[i].PlayerName);
                    first = false;
                }
            }

            return _winnerBuilder.ToString();
        }

        private void OnServerObjectSpawned(NetworkIdentity identity)
        {
            if (identity.TryGetComponent<CoinGrabPlayer>(out var player) && !_serverPlayers.Contains(player))
                _serverPlayers.Add(player);
        }

        private void OnServerObjectUnspawned(uint netId, NetworkIdentity identity)
        {
            if (identity != null && identity.TryGetComponent<CoinGrabPlayer>(out var player))
                _serverPlayers.Remove(player);

            // Clean up any destroyed references in case Unity destroyed the object before unspawn fired
            for (var i = _serverPlayers.Count - 1; i >= 0; i--)
            {
                if (_serverPlayers[i] == null)
                    _serverPlayers.RemoveAt(i);
            }
        }

        private void OnStateChanged(RoundState oldState, RoundState newState)
        {
            OnGameStateChanged?.Invoke();
        }

        private void OnWinnerTextChanged(string oldText, string newText)
        {
            OnGameStateChanged?.Invoke();
        }
    }
}
