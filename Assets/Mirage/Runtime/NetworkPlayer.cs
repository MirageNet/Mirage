using System;
using System.Collections.Generic;
using Mirage.Authentication;
using Mirage.Logging;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// A High level network connection. This is used for connections from client-to-server and for connection from server-to-client.
    /// </summary>
    /// <remarks>
    /// <para>A NetworkConnection corresponds to a specific connection for a host in the transport layer. It has a connectionId that is assigned by the transport layer and passed to the Initialize function.</para>
    /// <para>A NetworkClient has one NetworkConnection. A NetworkServerSimple manages multiple NetworkConnections. The NetworkServer has multiple "remote" connections and a "local" connection for the local client.</para>
    /// <para>The NetworkConnection class provides message sending and handling facilities. For sending data over a network, there are methods to send message objects, byte arrays, and NetworkWriter objects. To handle data arriving from the network, handler functions can be registered for message Ids, byte arrays can be processed by HandleBytes(), and NetworkReader object can be processed by HandleReader().</para>
    /// <para>NetworkConnection objects also act as observers for networked objects. When a connection is an observer of a networked object with a NetworkIdentity, then the object will be visible to corresponding client for the connection, and incremental state changes will be sent to the client.</para>
    /// <para>There are many virtual functions on NetworkConnection that allow its behaviour to be customized. NetworkClient and NetworkServer can both be made to instantiate custom classes derived from NetworkConnection by setting their networkConnectionClass member variable.</para>
    /// </remarks>
    public sealed class NetworkPlayer : INetworkPlayer
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkPlayer));

        private readonly HashSet<NetworkIdentity> _visList = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Transport level connection
        /// </summary>
        /// <remarks>
        /// <para>On a server, this Id is unique for every connection on the server. On a client this Id is local to the client, it is not the same as the Id on the server for this connection.</para>
        /// <para>Transport layers connections begin at one. So on a client with a single connection to a server, the connectionId of that connection will be one. In NetworkServer, the connectionId of the local connection is zero.</para>
        /// <para>Clients do not know their connectionId on the server, and do not know the connectionId of other clients on the server.</para>
        /// </remarks>
        private readonly IConnection _connection;

        public bool IsHost { get; }

        /// <summary>
        /// Has this player been marked as disconnected
        /// <para>Messages sent to disconnected players will be ignored</para>
        /// </summary>
        private bool _isDisconnected = false;

        /// <summary>
        /// Backing field for <see cref="Identity"/>
        /// </summary>
        private NetworkIdentity _identity;

        /// <summary>
        /// Authentication information for this NetworkPlayer
        /// </summary>
        public PlayerAuthentication Authentication { get; private set; }

        public void SetAuthentication(PlayerAuthentication authentication, bool allowReplace)
        {
            if (Authentication == null || allowReplace)
            {
                Authentication = authentication;
            }
            else
            {
                throw new InvalidOperationException("Can't set Authentication because it is already set");
            }
        }

        /// <summary>
        /// Helper methods to check if Authentication is set
        /// </summary>
        public bool IsAuthenticated => Authentication != null;

        /// <summary>
        /// Flag that tells us if the scene has fully loaded in for player.
        /// <para>This property is read-only. It is set by the system on the client when the scene has fully loaded, and set by the system on the server when a ready message is received from a client.</para>
        /// <para>A client that is ready is sent spawned objects by the server and updates to the state of spawned objects. A client that is not ready is not sent spawned objects.</para>
        /// <para>Starts as true, when a client connects it is assumed that it is already in a ready scene. It will be set to not ready if NetworkSceneManager loads a scene</para>
        /// </summary>
        public bool SceneIsReady { get; set; } = true;

        /// <summary>
        /// Checks if this player has a <see cref="Identity"/>
        /// </summary>
        public bool HasCharacter => Identity != null;

        /// <summary>
        /// The IP address / URL / FQDN associated with the connection.
        /// Can be useful for a game master to do IP Bans etc.
        /// </summary>
        public IEndPoint Address => _connection.EndPoint;

        public IConnection Connection => _connection;

        /// <summary>
        /// List of all networkIdentity that this player can see
        /// <para>Only valid on server</para>
        /// </summary>
        public IReadOnlyCollection<NetworkIdentity> VisList => _visList;


        /// <summary>
        /// Disconnects the player.
        /// <para>A disconnected player can not send messages</para>
        /// </summary>
        /// <remarks>
        /// This method exists so that users do not need to add reference to SocketLayer asmdef
        /// </remarks>
        public void Disconnect()
        {
            // dont need to call disconnect twice, so just return
            if (_isDisconnected)
                return;

            _connection.Disconnect();
            _isDisconnected = true;
        }

        /// <summary>
        /// Marks player as disconnected, used when the disconnect call is from peer
        /// <para>A disconnected player can not send messages</para>
        /// </summary>
        public void MarkAsDisconnected()
        {
            _isDisconnected = true;
        }

        /// <summary>
        /// Event called when <see cref="Identity"/> property is changed
        /// </summary>
        public event Action<NetworkIdentity> OnIdentityChanged;

        /// <summary>
        /// The NetworkIdentity for this connection.
        /// </summary>
        public NetworkIdentity Identity
        {
            get => _identity;
            set
            {
                if (_identity == value)
                    return;

                _identity = value;
                OnIdentityChanged?.Invoke(_identity);
            }
        }

        /// <summary>
        /// A list of the NetworkIdentity objects owned by this connection. This list is read-only.
        /// <para>This includes the player object for the connection - if it has localPlayerAuthority set, and any objects spawned with local authority or set with AssignLocalAuthority.</para>
        /// <para>This list can be used to validate messages from clients, to ensure that clients are only trying to control objects that they own.</para>
        /// </summary>
        // IMPORTANT: this needs to be <NetworkIdentity>, not <uint netId>. fixes a bug where DestroyOwnedObjects wouldn't find
        //            the netId anymore: https://github.com/vis2k/Mirror/issues/1380 . Works fine with NetworkIdentity pointers though.
        private readonly HashSet<NetworkIdentity> _ownedObjects = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Creates a new NetworkConnection with the specified address and connectionId
        /// </summary>
        /// <param name="networkConnectionId"></param>
        public NetworkPlayer(IConnection connection, bool isHost)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IsHost = isHost;
        }

        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(T message, Channel channelId = Channel.Reliable)
        {
            if (_isDisconnected) { return; }

            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(message, writer);

                var segment = writer.ToArraySegment();
                NetworkDiagnostics.OnSend(message, segment.Count, 1);
                if (logger.LogEnabled()) logger.Log($"Sending {typeof(T)} to {this} channel:{channelId}");
                Send(segment, channelId);
            }
        }

        /// <summary>
        /// Sends a block of data
        /// <para>Only use this method if data has message Id already included, otherwise receives wont know how to handle it. Otherwise use <see cref="Send{T}(T, int)"/></para>
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="channelId"></param>
        public void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable)
        {
            if (_isDisconnected) { return; }

            if (channelId == Channel.Reliable)
            {
                _connection.SendReliable(segment);
            }
            else
            {
                _connection.SendUnreliable(segment);
            }
        }

        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        /// <returns></returns>
        public void Send<T>(T message, INotifyCallBack callBacks)
        {
            if (_isDisconnected) { return; }

            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(message, writer);

                var segment = writer.ToArraySegment();
                NetworkDiagnostics.OnSend(message, segment.Count, 1);
                if (logger.LogEnabled()) logger.Log($"Sending {typeof(T)} to {this} channel:Notify");
                _connection.SendNotify(segment, callBacks);
            }
        }

        public override string ToString()
        {
            return $"connection({Address})";
        }

        public void AddToVisList(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log($"Adding {identity} to Player[{Address}] VisList");
            _visList.Add(identity);
        }

        public void RemoveFromVisList(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log($"Removing {identity} from Player[{Address}] VisList");
            _visList.Remove(identity);
        }

        /// <summary>
        /// Checks if player can see NetworkIdentity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public bool ContainsInVisList(NetworkIdentity identity)
        {
            return _visList.Contains(identity);
        }

        /// <summary>
        /// Removes all objects that this player can see
        /// <para>This is called when loading a new scene</para>
        /// </summary>
        public void RemoveAllVisibleObjects()
        {
            if (logger.LogEnabled()) logger.Log($"Removing all from Player[{Address}] VisList");

            foreach (var identity in _visList)
            {
                identity.RemoveObserverInternal(this);
            }
            _visList.Clear();
        }

        public void AddOwnedObject(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log($"Adding {identity} to Player[{Address}] OwnedObjects");

            _ownedObjects.Add(identity);
        }

        public void RemoveOwnedObject(NetworkIdentity identity)
        {
            if (logger.LogEnabled()) logger.Log($"Removing {identity} from Player[{Address}] OwnedObjects");

            _ownedObjects.Remove(identity);

            // if is main character, then also remove that
            if (Identity == identity)
            {
                Identity = null;
            }
        }

        /// <summary>
        /// Destroy all objects owned by this player
        /// <para>NOTE: only destroyed objects that are currently spawned</para>
        /// </summary>
        public void DestroyOwnedObjects()
        {
            if (logger.LogEnabled()) logger.Log($"Destroying all Player[{Address}] OwnedObjects");

            // create a copy because the list might be modified when destroying
            var ownedObjects = new HashSet<NetworkIdentity>(_ownedObjects);

            foreach (var netIdentity in ownedObjects)
            {
                //dont destroy self yet.
                if (netIdentity == Identity)
                    continue;

                TryDestroy(netIdentity);
            }

            // Destroy the connections own identity.
            TryDestroy(Identity);

            // clear the hashset because we destroyed them all
            _ownedObjects.Clear();
        }

        private static void TryDestroy(NetworkIdentity identity)
        {
            if (identity != null && identity.ServerObjectManager != null)
            {
                // use SOM on object we are destroying, it should be set if object is spawned,
                //     previous this used the SOM from the player's character,
                //     we can't use that because if Identity is null we wont have a SOM
                // make sure to check sceneObject, we dont want to destory server's copy of a Scene object
                identity.ServerObjectManager.Destroy(identity, destroyServerObject: !identity.IsSceneObject);
            }
        }
    }
}
