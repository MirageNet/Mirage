using System;

namespace Mirage.SocketLayer.ConnectionTrackers
{
    internal class TimeoutTracker
    {
        private float _lastRecvTime = float.MinValue;
        private readonly Config _config;
        private readonly Time _time;

        public TimeoutTracker(Config config, Time time)
        {
            _config = config;
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }

        public bool TimeToDisconnect()
        {
            return _lastRecvTime + _config.TimeoutDuration < _time.Now;
        }

        public void SetReceiveTime()
        {
            _lastRecvTime = _time.Now;
        }
    }

}
