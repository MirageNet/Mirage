using UnityEngine;

namespace Mirage.Examples.MatchScenes
{
    public class MatchScenesPlayer : NetworkBehaviour
    {
        [SyncVar]
        public Color color;

        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnStartClient()
        {
            var renderer = GetComponentInChildren<Renderer>();
            renderer.material.color = color;
        }
    }
}
