using System.Threading.Tasks;
using UnityEngine;

namespace Mirage.Examples.Trees
{
    public class TreeSpawner : MonoBehaviour
    {
        public static bool start = false;

        public GameObject Prefab;
        public int Count;
        public Vector2 Box;

        public int maxSpawnPerFrame;

        private void Awake()
        {
            NetworkServer server = GetComponent<NetworkServer>();
            server.Started.AddListener(ServerStarted);
            start = false;
        }

        private async void ServerStarted()
        {
            while (!start)
                await Task.Yield();

            ServerObjectManager som = GetComponent<ServerObjectManager>();
            // get id reference so we save on GetComponent for each prefab
            NetworkIdentity NI_Prefab = Prefab.GetComponent<NetworkIdentity>();

            // limit spawning pre frame so we dont overload network when spawning a lot
            int thisFrame = 0;
            for (int i = 0; i < Count; i++)
            {
                var pos = default(Vector3);
                pos.x = Random.Range(-Box.x, Box.x);
                pos.z = Random.Range(-Box.y, Box.y);

                NetworkIdentity clone = Instantiate(NI_Prefab, pos, Quaternion.identity);
                clone.name = $"Tree {i}";
                som.Spawn(clone);

                thisFrame++;
                if (thisFrame > maxSpawnPerFrame)
                {
                    thisFrame = 0;
                    Debug.Log($"Spawned {i}");
                    await Task.Yield();
                }
            }

            Debug.Log($"Spawned {Count}");
        }
    }
}
