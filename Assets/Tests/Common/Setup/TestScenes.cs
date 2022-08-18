using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Tests
{
    public class TestScenes
    {
        public const string Path = "Assets/Tests/Scenes/testScene.unity";
        public const string StartScene = "Assets/Tests/Scenes/editmodeStartScene.unity";

        // todo this is so hacker, maybe these should be editor tests instead and stop/start play mode each time
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

            // maybe test unload scene it self, if so wait for it to finish
            await AsyncUtil.WaitUntilWithTimeout(() => SceneManager.sceneCount == 1);

            Debug.Assert(SceneManager.sceneCount == 1, "Should have unloaded all expect 1 scene");
        }
    }
}
