using System;
using UnityEngine;

namespace Mirage.SocketLayer
{
    public abstract class Batch
    {
        public const int MESSAGE_LENGTH_SIZE = 2;
        public const int MAX_BATCH_SIZE = ushort.MaxValue;

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

        protected virtual void AddToBatch(byte[] message, int offset, int length)
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
        private readonly IRawConnection _connection;
        private readonly PacketType _packetType;
        private readonly SendMode _sendMode;
        private readonly byte[] _batch;
        protected readonly ILogger _logger;
        private int _batchLength;

        public ArrayBatch(int maxPacketSize, ILogger logger, IRawConnection connection, PacketType reliable, SendMode sendMode)
            : base(maxPacketSize)
        {
            _logger = logger;
            _batch = new byte[maxPacketSize];
            _connection = connection;
            _packetType = reliable;
            _sendMode = sendMode;
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
            _connection.SendRaw(_batch, _batchLength, _sendMode);
            _batchLength = 0;
        }

        protected override void AddToBatch(byte[] message, int offset, int length)
        {
            if (length > MAX_BATCH_SIZE)
            {
                var batch = GetBatch();
                ref var batchLength = ref GetBatchLength();
                _logger.Assert(batchLength == 1, "if length is large, then batch should be new (empty) packet");

                // write zero as flag for large message,
                // normal message will have atleast 1 length
                ByteUtils.WriteUShort(batch, ref batchLength, 0);
                Buffer.BlockCopy(message, offset, batch, batchLength, length);
                batchLength += length;

                // we can send right away, nothing else will fit in this message
                SendAndReset();
            }
            else
            {
                base.AddToBatch(message, offset, length);
            }
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
