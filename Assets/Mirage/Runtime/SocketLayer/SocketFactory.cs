using System;
using System.Net;
using UnityEngine;

namespace Mirage.SocketLayer
{
    /// <summary>
    /// Creates an instance of <see cref="ISocket"/>
    /// </summary>
    /// <remarks>
    /// <see cref="SocketFactory"/> as 2 jobs:<br />
    /// - To create an instance of <see cref="ISocket"/> that will be used by Peer to send/Receive data.<br />
    /// - Show config data to the user using the inspector, and give that data in the form of an <see cref="EndPoint"/>
    /// <para>This is a MonoBehaviour so can be attached in the inspector</para>
    /// </remarks>
    public abstract class SocketFactory : MonoBehaviour
    {
        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the server</summary>
        /// <exception cref="NotSupportedException">Throw when Server is not supported on current platform</exception>
        public abstract ISocket CreateServerSocket();

        /// <summary>Creates the <see cref="EndPoint"/> that the Server Socket will bind to</summary>
        public abstract IEndPoint GetBindEndPoint();


        /// <summary>Creates a <see cref="ISocket"/> to be used by <see cref="Peer"/> on the client</summary>
        /// <exception cref="NotSupportedException">Throw when Client is not supported on current platform</exception>
        public abstract ISocket CreateClientSocket();

        /// <summary>Creates the <see cref="EndPoint"/> that the Client Socket will connect to using the parameter given</summary>
        public abstract IEndPoint GetConnectEndPoint(string address = null, ushort? port = null);
    }
}
