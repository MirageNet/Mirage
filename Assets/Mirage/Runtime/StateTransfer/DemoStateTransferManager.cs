using System;
using System.Collections;
using System.Collections.Generic;
using Mirage.Sockets.Udp;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Experimental
{
    public class DemoStateTransferManager : MonoBehaviour
    {
        [SerializeField] int playerCount = 20;
        [SerializeField] int monsterCount = 100;

        private NetworkServer server;
        private ServerObjectManager som;
        private NetworkClient client;
        private ClientObjectManager com;
        private UdpSocketFactory socketFactory;
        private Scene serverScene;
        private Scene clientScene;
        private StateTransfer serverStateTranfer;
        private StateTransfer clientStateTranfer;

        Dictionary<uint, Func<GameObject>> clientSpawnDictionary = new Dictionary<uint, Func<GameObject>>();
        uint playerSpawnId;
        uint monsterSpawnId;
        uint serverNetId = 0;
        uint monsterNameIndex = 0;

        List<DemoNetworkIdentity> serverObjects = new List<DemoNetworkIdentity>();
        Dictionary<uint, DemoNetworkIdentity> clientObjects = new Dictionary<uint, DemoNetworkIdentity>();

        IEnumerator Start()
        {
            server = gameObject.AddComponent<NetworkServer>();
            som = gameObject.AddComponent<ServerObjectManager>();
            client = gameObject.AddComponent<NetworkClient>();
            com = gameObject.AddComponent<ClientObjectManager>();

            socketFactory = gameObject.AddComponent<UdpSocketFactory>();
            server.SocketFactory = socketFactory;
            client.SocketFactory = socketFactory;

            server.EnablePeerMetrics = true;
            client.EnablePeerMetrics = true;

            yield return null;
            playerSpawnId = GetRandomSpawnId();
            monsterSpawnId = GetRandomSpawnId();
            clientSpawnDictionary.Clear();
            clientSpawnDictionary.Add(playerSpawnId, CreatePlayer);
            clientSpawnDictionary.Add(monsterSpawnId, CreateMonster);


            server.StartServer();
            spawnServerObjects();

            yield return null;

            client.Connect();
            spawnClientObjects();

            yield return null;

            serverStateTranfer = StateTransfer.Create(server, serverObjects);
            clientStateTranfer = StateTransfer.Create(client, clientObjects, clientSpawnDictionary);

            while (true)
            {
                yield return new WaitForSeconds(1);

                for (int i = serverObjects.Count - 1; i >= 0; i--)
                {
                    if (serverObjects[i] == null)
                    {
                        // remove and respawn monster
                        serverObjects.RemoveAt(i);
                        SpawnServerMonster(ref serverNetId, monsterSpawnId);
                    }
                }
            }
        }


        private void spawnServerObjects()
        {
            serverScene = SceneManager.CreateScene("ServerScene", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });

            serverObjects.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                SpawnServerPlayer(ref serverNetId, playerSpawnId, i);
            }

            for (int i = 0; i < monsterCount; i++)
            {
                SpawnServerMonster(ref serverNetId, monsterSpawnId);
            }
        }
        private void spawnClientObjects()
        {
            clientScene = SceneManager.CreateScene("ClientScene", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });
        }


        private uint GetRandomSpawnId()
        {
            return (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        private GameObject CreatePlayer()
        {
            var clone = new GameObject($"Player (unspawned)");
            DemoNetworkIdentity identity = clone.AddComponent<DemoNetworkIdentity>();
            DemoPlayer player = clone.AddComponent<DemoPlayer>();
            DemoNetworkTransform netTransform = clone.AddComponent<DemoNetworkTransform>();
            DemoHealth health = clone.AddComponent<DemoHealth>();
            SphereCollider collider = clone.AddComponent<SphereCollider>();
            Rigidbody rb = clone.AddComponent<Rigidbody>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.player = player;

            return clone;
        }
        private GameObject CreateMonster()
        {
            var clone = new GameObject($"Monster (unspawned)");
            DemoNetworkIdentity identity = clone.AddComponent<DemoNetworkIdentity>();
            DemoNetworkTransform netTransform = clone.AddComponent<DemoNetworkTransform>();
            DemoHealth health = clone.AddComponent<DemoHealth>();
            SphereCollider collider = clone.AddComponent<SphereCollider>();
            Rigidbody rb = clone.AddComponent<Rigidbody>();
            DemoMonster monster = clone.AddComponent<DemoMonster>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.monster = monster;

            return clone;
        }

        private void SpawnServerPlayer(ref uint netid, uint spawnId, int i)
        {
            GameObject clone = CreatePlayer();
            SceneManager.MoveGameObjectToScene(clone, serverScene);
            clone.name = $"Player {i}";
            DemoNetworkIdentity identity = clone.GetComponent<DemoNetworkIdentity>();
            serverObjects.Add(identity);
            netid++;
            identity.Init(netid, spawnId);

            DemoPlayer player = clone.GetComponent<DemoPlayer>();
            DemoNetworkTransform netTransform = clone.GetComponent<DemoNetworkTransform>();
            DemoHealth health = clone.GetComponent<DemoHealth>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.player = player;

            health.Health = 20;

            player.Damage = 1;
            player.Money = 0;

            SphereCollider collider = clone.GetComponent<SphereCollider>();
            Rigidbody rb = clone.GetComponent<Rigidbody>();
            collider.isTrigger = true;
            collider.radius = 2;

            rb.isKinematic = true;

            netTransform.StartAutoMove(50);
        }

        private void SpawnServerMonster(ref uint netid, uint spawnId)
        {
            GameObject clone = CreateMonster();
            SceneManager.MoveGameObjectToScene(clone, serverScene);
            clone.name = $"Monster {monsterNameIndex++}";
            DemoNetworkIdentity identity = clone.AddComponent<DemoNetworkIdentity>();
            serverObjects.Add(identity);
            netid++;
            identity.Init(netid, spawnId);

            DemoNetworkTransform netTransform = clone.AddComponent<DemoNetworkTransform>();
            DemoHealth health = clone.AddComponent<DemoHealth>();

            identity.health = health;
            identity.networkTransform = netTransform;

            health.Health = 20;

            SphereCollider collider = clone.AddComponent<SphereCollider>();
            Rigidbody rb = clone.AddComponent<Rigidbody>();
            collider.isTrigger = true;
            collider.radius = 2;

            rb.isKinematic = true;

            netTransform.StartAutoMove(50, 1.5f);
        }
    }
    public class DemoMonster : MonoBehaviour
    {

    }
}
