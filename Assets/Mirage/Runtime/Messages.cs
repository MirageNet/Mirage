using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Mirage
{
    #region Public System Messages

    /// <summary>
    /// Sent to client to mark their scene as not ready
    /// <para>Client can sent <see cref="SceneReadyMessage"/> once its scene is ready again</para>
    /// </summary>
    [NetworkMessage]
    public struct SceneNotReadyMessage { }

    [NetworkMessage]
    public struct SceneMessage
    {
        public string MainActivateScene;
        // Normal = 0, LoadAdditive = 1, UnloadAdditive = 2
        public SceneOperation SceneOperation;
        public List<string> AdditiveScenes;
    }

    /// <summary>
    /// Sent to indicate the scene is finished loading
    /// </summary>
    [NetworkMessage]
    public struct SceneReadyMessage { }

    #endregion

    #region System Messages required for code gen path
    [NetworkMessage]
    public struct ServerRpcMessage
    {
        public uint NetId;
        public int ComponentIndex;
        public int FunctionIndex;

        // the parameters for the Cmd function
        // -> ArraySegment to avoid unnecessary allocations
        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct ServerRpcWithReplyMessage
    {
        public uint NetId;
        public int ComponentIndex;
        public int FunctionIndex;

        // if the server Rpc can return values
        // this then a ServerRpcReply will be sent with this id
        public int ReplyId;

        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct ServerRpcReply
    {
        public int ReplyId;
        public ArraySegment<byte> Payload;
    }

    [NetworkMessage]
    public struct RpcMessage
    {
        public uint NetId;
        public int ComponentIndex;
        public int FunctionIndex;
        public ArraySegment<byte> Payload;
    }
    #endregion

    #region Internal System Messages
    [NetworkMessage]
    public struct SpawnMessage
    {
        /// <summary>
        /// netId of new or existing object
        /// </summary>
        public uint NetId;
        /// <summary>
        /// Is the spawning object the local player. Sets ClientScene.localPlayer
        /// </summary>
        public bool IsLocalPlayer;
        /// <summary>
        /// Sets hasAuthority on the spawned object
        /// </summary>
        public bool IsOwner;
        /// <summary>
        /// The id of the scene object to spawn
        /// </summary>
        public ulong? SceneId;
        /// <summary>
        /// The id of the prefab to spawn
        /// <para>If sceneId != 0 then it is used instead of prefabHash</para>
        /// </summary>
        public int? PrefabHash;

        /// <summary>
        /// Spawn values to set after spawning object, values based on <see cref="NetworkIdentity.TransformSpawnSettings"/>
        /// </summary>
        public SpawnValues SpawnValues;

        /// <summary>
        /// The serialized component data
        /// <remark>ArraySegment to avoid unnecessary allocations</remark>
        /// </summary>
        public ArraySegment<byte> Payload;

        public override string ToString()
        {
            string spawnIDStr;
            if (SceneId.HasValue)
                spawnIDStr = $"SceneId:{SceneId.Value}";
            else if (PrefabHash.HasValue)
                spawnIDStr = $"PrefabHash:{PrefabHash.Value}";
            else
                spawnIDStr = $"SpawnId:Error";

            string authStr;
            if (IsLocalPlayer)
                authStr = "LocalPlayer";
            else if (IsOwner)
                authStr = "Owner";
            else
                authStr = "Remote";


            return $"SpawnMessage[NetId:{NetId},{spawnIDStr},Authority:{authStr},{SpawnValues},Payload:{Payload.Count}bytes]";

        }
    }

    public struct SpawnValues
    {
        public Vector3? Position;
        public Quaternion? Rotation;
        public Vector3? Scale;
        public string Name;
        public bool? SelfActive;

        [ThreadStatic]
        private static StringBuilder builder;

        public override string ToString()
        {
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Clear();

            builder.Append("SpawnValues(");
            var first = true;

            if (Position.HasValue)
                Append(ref first, $"Position={Position.Value}");

            if (Rotation.HasValue)
                Append(ref first, $"Rotation={Rotation.Value}");

            if (Scale.HasValue)
                Append(ref first, $"Scale={Scale.Value}");

            if (!string.IsNullOrEmpty(Name))
                Append(ref first, $"Name={Name}");

            if (SelfActive.HasValue)
                Append(ref first, $"SelfActive={SelfActive.Value}");

            builder.Append(")");
            return builder.ToString();
        }

        private static void Append(ref bool first, string value)
        {
            if (!first) builder.Append(", ");
            first = false;
            builder.Append(value);
        }
    }


    [NetworkMessage]
    public struct RemoveAuthorityMessage
    {
        public uint NetId;
    }

    [NetworkMessage]
    public struct RemoveCharacterMessage
    {
        public bool KeepAuthority;
    }

    [NetworkMessage]
    public struct ObjectDestroyMessage
    {
        public uint NetId;
    }

    [NetworkMessage]
    public struct ObjectHideMessage
    {
        public uint NetId;
    }

    [NetworkMessage]
    public struct UpdateVarsMessage
    {
        public uint NetId;
        public ArraySegment<byte> Payload;
    }

    // A client sends this message to the server
    // to calculate RTT and synchronize time
    [NetworkMessage]
    public struct NetworkPingMessage
    {
        public double ClientTime;
    }

    // The server responds with this message
    // The client can use this to calculate RTT and sync time
    [NetworkMessage]
    public struct NetworkPongMessage
    {
        public double ClientTime;
        public double ServerTime;
    }
    #endregion
}
