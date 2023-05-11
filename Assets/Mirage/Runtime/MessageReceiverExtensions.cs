namespace Mirage
{
    public static class MessageReceiverExtensions
    {
        /// <summary>
        /// Registers a handler for a network message that has INetworkPlayer and <typeparamref name="T"/> Message parameters
        /// <para>
        /// When network message are sent, the first 2 bytes are the Id for the type <typeparamref name="T"/>.
        /// When message is received the <paramref name="handler"/> with the matching Id is found and invoked
        /// </para>
        /// </summary>
        public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateWithPlayer<T> handler)
        {
            receiver.RegisterHandler(handler, allowUnauthenticated: false);
        }

        /// <summary>
        /// Registers a handler for a network message that has just <typeparamref name="T"/> Message parameter
        /// <para>
        /// When network message are sent, the first 2 bytes are the Id for the type <typeparamref name="T"/>.
        /// When message is received the <paramref name="handler"/> with the matching Id is found and invoked
        /// </para>
        /// </summary>
        public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegate<T> handler, bool allowUnauthenticated = false)
        {
            receiver.RegisterHandler<T>((_, value) => handler.Invoke(value), allowUnauthenticated);
        }

        /// <summary>
        /// Registers a handler for a network message that has INetworkPlayer and T Message parameters and returns UniTaskVoid.
        /// <para>
        /// This allows for async handles without allocations
        /// </para>
        /// <para>
        /// When network message are sent, the first 2 bytes are the Id for the type <typeparamref name="T"/>.
        /// When message is received the <paramref name="handler"/> with the matching Id is found and invoked
        /// </para>
        /// </summary>
        public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateWithPlayerAsync<T> handler, bool allowUnauthenticated = false)
        {
            receiver.RegisterHandler<T>((player, value) => handler.Invoke(player, value).Forget(), allowUnauthenticated);
        }

        /// <summary>
        /// Registers a handler for a network message that has just <typeparamref name="T"/> Message parameter and returns UniTaskVoid.
        /// <para>
        /// This allows for async handles without allocations
        /// </para>
        /// <para>
        /// When network message are sent, the first 2 bytes are the Id for the type <typeparamref name="T"/>.
        /// When message is received the <paramref name="handler"/> with the matching Id is found and invoked
        /// </para>
        /// </summary>
        public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateAsync<T> handler, bool allowUnauthenticated = false)
        {
            receiver.RegisterHandler<T>((_, value) => handler.Invoke(value).Forget(), allowUnauthenticated);
        }
    }
}
