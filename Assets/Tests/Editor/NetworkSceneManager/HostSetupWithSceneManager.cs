using Mirage.Tests.EnterRuntime;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetupWithSceneManager<T> : HostSetup_EditorModeTest<T> where T : NetworkBehaviour
    {
        protected NetworkSceneManager sceneManager;

        protected override void ExtraServerSetup()
        {
            sceneManager = serverGo.AddComponent<NetworkSceneManager>();
            sceneManager.Client = client;
            sceneManager.Server = server;
            sceneManager.ServerObjectManager = serverObjectManager;
            clientObjectManager.NetworkSceneManager = sceneManager;
        }
    }
}
