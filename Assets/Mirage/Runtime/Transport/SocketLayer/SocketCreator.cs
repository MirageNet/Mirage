using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Creates <see cref="ISocket"/>
    /// </summary>
    /// <remarks>
    /// The only job of Transport is to create a <see cref="ISocket"/> that will be used by mirage to send/recieve data.
    /// <para>This is a MonoBehaviour so can be attached in the inspector</para>
    /// </remarks>
    public abstract class SocketCreator : MonoBehaviour
    {
        public abstract ISocket CreateClientSocket();
        public abstract ISocket CreateServerSocket();

        public abstract EndPoint GetBindEndPoint();
        public abstract EndPoint GetConnectEndPoint(string address);

        public abstract bool ClientSupported { get; }
        public abstract bool ServerSupported { get; }
    }
}
