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
        /// <summary>
        /// The main object owned by this player, normally the player's character
        /// </summary>
        NetworkIdentity Identity { get; set; }
        bool HasCharacter { get; }

        /// <summary>
        /// All the objects owned by the player
        /// </summary>
        IReadOnlyCollection<NetworkIdentity> OwnedObjects { get; }
        void AddOwnedObject(NetworkIdentity networkIdentity);
        void RemoveOwnedObject(NetworkIdentity networkIdentity);
        /// <summary>
        /// Removes all owned objects. This is useful to call when player disconnects to avoid objects being destroyed
        /// </summary>
        /// <param name="sendAuthorityChangeEvent">Should message be send to owner client? If player is disconnecting you should set this false</param>
        void RemoveAllOwnedObject(bool sendAuthorityChangeEvent);
        /// <summary>
        /// Destroys or unspawns all owned objects.
        /// This is called when the player is disconnects.
        /// It will be called after <see cref="NetworkServer.Disconnected"/>, so Disconnected can be used to remove any owned objects from the list before they are destroyed.
        /// </summary>
        void DestroyOwnedObjects();
    }

    /// <summary>
    /// An object owned by a player that can: send/receive messages, have network visibility, be an object owner, authenticated permissions, and load scenes.
    /// May be from the server to client or from client to server
    /// </summary>
    public interface INetworkPlayer : IMessageSender, IVisibilityTracker, IObjectOwner, ISceneLoader
    {
        /// <summary>
        /// Connection object managed by <see cref="Peer"/>
        /// <para>
        /// This is used to send messages and handle any reliability state for the underlying connection
        /// </para>
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// The low-level handle returned by <see cref="ISocket"/>.
        /// Can be used to find out more information about the low-level transport used or to get the Address of the connection.
        /// <para>Cast this to the handle type for the transport you are using.<br />
        /// example: <c>if (ConnectionHandle is UdpConnectionHandle udpHandle)</c> and then get the address via <c>udpHandle.Endpoint</c></para>
        /// </summary>
        IConnectionHandle ConnectionHandle { get; }

        /// <summary>Connect called on client, but server has not replied yet</summary>
        bool IsConnecting { get; }

        /// <summary>Server and Client are connected and can send messages</summary>
        bool IsConnected { get; }

        PlayerAuthentication Authentication { get; }
        void SetAuthentication(PlayerAuthentication authentication, bool allowReplace = false);
        bool IsAuthenticated { get; }


        /// <summary>Error rate limiting, will invoke disconnect player (or call <see cref="NetworkServer._errorRateLimitReached"/> if set) when limit is reached</summary>
        RateLimitBucket ErrorRateLimit { get; }
        /// <summary>Any flags set from catching errors</summary>
        PlayerErrorFlags ErrorFlags { get; }

        /// <summary>
        /// Call this when player causes an error
        /// </summary>
        /// <param name="cost">how bad or costly is the error. higher cost means player will trigger limit faster</param>
        /// <param name="flags">optional flag for error type</param>
        void SetError(int cost, PlayerErrorFlags flags);
        /// <summary>
        /// Call this when player causes an error, will set cost to be above maxTokens to ensure that limit is cheated to trigger disconnect.
        /// <para>If <see cref="ErrorRateLimit"/> is null will call <see cref="Disconnect"/> instead</para>
        /// </summary>
        /// <param name="flags">optional flag for error type</param>
        void SetErrorAndDisconnect(PlayerErrorFlags flags);

        /// <summary>Call to reset error flags</summary>
        void ResetErrorFlag();

        /// <summary>True if this Player is the local player on the server or client</summary>
        bool IsHost { get; }

        void Disconnect();
        void MarkAsDisconnected();
    }

    public interface ISceneLoader
    {
        /// <summary>Scene is fully loaded and we now can do things with player.</summary>
        bool SceneIsReady { get; set; }
    }

    [Flags]
    public enum PlayerErrorFlags
    {
        /// <summary>No custom errors code set.</summary>
        None = 0,

        //** Likely developer bugs **

        /// <summary>Rpc function threw <see cref="NullReferenceException"/> or <see cref="UnityEngine.MissingReferenceException"/>. Likely logic error in code</summary>
        RpcNullException = 1 << 0,

        /// <summary>Rpc function threw <see cref="Exception"/>. Likely logic error in code</summary>
        RpcException = 1 << 1,


        //** Connection/versioning issues, could be normal player or hacker but they should be disconnected **

        /// <summary>NetworkReader threw <see cref="Exception"/>. More likely to be out of sync version than logic error, but could be caused by custom reader.</summary>
        DeserializationException = 1 << 2,

        /// <summary>Rpc index or message type was wrong. This could be from out-of-date build.</summary>
        RpcSync = 1 << 3,

        /// <summary>User hit a rate limit, rather than causing a direct error</summary>
        RateLimit = 1 << 4,


        //** Security/Malicious Intent **

        /// <summary>Player does not have authority to call this object. Could happen in normal gameplay if changing the owner of an object.</summary>
        NoAuthority = 1 << 5,

        /// <summary>Message send before Authentication is complete</summary>
        Unauthenticated = 1 << 6,

        /// <summary>Error was critical, should be used to indicate player should be kicked/timed out/banned</summary>
        Critical = 1 << 7,

        /// <summary>Message value only possible with cheats/mods/etc</summary>
        LikelyCheater = 1 << 8,


        //** Custom developer defined errors **

        /// <summary> 
        /// Mirage errors will be defined from bits 0 to 16. the remaining 16 bits can be used for custom error.
        /// <para>
        /// <c>CustomError</c> can be used as start of bit shift. <br />
        /// MyError1 = CustomError &lt;&lt; 0 <br />
        /// MyError2 = CustomError &lt;&lt; 1 <br />
        /// </para>
        /// </summary>
        CustomError = 1 << 16
    }
}
