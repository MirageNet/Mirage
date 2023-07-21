using System;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// SyncVars are used to synchronize a variable from the server to all clients automatically.
    /// <para>Value must be changed on server, not directly by clients.  Hook parameter allows you to define a client-side method to be invoked when the client gets an update from the server.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncVarAttribute : PropertyAttribute
    {
        ///<summary>A function that should be called on the client when the value changes.</summary>
        public string hook;
        /// <summary>
        /// If true, this syncvar will only be sent with spawn message, any other changes will not be sent to existing objects
        /// </summary>
        public bool initialOnly;

        /// <summary>
        /// If true this syncvar hook will also fire on the server side.
        /// </summary>
        // todo add test to make sure this runs on owner client
        // public bool invokeHookOnSender;
        // [System.Obsolete("Use invokeHookOnSender instead", false)]
        public bool invokeHookOnServer;

        /// <summary>
        /// If true this syncvar hook will also fire the owner when it is sending data
        /// </summary>
        public bool invokeHookOnOwner;

        /// <summary>
        /// What type of look Mirage should look for
        /// </summary>
        public SyncHookType hookType = SyncHookType.Automatic;
    }

    public enum SyncHookType
    {
        /// <summary>
        /// Looks for hooks matching the signature, gives compile error if none or more than 1 is found
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Hook with signature <c>void hookName()</c>
        /// </summary>
        MethodWith0Arg,

        /// <summary>
        /// Hook with signature <c>void hookName(T newValue)</c>
        /// </summary>
        MethodWith1Arg,

        /// <summary>
        /// Hook with signature <c>void hookName(T oldValue, T newValue)</c>
        /// </summary>
        MethodWith2Arg,

        /// <summary>
        /// Hook with signature <c>event Action hookName;</c>
        /// </summary>
        EventWith0Arg,

        /// <summary>
        /// Hook with signature <c>event Action{T} hookName;</c>
        /// </summary>
        EventWith1Arg,

        /// <summary>
        /// Hook with signature <c>event Action{T,T} hookName;</c>
        /// </summary>
        EventWith2Arg,
    }

    /// <summary>
    /// Call this from a client to run this function on the server.
    /// <para>Make sure to validate input etc. It's not possible to call this from a server.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerRpcAttribute : Attribute
    {
        public Channel channel = Channel.Reliable;
        public bool requireAuthority = true;
    }

    /// <summary>
    /// Used by ClientRpc to tell mirage who to send remote call to
    /// </summary>
    public enum RpcTarget
    {
        /// <summary>
        /// Sends to the <see cref="NetworkPlayer">Player</see> that owns the object
        /// </summary>
        Owner,
        /// <summary>
        /// Sends to all <see cref="NetworkPlayer">Players</see> that can see the object
        /// </summary>
        Observers,
        /// <summary>
        /// Sends to the <see cref="NetworkPlayer">Player</see> that is given as an argument in the RPC function (requires target to be an observer)
        /// </summary>
        Player
    }

    /// <summary>
    /// The server uses a Remote Procedure Call (RPC) to run this function on specific clients.
    /// <para>Note that if you set the target as Connection, you need to pass a specific connection as a parameter of your method</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
        public Channel channel = Channel.Reliable;
        public RpcTarget target = RpcTarget.Observers;
        public bool excludeOwner;
    }

    /// <summary>
    /// Tell the weaver to generate  reader and writer for a class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NetworkMessageAttribute : Attribute
    {
    }

    /// <summary>
    /// Prevents a method from running if server is not active.
    /// <para>Can only be used inside a NetworkBehaviour</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerAttribute : Attribute
    {
        /// <summary>
        /// If true,  when the method is called from a client, it throws an error
        /// If false, no error is thrown, but the method won't execute
        /// useful for unity built in methods such as Await, Update, Start, etc.
        /// </summary>
        public bool error = true;
    }

    /// <summary>
    /// Prevents this method from running if client is not active.
    /// <para>Can only be used inside a NetworkBehaviour</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientAttribute : Attribute
    {
        /// <summary>
        /// If true,  when the method is called from a client, it throws an error
        /// If false, no error is thrown, but the method won't execute
        /// useful for unity built in methods such as Await, Update, Start, etc.
        /// </summary>
        public bool error = true;
    }

    /// <summary>
    /// Prevents players without authority from running this method.
    /// <para>Can only be used inside a NetworkBehaviour</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HasAuthorityAttribute : Attribute
    {
        /// <summary>
        /// If true,  when the method is called from a client, it throws an error
        /// If false, no error is thrown, but the method won't execute
        /// useful for unity built in methods such as Await, Update, Start, etc.
        /// </summary>
        public bool error = true;
    }

    /// <summary>
    /// Prevents nonlocal players from running this method.
    /// <para>Can only be used inside a NetworkBehaviour</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LocalPlayerAttribute : Attribute
    {
        /// <summary>
        /// If true,  when the method is called from a client, it throws an error
        /// If false, no error is thrown, but the method won't execute
        /// useful for unity built in methods such as Await, Update, Start, etc.
        /// </summary>
        public bool error = true;
    }

    /// <summary>
    /// Prevents this method from running unless the NetworkFlags match the current state
    /// <para>Can only be used inside a NetworkBehaviour</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NetworkMethodAttribute : Attribute
    {
        /// <summary>
        /// If true, if called incorrectly method will throw.<br/>
        /// If false, no error is thrown, but the method won't execute.<br/>
        /// <para>
        /// useful for unity built in methods such as Await, Update, Start, etc.
        /// </para>
        /// </summary>
        public bool error = true;

        public NetworkMethodAttribute(NetworkFlags flags) { }
    }

    [Flags]
    public enum NetworkFlags
    {
        // note: NotActive can't be 0 as it needs its own flag
        //       This is so that people can check for (Server | NotActive)
        /// <summary>
        /// If both server and client are not active. Can be used to check for singleplayer or unspawned object
        /// </summary>
        NotActive = 1,
        Server = 2,
        Client = 4,
        /// <summary>
        /// If either Server or Client is active.
        /// <para>
        /// Note this will not check host mode. For host mode you need to use <see cref="ServerAttribute"/> and <see cref="ClientAttribute"/>
        /// </para>
        /// </summary>
        Active = Server | Client,
        HasAuthority = 8,
        LocalOwner = 16,
    }

    /// <summary>
    /// Converts a string property into a Scene property in the inspector
    /// </summary>
    public sealed class SceneAttribute : PropertyAttribute { }

    /// <summary>
    /// Used to show private SyncList in the inspector,
    /// <para> Use instead of SerializeField for non Serializable types </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowInInspectorAttribute : Attribute { }

    /// <summary>
    /// Draws UnityEvent as a foldout
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FoldoutEventAttribute : PropertyAttribute { }

    /// <summary>
    /// Makes field readonly in inspector.
    /// <para>This is useful for fields that are set by code, but are shown iin inpector for debuggiing</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyInspectorAttribute : PropertyAttribute { }

    /// <summary>
    /// Forces the user to provide a prefab that has a NetworkIdentity component and is registered.
    /// Also provides a fix button to fix the prefab if it hasn't been networked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NetworkedPrefabAttribute : PropertyAttribute { }
}
