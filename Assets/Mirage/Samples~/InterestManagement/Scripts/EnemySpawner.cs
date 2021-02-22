using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mirage.Examples.InterestManagement
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity _enemyPrefab;
        [SerializeField] private int _numberOfEnemiesSpawn = 100;
        [SerializeField] private ServerObjectManager _serverObject;
        [SerializeField] private Transform _plane;

        private float _planeX, _planeZ;

        private void Awake()
        {
            _serverObject.Server.Started.AddListener(OnStartServer);

            Mesh mesh = _plane.GetComponent<MeshFilter>().mesh;

            _planeX = (mesh.bounds.size.x / 2) * _plane.localScale.x;
            _planeZ = (mesh.bounds.size.z / 2) * _plane.localScale.z;
        }

        private void OnStartServer()
        {
            StartCoroutine(nameof(SpawnEnemies));
        }

        private IEnumerator SpawnEnemies()
        {
            var spawned = 0;

            for (int i = 0; i < _numberOfEnemiesSpawn; i++)
            {
                float xRand = Random.Range(-_planeX, _planeX);
                float zRand = Random.Range(-_planeZ, _planeZ);

                NetworkIdentity enemy = Instantiate(_enemyPrefab, new Vector3(xRand, 1, zRand), Quaternion.identity);

                _serverObject.Spawn(enemy);

                spawned++;

                if(spawned == 100)
                {
                    yield return new WaitForEndOfFrame();

                    spawned = 0;
                }
            }
        }
    }
}
