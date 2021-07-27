using System;
using System.Collections;
using System.Collections.Generic;
using JamesFrowen.NetworkingBenchmark;
using Mirage.Sockets.Udp;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Experimental.StateSyncVar
{
    public class DemoStateTransferManager : MonoBehaviour
    {
        [SerializeField] int playerCount = 20;
        [SerializeField] int monsterCount = 100;
        private GameObject serverInstance;
        private NetworkServer server;
        private GameObject clientInstance;
        private NetworkClient client;

        private ServerObjectManager som;
        private ClientObjectManager com;

        private Scene serverScene;
        private Scene clientScene;

        private PhysicsScene? serverPhysicsScene;

        Guid playerSpawnId;
        Guid monsterSpawnId;
        uint serverNetId = 0;
        uint monsterNameIndex = 0;

        List<DemoNetworkIdentity> serverObjects = new List<DemoNetworkIdentity>();
        Dictionary<uint, DemoNetworkIdentity> clientObjects = new Dictionary<uint, DemoNetworkIdentity>();

        IEnumerator Start()
        {
            {
                serverInstance = new GameObject("Server");
                serverInstance.transform.parent = transform;
                server = serverInstance.AddComponent<NetworkServer>();
                server.SocketFactory = serverInstance.AddComponent<UdpSocketFactory>();
                server.EnablePeerMetrics = true;
                som = serverInstance.AddComponent<ServerObjectManager>();
                som.Server = server;

                DisplayMetrics_AverageGui display = serverInstance.AddComponent<DisplayMetrics_AverageGui>();
                server.Started.AddListener(() => display.Metrics = server.Metrics);
            }

            {
                clientInstance = new GameObject("Client");
                clientInstance.transform.parent = transform;
                client = clientInstance.AddComponent<NetworkClient>();
                client.SocketFactory = clientInstance.AddComponent<UdpSocketFactory>();
                client.EnablePeerMetrics = true;
                com = clientInstance.AddComponent<ClientObjectManager>();
                com.Client = client;



                //DisplayMetrics_AverageGui display = clientInstance.AddComponent<DisplayMetrics_AverageGui>();
                //client.Connected.AddListener((_) => display.Metrics = client.Metrics);

            }



            yield return null;
            playerSpawnId = Guid.NewGuid();
            monsterSpawnId = Guid.NewGuid();

            server.StartServer();

            var playerGUID = Guid.NewGuid();
            server.Connected.AddListener(player =>
            {
                var go = new GameObject();
                go.AddComponent<NetworkIdentity>();
                som.AddCharacter(player, go, playerGUID);
            });

            yield return null;
            spawnServerObjects();

            yield return null;

            createClientScene();
            com.RegisterSpawnHandler(playerSpawnId, (_) =>
            {
                NetworkIdentity netId = CreatePlayer().GetComponent<NetworkIdentity>();
                SceneManager.MoveGameObjectToScene(netId.gameObject, clientScene);
                return netId;
            }, (identity) => Destroy(identity.gameObject));
            com.RegisterSpawnHandler(monsterSpawnId, (_) =>
            {
                NetworkIdentity netId = CreateMonster().GetComponent<NetworkIdentity>();
                SceneManager.MoveGameObjectToScene(netId.gameObject, clientScene);
                return netId;
            }, (identity) => Destroy(identity.gameObject));
            com.RegisterSpawnHandler(playerGUID, (_) =>
            {
                return new GameObject().AddComponent<NetworkIdentity>();
            }, (identity) => Destroy(identity.gameObject));
            client.Connect();

            yield return null;

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

        private void FixedUpdate()
        {
            serverPhysicsScene?.Simulate(Time.fixedDeltaTime);
        }

        private void spawnServerObjects()
        {
            serverScene = SceneManager.CreateScene("ServerScene", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });
            serverPhysicsScene = serverScene.GetPhysicsScene();
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
        private void createClientScene()
        {
            clientScene = SceneManager.CreateScene("ClientScene", new CreateSceneParameters { localPhysicsMode = LocalPhysicsMode.Physics3D });
        }


        private GameObject CreatePlayer()
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clone.name = $"Player (unspawned)";
            Renderer renderer = clone.GetComponent<Renderer>();
            renderer.material.color = Color.blue;

            clone.AddComponent<NetworkIdentity>();
            DemoNetworkIdentity identity = clone.AddComponent<DemoNetworkIdentity>();
            DemoPlayer player = clone.AddComponent<DemoPlayer>();
            player.syncInterval = 0;
            DemoNetworkTransform netTransform = clone.AddComponent<DemoNetworkTransform>();
            netTransform.syncInterval = 0;
            DemoHealth health = clone.AddComponent<DemoHealth>();
            health.syncInterval = 0;
            Rigidbody rb = clone.AddComponent<Rigidbody>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.player = player;

            return clone;
        }
        private GameObject CreateMonster()
        {
            var clone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            clone.name = $"Monster (unspawned)";
            Renderer renderer = clone.GetComponent<Renderer>();
            renderer.material.color = Color.blue;

            clone.AddComponent<NetworkIdentity>();
            DemoNetworkIdentity identity = clone.AddComponent<DemoNetworkIdentity>();
            DemoNetworkTransform netTransform = clone.AddComponent<DemoNetworkTransform>();
            netTransform.syncInterval = 0;
            DemoHealth health = clone.AddComponent<DemoHealth>();
            health.syncInterval = 0;
            Rigidbody rb = clone.AddComponent<Rigidbody>();
            DemoMonster monster = clone.AddComponent<DemoMonster>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.monster = monster;

            return clone;
        }

        private void SpawnServerPlayer(ref uint netid, Guid spawnId, int i)
        {
            GameObject clone = CreatePlayer();
            SceneManager.MoveGameObjectToScene(clone, serverScene);
            clone.name = $"Player {i}";
            DemoNetworkIdentity identity = clone.GetComponent<DemoNetworkIdentity>();
            serverObjects.Add(identity);
            netid++;
            identity.Init(netid);

            Renderer renderer = clone.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            DemoPlayer player = clone.GetComponent<DemoPlayer>();
            DemoNetworkTransform netTransform = clone.GetComponent<DemoNetworkTransform>();
            DemoHealth health = clone.GetComponent<DemoHealth>();

            identity.health = health;
            identity.networkTransform = netTransform;
            identity.player = player;

            health.Health = 20;

            player.Damage = 1;
            player.Money = 0;

            SphereCollider collider = clone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 2;
            Rigidbody rb = clone.GetComponent<Rigidbody>();

            rb.isKinematic = true;

            netTransform.StartAutoMove(25);

            som.Spawn(clone, spawnId);
        }

        private void SpawnServerMonster(ref uint netid, Guid spawnId)
        {
            GameObject clone = CreateMonster();
            SceneManager.MoveGameObjectToScene(clone, serverScene);
            clone.name = $"Monster {monsterNameIndex++}";
            DemoNetworkIdentity identity = clone.GetComponent<DemoNetworkIdentity>();
            serverObjects.Add(identity);
            netid++;
            identity.Init(netid);

            Renderer renderer = clone.GetComponent<Renderer>();
            renderer.material.color = Color.red;

            DemoNetworkTransform netTransform = clone.GetComponent<DemoNetworkTransform>();
            DemoHealth health = clone.GetComponent<DemoHealth>();

            identity.health = health;
            identity.networkTransform = netTransform;

            health.Health = 20;

            Rigidbody rb = clone.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            netTransform.StartAutoMove(25, 1.5f);

            som.Spawn(clone, spawnId);
        }
    }
    public class DemoMonster : MonoBehaviour
    {

    }
}
