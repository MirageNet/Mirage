namespace Mirage.SocketLayer
{
    public class Metrics
    {
        public readonly Sequencer Sequencer;
        public readonly Frame[] buffer;
        public uint tick;

        public Metrics(int bitSize = 10)
        {
            buffer = new Frame[1 << bitSize];
            Sequencer = new Sequencer(bitSize);
        }

        public void OnTick(int connectionCount)
        {
            tick = (uint)Sequencer.NextAfter(tick);
            buffer[tick] = Frame.CreateNew();
            buffer[tick].connectionCount = connectionCount;
        }

        public void OnSend(int length)
        {
            buffer[tick].sendCount++;
            buffer[tick].sendBytes += length;
        }

        internal void OnSendUnconnected(int length)
        {
            buffer[tick].sendUnconnectedCount++;
            buffer[tick].sendUnconnectedBytes += length;
        }

        public void OnResend(int length)
        {
            buffer[tick].resendCount++;
            buffer[tick].resendBytes += length;
        }

        public void OnReceive(int length)
        {
            buffer[tick].receiveCount++;
            buffer[tick].receiveBytes += length;
        }

        public void OnReceiveUnconnected(int length)
        {
            buffer[tick].receiveUnconnectedCount++;
            buffer[tick].receiveUnconnectedBytes += length;
        }

        public void OnSendMessageUnreliable(int length)
        {
            buffer[tick].sendMessagesUnreliableCount++;
            buffer[tick].sendMessagesUnreliableBytes += length;
        }

        public void OnReceiveMessageUnreliable(int length)
        {
            buffer[tick].receiveMessagesUnreliableCount++;
            buffer[tick].receiveMessagesUnreliableBytes += length;
        }

        public void OnSendMessageReliable(int length)
        {
            buffer[tick].sendMessagesReliableCount++;
            buffer[tick].sendMessagesReliableBytes += length;
        }

        public void OnReceiveMessageReliable(int length)
        {
            buffer[tick].receiveMessagesReliableCount++;
            buffer[tick].receiveMessagesReliableBytes += length;
        }

        public void OnSendMessageNotify(int length)
        {
            buffer[tick].sendMessagesNotifyCount++;
            buffer[tick].sendMessagesNotifyBytes += length;
        }

        public void OnReceiveMessageNotify(int length)
        {
            buffer[tick].receiveMessagesNotifyCount++;
            buffer[tick].receiveMessagesNotifyBytes += length;
        }

        public void OnReceiveMessage(PacketType packetType, int length)
        {
            switch (packetType)
            {
                case PacketType.Reliable:
                    OnReceiveMessageReliable(length);
                    break;
                case PacketType.Unreliable:
                    OnReceiveMessageUnreliable(length);
                    break;
                case PacketType.Notify:
                    OnReceiveMessageNotify(length);
                    break;
            }
        }

        public struct Frame
        {
            /// <summary>
            /// Clears frame ready to be used
            /// <para>Default will have init has false so can be used to exclude frames that are not used yet</para>
            /// <para>Use this function to create a new frame with init set to true</para>
            /// </summary>
            internal static Frame CreateNew()
            {
                return new Frame { init = true };
            }

            /// <summary>Is this frame initialized (uninitialized frames can be excluded from averages)</summary>
            public bool init;

            /// <summary>Number of connections</summary>
            public int connectionCount;

            /// <summary>Number of send calls to connections</summary>
            public int sendCount;
            /// <summary>Number of bytes sent to connections</summary>
            public int sendBytes;

            /// <summary>Number of resend calls by reliable system</summary>
            public int resendCount;
            /// <summary>Number of bytes resent by reliable system</summary>
            public int resendBytes;

            /// <summary>Number of packets received from connections</summary>
            public int receiveCount;
            /// <summary>Number of bytes received from connections</summary>
            public int receiveBytes;

            #region Unconnected
            /// <summary>Number of send calls to unconnected addresses</summary>
            public int sendUnconnectedCount;
            /// <summary>Number of bytes sent to unconnected addresses</summary>
            public int sendUnconnectedBytes;

            /// <summary>Number of packets received from unconnected addresses</summary>
            public int receiveUnconnectedBytes;
            /// <summary>Number of bytes received from unconnected addresses</summary>
            public int receiveUnconnectedCount;
            #endregion

            #region Messages
            /// <summary>Number of Unreliable message sent to connections</summary>
            public int sendMessagesUnreliableCount;
            /// <summary>Number of Unreliable bytes sent to connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int sendMessagesUnreliableBytes;

            /// <summary>Number of Unreliable message received from connections</summary>
            public int receiveMessagesUnreliableCount;
            /// <summary>Number of Unreliable bytes received from connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int receiveMessagesUnreliableBytes;

            /// <summary>Number of Reliable message sent to connections</summary>
            public int sendMessagesReliableCount;
            /// <summary>Number of Reliable bytes sent to connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int sendMessagesReliableBytes;

            /// <summary>Number of Reliable message received from connections</summary>
            public int receiveMessagesReliableCount;
            /// <summary>Number of Reliable bytes received from connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int receiveMessagesReliableBytes;

            /// <summary>Number of Notify message sent to connections</summary>
            public int sendMessagesNotifyCount;
            /// <summary>Number of Notify bytes sent to connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int sendMessagesNotifyBytes;

            /// <summary>Number of Notify message received from connections</summary>
            public int receiveMessagesNotifyCount;
            /// <summary>Number of Notify bytes received from connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int receiveMessagesNotifyBytes;

            /// <summary>Number of message sent to connections</summary>
            public int sendMessagesCountTotal => sendMessagesUnreliableCount + sendMessagesReliableCount + sendMessagesNotifyCount;
            /// <summary>Number of bytes sent to connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int sendMessagesBytesTotal => sendMessagesUnreliableBytes + sendMessagesReliableBytes + sendMessagesNotifyBytes;

            /// <summary>Number of message received from connections</summary>
            public int receiveMessagesCountTotal => receiveMessagesUnreliableCount + receiveMessagesReliableCount + receiveMessagesNotifyCount;
            /// <summary>Number of bytes received from connections (excludes packets headers, will just be the message sent by high level)</summary>
            public int receiveMessagesBytesTotal => receiveMessagesUnreliableBytes + receiveMessagesReliableBytes + receiveMessagesNotifyBytes;
            #endregion
        }
    }
}
