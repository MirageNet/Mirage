using Mirage.Events;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Callbacks for <see cref="NetworkIdentity"/>
    /// </summary>
    public class NetworkInspectorCallbacks : NetworkBehaviour
    {
        [SerializeField] private AddLateEvent _onStartServer = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStartClient = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStartLocalPlayer = new AddLateEvent();
        [SerializeField] private BoolAddLateEvent _onAuthorityChanged = new BoolAddLateEvent();
        [SerializeField] private NetworkPlayerAddLateEvent _onOwnerChanged = new NetworkPlayerAddLateEvent();
        [SerializeField] private AddLateEvent _onStopClient = new AddLateEvent();
        [SerializeField] private AddLateEvent _onStopServer = new AddLateEvent();

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


#if UNITY_EDITOR
        public void Convert()
        {
            _onStartServer = GetAndClear<AddLateEvent>(nameof(_onStartServer));
            _onStartClient = GetAndClear<AddLateEvent>(nameof(_onStartClient));
            _onStartLocalPlayer = GetAndClear<AddLateEvent>(nameof(_onStartLocalPlayer));
            _onAuthorityChanged = GetAndClear<BoolAddLateEvent>(nameof(_onAuthorityChanged));
            _onOwnerChanged = GetAndClear<NetworkPlayerAddLateEvent>(nameof(_onOwnerChanged));
            _onStopClient = GetAndClear<AddLateEvent>(nameof(_onStopClient));
            _onStopServer = GetAndClear<AddLateEvent>(nameof(_onStopServer));

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(Identity);

            if (Identity.gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(Identity.gameObject.scene);
            }
        }

        private T GetAndClear<T>(string field) where T : new()
        {
            var type = typeof(NetworkIdentity);
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var info = type.GetField(field, flags);
            var value = (T)info.GetValue(Identity);
            info.SetValue(Identity, new T());
            return value;
        }
#endif
    }
}

#if UNITY_EDITOR
namespace Mirage.EditorScripts
{
    [UnityEditor.CustomEditor(typeof(NetworkInspectorCallbacks))]
    public class NetworkInspectorCallbacksEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var target = (NetworkInspectorCallbacks)base.target;
            if (GUILayout.Button("Copy Identity events"))
            {
                target.Convert();
            }
        }
    }
}
#endif
