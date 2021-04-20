using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Creates an instance of <see cref="ISocket"/>
    /// </summary>
    /// <remarks>
    /// <see cref="SocketCreator"/> as 2 jobs:<br />
    /// - To create an instance of <see cref="ISocket"/> that will be used by Peer to send/Receive data.<br />
    /// - Show config data to the user using the inspector, and give that data in the form of an <see cref="EndPoint"/>
    /// <para>This is a MonoBehaviour so can be attached in the inspector</para>
    /// </remarks>
    public abstract class SocketCreator : MonoBehaviour
    {
        public abstract ISocket CreateClientSocket();
        public abstract ISocket CreateServerSocket();

        public abstract EndPoint GetBindEndPoint();
        public abstract EndPoint GetConnectEndPoint(string address = null, ushort? port = null);

        public abstract bool ClientSupported { get; }
        public abstract bool ServerSupported { get; }
    }
}
