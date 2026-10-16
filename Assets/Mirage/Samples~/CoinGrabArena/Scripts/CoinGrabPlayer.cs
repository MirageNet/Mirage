using System;
using UnityEngine;

namespace Mirage.Examples.CoinGrabArena
{
    /// <summary>
    /// Player character for Coin Grab Arena.
    /// <para>
    /// Uses owner-authoritative movement via <see cref="NetworkTransform"/> (<c>ClientAuthority = true</c>)
    /// so movement feels instant for the local player without waiting for a round-trip to the server.
    /// This is acceptable in a casual arena game because the server still owns all gameplay rules:
    /// coin triggers only run on the server, and the server checks distance and round state before awarding points.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkTransform))]
    public class CoinGrabPlayer : NetworkBehaviour
    {
        private static readonly int ColorShaderId = Shader.PropertyToID("_Color");

        [Header("Components")]
        [RequiredField]
        public CharacterController CharacterController;
        [Tooltip("Non-trigger CapsuleCollider (isTrigger = false) enabled only on the server so Coin triggers detect the player.")]
        [RequiredField]
        public CapsuleCollider ServerCollider;
        [RequiredField]
        public Renderer BodyRenderer;
        [RequiredField]
        public TextMesh NameLabel;

        [Header("Movement")]
        public float MoveSpeed = 8f;

        [Header("Synced State")]
        [SyncVar(hook = nameof(OnNameChanged)), MaxLength(20)]
        public string PlayerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color32 Color;

        [SyncVar(hook = nameof(OnScoreChanged))]
        public int Score;

        /// <summary>
        /// Fired on the client whenever name, color, score, or local-player status changes so the HUD
        /// only rebuilds the scoreboard when data actually changes instead of every frame.
        /// </summary>
        public event Action OnStatsChanged;

        private Transform _mainCameraTransform;
        private MaterialPropertyBlock _propertyBlock;

        private void OnValidate()
        {
            if (CharacterController == null)
                CharacterController = GetComponent<CharacterController>();

            if (ServerCollider == null)
                ServerCollider = GetComponent<CapsuleCollider>();
        }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStartClient.AddListener(OnStartClient);
            Identity.OnStartLocalPlayer.AddListener(OnStartLocalPlayer);
        }

        private void OnStartServer()
        {
            // Enable the capsule collider only on the server so Coin.OnTriggerEnter only fires server-side
            ServerCollider.enabled = true;
        }

        private void OnStartClient()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
                _mainCameraTransform = mainCamera.transform;

            ApplyNameVisuals();
            ApplyColorVisuals();
            OnStatsChanged?.Invoke();
        }

        private void OnStartLocalPlayer()
        {
            // Only the local player's CharacterController is enabled so remote players are driven purely by NetworkTransform
            CharacterController.enabled = true;

            // Tint the local player's floating label green so it is easy to spot in the arena
            NameLabel.color = UnityEngine.Color.green;
            OnStatsChanged?.Invoke();
        }

        private void Update()
        {
            if (!IsLocalPlayer || !CharacterController.enabled)
                return;

            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            // Clamp magnitude so diagonal movement is not faster than cardinal movement
            var direction = Vector3.ClampMagnitude(new Vector3(horizontal, 0f, vertical), 1f);
            CharacterController.SimpleMove(direction * MoveSpeed);

            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private void LateUpdate()
        {
            // Keep the 3D name label facing the fixed top-down camera even as the player capsule rotates
            if (_mainCameraTransform != null)
                NameLabel.transform.rotation = _mainCameraTransform.rotation;
        }

        /// <summary>
        /// Called on the server at the start of Countdown to reset score and snap the player back to a spawn point.
        /// </summary>
        [Server]
        public void ServerResetForRound(Vector3 position, Quaternion rotation)
        {
            Score = 0;
            ApplyPositionAndRotation(position, rotation);
            RpcTeleport(position, rotation);
        }

        [ClientRpc]
        private void RpcTeleport(Vector3 position, Quaternion rotation)
        {
            // Because NetworkTransform uses ClientAuthority, the server must tell the owning client
            // (and observers) to snap their transform to the spawn point
            ApplyPositionAndRotation(position, rotation);
        }

        private void ApplyPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            // CharacterController caches its internal position while enabled and would overwrite transform.position
            // on the next SimpleMove call unless temporarily disabled
            var wasEnabled = CharacterController.enabled;
            if (wasEnabled)
                CharacterController.enabled = false;

            transform.SetPositionAndRotation(position, rotation);

            if (wasEnabled)
                CharacterController.enabled = true;
        }

        /// <summary>
        /// Allows the owning player to set a custom display name, validated and rate-limited on the server.
        /// </summary>
        [ServerRpc]
        [RateLimit(Interval = 2f, Refill = 1, MaxTokens = 3, Penalty = 5)]
        public void CmdSetPlayerName([MaxLength(20)] string requestedName)
        {
            if (string.IsNullOrWhiteSpace(requestedName))
            {
                Owner.SetError(2, PlayerErrorFlags.CustomError);
                return;
            }

            var trimmed = requestedName.Trim();

            // Reject control characters so malicious clients cannot break the scoreboard layout
            for (var i = 0; i < trimmed.Length; i++)
            {
                if (char.IsControl(trimmed[i]))
                {
                    Owner.SetError(5, PlayerErrorFlags.LikelyCheater);
                    return;
                }
            }

            PlayerName = trimmed;
        }

        private void OnNameChanged(string oldName, string newName)
        {
            ApplyNameVisuals();
            OnStatsChanged?.Invoke();
        }

        private void OnColorChanged(Color32 oldColor, Color32 newColor)
        {
            ApplyColorVisuals();
            OnStatsChanged?.Invoke();
        }

        private void OnScoreChanged(int oldScore, int newScore)
        {
            OnStatsChanged?.Invoke();
        }

        private void ApplyNameVisuals()
        {
            NameLabel.text = PlayerName;
        }

        private void ApplyColorVisuals()
        {
            // Use a MaterialPropertyBlock instead of BodyRenderer.material so we do not instantiate and leak materials
            BodyRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(ColorShaderId, Color);
            BodyRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
