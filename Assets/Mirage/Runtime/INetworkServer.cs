namespace Mirage
{
    public interface INetworkServer
    {
        void Disconnect();

        void AddConnection(INetworkConnection conn);

        void RemoveConnection(INetworkConnection conn);

        void SendToAll<T>(T msg, int channelId = Channel.Reliable);
    }
}
