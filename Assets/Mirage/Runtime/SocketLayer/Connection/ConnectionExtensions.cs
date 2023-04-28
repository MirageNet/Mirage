using System;

namespace Mirage.SocketLayer
{
    public static class ConnectionExtensions
    {
        public static void SendUnreliable(this IConnection conn, byte[] packet)
        {
            conn.SendUnreliable(packet, 0, packet.Length);
        }
        public static void SendUnreliable(this IConnection conn, ArraySegment<byte> packet)
        {
            conn.SendUnreliable(packet.Array, packet.Offset, packet.Count);
        }

        public static INotifyToken SendNotify(this IConnection conn, byte[] packet)
        {
            return conn.SendNotify(packet, 0, packet.Length);
        }
        public static INotifyToken SendNotify(this IConnection conn, ArraySegment<byte> packet)
        {
            return conn.SendNotify(packet.Array, packet.Offset, packet.Count);
        }

        public static void SendNotify(this IConnection conn, byte[] packet, INotifyCallBack callBacks)
        {
            conn.SendNotify(packet, 0, packet.Length, callBacks);
        }
        public static void SendNotify(this IConnection conn, ArraySegment<byte> packet, INotifyCallBack callBacks)
        {
            conn.SendNotify(packet.Array, packet.Offset, packet.Count, callBacks);
        }

        public static void SendReliable(this IConnection conn, byte[] packet)
        {
            conn.SendReliable(packet, 0, packet.Length);
        }
        public static void SendReliable(this IConnection conn, ArraySegment<byte> packet)
        {
            conn.SendReliable(packet.Array, packet.Offset, packet.Count);
        }
    }
}
