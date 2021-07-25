using System;
using Mirage.Serialization;

namespace Mirage
{
    public class Encryption : IEncryption
    {
        private readonly MessageHandler _messageHandler;

        public Encryption(MessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
            messageHandler.RegisterHandler<EncryptedMessage>(HandleEncryptedMessage);
        }

        private void HandleEncryptedMessage(INetworkPlayer player, EncryptedMessage message)
        {
            ArraySegment<byte> decryptedBytes = DecryptMessage(message.Payload);
            _messageHandler.HandleMessage(player, decryptedBytes);
        }

        public void Send<T>(INetworkPlayer player, T msg) where T : struct
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(msg, writer);

                var packed = writer.ToArraySegment();

                ArraySegment<byte> encryptedBytes = EncryptMessage(packed);

                player.Send(new EncryptedMessage
                {
                    Payload = encryptedBytes
                });
            }
        }

        private ArraySegment<byte> EncryptMessage(ArraySegment<byte> payload)
        {
            throw new NotImplementedException();
        }

        private ArraySegment<byte> DecryptMessage(ArraySegment<byte> payload)
        {
            throw new NotImplementedException();
        }
    }

    [NetworkMessage]
    internal struct EncryptedMessage
    {
        public ArraySegment<byte> Payload;
    }
}
