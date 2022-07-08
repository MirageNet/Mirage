using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime
{
    public class TestScene
    {
        public const string Path = "Assets/Tests/Runtime/Scenes/testScene.unity";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if UNITY_EDITOR
            Debug.Log("[TestScene] Reimport");
            UnityEditor.AssetDatabase.Refresh();

            // re-import scene to ensure that it is working correctly in CI
            UnityEditor.AssetDatabase.ImportAsset(Path, UnityEditor.ImportAssetOptions.ForceUpdate);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
