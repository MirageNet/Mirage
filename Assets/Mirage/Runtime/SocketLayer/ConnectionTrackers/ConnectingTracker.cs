namespace Mirage.SocketLayer.ConnectionTrackers
{
    internal class ConnectingTracker
    {
        private readonly Config _config;
        private readonly Time _time;
        private float _lastAttempt = float.MinValue;
        private int _attemptCount = 0;

        public ConnectingTracker(Config config, Time time)
        {
            _config = config;
            _time = time;
        }

        public bool TimeAttempt()
        {
            return _lastAttempt + _config.ConnectAttemptInterval < _time.Now;
        }

        public bool MaxAttempts()
        {
            return _attemptCount >= _config.MaxConnectAttempts;
        }

        public void OnAttempt()
        {
            _attemptCount++;
            _lastAttempt = _time.Now;
        }
    }

}
