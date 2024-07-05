using System;

namespace Mirage.SocketLayer
{
    public enum SocketReliability
    {
        // note: 0 is unset, it will be used to check if SocketInfo is default or not

        /// <summary>
        /// all packets are unreliable, eg udp
        /// </summary>
        Unreliable = 1,

        /// <summary>
        /// all packets are reliable, eg tcp or webSockets
        /// </summary>
        Reliable = 2,

        /// <summary>
        /// if socket supports both reliable and unreliable, eg steam or epic relay
        /// </summary>
        Both = 3,
    }

    public readonly struct SocketInfo
    {
        /// <summary>
        /// How the socket handles reliability
        /// </summary>
        public readonly SocketReliability Reliability;

        /// <summary>
        /// If socket supports Reliable, what is the max packet size. This should include max Fragmentation size if socket handles that
        /// </summary>
        public readonly int MaxReliableSize;

        /// <summary>
        /// If socket supports Unreliable, what is the max packet size
        /// </summary>
        public readonly int MaxUnreliableSize;

        /// <summary>
        /// Will the Socket handle Fragmentation for Reliable messages
        /// <para>if false, Mirage will fragment message before sending them to socket</para>
        /// </summary>
        public readonly bool ReliableFragmentation;

        /// <summary>
        /// Max size required by either reliable or unreliable
        /// </summary>
        public readonly int MaxSize;

        public SocketInfo(SocketReliability reliability, int maxReliableSize, int maxUnreliableSize, bool reliableFragmentation)
        {
            Reliability = reliability;
            MaxReliableSize = maxReliableSize;
            MaxUnreliableSize = maxUnreliableSize;
            ReliableFragmentation = reliableFragmentation;
            MaxSize = Math.Max(MaxReliableSize, MaxUnreliableSize);

            // this smallest size that a socket must support to work with Mirage
            // note: this number is arbitrary, but is a reasonable size, any smaller and too many packets will need to be fragmented and sent
            const int minMessageSize = 100;
            switch (reliability)
            {
                case SocketReliability.Unreliable:
                    // Mirage will handle reliability, so max size must be big enough so that header can fit
                    if (MaxUnreliableSize < AckSystem.MIN_RELIABLE_HEADER_SIZE + minMessageSize)
                        throw new ArgumentException($"Max unreliable size too small for AckSystem header", nameof(maxUnreliableSize));
                    break;
                case SocketReliability.Reliable:
                    // Mirage will just batch message and send them to socket
                    if (MaxReliableSize < Batch.MESSAGE_LENGTH_SIZE + minMessageSize)
                        throw new ArgumentException($"Max reliable size too small for Batch header", nameof(maxUnreliableSize));
                    break;

                case SocketReliability.Both:
                    if (MaxUnreliableSize < AckSystem.NOTIFY_HEADER_SIZE + minMessageSize)
                        throw new ArgumentException($"Max unreliable size too small for Notify header", nameof(maxUnreliableSize));

                    if (MaxReliableSize < Batch.MESSAGE_LENGTH_SIZE + minMessageSize)
                        throw new ArgumentException($"Max reliable size too small for AckSystem header", nameof(maxUnreliableSize));
                    break;
            }


            if (MaxReliableSize > ushort.MaxValue)
            {
                throw new ArgumentException($"Max package size can not bigger than {ushort.MaxValue}. NoReliableConnection uses 2 bytes for message length, maxPacketSize over that value will mean that message will be incorrectly batched.");
            }
            if (MaxUnreliableSize > ushort.MaxValue)
            {
                throw new ArgumentException($"Max package size can not bigger than {ushort.MaxValue}. NoReliableConnection uses 2 bytes for message length, maxPacketSize over that value will mean that message will be incorrectly batched.");
            }
        }
    }
}
