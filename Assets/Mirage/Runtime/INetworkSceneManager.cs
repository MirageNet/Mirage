using System;
using UnityEngine.Events;

namespace Mirage
{
    public enum SceneOperation : byte
    {
        Normal,
        LoadAdditive,
        UnloadAdditive
    }

    /// <summary>
    /// Event fires from <see cref="INetworkSceneManager">INetworkSceneManager</see> when a scene change happens on either Server or Client.
    /// <para>string - New ScenePath</para>
    /// <para>SceneOperation - Scene change type (Normal, Additive Load, Additive Unload).</para>
    /// </summary>
    [Serializable] public class ClientSceneChangeEvent : UnityEvent<string, SceneOperation> { }

    public interface INetworkSceneManager
    {
        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        ClientSceneChangeEvent ClientChangeScene { get; }

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        ClientSceneChangeEvent ClientSceneChanged { get; }

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        ClientSceneChangeEvent ServerChangeScene { get; }

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        ClientSceneChangeEvent ServerSceneChanged { get; }

        void ChangeServerScene(string scenePath, SceneOperation sceneOperation = SceneOperation.Normal);
    }
}
