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
        private T _message;
        private NetworkClient _client;
        private MessageHandler _messageHandler;
        private MessageDelegateWithPlayer<T> callback;

        public MessageWaiter(NetworkClient client, bool allowUnauthenticated = false)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _messageHandler = _client.MessageHandler;
            _messageHandler.RegisterHandler<T>(HandleMessage, allowUnauthenticated);
        }

        private void HandleMessage(INetworkPlayer player, T message)
        {
            _message = message;
            _received = true;

            _messageHandler.UnregisterHandler<T>();
            callback?.Invoke(player, message);
        }

        public async UniTask<(bool disconnected, T message)> WaitAsync()
        {
            await UniTask.WaitUntil(() => _received || !_client.IsConnected);

            // check _client.IsConnected again here, incase we disconnected after _receiving 
            return (!_client.IsConnected, _message);
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
