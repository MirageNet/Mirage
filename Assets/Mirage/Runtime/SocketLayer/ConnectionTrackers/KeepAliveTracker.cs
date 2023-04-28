using System;

namespace Mirage.SocketLayer.ConnectionTrackers
{
    internal class KeepAliveTracker
    {
        private float _lastSendTime = float.MinValue;
        private readonly Config _config;
        private readonly Time _time;

        public KeepAliveTracker(Config config, Time time)
        {
            _config = config;
            _time = time ?? throw new ArgumentNullException(nameof(time));
        }


        public bool TimeToSend()
        {
            return _lastSendTime + _config.KeepAliveInterval < _time.Now;
        }

        public void SetSendTime()
        {
            _lastSendTime = _time.Now;
        }
    }

}
