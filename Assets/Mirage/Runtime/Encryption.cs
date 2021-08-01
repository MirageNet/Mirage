using System;
using Mirage.Serialization;

namespace Mirage
{
    public abstract class Encryption : IEncryption
    {
        private readonly MessageHandler _messageHandler;

        protected Encryption(MessageHandler messageHandler)
        {
            _messageHandler = messageHandler;
            messageHandler.RegisterHandler<EncryptedMessage>(HandleEncryptedMessage);
        }

        private void HandleEncryptedMessage(INetworkPlayer player, EncryptedMessage message)
        {
            ArraySegment<byte> decryptedBytes = DecryptMessage(player, message.Payload);
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

        /// <summary>
        ///     Encrypt a message before we send it out through mirage.
        /// </summary>
        /// <param name="payload">The data we want to encrypt.</param>
        /// <param name="player">The player we want to send encrypted data to.</param>
        /// <returns>Will return a new data that has been encrypted.</returns>
        protected abstract ArraySegment<byte> EncryptMessage(INetworkPlayer player, ArraySegment<byte> payload);

        /// <summary>
        ///     Decrypt an incoming message sent out by mirage.
        /// </summary>
        /// <param name="payload">The data we want to encrypt.</param>
        /// <param name="player">The player we want to send decrypted data to.</param>
        /// <returns>Will return data that has been decrypted.</returns>
        protected abstract ArraySegment<byte> DecryptMessage(INetworkPlayer player, ArraySegment<byte> payload);
    }

    [NetworkMessage]
    internal struct EncryptedMessage
    {
        public ArraySegment<byte> Payload;
    }
}
