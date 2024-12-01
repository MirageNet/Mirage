using System;

namespace Mirage.RemoteCalls
{
    [NetworkMessage]
    public struct RpcMessage
    {
        public uint NetId;
        public int FunctionIndex;
        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct RpcWithReplyMessage
    {
        public uint NetId;
        public int FunctionIndex;

        /// <summary>
        /// Id sent with rpc so that server can reply with <see cref="RpcReply"/> and send the same Id
        /// </summary>
        public int ReplyId;

        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct RpcReply
    {
        public int ReplyId;
        /// <summary>If result is returned, or exception was thrown</summary>
        public bool Success;
        public ArraySegment<byte> Payload;
    }
}
