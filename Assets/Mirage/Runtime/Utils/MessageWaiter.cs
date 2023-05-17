using System;
using Cysharp.Threading.Tasks;

namespace Mirage
{
    /// <summary>
    /// Register handler just for 1 message
    /// <para>Useful on client when you want too receive a single auth message</para>
    /// </summary>
    public class MessageWaiter<T>
    {
        private bool _received;
        private INetworkPlayer _sender;
        private T _message;
        private MessageHandler _messageHandler;
        private MessageDelegateWithPlayer<T> callback;

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
            callback?.Invoke(player, message);
        }

        public async UniTask<(INetworkPlayer sender, T message)> WaitAsync()
        {
            await UniTask.WaitUntil(() => _received);
            return (_sender, _message);
        }

        /// <summary>
        /// Use callback instead of async for methods that uses ArraySegment, because internal buffer will be recylced and data will be load before Async completes
        /// </summary>
        /// <param name="callback"></param>
        public void Callback(MessageDelegateWithPlayer<T> callback)
        {
            this.callback = callback;
        }
    }
}
