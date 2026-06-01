using Mirage;
using UnityEngine;

namespace Mirage.Snippets.GameObjects
{
    public class SpawningExample : MonoBehaviour
    {
        public GameObject boxPrefab;
        public ServerObjectManager ServerObjectManager;
        public GameObject treePrefab;

        public void SpawnBox()
        {
            // CodeEmbed-Start: spawn-box-example
            var boxGo = Instantiate(boxPrefab);
            ServerObjectManager.Spawn(boxGo);
            // CodeEmbed-End: spawn-box-example
        }

        // CodeEmbed-Start: spawn-trees-example
        void SpawnTrees()
        {
            int x = 0;
            for (int i = 0; i < 5; ++i)
            {
                GameObject treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
                Tree tree = treeGo.GetComponent<Tree>();
                tree.numLeaves = Random.Range(10, 200);
                Debug.Log("Spawning leaf with leaf count " + tree.numLeaves);
                ServerObjectManager.Spawn(treeGo);
            }
        }
        // CodeEmbed-End: spawn-trees-example

        // CodeEmbed-Start: spawn-trees-authority-example
        void SpawnTrees(INetworkPlayer player)
        {
            int x = 0;
            for (int i = 0; i < 5; ++i)
            {
                GameObject treeGo = Instantiate(treePrefab, new Vector3(x++, 0, 0), Quaternion.identity);
                Tree tree = treeGo.GetComponent<Tree>();
                tree.numLeaves = Random.Range(10, 200);
                Debug.Log("Spawning leaf with leaf count " + tree.numLeaves);
                ServerObjectManager.Spawn(treeGo, player);
            }
        }
        // CodeEmbed-End: spawn-trees-authority-example
    }
}
