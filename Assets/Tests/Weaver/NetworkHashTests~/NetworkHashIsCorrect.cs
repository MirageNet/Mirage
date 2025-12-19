using Mirage;
using Mirage.Serialization;
using System.Collections.Generic;

namespace NetworkHashTests.NetworkHashIsCorrect
{
    [NetworkMessage]
    public struct MyMessage { public int someValue; }

    public class MyTestBehaviour : NetworkBehaviour 
    {
        [SyncVar] public int mySyncVar;

        [ServerRpc]
        public void RpcMyTestRpc(int value) {}
    }

    public static class MyWriters
    {
        public static void WriteMyMessage(this Mirage.Serialization.NetworkWriter writer, MyMessage msg) {}
        public static MyMessage ReadMyMessage(this Mirage.Serialization.NetworkReader reader) { return default; }
    }
}
