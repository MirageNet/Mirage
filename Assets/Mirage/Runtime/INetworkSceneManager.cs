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

    [Serializable] public class ClientSceneChangeEvent : UnityEvent<string, SceneOperation> { }

    public interface INetworkSceneManager
    {
        void ChangeServerScene(string scenePath, SceneOperation sceneOperation = SceneOperation.Normal);
    }
}
