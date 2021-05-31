using System;

namespace Mirage
{
    /// <summary>
    /// An object that can send messages
    /// </summary>
    public interface IMessageSender
    {
        void Send<T>(T message, int channelId = Channel.Reliable);

        void Send(ArraySegment<byte> segment, int channelId = Channel.Reliable);
    }

    /// <summary>
    /// An object that can receive messages
    /// </summary>
    public interface IMessageReceiver
    {
        void RegisterHandler<T>(Action<INetworkPlayer, T> handler);

        void RegisterHandler<T>(Action<T> handler);

        void UnregisterHandler<T>();

        void ClearHandlers();
    }

    /// <summary>
    /// An object that can send and receive messages and notify messages
    /// </summary>
    public interface IMessageHandler : IMessageSender, IMessageReceiver
    {
        void HandleMessage(ArraySegment<byte> packet);
    }

    /// <summary>
    /// An object that can observe NetworkIdentities.
    /// this is useful for interest management
    /// </summary>
    public interface IVisibilityTracker
    {
        void AddToVisList(NetworkIdentity identity);
        void RemoveFromVisList(NetworkIdentity identity);
        void RemoveObservers();
    }

    /// <summary>
    /// An object that can own networked objects
    /// </summary>
    public interface IObjectOwner
    {
        NetworkIdentity Identity { get; set; }
        void RemoveOwnedObject(NetworkIdentity networkIdentity);
        void AddOwnedObject(NetworkIdentity networkIdentity);
        void DestroyOwnedObjects();
    }

    /// <summary>
    /// An object owned by a player that can: send/receive messages, have network visibility, be an object owner, authenticated permissions, and load scenes.
    /// May be from the server to client or from client to server
    /// </summary>
    public interface INetworkPlayer : IMessageHandler, IVisibilityTracker, IObjectOwner, IAuthenticatedObject, ISceneLoader
    {
        SocketLayer.IConnection Connection { get; }
        void Disconnect();
        void MarkAsDisconnected();
    }

    public interface IAuthenticatedObject
    {
        object AuthenticationData { get; set; }
    }

    public interface ISceneLoader
    {
        bool IsReady { get; set; }
    }
}
