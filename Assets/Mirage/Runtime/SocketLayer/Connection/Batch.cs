using System;

namespace Mirage.SocketLayer
{
    public abstract class Batch
    {
        public const int MESSAGE_LENGTH_SIZE = 2;

        private readonly int _maxPacketSize;

        public Batch(int maxPacketSize)
        {
            _maxPacketSize = maxPacketSize;
        }

        protected abstract bool Created { get; }
        protected abstract byte[] GetBatch();
        protected abstract ref int GetBatchLength();

        protected abstract void CreateNewBatch();
        protected abstract void SendAndReset();

        public void AddMessage(byte[] message, int offset, int length)
        {
            if (Created)
            {
                var msgLength = length + MESSAGE_LENGTH_SIZE;
                var batchLength = GetBatchLength();
                if (batchLength + msgLength > _maxPacketSize)
                {
                    // if full, send and create new
                    SendAndReset();
                }
            }

            if (!Created)
                CreateNewBatch();

            AddToBatch(message, offset, length);
        }

        private void AddToBatch(byte[] message, int offset, int length)
        {
            var batch = GetBatch();
            ref var batchLength = ref GetBatchLength();
            ByteUtils.WriteUShort(batch, ref batchLength, checked((ushort)length));
            Buffer.BlockCopy(message, offset, batch, batchLength, length);
            batchLength += length;
        }

        public void Flush()
        {
            if (Created)
                SendAndReset();
        }
    }

    public class ArrayBatch : Batch
    {
        private readonly Action<byte[], int> _send;
        private readonly PacketType _packetType;

        private readonly byte[] _batch;
        private int _batchLength;

        public ArrayBatch(int maxPacketSize, Action<byte[], int> send, PacketType reliable)
            : base(maxPacketSize)
        {
            _batch = new byte[maxPacketSize];
            _send = send;
            _packetType = reliable;
        }

        protected override bool Created => _batchLength > 0;

        protected override byte[] GetBatch() => _batch;
        protected override ref int GetBatchLength() => ref _batchLength;

        protected override void CreateNewBatch()
        {
            _batch[0] = (byte)_packetType;
            _batchLength = 1;
        }

        protected override void SendAndReset()
        {
            _send.Invoke(_batch, _batchLength);
            _batchLength = 0;
        }
    }

    public class ReliableBatch : Batch, IDisposable
    {
        private AckSystem.ReliablePacket _nextBatch;
        private readonly Func<PacketType, AckSystem.ReliablePacket> _createReliableBuffer;
        private readonly Action<AckSystem.ReliablePacket> _sendReliablePacket;

        public ReliableBatch(int maxPacketSize, Func<PacketType, AckSystem.ReliablePacket> createReliableBuffer, Action<AckSystem.ReliablePacket> sendReliablePacket)
           : base(maxPacketSize)
        {
            _createReliableBuffer = createReliableBuffer;
            _sendReliablePacket = sendReliablePacket;
        }

        protected override bool Created => _nextBatch != null;

        protected override byte[] GetBatch() => _nextBatch.Buffer.array;
        protected override ref int GetBatchLength() => ref _nextBatch.Length;

        protected override void CreateNewBatch()
        {
            _nextBatch = _createReliableBuffer.Invoke(PacketType.Reliable);
        }

        protected override void SendAndReset()
        {
            _sendReliablePacket.Invoke(_nextBatch);
            _nextBatch = null;
        }

        void IDisposable.Dispose()
        {
            if (_nextBatch != null)
            {
                _nextBatch.Buffer.Release();
                _nextBatch = null;
            }
        }
    }
}
