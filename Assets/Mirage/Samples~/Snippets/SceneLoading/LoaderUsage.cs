using UnityEngine;
using Mirage;
using Mirage.Components;
using Cysharp.Threading.Tasks;

namespace Mirage.Snippets.SceneLoading
{
    // CodeEmbed-Start: loader-usage
    public class MyGameManager : MonoBehaviour
    {
        public NetworkSceneLoader SceneLoader;

        public void StartGame()
        {
            SceneLoader.ServerLoadScene("BattleMap").Forget();
        }
    }
    // CodeEmbed-End: loader-usage
}
