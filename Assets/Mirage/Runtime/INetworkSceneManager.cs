using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    [Serializable] public class SceneChangeEvent : UnityEvent<string, SceneOperation> { }

    public interface INetworkSceneManager
    {
        /// <summary>
        /// Event fires when the Client starts changing scene.
        /// </summary>
        SceneChangeEvent ClientStartedSceneChange { get; }

        /// <summary>
        /// Event fires after the Client has completed its scene change.
        /// </summary>
        SceneChangeEvent ClientFinishedSceneChange { get; }

        /// <summary>
        /// Event fires before Server changes scene.
        /// </summary>
        SceneChangeEvent ServerStartedSceneChange { get; }

        /// <summary>
        /// Event fires after Server has completed scene change.
        /// </summary>
        SceneChangeEvent ServerFinishedSceneChange { get; }

        /// <summary>
        ///     Allows server to fully load in a new scene and override current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        void ServerLoadSceneNormal(string scenePath);

        /// <summary>
        ///     Allows server to fully load in another scene on top of current active scene.
        /// </summary>
        /// <param name="scenePath">The full path to the scene file or the name of the scene.</param>
        /// <param name="players">List of player's that are receiving the new scene load.</param>
        /// <param name="shouldClientLoadNormally">Should the clients load this additively too or load it full normal scene change.</param>
        void ServerLoadSceneAdditively(string scenePath, INetworkPlayer[] players, bool shouldClientLoadNormally = false);

        /// <summary>
        ///     Allows server to fully unload a scene additively.
        /// </summary>
        /// <param name="scene">The scene handle which we want to unload additively.</param>
        /// <param name="players">List of player's that are receiving the new scene unload.</param>
        /// <param name="shouldClientUnloadNormally">Should the clients unload this additively too or unload it full normal scene change.</param>
        void ServerUnloadSceneAdditively(Scene scene, INetworkPlayer[] players, bool shouldClientUnloadNormally = false);
    }
}
