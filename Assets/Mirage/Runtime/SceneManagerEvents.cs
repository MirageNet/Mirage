using System;
using System.Collections.Generic;
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
    /// Event fires from <see cref="INetworkSceneManager">INetworkSceneManager</see> when a scene change finishes on either Server or Client.
    /// <para>Scene - Loaded scene</para>
    /// <para>SceneOperation - Scene change type (Normal, Additive Load, Additive Unload).</para>
    /// </summary>
    [Serializable] public class SceneChangeFinishedEvent : UnityEvent<Scene, SceneOperation> { }

    /// <summary>
    /// Event fires from <see cref="INetworkSceneManager">INetworkSceneManager</see> when a scene change begins on either Server or Client.
    /// <para>Scene - Name or path of the scene that's about to be loaded</para>
    /// <para>SceneOperation - Scene change type (Normal, Additive Load, Additive Unload).</para>
    /// </summary>
    [Serializable] public class SceneChangeStartedEvent : UnityEvent<string, SceneOperation> { }

    [Serializable] public class PlayerSceneChangeEvent : UnityEvent<INetworkPlayer> { }
}
