using System;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Extra helper methods for <see cref="ServerObjectManager"/> that dont add any extra logic
    /// </summary>
    public static class ServerObjectManagerExtensions
    {
        /// <summary>
        /// <para>When <see cref="AddCharacterMessage"/> is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.</para>
        /// <para>When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.</para>
        /// <para>This function is used for adding a character, not replacing. If there is already a character then use <see cref="ReplaceCharacter"/> instead.</para>
        /// </summary>
        /// <param name="player">the Player to add the character to</param>
        /// <param name="character">The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it.</param>
        /// <param name="prefabHash">New prefab hash to give to the player, used for dynamically creating objects at runtime.</param>
        /// <exception cref="ArgumentException">throw when the player already has a character</exception>
        public static void AddCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, int prefabHash)
        {
            var identity = character.GetNetworkIdentity();
            som.AddCharacter(player, identity, prefabHash);
        }

        /// <summary>
        /// <para>When <see cref="AddCharacterMessage"/> is received from a player, the server calls this to associate the character GameObject with the NetworkPlayer.</para>
        /// <para>When a character is added for a player the object is automatically spawned, so you do not need to call ServerObjectManager.Spawn for that object.</para>
        /// <para>This function is used for adding a character, not replacing. If there is already a character then use <see cref="ReplaceCharacter"/> instead.</para>
        /// </summary>
        /// <param name="player">the Player to add the character to</param>
        /// <param name="character">The Network Object to add to the Player. Can be spawned or unspawned. Calling this method will respawn it.</param>
        /// <exception cref="ArgumentException">throw when the player already has a character</exception>
        public static void AddCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character)
        {
            var identity = character.GetNetworkIdentity();
            som.AddCharacter(player, identity);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public static void ReplaceCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, bool keepAuthority = false)
        {
            var identity = character.GetNetworkIdentity();
            som.ReplaceCharacter(player, identity, keepAuthority);
        }

        /// <summary>
        /// This replaces the player object for a connection with a different player object. The old player object is not destroyed.
        /// <para>If a connection already has a player object, this can be used to replace that object with a different player object. This does NOT change the ready state of the connection, so it can safely be used while changing scenes.</para>
        /// </summary>
        /// <param name="player">Connection which is adding the player.</param>
        /// <param name="character">Player object spawned for the player.</param>
        /// <param name="prefabHash"></param>
        /// <param name="keepAuthority">Does the previous player remain attached to this connection?</param>
        /// <returns></returns>
        public static void ReplaceCharacter(this ServerObjectManager som, INetworkPlayer player, GameObject character, int prefabHash, bool keepAuthority = false)
        {
            var identity = character.GetNetworkIdentity();
            som.ReplaceCharacter(player, identity, prefabHash, keepAuthority);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and settings its owner to the player that owns <paramref name="ownerObject"/>
        /// </summary>
        /// <param name="ownerObject">An object owned by a player</param>
        public static void Spawn(this ServerObjectManager som, GameObject obj, GameObject ownerObject)
        {
            var ownerIdentity = ownerObject.GetNetworkIdentity();

            if (ownerIdentity.Owner == null)
            {
                throw new InvalidOperationException("Player object is not a player in the connection");
            }

            som.Spawn(obj, ownerIdentity.Owner);
        }

        /// <summary>
        /// Assigns <paramref name="prefabHash"/> to the <paramref name="obj"/> and then spawns it with <paramref name="owner"/>
        /// <para>
        ///     <see cref="NetworkIdentity.PrefabHash"/> can only be set on an identity if the current value is Empty
        /// </para>
        /// <para>
        ///     This method is useful if you are creating network objects at runtime and both server and client know what <see cref="Guid"/> to set on an object
        /// </para>
        /// </summary>
        /// <param name="obj">The object to spawn.</param>
        /// <param name="prefabHash">The prefabHash of the object to spawn. Used for custom spawn handlers.</param>
        /// <param name="owner">The connection that has authority over the object</param>
        public static void Spawn(this ServerObjectManager som, GameObject obj, int prefabHash, INetworkPlayer owner = null)
        {
            var identity = obj.GetNetworkIdentity();
            som.Spawn(identity, prefabHash, owner);
        }

        /// <summary>
        /// Spawns the <paramref name="identity"/> and settings its owner to <paramref name="owner"/>
        /// </summary>
        public static void Spawn(this ServerObjectManager som, GameObject obj, INetworkPlayer owner = null)
        {
            var identity = obj.GetNetworkIdentity();
            som.Spawn(identity, owner);
        }


        /// <summary>
        /// Instantiate a prefab an then Spawns it with ServerObjectManager
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="prefabHash"></param>
        /// <param name="owner"></param>
        public static GameObject SpawnInstantiate(this ServerObjectManager som, GameObject prefab, int? prefabHash = null, INetworkPlayer owner = null)
        {
            var clone = GameObject.Instantiate(prefab);
            if (prefabHash.HasValue)
                som.Spawn(clone, prefabHash.Value, owner);
            else
                som.Spawn(clone, owner);
            return clone;
        }

        /// <summary>
        /// Instantiate a prefab an then Spawns it with ServerObjectManager
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="prefabHash"></param>
        /// <param name="owner"></param>
        public static NetworkIdentity SpawnInstantiate(this ServerObjectManager som, NetworkIdentity prefab, int? prefabHash = null, INetworkPlayer owner = null)
        {
            var clone = GameObject.Instantiate(prefab);
            if (prefabHash.HasValue)
                som.Spawn(clone, prefabHash.Value, owner);
            else
                som.Spawn(clone, owner);
            return clone;
        }
    }
}
