using Mirage;

namespace Mirage.Snippets.General
{
    public class ClockSyncSnippets
    {
        public void ShowTime(NetworkTime NetworkTime)
        {
            // CodeEmbed-Start: clock-sync-time
            double now = NetworkTime.Time;
            // CodeEmbed-End: clock-sync-time
        }

        public void ShowRtt(NetworkTime NetworkTime)
        {
            // CodeEmbed-Start: clock-sync-rtt
            double rtt = NetworkTime.Rtt;
            // CodeEmbed-End: clock-sync-rtt
        }

        public void ShowTimeSd(NetworkTime NetworkTime)
        {
            // CodeEmbed-Start: clock-sync-time-sd
            double timeStandardDeviation = NetworkTime.TimeSd;
            // CodeEmbed-End: clock-sync-time-sd
        }

        public void ConfigurePing(NetworkTime NetworkTime)
        {
            // CodeEmbed-Start: clock-sync-ping-interval
            NetworkTime.PingInterval = 2.0f;
            // CodeEmbed-End: clock-sync-ping-interval
        }

        public void ConfigurePingWindow(NetworkTime NetworkTime)
        {
            // CodeEmbed-Start: clock-sync-ping-window-size
            NetworkTime.PingWindowSize = 10;
            // CodeEmbed-End: clock-sync-ping-window-size
        }
    }
}
