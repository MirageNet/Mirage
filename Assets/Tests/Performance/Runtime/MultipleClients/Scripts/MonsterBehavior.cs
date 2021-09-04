using System.Collections;
using UnityEngine;

namespace Mirage.Tests.Performance.Runtime
{
    public class MonsterBehavior : NetworkBehaviour
    {
        [SyncVar]
        public Vector3 position;

        [SyncVar]
        public int MonsterId;

        public void Awake()
        {
            Identity.OnStartServer.AddListener(StartServer);
            Identity.OnStopServer.AddListener(StopServer);
        }

        private void StopServer()
        {
            StopAllCoroutines();
        }

        private void StartServer()
        {
            StartCoroutine(MoveMonster());
        }

        private IEnumerator MoveMonster()
        {
            while (true)
            {
                position = Random.insideUnitSphere;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}