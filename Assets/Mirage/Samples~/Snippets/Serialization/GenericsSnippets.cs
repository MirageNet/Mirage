using UnityEngine;
using Mirage;
using Mirage.Serialization;
using Mirage.Collections;

namespace Mirage.Snippets.Serialization.Generics
{
    // CodeEmbed-Start: generic-behaviour
    public class MyGenericBehaviour<T> : NetworkBehaviour
    {
        [SyncVar]
        public T Value;

        public void MyRpc(T value) 
        {
            // do stuff
        }
    }
    // CodeEmbed-End: generic-behaviour

    // CodeEmbed-Start: custom-type
    [NetworkMessage]
    public struct MyCustomType
    {
        public int Value;
    }
    // CodeEmbed-End: custom-type

    // CodeEmbed-Start: custom-type-extensions
    public static class MyCustomTypeExtensions 
    {
        public static void Write(this NetworkWriter writer, MyCustomType value) 
        {
            // write here
        }

        public static MyCustomType Read(this NetworkReader reader) 
        {
            // read here
            return default;
        }
    }
    // CodeEmbed-End: custom-type-extensions

    // CodeEmbed-Start: generic-message
    public struct MyMessage<T>
    {
        public T Value;
    }

    class Manager 
    {
        public NetworkServer Server;

        void Start() 
        {
            Server.MessageHandler.RegisterHandler<MyMessage<int>>(HandleIntMessage);
        }

        void HandleIntMessage(INetworkPlayer player, MyMessage<int> msg)
        {
            // do stuff
        }
    }
    // CodeEmbed-End: generic-message

    // CodeEmbed-Start: generic-collections
    public struct MyType<T>
    {
        public bool Option;
        public T Value;
    }

    public class MyBehaviour : NetworkBehaviour
    {
        public SyncList<MyType<float>> myList = new SyncList<MyType<float>>();
    }
    // CodeEmbed-End: generic-collections
}
