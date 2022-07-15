using UnityEngine;

namespace Mirage.Examples.Additive
{
    public class RandomColor : NetworkBehaviour
    {
        private void Awake()
        {
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        public void OnStartServer()
        {
            color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        // Color32 packs to 4 bytes
        [SyncVar(hook = nameof(SetColor))]
        public Color32 color = Color.black;

        // Unity clones the material when GetComponent<Renderer>().material is called
        // Cache it here and destroy it in OnDestroy to prevent a memory leak
        private Material cachedMaterial;

        private void SetColor(Color32 oldColor, Color32 newColor)
        {
            if (cachedMaterial == null) cachedMaterial = GetComponentInChildren<Renderer>().material;
            cachedMaterial.color = newColor;
        }

        private void OnDestroy()
        {
            Destroy(cachedMaterial);
        }
    }
}
