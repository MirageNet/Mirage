using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.Authentication;
using Mirage.SocketLayer;

namespace Mirage
{
    /// <summary>
    /// An object that can send messages
    /// </summary>
    public interface IMessageSender
    {
        void Send<T>(T message, Channel channelId = Channel.Reliable);
        void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable);
        void Send<T>(T message, INotifyCallBack notifyCallBack);
    }

    // delegates to give names to variables in handles
    public delegate void MessageDelegate<in T>(T message);
    public delegate void MessageDelegateWithPlayer<in T>(INetworkPlayer player, T message);
    public delegate UniTaskVoid MessageDelegateAsync<in T>(T message);
    public delegate UniTaskVoid MessageDelegateWithPlayerAsync<in T>(INetworkPlayer player, T message);


    /// <summary>
    /// An object that can receive messages
    /// </summary>
    public interface IMessageReceiver
    {
        /// <summary>
        /// Registers a handler for a network message that has INetworkPlayer and <typeparamref name="T"/> Message parameters
        /// <para>
        /// When network message are sent, the first 2 bytes are the Id for the type <typeparamref name="T"/>.
        /// When message is received the <paramref name="handler"/> with the matching Id is found and invoked
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="allowUn authenticated">set this to true to allow message to be invoked before player is authenticated</param>
        void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler, bool allowUnauthenticated);
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
        /// <summary>
        /// Called when sending spawn message to client
        /// </summary>
        /// <param name="identity"></param>
        void AddToVisList(NetworkIdentity identity);

        /// <summary>
        /// Called when sending destroy message to client
        /// </summary>
        /// <param name="identity"></param>
        void RemoveFromVisList(NetworkIdentity identity);

        /// <summary>
        /// Removes all <see cref="NetworkIdentity"/> that this player can see
        /// <para>This is called when loading a new scene</para>
        /// </summary>
        void RemoveAllVisibleObjects();

        /// <summary>
        /// Checks if player can see <see cref="NetworkIdentity"/>
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        bool ContainsInVisList(NetworkIdentity identity);

        /// <summary>
        /// HashSet of all <see cref="NetworkIdentity"/> that this player can see
        /// <para>Only valid on server</para>
        /// <para>Reverse collection for <see cref="NetworkIdentity.observers"/></para>
        /// </summary>
        IReadOnlyCollection<NetworkIdentity> VisList { get; }
    }

    /// <summary>
    /// An object that can own networked objects
    /// </summary>
    public interface IObjectOwner
    {
        event Action<NetworkIdentity> OnIdentityChanged;
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
    public interface INetworkPlayer : IMessageSender, IVisibilityTracker, IObjectOwner, ISceneLoader
    {
        SocketLayer.IEndPoint Address { get; }
        SocketLayer.IConnection Connection { get; }
        PlayerAuthentication Authentication { get; }
        void SetAuthentication(PlayerAuthentication authentication, bool allowReplace = false);
        bool IsAuthenticated { get; }

        void Disconnect();
        void MarkAsDisconnected();
    }

    public interface ISceneLoader
    {
        /// <summary>
        ///     Scene is fully loaded and we now can do things with player.
        /// </summary>
        bool SceneIsReady { get; set; }
    }
}
