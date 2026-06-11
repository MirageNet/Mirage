using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    // CodeEmbed-Start: lifecycle-start-server
    public class LifecycleComponent : MonoBehaviour
    {
        public void Awake()
        {
            GetComponent<NetworkIdentity>().OnStartServer.AddListener(OnStartServer);
        }

        public void OnStartServer()
        {
            Debug.Log("The object started on the server");
        }
    }
    // CodeEmbed-End: lifecycle-start-server
}
