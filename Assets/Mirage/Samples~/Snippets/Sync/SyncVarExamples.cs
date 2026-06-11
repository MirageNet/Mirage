using UnityEngine;
using Mirage;

namespace Mirage.Snippets.Sync.Vars.Basic
{
    // CodeEmbed-Start: SyncVarBasicExample
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public int clickCount { get; set; }

        private void Update()
        {
            if (IsLocalPlayer && Input.GetMouseButtonDown(0))
                ServerRpc_IncreaseClicks();
        }

        [ServerRpc]
        public void ServerRpc_IncreaseClicks()
        {
            // This is executed on the server
            clickCount++;
        }
    }
    // CodeEmbed-End: SyncVarBasicExample
}

namespace Mirage.Snippets.Sync.Vars.Inheritance
{
    public class ClassInheritanceExample
    {
        // CodeEmbed-Start: SyncVarInheritanceExample
        private class Pet : NetworkBehaviour
        {
            [SyncVar] 
            private string name { get; set; }
        }

        private class Cat : Pet
        {
            [SyncVar]
            private Color32 color { get; set; }
        }
        // CodeEmbed-End: SyncVarInheritanceExample
    }
}

namespace Mirage.Snippets.Sync.Vars.ClientOnlyHook
{
    // CodeEmbed-Start: SyncVarClientOnlyHookExample
    public class Player : NetworkBehaviour
    {
        [SyncVar(hook = nameof(UpdateColor))]
        private Color playerColor { get; set; } = Color.black;

        private Renderer renderer;

        // Unity makes a clone of the Material every time renderer.material is used.
        // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
        private Material cachedMaterial;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        private void UpdateColor(Color oldColor, Color newColor)
        {
            // this is executed on this player for each client
            if (cachedMaterial == null)
                cachedMaterial = renderer.material;

            cachedMaterial.color = newColor;
        }

        private void OnDestroy()
        {
            Destroy(cachedMaterial);
        }
    }
    // CodeEmbed-End: SyncVarClientOnlyHookExample
}

namespace Mirage.Snippets.Sync.Vars.ServerClientHook
{
    // CodeEmbed-Start: SyncVarServerClientHookExample
    public class Player : NetworkBehaviour
    {
        [SyncVar(hook = nameof(UpdateColor), invokeHookOnServer = true)]
        private Color playerColor { get; set; } = Color.black;

        private Renderer renderer;

        // Unity makes a clone of the Material every time renderer.material is used.
        // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
        private Material cachedMaterial;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        private void UpdateColor(Color oldColor, Color newColor)
        {
            // this is executed on this player for each client
            if (cachedMaterial == null)
                cachedMaterial = renderer.material;

            cachedMaterial.color = newColor;
        }

        private void OnDestroy()
        {
            Destroy(cachedMaterial);
        }
    }
    // CodeEmbed-End: SyncVarServerClientHookExample
}

namespace Mirage.Snippets.Sync.Vars.InitialOnly
{
    // CodeEmbed-Start: SyncVarInitialOnlyExample
    public class Player : NetworkBehaviour
    {
        [SyncVar(initialOnly = true)]
        private int weaponId { get; set; }

        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartClient()
        {
            // Update weapon using id from syncvar (sent to client via spawn message
            UpdateWeapon(weaponId);
        }

        private void Update()
        {
            // Client Request weapon change
            if (Input.GetKeyDown(KeyCode.Q))
                ServerRpc_SetSyncVarWeaponId(7);
        }

        [ServerRpc]
        private void ServerRpc_SetSyncVarWeaponId(int weaponId)
        {
            // Set weapon id on server so new players get it
            this.weaponId = weaponId;

            // Tell current players about it
            ClientRpc_SetSyncVarWeaponId(weaponId);

            // Update weapon on server
            UpdateWeapon(weaponId);
        }

        [ClientRpc]
        private void ClientRpc_SetSyncVarWeaponId(int weaponId)
        {
            // Set id on client
            this.weaponId = weaponId;

            // Update weapon on client
            UpdateWeapon(weaponId);
        }

        public void UpdateWeapon(int weaponId)
        {
            // Do stuff to update weapon here
            // For example, its spawning model
        }
    }
    // CodeEmbed-End: SyncVarInitialOnlyExample
}
