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
    /// <para>
    /// A NetworkClient has one NetworkPlayer.
    /// A NetworkServer manages multiple NetworkPlayers.
    /// The NetworkServer has multiple "remote" connections and a "local" connection for the local client.
    /// </para>
    /// <para>
    /// The NetworkPlayer class provides methods to send and handle network messages.
    /// To send data, you can pass a message object to the <see cref="NetworkPlayer.Send{T}(T, Channel)"/> method, and Mirage will handle the serialization.
    /// To handle incoming messages, you can register handlers for specific message types using <see cref="MessageHandler"/> found on <see cref="NetworkServer"/> or <see cref="NetworkClient"/>
    /// </para>
    /// <para>
    /// NetworkPlayer objects also act as observers for networked objects.
    /// When a connection is an observer of a networked object with a NetworkIdentity, then the object will be visible to corresponding client for the connection, and incremental state changes will be sent to the client.
    /// </para>
    /// </remarks>
    public sealed class NetworkPlayer : INetworkPlayer
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkPlayer));

        private readonly HashSet<NetworkIdentity> _visList = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Transport level connection
        /// </summary>
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
        /// <para>
        /// A client that is ready is sent spawned objects by the server and updates to the state of spawned objects. A client that is not ready is not sent spawned objects.
        /// </para>
        /// <para>
        /// Starts as true, when a client connects it is assumed that it is already in a ready scene.
        /// </para>
        /// <para>
        /// It will be set to not ready if NetworkSceneManager loads a scene.
        /// If you are controlling scene loading manually, you need to set this property to true or false before and after loading a scene.
        /// This is normally done using <see cref="SceneReadyMessage"/> and <see cref="SceneNotReadyMessage"/>
        /// </para>
        /// <para>
        /// On the client, this property is used to keep track of if the local scene is loading or ready.
        /// On the server, it is used to track if the player's scene is loading or ready.
        /// When server loads a new scene for everyone, it will normally set this property to false for all players.
        /// </para>
        /// </summary>
        public bool SceneIsReady { get; set; } = true;

        /// <summary>
        /// Checks if this player has a <see cref="Identity"/>
        /// </summary>
        public bool HasCharacter => Identity != null;

        public IConnection Connection => _connection;

        /// <summary>
        /// The IP address / URL / FQDN associated with the connection.
        /// Can be useful for a game master to do IP Bans etc.
        /// <para>
        /// Best used to get concrete Endpoint type based on the <see cref="SocketFactory"/> being used
        /// </para>
        /// </summary>
        public IEndPoint Address => _connection.EndPoint;

        /// <summary>Connect called on client, but server has not replied yet</summary>
        public bool IsConnecting => _connection.State == ConnectionState.Connecting;

        /// <summary>Server and Client are connected and can send messages</summary>
        public bool IsConnected => _connection.State == ConnectionState.Connected;

        /// <summary>
        /// List of all networkIdentity that this player can see
        /// <para>Only valid on server</para>
        /// </summary>
        public IReadOnlyCollection<NetworkIdentity> VisList => _visList;

        /// <summary>
        /// A list of the NetworkIdentity objects owned by this connection. This list is read-only.
        /// <para>Only valid on server</para>
        /// <para>This includes the player's character</para>
        /// <para>This list can be used to validate messages from clients, to ensure that clients are only trying to control objects that they own.</para>
        /// <para>Objects in the list will also have their <see cref="NetworkIdentity.Owner"/> field set to this NetworkPlayer</para>
        /// </summary>
        public IReadOnlyCollection<NetworkIdentity> OwnedObjects => _ownedObjects;

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
        /// Disconnects the player.
        /// <para>A disconnected player can not send messages</para>
        /// </summary>
        /// <remarks>
        /// This method exists so that users do not need to add reference to SocketLayer asmdef
        /// </remarks>
        public void Disconnect(DisconnectReason reason)
        {
            // dont need to call disconnect twice, so just return
            if (_isDisconnected)
                return;

            _connection.Disconnect(reason);
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

        /// <summary>backing field for <see cref="OwnedObjects"/></summary>
        private readonly HashSet<NetworkIdentity> _ownedObjects = new HashSet<NetworkIdentity>();

        /// <summary>
        /// Creates a new NetworkPlayer
        /// </summary>
        /// <param name="connection">Transport level connection for this player</param>
        /// <param name="isHost">True if this player is the host player</param>
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
        /// <para>
        /// This is a low-level method to send a raw bytes.
        /// Only use this method if you have manually included the message id and serialized the payload.
        /// Otherwise, receivers will not know how to handle it.
        /// It is recommended to use <see cref="Send{T}(T, Channel)"/> instead.</para>
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="channelId"></param>
        public void Send(ArraySegment<byte> segment, Channel channelId = Channel.Reliable)
        {
            if (_isDisconnected)
                return;

            try
            {
                if (channelId == Channel.Reliable)
                {
                    _connection.SendReliable(segment);
                }
                else
                {
                    _connection.SendUnreliable(segment);
                }
            }
            catch (BufferFullException e)
            {
                logger.LogError($"Disconnecting player because send buffer was full. {e}");
                Disconnect(DisconnectReason.SendBufferFull);
            }
            catch (NoConnectionException e)
            {
                logger.LogError($"Inner connection was disconnected, but disconnected flag not yet. {e}");
                Disconnect();
            }
        }

        /// <summary>
        /// This sends a network message to the connection.
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="msg">The message to send.</param>
        /// <param name="channelId">The transport layer channel to send on.</param>
        public void Send<T>(T message, INotifyCallBack callBacks)
        {
            if (_isDisconnected) { return; }

            using (var writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(message, writer);

                var segment = writer.ToArraySegment();
                NetworkDiagnostics.OnSend(message, segment.Count, 1);
                if (logger.LogEnabled()) logger.Log($"Sending {typeof(T)} to {this} channel:Notify");

                try
                {
                    _connection.SendNotify(segment, callBacks);
                }
                catch (BufferFullException e)
                {
                    logger.LogError($"Disconnecting player because send buffer was full. {e}");
                    Disconnect(DisconnectReason.SendBufferFull);
                }
                catch (NoConnectionException e)
                {
                    logger.LogError($"Inner connection was disconnected, but disconnected flag not yet. {e}");
                    Disconnect();
                }
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

        public void RemoveAllOwnedObject(bool sendAuthorityChangeEvent)
        {
            if (logger.LogEnabled()) logger.Log($"Removing all Player[{Address}] OwnedObjects");

            // create a copy because the list might be modified when destroying
            var ownedObjects = new HashSet<NetworkIdentity>(_ownedObjects);
            var mainIdentity = Identity;

            foreach (var netIdentity in ownedObjects)
            {
                // remove main object last
                if (netIdentity == mainIdentity)
                    continue;

                if (netIdentity == null)
                    continue;

                // code from Identity.RemoveClientAuthority, but without the safety checks, we dont need them here
                netIdentity.SetOwner(null);

                if (sendAuthorityChangeEvent && netIdentity.ServerObjectManager != null)
                    Send(new RemoveAuthorityMessage { NetId = netIdentity.NetId });
            }

            if (mainIdentity != null)
            {
                // code from ServerObjectManager.RemoveCharacter, but without the safety checks, we dont need them here
                mainIdentity.SetOwner(null);

                if (sendAuthorityChangeEvent && mainIdentity.ServerObjectManager != null)
                    Send(new RemoveCharacterMessage { KeepAuthority = false });

            }

            // clear the hashset because we destroyed them all
            _ownedObjects.Clear();
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
