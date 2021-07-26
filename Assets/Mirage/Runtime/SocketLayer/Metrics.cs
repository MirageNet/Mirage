namespace Mirage.SocketLayer
{
    public class Metrics
    {
        public readonly Sequencer Sequencer;
        public readonly Frame[] buffer;
        public uint tick;
        public Frame Current => buffer[tick];

        public Metrics(int bitSize = 10)
        {
            buffer = new Frame[1 << bitSize];
            Sequencer = new Sequencer(bitSize);
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new Frame();
            }
        }

        public void OnTick(int connectionCount)
        {
            tick = (uint)Sequencer.NextAfter(tick);
            buffer[tick].Init();
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

        // todo add metrics for batching
        //public void OnReceiveMessage(int length)
        //{
        //    buffer[tick].receiveMessagesCount++;
        //    buffer[tick].receiveMessagesBytes += length;
        //}

        public class Frame
        {
            public bool init;
            public int connectionCount;

            public int sendCount;
            public int sendBytes;

            public int sendUnconnectedCount;
            public int sendUnconnectedBytes;

            public int resendCount;
            public int resendBytes;

            public int receiveCount;
            public int receiveBytes;

            public int receiveUnconnectedBytes;
            public int receiveUnconnectedCount;

            //public int receiveMessagesCount;
            //public int receiveMessagesBytes;

            /// <summary>
            /// Clears frame ready to be used
            /// </summary>
            internal void Init()
            {
                init = true;

                sendCount = 0;
                sendBytes = 0;

                sendUnconnectedCount = 0;
                sendUnconnectedBytes = 0;

                resendCount = 0;
                resendBytes = 0;

                receiveCount = 0;
                receiveBytes = 0;

                receiveUnconnectedBytes = 0;
                receiveUnconnectedCount = 0;

                //receiveMessagesCount = 0;
                //receiveMessagesBytes = 0;
            }
        }
    }
}
