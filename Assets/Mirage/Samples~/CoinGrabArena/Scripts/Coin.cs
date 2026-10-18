using UnityEngine;

namespace Mirage.Examples.CoinGrabArena
{
    /// <summary>
    /// Collectible coin spawned by <see cref="CoinSpawner"/>.
    /// <para>
    /// Pickup detection runs exclusively on the server: both the coin's trigger collider and the player's
    /// trigger collider are only enabled on the server. Clients never decide if they picked up a coin,
    /// preventing clients from claiming coins that were already taken or out of reach.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Coin : NetworkBehaviour
    {
        [Header("Components")]
        [RequiredField]
        public Renderer CoinRenderer;
        [RequiredField]
        public MeshFilter CoinMeshFilter;
        [RequiredField]
        public SphereCollider TriggerCollider;
        [RequiredField]
        public ParticleSystem PickupEffectPrefab;

        [Header("Visuals")]
        [RequiredField]
        public Mesh NormalMesh;
        [RequiredField]
        public Mesh GoldMesh;
        [RequiredField]
        public Material NormalMaterial;
        [RequiredField]
        public Material GoldMaterial;
        public Color NormalColor = new Color(1f, 0.85f, 0.1f);
        public Color GoldColor = new Color(1f, 0.45f, 0f);
        public float NormalScale = 1f;
        public float GoldScale = 1.15f;
        public float SpinSpeed = 120f;

        [Header("Anti-Cheat")]
        [Tooltip("Maximum distance allowed on the server between player and coin to accept a pickup.")]
        public float MaxPickupDistance = 4f;

        [Header("Synced State")]
        // Value is set before Spawn and never changes, so initialOnly avoids any future dirty checks
        [SyncVar(initialOnly = true)]
        public int Value = 1;

        [SyncVar(initialOnly = true)]
        public bool IsGold;

        private CoinSpawner _spawner;
        private CoinGrabGameManager _gameManager;
        private Material[] _normalMaterials;
        private Material[] _goldMaterials;

        // Guards against two players overlapping the same coin in the same physics step before Destroy completes
        private bool _collected;

        private void OnValidate()
        {
            if (TriggerCollider == null)
                TriggerCollider = GetComponent<SphereCollider>();
        }

        private void Awake()
        {
            _normalMaterials = CreateMaterialArray(NormalMesh, NormalMaterial);
            _goldMaterials = CreateMaterialArray(GoldMesh, GoldMaterial);

            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private static Material[] CreateMaterialArray(Mesh mesh, Material material)
        {
            int count = mesh != null ? Mathf.Max(1, mesh.subMeshCount) : 1;
            var materials = new Material[count];
            for (int i = 0; i < count; i++)
                materials[i] = material;
            return materials;
        }

        /// <summary>
        /// Configures the coin on the server before <see cref="ServerObjectManager.Spawn(GameObject)"/> is called
        /// so <see cref="Value"/> and <see cref="IsGold"/> are included in the initial SpawnMessage.
        /// </summary>
        public void InitializeServer(CoinSpawner spawner, CoinGrabGameManager gameManager, int value, bool isGold)
        {
            _spawner = spawner;
            _gameManager = gameManager;
            Value = value;
            IsGold = isGold;
            _collected = false;
        }

        private void OnStartServer()
        {
            // Only the server enables the trigger collider so physics triggers never fire on clients
            TriggerCollider.enabled = true;
        }

        private void OnStartClient()
        {
            // Swap sharedMesh and sharedMaterials so Gold Coins show the 3D star mesh across all submeshes
            CoinMeshFilter.sharedMesh = IsGold ? GoldMesh : NormalMesh;
            CoinRenderer.sharedMaterials = IsGold ? _goldMaterials : _normalMaterials;
            transform.localScale = Vector3.one * (IsGold ? GoldScale : NormalScale);
        }

        private void Update()
        {
            transform.Rotate(0f, SpinSpeed * Time.deltaTime, 0f, Space.World);
        }

        [Server(error = false)]
        private void OnTriggerEnter(Collider other)
        {
            if (_collected)
                return;

            // Ignore pickups outside the active Playing state (e.g. if a round just ended this frame)
            if (_gameManager.State != RoundState.Playing)
                return;

            if (!other.TryGetComponent<CoinGrabPlayer>(out var player))
                return;

            // Server-side distance validation protects against position desync or spoofed transforms
            var sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance > MaxPickupDistance * MaxPickupDistance)
            {
                player.Owner?.SetError(5, PlayerErrorFlags.LikelyCheater);
                return;
            }

            // Flip flag immediately so a second player touching the coin in the same physics step is ignored
            _collected = true;

            player.Score += Value;
            if (IsGold)
                player.ApplySpeedBoost();

            // Notify all clients to play the pickup burst before destroying the networked coin object
            RpcPlayPickupEffect(transform.position, IsGold);

            _spawner.OnCoinCollected(this);
            _gameManager.OnPlayerScored(player);

            ServerObjectManager.Destroy(gameObject);
        }

        [ClientRpc]
        private void RpcPlayPickupEffect(Vector3 position, bool isGold)
        {
            var effect = Instantiate(PickupEffectPrefab, position, Quaternion.identity);
            var main = effect.main;
            main.startColor = isGold ? GoldColor : NormalColor;
            effect.Play();
            Destroy(effect.gameObject, main.duration + main.startLifetime.constantMax);
        }
    }
}
