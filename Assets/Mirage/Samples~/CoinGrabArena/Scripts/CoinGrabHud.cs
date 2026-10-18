using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Mirage.Examples.CoinGrabArena
{
    /// <summary>
    /// Pure UI script that displays the round state, countdown timer, sorted scoreboard, and results banner.
    /// <para>
    /// Contains no netcode itself: it subscribes to events from <see cref="CoinGrabGameManager"/> and
    /// <see cref="CoinGrabPlayer"/> so the scoreboard only rebuilds when scores or players change.
    /// </para>
    /// </summary>
    public class CoinGrabHud : MonoBehaviour
    {
        [Header("Network References")]
        [RequiredField(autoFind: true)]
        public NetworkClient Client;
        [RequiredField(autoFind: true)]
        public NetworkServer Server;
        [RequiredField]
        public CoinGrabGameManager GameManager;

        [Header("HUD Elements")]
        [RequiredField]
        public GameObject HudRoot;
        [RequiredField]
        public Text StateText;
        [RequiredField]
        public Text TimerText;
        [RequiredField]
        public Button StartAloneButton;
        [RequiredField]
        public Text ScoreboardText;
        [RequiredField]
        public GameObject ResultsPanel;
        [RequiredField]
        public Text WinnerText;

        [Header("Player Name Bar")]
        [RequiredField]
        public InputField NameInputField;
        [RequiredField]
        public Button SetNameButton;

        private static readonly System.Comparison<CoinGrabPlayer> PlayerScoreComparison = ComparePlayersByScore;

        private readonly List<CoinGrabPlayer> _players = new List<CoinGrabPlayer>();
        private readonly StringBuilder _scoreboardBuilder = new StringBuilder(256);

        private NetworkWorld _subscribedWorld;
        private bool _scoreboardDirty;
        private int _lastDisplayedSeconds = -1;
        private int _lastWaitingPlayerCount = -1;

        private void Awake()
        {
            Client.Started.AddListener(OnNetworkStarted);
            Client.Disconnected.AddListener(OnClientDisconnected);
            Server.Started.AddListener(OnNetworkStarted);
            Server.Stopped.AddListener(OnServerStopped);

            GameManager.OnGameStateChanged += OnGameStateChanged;
            SetNameButton.onClick.AddListener(OnSetNameClicked);
            StartAloneButton.onClick.AddListener(OnStartAloneClicked);

            HudRoot.SetActive(false);
            ResultsPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Client != null)
            {
                Client.Started.RemoveListener(OnNetworkStarted);
                Client.Disconnected.RemoveListener(OnClientDisconnected);
            }

            if (Server != null)
            {
                Server.Started.RemoveListener(OnNetworkStarted);
                Server.Stopped.RemoveListener(OnServerStopped);
            }

            GameManager.OnGameStateChanged -= OnGameStateChanged;
            UnsubscribeFromWorld();
        }

        private void Update()
        {
            if (!Client.Active && !Server.Active)
                return;

            if (_scoreboardDirty)
            {
                _scoreboardDirty = false;
                RebuildScoreboard();
                UpdateStateLabel();
            }

            UpdateTimerLabel();
        }

        private void OnNetworkStarted()
        {
            // Prefer Client.World (which matches Server.World in Host mode); fall back to Server.World for dedicated Server mode
            var world = Client.Active ? Client.World : Server.World;
            if (world != _subscribedWorld)
            {
                UnsubscribeFromWorld();
                _subscribedWorld = world;
                _subscribedWorld.onSpawn += OnWorldSpawn;
                _subscribedWorld.onUnspawn += OnWorldUnspawn;

                foreach (var identity in _subscribedWorld.SpawnedIdentities)
                    OnWorldSpawn(identity);
            }

            HudRoot.SetActive(true);
            _scoreboardDirty = true;
            OnGameStateChanged();
        }

        private void OnClientDisconnected(ClientStoppedReason reason)
        {
            if (!Server.Active)
                CleanupHud();
        }

        private void OnServerStopped()
        {
            if (!Client.Active)
                CleanupHud();
        }

        private void CleanupHud()
        {
            UnsubscribeFromWorld();
            HudRoot.SetActive(false);
            ResultsPanel.SetActive(false);
            _lastDisplayedSeconds = -1;
            _lastWaitingPlayerCount = -1;
        }

        private void UnsubscribeFromWorld()
        {
            if (_subscribedWorld != null)
            {
                _subscribedWorld.onSpawn -= OnWorldSpawn;
                _subscribedWorld.onUnspawn -= OnWorldUnspawn;
                _subscribedWorld = null;
            }

            for (var i = 0; i < _players.Count; i++)
            {
                if (_players[i] != null)
                    _players[i].OnStatsChanged -= MarkScoreboardDirty;
            }

            _players.Clear();
        }

        private void OnWorldSpawn(NetworkIdentity identity)
        {
            if (!identity.TryGetComponent<CoinGrabPlayer>(out var player))
                return;

            if (_players.Contains(player))
                return;

            _players.Add(player);
            player.OnStatsChanged += MarkScoreboardDirty;
            _scoreboardDirty = true;
        }

        private void OnWorldUnspawn(uint netId, NetworkIdentity identity)
        {
            if (identity != null && identity.TryGetComponent<CoinGrabPlayer>(out var player))
            {
                player.OnStatsChanged -= MarkScoreboardDirty;
                _players.Remove(player);
            }

            for (var i = _players.Count - 1; i >= 0; i--)
            {
                if (_players[i] == null)
                    _players.RemoveAt(i);
            }

            _scoreboardDirty = true;
        }

        private void MarkScoreboardDirty()
        {
            _scoreboardDirty = true;
        }

        private void OnGameStateChanged()
        {
            _lastDisplayedSeconds = -1;
            _lastWaitingPlayerCount = -1;

            StartAloneButton.gameObject.SetActive(GameManager.State == RoundState.Waiting);
            ResultsPanel.SetActive(GameManager.State == RoundState.Results);
            if (GameManager.State == RoundState.Results)
                WinnerText.text = GameManager.WinnerText;

            UpdateStateLabel();
            UpdateTimerLabel();
        }

        private void UpdateStateLabel()
        {
            switch (GameManager.State)
            {
                case RoundState.Waiting:
                    if (_players.Count != _lastWaitingPlayerCount)
                    {
                        _lastWaitingPlayerCount = _players.Count;
                        StateText.text = $"Waiting for players ({_players.Count}/{GameManager.MinPlayers})";
                    }
                    break;

                case RoundState.Countdown:
                    StateText.text = "Get Ready!";
                    break;

                case RoundState.Playing:
                    StateText.text = $"Collect Coins! First to {GameManager.TargetScore} wins";
                    break;

                case RoundState.Results:
                    StateText.text = "Round Over";
                    break;
            }
        }

        private void UpdateTimerLabel()
        {
            if (GameManager.State == RoundState.Waiting)
            {
                if (_lastDisplayedSeconds != 0)
                {
                    _lastDisplayedSeconds = 0;
                    TimerText.text = string.Empty;
                }
                return;
            }

            // Only format a new timer string when the whole-second count changes to avoid per-frame GC allocations
            var wholeSeconds = Mathf.CeilToInt(GameManager.GetRemainingTime());
            if (wholeSeconds == _lastDisplayedSeconds)
                return;

            _lastDisplayedSeconds = wholeSeconds;

            if (GameManager.State == RoundState.Countdown)
                TimerText.text = wholeSeconds > 0 ? wholeSeconds.ToString() : "GO!";
            else if (GameManager.State == RoundState.Playing)
                TimerText.text = $"Time: {wholeSeconds}s";
            else
                TimerText.text = $"Next round in {wholeSeconds}s";
        }

        private void RebuildScoreboard()
        {
            // Sort descending by score; break ties by NetId so ordering stays deterministic
            _players.Sort(PlayerScoreComparison);

            _scoreboardBuilder.Clear();
            _scoreboardBuilder.AppendLine("<b>SCOREBOARD</b>");

            for (var i = 0; i < _players.Count; i++)
            {
                var player = _players[i];
                if (player == null)
                    continue;

                var colorHex = ColorUtility.ToHtmlStringRGB(player.Color);
                _scoreboardBuilder.Append("<color=#").Append(colorHex).Append(">●</color> ");

                if (player.IsLocalPlayer)
                    _scoreboardBuilder.Append("<b>").Append(player.PlayerName).Append(" (You)</b>");
                else
                    _scoreboardBuilder.Append(player.PlayerName);

                _scoreboardBuilder.Append(": ").Append(player.Score).AppendLine();
            }

            ScoreboardText.text = _scoreboardBuilder.ToString();
        }

        private static int ComparePlayersByScore(CoinGrabPlayer a, CoinGrabPlayer b)
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            var scoreComparison = b.Score.CompareTo(a.Score);
            if (scoreComparison != 0)
                return scoreComparison;

            return a.NetId.CompareTo(b.NetId);
        }

        private void OnSetNameClicked()
        {
            if (!Client.Active || Client.Player == null || !Client.Player.HasCharacter)
                return;

            // Ignore empty input locally so clicking Set Name with an blank field does not trigger a server error penalty
            var requestedName = NameInputField.text;
            if (string.IsNullOrWhiteSpace(requestedName))
                return;

            if (Client.Player.Identity.TryGetComponent<CoinGrabPlayer>(out var localPlayer))
                localPlayer.CmdSetPlayerName(requestedName);
        }

        private void OnStartAloneClicked()
        {
            GameManager.RequestStartAlone();
        }
    }
}
