using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace Mirage.Serialization
{
    /// <summary>
    /// Class that will cache the ID for type T
    /// <para>avoids needing to calculate the stable hash of the full name each time a message is sent</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class MessageIdCache<T>
    {
        public static readonly int Id = MessagePacker.GetId(typeof(T));

        public static readonly ProfilerMarker SendMarker = new ProfilerMarker($"Send_{typeof(T).Name}");
        public static readonly ProfilerMarker ReceiveMarker = new ProfilerMarker($"Handle_{typeof(T).Name}");
    }

    // message packing all in one place, instead of constructing headers in all
    // kinds of different places
    //
    //   MsgType     (1-n bytes)
    //   Content     (ContentSize bytes)
    //
    // -> we use varint for headers because most messages will result in 1 byte
    //    type/size headers then instead of always
    //    using 2 bytes for shorts.
    // -> this reduces bandwidth by 10% if average message size is 20 bytes
    //    (probably even shorter)
    public static class MessagePacker
    {
        private static readonly ProfilerMarker packMarker = new ProfilerMarker(nameof(Pack));
        /// <summary>
        /// Backing field for <see cref="MessageTypes"/>
        /// </summary>
        private static readonly Dictionary<int, Type> messageTypes = new Dictionary<int, Type>();

        /// <summary>
        /// Map of Message Id => Type
        /// When we receive a message, we can lookup here to find out what type it was.
        /// This is populated by the weaver.
        /// </summary>
        public static IReadOnlyDictionary<int, Type> MessageTypes => messageTypes;

        /// <summary>
        /// Registers a message with its ID, Useful for debugging if a message handler is missing
        /// <para>Used by weaver</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterMessage<T>()
        {
            var id = GetId<T>();

            if (messageTypes.TryGetValue(id, out var type) && type != typeof(T))
            {
                throw new ArgumentException($"Message {typeof(T)} and {messageTypes[id]} have the same ID. Change the name of one of those messages");
            }
            messageTypes[id] = typeof(T);
        }

        /// <summary>
        /// Gets the Id from <see cref="MessageIdCache"/> for <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetId<T>()
        {
            return MessageIdCache<T>.Id;
        }

        /// <summary>
        /// Used to calculate new hash for <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetId(Type type)
        {
            // paul: 16 bits is enough to avoid collisions
            //  - keeps the message size small because it gets varinted
            //  - in case of collisions,  Mirage will display an error
            return type.FullName.GetStableHashCode() & 0xFFFF;
        }

        // pack message before sending
        // -> NetworkWriter passed as arg so that we can use .ToArraySegment
        //    and do an allocation free send before recycling it.
        public static void Pack<T>(T message, NetworkWriter writer)
        {
            using var _ = packMarker.Auto();

            // if it is a value type,  just use typeof(T) to avoid boxing
            // this works because value types cannot be derived
            // if it is a reference type (for example IMessageBase),
            // ask the message for the real type
            var id = default(T) == null && message != null
                // for class we need to use GetType incase T is base class
                ? GetId(message.GetType())
                // for struct, we can use the cached Id
                : GetId<T>();

            writer.WriteUInt16((ushort)id);

            writer.Write(message);
        }

        // helper function to pack message into a simple byte[] (which allocates)
        // => useful for tests
        // => useful for local client message enqueue
        public static byte[] Pack<T>(T message)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                Pack(message, writer);
                var data = writer.ToArray();

                return data;
            }
        }

        /// <summary>
        /// unpack a message we received
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="objectLocator">Can be null, but must be set in order to read NetworkIdentity Values</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static T Unpack<T>(byte[] data, IObjectLocator objectLocator)
        {
            using (var networkReader = NetworkReaderPool.GetReader(data, objectLocator))
            {
                ValidateId<T>(networkReader);

                return networkReader.Read<T>();
            }
        }

        /// <summary>
        /// Check that id of type is the same as message header
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="networkReader"></param>
        /// <exception cref="FormatException"></exception>
        private static void ValidateId<T>(PooledNetworkReader networkReader)
        {
            var typeId = GetId<T>();

            int id = networkReader.ReadUInt16();
            if (id != typeId)
                throw new FormatException("Invalid message,  could not unpack " + typeof(T).FullName);
        }

        // unpack message after receiving
        // -> pass NetworkReader so it's less strange if we create it in here
        //    and pass it upwards.
        // -> NetworkReader will point at content afterwards!
        public static int UnpackId(NetworkReader messageReader)
        {
            return messageReader.ReadUInt16();
        }
    }
}
