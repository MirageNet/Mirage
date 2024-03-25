using Mirage.Events;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Callbacks for <see cref="NetworkIdentity"/>
    /// </summary>
    public class NetworkInspectorCallbacks : NetworkBehaviour
    {
        [SerializeField] private AddLateEventUnity _onStartServer = new AddLateEventUnity();
        [SerializeField] private AddLateEventUnity _onStartClient = new AddLateEventUnity();
        [SerializeField] private AddLateEventUnity _onStartLocalPlayer = new AddLateEventUnity();
        [SerializeField] private BoolAddLateEvent _onAuthorityChanged = new BoolAddLateEvent();
        [SerializeField] private NetworkPlayerAddLateEvent _onOwnerChanged = new NetworkPlayerAddLateEvent();
        [SerializeField] private AddLateEventUnity _onStopClient = new AddLateEventUnity();
        [SerializeField] private AddLateEventUnity _onStopServer = new AddLateEventUnity();

        private void Awake()
        {
            Identity.OnStartServer.AddListener(_onStartServer.Invoke);
            Identity.OnStartClient.AddListener(_onStartClient.Invoke);
            Identity.OnStartLocalPlayer.AddListener(_onStartLocalPlayer.Invoke);
            Identity.OnAuthorityChanged.AddListener(_onAuthorityChanged.Invoke);
            Identity.OnOwnerChanged.AddListener(_onOwnerChanged.Invoke);
            Identity.OnStopClient.AddListener(_onStopClient.Invoke);
            Identity.OnStopServer.AddListener(_onStopServer.Invoke);
        }
    }
}
