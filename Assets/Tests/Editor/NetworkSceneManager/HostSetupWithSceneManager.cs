using Mirage.Tests.EnterRuntime;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetupWithSceneManager<T> : HostSetup_EditorModeTest<T> where T : NetworkBehaviour
    {
        protected NetworkSceneManager sceneManager;

        public override void ExtraSetup()
        {
            sceneManager = networkManagerGo.AddComponent<NetworkSceneManager>();
            sceneManager.Client = client;
            sceneManager.Server = server;

            sceneManager.ServerObjectManager = serverObjectManager;
            clientObjectManager.NetworkSceneManager = sceneManager;
        }
    }
}
