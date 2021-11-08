namespace Mirage.Components
{
    public class NetworkMatchChecker : BaseVisibilityInspector
    {
        #region Overrides of BaseVisibilityInspector

        protected override void Start()
        {
            NetworkVisibility = new NetworkMatchCheckerVisibility(ServerObjectManager);

            base.Start();
        }

        #endregion
    }
}
