using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Mirage.Tests.Runtime
{
    public class TestScene
    {
        public const string Path = "Assets/Tests/Runtime/Scenes/testScene.unity";

        public static async UniTask UnloadAdditiveScenes()
        {
            var active = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (active == scene) { continue; }
                var op = SceneManager.UnloadSceneAsync(scene);
                if (op != null)
                    await op;
            }
        }
    }
}
