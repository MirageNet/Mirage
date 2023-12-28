using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Disables all Renders on GameObject when the NetworkIdentity is not visible too the host player because of a <see cref="NetworkVisibility"/>
    /// </summary>
    [RequireComponent(typeof(NetworkVisibility))]
    public class HostRendererVisibility : NetworkBehaviour
    {
        private NetworkVisibility _visibility;
        private Renderer[] _renderers;

        private void Awake()
        {
            _visibility = GetComponent<NetworkVisibility>();
            _visibility.OnVisibilityChanged += OnVisibilityChanged;
            _renderers = GetComponentsInChildren<Renderer>();

            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            if (!IsHost)
                return;

            // set host visibility here because "OnVisibilityChanged(..., false)" is not called when spawning
            var visibility = Identity.Visibility;
            var visible = visibility.OnCheckObserver(Server.LocalPlayer);
            SetHostVisibility(visible);
        }

        private void OnDestroy()
        {
            if (_visibility != null)
                _visibility.OnVisibilityChanged -= OnVisibilityChanged;
        }

        /// <summary>
        /// Can be used to override default GetComponentsInChildren for renderers. Useful if setting up Renderer after Awake is called
        /// </summary>
        /// <param name="renderers"></param>
        public void SetRenderers(Renderer[] renderers)
        {
            _renderers = renderers;
        }

        private void OnVisibilityChanged(INetworkPlayer player, bool visible)
        {
            if (!IsHost)
                return;

            if (player == Server.LocalPlayer)
                SetHostVisibility(visible);
        }

        protected virtual void SetHostVisibility(bool visible)
        {
            for (var i = 0; i < _renderers.Length; i++)
                _renderers[i].enabled = visible;
        }
    }
}
