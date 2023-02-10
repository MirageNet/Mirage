namespace Mirage.Tests.Runtime
{
    public class MockVisibility : NetworkVisibility
    {
        private bool _visibile;
        public bool Visible
        {
            get
            {
                return _visibile;
            }

            set
            {
                _visibile = value;

                if (Identity.IsSpawned)
                    Identity.RebuildObservers(false);
            }
        }

        public override bool OnCheckObserver(INetworkPlayer player)
        {
            return _visibile;
        }
    }
}
