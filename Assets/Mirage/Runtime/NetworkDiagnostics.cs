using System;
using Mirage.Serialization;

namespace Mirage
{
    /// <summary>
    /// Provides profiling information from mirror
    /// A profiler can subscribe to these events and
    /// present the data in a friendly way to the user
    /// </summary>
    // todo find a way to combine this with new peer metrics for more data
    public static class NetworkDiagnostics
    {
        /// <summary>
        /// Describes an outgoing message
        /// </summary>
        public readonly struct MessageInfo
        {
            /// <summary>
            /// The message being sent
            /// </summary>
            public readonly object message;
            /// <summary>
            /// how big was the message (does not include transport headers)
            /// </summary>
            public readonly int bytes;
            /// <summary>
            /// How many connections was the message sent to
            /// If an object has a lot of observers this count could be high
            /// </summary>
            public readonly int count;

            internal MessageInfo(object message, int bytes, int count)
            {
                this.message = message;
                this.bytes = bytes;
                this.count = count;
            }
        }

        #region Out messages
        /// <summary>
        /// Event that gets raised when Mirage sends a message
        /// Subscribe to this if you want to diagnose the network
        /// </summary>
        public static event Action<MessageInfo> OutMessageEvent;

        internal static void OnSend<T>(T message, int bytes, int count)
        {
            if (count > 0 && OutMessageEvent != null)
            {
                var outMessage = new MessageInfo(message, bytes, count);
                OutMessageEvent.Invoke(outMessage);
            }
        }
        #endregion

        #region In messages

        /// <summary>
        /// Event that gets raised when Mirage receives a message
        /// Subscribe to this if you want to profile the network
        /// </summary>
        public static event Action<MessageInfo> InMessageEvent;

        internal static void OnReceive<T>(T message, int bytes)
        {
            if (InMessageEvent != null)
            {
                var inMessage = new MessageInfo(message, bytes, 1);
                InMessageEvent.Invoke(inMessage);
            }
        }

        #endregion

        /// <summary>
        /// Calls <see cref="Reader{T}.Read"/> and measures number of bytes read
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static T ReadWithDiagnostics<T>(NetworkReader reader)
        {
            var message = default(T);

            // record start position for NetworkDiagnostics because reader might contain multiple messages if using batching
            int startPos = reader.BitPosition;
            try
            {
                message = reader.Read<T>();
            }
            finally
            {
                int endPos = reader.BitPosition;
                int byteLength = (endPos - startPos) / 8;
                NetworkDiagnostics.OnReceive(message, byteLength);
            }

            return message;
        }
    }
}
