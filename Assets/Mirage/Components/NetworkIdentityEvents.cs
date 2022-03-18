using Mirage.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Mirage
{
    /// <summary>
    /// Inspector events for NetworkIdentity
    /// </summary>
    public class NetworkIdentityEvents : NetworkBehaviour
    {
        [SerializeField] UnityEvent _onStartServer = new UnityEvent();
        [SerializeField] UnityEvent _onStartClient = new UnityEvent();
        [SerializeField] UnityEvent _onStartLocalPlayer = new UnityEvent();
        [SerializeField] BoolUnityEvent _onAuthorityChanged = new BoolUnityEvent();
        [SerializeField] UnityEvent _onStopClient = new UnityEvent();
        [SerializeField] UnityEvent _onStopServer = new UnityEvent();

        private void Awake()
        {
            Identity.OnStartServer.AddListener(_onStartServer.Invoke);
            Identity.OnStartClient.AddListener(_onStartClient.Invoke);
            Identity.OnStartLocalPlayer.AddListener(_onStartLocalPlayer.Invoke);
            Identity.OnAuthorityChanged.AddListener(_onAuthorityChanged.Invoke);
            Identity.OnStopClient.AddListener(_onStopClient.Invoke);
            Identity.OnStopServer.AddListener(_onStopServer.Invoke);
        }
    }
}
