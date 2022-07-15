using System.Collections.Generic;
using Mirage.Serialization;
using Mirage.SocketLayer;

namespace Mirage.Examples.Notify
{
    /// <summary>
    /// This is an example of how to use Notify to send changes over unreliable
    /// <para>
    /// When using unreliable message they might not reach the other side,
    /// You can use Notify so be told if the message is delivered or lost.<br/>
    /// If you have some values or collection that changes every update you
    /// might just want to send the changed values from that, using notify you
    /// can tell which was the last update the other side received.
    /// </para>
    /// </summary>
    public class SendChangesNotify
    {
        private readonly INetworkPlayer player;
        public List<float> someValues = new List<float>();
        public int lastReceivedCount;

        // pool for MyNotifyCallbacks so they can be re-used and not allocate
        private Stack<MyNotifyCallbacks> callbackPool = new Stack<MyNotifyCallbacks>();

        private MyNotifyCallbacks GetCallbacks()
        {
            if (callbackPool.Count > 0)
            {
                return callbackPool.Pop();
            }
            else
            {
                return new MyNotifyCallbacks(this);
            }
        }


        public SendChangesNotify(INetworkPlayer player)
        {
            this.player = player;
        }


        public void Update(float newValue)
        {
            // some changes value or collection,
            someValues.Add(newValue);

            // create message and add new undelivered values
            var message = new Changes
            {
                // Send Count so other side knows what values to expecct
                valueCount = someValues.Count,
                newValues = new List<float>(),
            };

            // add values from last know received
            for (var i = lastReceivedCount; i < someValues.Count; i++)
            {
                message.newValues.Add(someValues[i]);
            }

            // get callback from pool and set count
            var callbacks = GetCallbacks();
            callbacks.valueCount = someValues.Count;

            // send message and callbacks
            SendNotify(message, callbacks);
        }


        /// <summary>
        /// This function Packs a message and sends it to the player's connection, This function may be added to NetworkPlayer in the future
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callBacks"></param>
        public void SendNotify(Changes message, INotifyCallBack callBacks)
        {
            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(message, writer);

                var segment = writer.ToArraySegment();
                player.Connection.SendNotify(segment, callBacks);
            }
        }

        // A message with changes
        [NetworkMessage]
        public struct Changes
        {
            // Note: using a list here will allocate, to avoid allocations use an array segment and write length + values

            public int valueCount;
            public List<float> newValues;
        }

        // A class that implements INotifyCallBack so the notify system can call methods when message is marked as lost or delieved
        // This should not be a struct as it will cause the value to be boxed when pasted into SendNotify.
        // A pool can be used to hold these so they can be re-used multiple times and avoid allocations
        public class MyNotifyCallbacks : INotifyCallBack
        {
            private readonly SendChangesNotify owner;
            public int valueCount;

            public MyNotifyCallbacks(SendChangesNotify owner)
            {
                this.owner = owner;
            }

            public void OnDelivered()
            {
                // if delivered is newer, then update it
                if (owner.lastReceivedCount < valueCount)
                {
                    owner.lastReceivedCount = valueCount;
                }

                // return to pool so it can be re-used
                owner.callbackPool.Push(this);
            }

            public void OnLost()
            {
                // nothing if lost

                // return to pool so it can be re-used
                owner.callbackPool.Push(this);
            }
        }
    }

    /// <summary>
    /// This class does not show anything special for Notify but shows how you might receives an unreliable message
    /// <para>
    /// The new received values might have data that has already been received, so in this case you need to only take the new values from Changes and add them
    /// </para>
    /// <para>
    /// If the server is receiving this message you will need to find the "someValues" and "lastReceivedCount" that belongs the the player first
    /// </para>
    /// </summary>
    public class RecieveChangesNotify
    {
        public RecieveChangesNotify(IMessageReceiver receiver)
        {
            // Register Handler so that message can be received
            receiver.RegisterHandler<SendChangesNotify.Changes>(HandleChange);
        }

        // if on client you can just have single pair of these values,
        // but on server you will need a dictionary or another way to find the ones that belong to the player that sent the message
        public List<float> someValues = new List<float>();
        public int lastReceivedCount;

        private void HandleChange(INetworkPlayer player, SendChangesNotify.Changes message)
        {
            // if server find values belonging to player

            // work out how many new values there are
            var newValues = message.newValues;
            var newCount = message.valueCount;

            var numberOfNewValues = newCount - lastReceivedCount;
            var offset = newValues.Count - numberOfNewValues;

            // add new values to list
            for (var i = offset; i < newValues.Count; i++)
            {
                someValues.Add(newValues[i]);
            }
        }
    }
}
