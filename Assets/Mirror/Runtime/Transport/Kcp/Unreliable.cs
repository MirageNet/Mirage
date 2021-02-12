using System;
using System.Collections.Generic;

namespace Mirror.KCP
{
    /// <summary>
    /// Manages unreliable channel
    /// </summary>
    public class Unreliable
    {
        private readonly Action<byte[], int> output;
        public const int OVERHEAD = 4; //related to MTU

        public int Reserved { get; set; }

        // Start is called before the first frame update
        public Unreliable(Action<byte[], int> output_)
        {
            output = output_;
        }

        public void Send(byte[] buffer, int offset, int length)
        {
            var segment = Segment.Lease();

            System.IO.MemoryStream sendBuffer = segment.data;

            sendBuffer.SetLength(length + Reserved + OVERHEAD);

            var encoder = new Encoder(sendBuffer.GetBuffer(), Reserved);
            encoder.Encode32U(Channel.Unreliable);

            sendBuffer.Position = encoder.Position;

            sendBuffer.Write(buffer, offset, length);
            output(sendBuffer.GetBuffer(), length + Reserved + OVERHEAD);

            Segment.Release(segment);
        }
    }
}