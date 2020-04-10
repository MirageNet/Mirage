using System;
using System.Net;
using System.Threading.Tasks;

namespace Mirror
{
    public interface INetworkConnection
    {
        NetworkIdentity Identity { get; set; }
        bool IsReady { get; set; }
        EndPoint Address { get; }

        void Disconnect();

        void RegisterHandler<T>(Action<INetworkConnection, T> handler)
                where T : IMessageBase, new();

        void RegisterHandler<T>(Action<T> handler) where T : IMessageBase, new();

        void UnregisterHandler<T>() where T : IMessageBase;

        void ClearHandlers();

        void Send<T>(T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase;

        Task SendAsync<T>(T msg, int channelId = Channels.DefaultReliable) where T : IMessageBase;

        string ToString();
        Task ProcessMessagesAsync();
        void AddToVisList(NetworkIdentity identity);
        void RemoveFromVisList(NetworkIdentity identity);
        void RemoveOwnedObject(NetworkIdentity networkIdentity);
        void AddOwnedObject(NetworkIdentity networkIdentity);
        void RemoveObservers();
        void DestroyOwnedObjects();
    }
}
