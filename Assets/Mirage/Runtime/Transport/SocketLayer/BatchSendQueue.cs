using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirage.SocketLayer
{
    // todo make this class more robust
    public class BatchSendQueue
    {
        readonly Queue<byte[]> sendBuffer = new Queue<byte[]>();

        /// <summary>
        /// Adds message to be sent
        /// </summary>
        /// <param name="segment"></param>
        public void EnqueueMessage(ArraySegment<byte> segment)
        {
            // todo what id segment that is too big is added?
            // todo use pool to reduce allocations
            // copy segment to new array so that segment can be re-used
            sendBuffer.Enqueue(segment.ToArray());
        }

        /// <summary>
        /// Creates packet from queued message up to maxSize
        /// </summary>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public byte[] CreatePacket(int maxSize)
        {
            // todo use pool to reduce allocations
            byte[] packet = new byte[maxSize];
            int position = 0;
            int space = maxSize;

            // keep track of how many are checked
            int tooBig = 0;
            while (space > 0 && sendBuffer.Count > tooBig)
            {
                int nextLength = sendBuffer.Peek().Length;
                if (nextLength < space)
                {
                    byte[] next = sendBuffer.Dequeue();
                    Buffer.BlockCopy(next, 0, packet, position, nextLength);

                    position += nextLength;
                    space -= nextLength;
                }
                else
                {
                    tooBig++;
                }
            }

            return packet;
        }
    }
}
