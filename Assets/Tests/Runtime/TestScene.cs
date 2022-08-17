using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Tests.Runtime
{
    public class TestScene
    {
        public const string Path = "Assets/Tests/Runtime/Scenes/testScene.unity";

        public static async UniTask UnloadAdditiveScenes()
        {
            var active = SceneManager.GetActiveScene();

            // get all scenes and add to list first, so that because sceneCount will change as scene are unloaded
            var toUnload = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (active == scene) { continue; }

                toUnload.Add(scene);
            }

            foreach (var scene in toUnload)
            {
                var op = SceneManager.UnloadSceneAsync(scene);
                if (op != null)
                    await op;
            }

            Debug.Assert(SceneManager.sceneCount == 1, "Should have unloaded all expect 1 scene");
        }
    }
}
