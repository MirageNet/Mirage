using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Mirage.KCP
{
    public abstract class KcpConnection : IConnection
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(KcpConnection));

        const int MinimumKcpTickInterval = 10;

        private readonly Socket socket;
        private readonly EndPoint remoteEndpoint;
        private readonly Kcp kcp;
        private readonly Unreliable unreliable;

        public event MessageReceivedDelegate MessageReceived;

        private bool open;

        public int CHANNEL_SIZE = 4;
        public event Action Disconnected;
        internal event Action<int> DataSent;

        // If we don't receive anything these many milliseconds
        // then consider us disconnected
        public int Timeout { get; set; } = 15000;

        private static readonly Stopwatch stopWatch = new Stopwatch();

        static KcpConnection()
        {
            stopWatch.Start();
        }

        private long lastReceived;

        /// <summary>
        /// Space for CRC64
        /// </summary>
        public const int RESERVED = sizeof(ulong);

        internal static readonly ArraySegment<byte> Hello = new ArraySegment<byte>(new byte[] { 0 });
        private static readonly ArraySegment<byte> Goodby = new ArraySegment<byte>(new byte[] { 1 });

        protected KcpConnection(Socket socket, EndPoint remoteEndpoint, KcpDelayMode delayMode, int sendWindowSize, int receiveWindowSize)
        {
            this.socket = socket;
            this.remoteEndpoint = remoteEndpoint;

            unreliable = new Unreliable(SendPacket)
            {
                Reserved = RESERVED
            };

            kcp = new Kcp(0, SendPacket)
            {
                Reserved = RESERVED
            };

            kcp.SetNoDelay(delayMode);
            kcp.SetWindowSize((uint)sendWindowSize, (uint)receiveWindowSize);
            open = true;

            Tick().Forget();
        }

        /// <summary>
        /// Ticks the KCP object.  This is needed for retransmits and congestion control flow messages
        /// Note no events are raised here
        /// </summary>
        async UniTaskVoid Tick()
        {
            try
            {
                lastReceived = stopWatch.ElapsedMilliseconds;

                while (open)
                {
                    long now = stopWatch.ElapsedMilliseconds;
                    if (now > lastReceived + Timeout)
                        break;

                    kcp.Update((uint)now);

                    uint check = kcp.Check((uint)now);

                    int delay = (int)(check - now);

                    if (delay <= 0)
                        delay = MinimumKcpTickInterval;

                    await UniTask.Delay(delay);
                }
            }
            catch (SocketException)
            {
                // this is ok, the connection was closed
            }
            catch (ObjectDisposedException)
            {
                // fine,  socket was closed,  no more ticking needed
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
            finally
            {
                open = false;
                Disconnected?.Invoke();
            }
        }

        readonly MemoryStream receiveBuffer = new MemoryStream(1200);

        private void DispatchKcpMessages()
        {
            int msgSize = kcp.PeekSize();

            while (msgSize >=0)
            {
                receiveBuffer.SetLength(msgSize);

                kcp.Receive(receiveBuffer.GetBuffer());

                // if we receive a disconnect message,  then close everything

                var dataSegment = new ArraySegment<byte>(receiveBuffer.GetBuffer(), 0, msgSize);
                if (Utils.Equal(dataSegment, Goodby))
                {
                    Debug.Log("Received goodby");
                    open = false;
                    break;
                }

                MessageReceived?.Invoke(dataSegment, Channel.Reliable);
                msgSize = kcp.PeekSize();
            }
        }

        internal void HandlePacket(byte[] buffer, int msgLength)
        {
            // check packet integrity
            if (!Validate(buffer, msgLength))
                return;

            if (!open)
                return;

            int channel = GetChannel(buffer);
            if (channel == Channel.Reliable)
                HandleReliablePacket(buffer, msgLength);
            else if (channel == Channel.Unreliable)
                HandleUnreliablePacket(buffer, msgLength);
        }

        private void HandleUnreliablePacket(byte[] buffer, int msgLength)
        {
            var data = new ArraySegment<byte>(buffer, RESERVED + Unreliable.OVERHEAD, msgLength - RESERVED - Unreliable.OVERHEAD);

            MessageReceived?.Invoke(data, Channel.Unreliable);
        }

        private void HandleReliablePacket(byte[] buffer, int msgLength)
        {
            kcp.Input(buffer, msgLength);
            DispatchKcpMessages();

            lastReceived = stopWatch.ElapsedMilliseconds;
        }

        private bool Validate(byte[] buffer, int msgLength)
        {
            // Recalculate CRC64 and check against checksum in the head
            var decoder = new Decoder(buffer, 0);
            ulong receivedCrc = decoder.Decode64U();
            ulong calculatedCrc = Crc64.Compute(buffer, decoder.Position, msgLength - decoder.Position);
            return receivedCrc == calculatedCrc;
        }

        private void SendBuffer(byte[] data, int length)
        {
            DataSent?.Invoke(length);
            socket.SendTo(data, 0, length, SocketFlags.None, remoteEndpoint);
        }

        private void SendPacket(byte [] data, int length)
        {
            // add a CRC64 checksum in the reserved space
            ulong crc = Crc64.Compute(data, RESERVED, length - RESERVED);
            var encoder = new Encoder(data, 0);
            encoder.Encode64U(crc);
            SendBuffer(data, length);

            if (kcp.WaitSnd > 1000 && logger.WarnEnabled())
            {
                logger.LogWarning("Too many packets waiting in the send queue " + kcp.WaitSnd + ", you are sending too much data,  the transport can't keep up");
            }
        }

        public void Send(ArraySegment<byte> data, int channel = Channel.Reliable)
        {
            if (channel == Channel.Reliable)
                kcp.Send(data.Array, data.Offset, data.Count);
            else if (channel == Channel.Unreliable)
                unreliable.Send(data.Array, data.Offset, data.Count);
        }

        /// <summary>
        ///     Disconnect this connection
        /// </summary>
        public virtual void Disconnect()
        {
            // send a disconnect message and disconnect
            if (open && socket != null)
            {
                try
                {
                    Send(Goodby);
                    kcp.Flush();
                }
                catch (SocketException ex)
                {
                    // this is ok,  the connection was already closed
                }
                catch (ObjectDisposedException ex)
                {
                    // this is normal when we stop the server
                    // the socket is stopped so we can't send anything anymore
                    // to the clients

                    // the clients will eventually timeout and realize they
                    // were disconnected
                }
            }
            open = false;
        }

        /// <summary>
        ///     the address of endpoint we are connected to
        ///     Note this can be IPEndPoint or a custom implementation
        ///     of EndPoint, which depends on the transport
        /// </summary>
        /// <returns></returns>
        public EndPoint GetEndPointAddress()
        {
            return remoteEndpoint;
        }

        public static int GetChannel(byte[] data)
        {
            var decoder = new Decoder(data, RESERVED);
            return (int)decoder.Decode32U();
        }

        protected UniTask WaitForHello()
        {
            var completionSource = AutoResetUniTaskCompletionSource.Create();

            void ReceiveHello(ArraySegment<byte> helloData, int channel)
            {
                completionSource.TrySetResult();
                MessageReceived -= ReceiveHello;
            }
            MessageReceived += ReceiveHello;

            return completionSource.Task;
        }
    }
}
