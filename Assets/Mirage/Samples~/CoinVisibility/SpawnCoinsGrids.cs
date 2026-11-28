using Cysharp.Threading.Tasks;
using Mirage;
using UnityEngine;

namespace Examples.SpatialHash
{
    public class SpawnCoinsGrids : MonoBehaviour
    {
        public GameObject CoinPrefab;
        public NetworkServer Server;
        public ServerObjectManager ServerObjectManager;

        public float gridSize;
        public Vector2 gridExtends;

        public void Awake()
        {
            Server.Started.AddListener(UniTask.UnityAction(ServerStarted));
        }

        private async UniTaskVoid ServerStarted()
        {
            await UniTask.Yield();

            for (float x = 0; x < gridExtends.x; x += gridSize)
            {
                for (float y = 0; y < gridExtends.y; y += gridSize)
                {
                    float halfGrid = gridSize / 2;
                    // spawn in all 4 quadrants
                    SpawnCoin(x + halfGrid, y + halfGrid);
                    SpawnCoin(x + halfGrid, -y - halfGrid);
                    SpawnCoin(-x - halfGrid, y + halfGrid);
                    SpawnCoin(-x - halfGrid, -y - halfGrid);
                }
            }
        }

        private void SpawnCoin(float x, float y)
        {
            GameObject clone = Instantiate(CoinPrefab);
            clone.transform.position = new Vector3(x, 0, y);

            ServerObjectManager.Spawn(clone);
        }
    }
}
