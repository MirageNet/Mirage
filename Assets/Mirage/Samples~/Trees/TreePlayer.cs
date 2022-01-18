using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Examples.Trees
{
    public class TreePlayer : NetworkBehaviour
    {
        public float speed;

        [HasAuthority(error = false)]
        private void Update()
        {
            move();
            attack();
        }
        private void Awake()
        {
            TreeSpawner.start = true;
        }

        private void move()
        {
            // rotate
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 dir = new Vector3(horizontal, 0, vertical).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }

        static List<Tree> cache = new List<Tree>();

        private void attack()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3 pos = transform.position;
                foreach (NetworkIdentity identity in Client.World.SpawnedIdentities)
                {
                    if (identity.TryGetComponent(out Tree tree))
                    {
                        if (Vector3.Distance(tree.transform.position, pos) < 10)
                        {
                            cache.Add(tree);
                        }
                    }
                }
                AttackTrees(this, cache.ToArray());
                cache.Clear();
            }
        }

        [ServerRpc]
        // treePlayer in this rpc is for debug
        void AttackTrees(TreePlayer treePlayer, Tree[] trees)
        {
            Debug.Log($"{treePlayer.NetId} attacks {trees.Length} Trees");
            foreach (Tree tree in trees)
            {
                tree.health -= 5;
            }
        }
    }
}
