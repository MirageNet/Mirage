namespace Mirage.Components
{
    public class NetworkSceneChecker : BaseVisibilityInspector
    {
        protected override void Start()
        {
            NetworkVisibility = new SceneVisibilityChecker(ServerObjectManager);

            base.Start();
        }
    }
}
