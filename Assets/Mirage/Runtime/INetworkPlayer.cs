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

    // delegates to give names to variables in handles
    public delegate void MessageDelegate<in T>(T message);
    public delegate void MessageDelegateWithPlayer<in T>(INetworkPlayer player, T message);

    /// <summary>
    /// An object that can receive messages
    /// </summary>
    public interface IMessageReceiver
    {
        void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler);

        void RegisterHandler<T>(MessageDelegate<T> handler);

        void UnregisterHandler<T>();

        void ClearHandlers();

        void HandleMessage(INetworkPlayer player, ArraySegment<byte> packet);
    }

    /// <summary>
    /// An object that can observe NetworkIdentities.
    /// this is useful for interest management
    /// </summary>
    public interface IVisibilityTracker
    {
        void AddToVisList(NetworkIdentity identity);
        void RemoveFromVisList(NetworkIdentity identity);
        void RemoveAllVisibleObjects();
    }

    /// <summary>
    /// An object that can own networked objects
    /// </summary>
    public interface IObjectOwner
    {
        NetworkIdentity Identity { get; set; }
        bool HasCharacter { get; }
        void RemoveOwnedObject(NetworkIdentity networkIdentity);
        void AddOwnedObject(NetworkIdentity networkIdentity);
        void DestroyOwnedObjects();
    }

    /// <summary>
    /// An object owned by a player that can: send/receive messages, have network visibility, be an object owner, authenticated permissions, and load scenes.
    /// May be from the server to client or from client to server
    /// </summary>
    public interface INetworkPlayer : IMessageSender, IVisibilityTracker, IObjectOwner, IAuthenticatedObject, ISceneLoader
    {
        SocketLayer.IConnection Connection { get; }
        void Disconnect();
        void MarkAsDisconnected();
    }

    public interface IAuthenticatedObject
    {
        /// <summary>
        /// Marks if this player has been accepted by a <see cref="NetworkAuthenticator"/>
        /// </summary>
        bool IsAuthenticated { get; set; }

        /// <summary>
        /// General purpose object to hold authentication data, character selection, tokens, etc.
        /// associated with the connection for reference after Authentication completes.
        /// </summary>
        object AuthenticationData { get; set; }
    }

    public interface ISceneLoader
    {
        /// <summary>
        ///     Scene is fully loaded and we now can do things with player.
        /// </summary>
        bool SceneIsReady { get; set; }
    }
}
