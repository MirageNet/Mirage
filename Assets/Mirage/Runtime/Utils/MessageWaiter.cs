using System;
using Cysharp.Threading.Tasks;

namespace Mirage
{
    /// <summary>
    /// Waits for 1 message
    /// </summary>
    public class MessageWaiter<T>
    {
        private bool _received;
        private INetworkPlayer _sender;
        private T _message;
        private MessageHandler _messageHandler;

        public MessageWaiter(MessageHandler messageHandler, bool allowUnauthenticated = false)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _messageHandler.RegisterHandler<T>(HandleMessage, allowUnauthenticated);
        }

        private void HandleMessage(INetworkPlayer player, T message)
        {
            _sender = player;
            _message = message;
            _received = true;

            _messageHandler.UnregisterHandler<T>();
        }

        public async UniTask<(INetworkPlayer sender, T message)> WaitAsync()
        {
            await UniTask.WaitUntil(() => _received);
            return (_sender, _message);
        }
    }
}
