using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Tests.Runtime
{
    public class TestScene
    {
        public const string Path = "Assets/Tests/Runtime/Scenes/testScene.unity";

        // todo this is so hacker, maybe these should be editor tests instead and stop/start play mode each time
        public static async UniTask UnloadAdditiveScenes()
        {
            // wait for all to load first, otherwise we will have problems unloading them
            if (SceneManager.sceneCount > 1)
                await WaitForAllToLoad();

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

        private static async Task WaitForAllToLoad()
        {
            // start true so we enter first loop
            var notLoaded = true;
            while (notLoaded)
            {
                // set to false till we see a scene that is not loaded
                notLoaded = false;

                // check all scenes to see if any are still loading
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded)
                    {
                        notLoaded = true;
                        Debug.Log($"{i} ({scene.name}) not loaded...waiting");
                        await AsyncUtil.WaitUntilWithTimeout(() => scene.isLoaded);
                    }
                }

                // once we are done, go back to start incase scene was unloaded and scene count was effected
            }
        }
    }
}
