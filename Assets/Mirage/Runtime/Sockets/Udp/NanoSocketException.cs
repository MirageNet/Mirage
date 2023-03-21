using System;

namespace Mirage.Sockets.Udp
{
    // todo Create an Exception in mirage that can be re-used by multiple sockets (makes it easier for user to catch)
    public class NanoSocketException : Exception
    {
        public NanoSocketException(string message) : base(message) { }
    }
}
