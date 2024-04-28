using System;

namespace Mirage.SocketLayer.ConnectionTrackers
{
    internal class DisconnectedTracker
    {
        private bool _isDisonnected;
        private double _disconnectTime;
        private readonly Config _config;
        private readonly Time _time;

        public DisconnectedTracker(Config config, Time time)
        {
            _config = config;
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        public void OnDisconnect()
        {
            _disconnectTime = _time.Now + _config.DisconnectDuration;
            _isDisonnected = true;
        }

        public bool TimeToRemove()
        {
            return _isDisonnected && _disconnectTime < _time.Now;
        }
    }

}
