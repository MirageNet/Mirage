using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Tests.Runtime
{
    public class TestScene
    {
        public const string Path = "Assets/Tests/Runtime/Scenes/testScene.unity";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // todo try removing this
#if UNITY_EDITOR
            Debug.Log("[TestScene] Reimport");
            UnityEditor.AssetDatabase.Refresh();

            // re-import scene to ensure that it is working correctly in CI
            UnityEditor.AssetDatabase.ImportAsset(Path, UnityEditor.ImportAssetOptions.ForceUpdate);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

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
