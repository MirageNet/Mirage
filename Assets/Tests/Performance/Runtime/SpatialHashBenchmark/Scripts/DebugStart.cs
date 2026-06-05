using System.Collections;
using System.Collections.Generic;
using Mirage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    public class DebugStart : MonoBehaviour
    {
        public NetworkIdentity prefab2;
        public bool DebugGUI;

        [Scene] public string scene;
        public NetworkManager prefab;
        public int CreateClientCount;
        private Dictionary<NetworkManager, (Scene scene, PhysicsScene physics)> scenes = new Dictionary<NetworkManager, (Scene scene, PhysicsScene physics)>();

        private NetworkManager serverManager;
        private List<NetworkManager> clientManagers = new List<NetworkManager>();
        private Vector2 scrollPosition;

        private IEnumerator Start()
        {
            Physics.autoSimulation = false;
            yield return createServer();
            yield return null;
            for (var i = 0; i < CreateClientCount; i++)
            {
                yield return createClient();
            }
        }

        private IEnumerator createServer()
        {
            Debug.Log("Create Server");
            yield return SceneManager.LoadSceneAsync(this.scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            var camera = FindObjectOfType<Camera>();
            var light = FindObjectOfType<Light>();
            if (camera != null)
                Destroy(camera.gameObject);
            if (light != null)
                Destroy(light.gameObject);
            var manager = Instantiate(prefab);
            scenes.Add(manager, (scene, scene.GetPhysicsScene()));
            serverManager = manager;
            var gui = manager.GetComponent<NetworkManagerGUI>();
            if (gui != null) gui.enabled = false;
            SceneManager.MoveGameObjectToScene(manager.gameObject, scene);
            var server = manager.Server;
            server.StartServer();
            server.World.onSpawn += (identity) => MoveIfNotChild(identity.gameObject, scene);
            server.World.onSpawn += (identity) => disableOnServer(identity);
            foreach (var identity in server.World.SpawnedIdentities) { MoveIfNotChild(identity.gameObject, scene); disableOnServer(identity); }
            yield return null;
        }

        private void disableOnServer(NetworkIdentity identity)
        {
            foreach (var renderer in identity.gameObject.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }

        private IEnumerator createClient()
        {
            Debug.Log($"Creating Client {clientManagers.Count + 1}");
            yield return SceneManager.LoadSceneAsync(this.scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            var cameras = FindObjectsOfType<Camera>();
            var lights = FindObjectsOfType<Light>();
            for (var i = 1; i < cameras.Length; i++) { Destroy(cameras[i].gameObject); }
            for (var i = 1; i < lights.Length; i++) { Destroy(lights[i].gameObject); }

            var manager = Instantiate(prefab);
            scenes.Add(manager, (scene, scene.GetPhysicsScene()));
            clientManagers.Add(manager);
            var gui = manager.GetComponent<NetworkManagerGUI>();
            if (gui != null) gui.enabled = false;
            SceneManager.MoveGameObjectToScene(manager.gameObject, scene);
            var client = manager.Client;
            client.Connect();
            client.World.onSpawn += (identity) => MoveIfNotChild(identity.gameObject, scene);
            foreach (var identity in client.World.SpawnedIdentities)
                MoveIfNotChild(identity.gameObject, scene);
            yield return null;
        }

        private void DestroyClient(NetworkManager manager)
        {
            var scene = scenes[manager];
            _ = SceneManager.UnloadSceneAsync(scene.scene);

            scenes.Remove(manager);
            GameObject.Destroy(manager);
        }

        private void MoveIfNotChild(GameObject target, Scene scene)
        {
            if (target.transform.parent == null)
            {
                SceneManager.MoveGameObjectToScene(target, scene);
            }
        }

        private void FixedUpdate()
        {
            foreach (var scene in scenes.Values)
            {
                scene.physics.Simulate(Time.fixedDeltaTime);
            }
        }

        private void OnGUI()
        {
            if (!DebugGUI)
                return;

            if (serverManager != null)
            {
                var server = serverManager.Server;
                GUILayout.Label($"Server Active:{server.Active}");
                GUILayout.Label($"Players:{server.AllPlayers.Count}");
                GUILayout.Label($"Buffer: {server.PeerPoolMetrics}");

                ToggleGameObject(serverManager.gameObject);
            }
            else
            {
                GUILayout.Label("No Server");
            }


            GUILayout.Space(20);

            using (var scroll = new GUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;

                if (GUILayout.Button($"Create New Client"))
                {
                    StartCoroutine(createClient());
                }

                if (GUILayout.Button($"Try Lag out"))
                {
                    StartCoroutine(Lagger());
                }
                if (GUILayout.Button($"Try Lag out Long"))
                {
                    StartCoroutine(LaggerLong());
                }
                if (GUILayout.Button($"Remove all"))
                {

                }


                GUILayout.Label($"Client Count:{clientManagers.Count}");
                for (var i = 0; i < clientManagers.Count; i++)
                {
                    var clientManager = clientManagers[i];
                    var client = clientManager.Client;

                    GUILayout.Label($"[{i}] Client Active:{client.Active} Connected:{client.IsConnected}");
                    GUILayout.Label($"Buffer: {client.PeerPoolMetrics}");

                    if (client != null)
                        ToggleGameObject(client.gameObject);

                    if (GUILayout.Button($"Destroy Client"))
                    {
                        DestroyClient(clientManager);
                        clientManagers.RemoveAt(i);
                        i--;
                    }
                    GUILayout.Space(10);
                }
            }
        }

        private void RemoveAllClients()
        {
            for (var i = 0; i < clientManagers.Count; i++)
            {
                var client = clientManagers[i];
                DestroyClient(client);
                clientManagers.RemoveAt(i);
                i--;
            }
        }

        private IEnumerator Lagger()
        {
            for (var i = 0; i < 20; i++)
            {
                yield return createClient();
            }

            yield return new WaitForSeconds(2);

            foreach (var clientManager in clientManagers)
            {
                clientManager.gameObject.SetActive(false);
            }
        }

        private IEnumerator LaggerLong()
        {
            while (true)
            {
                for (var i = 0; i < 20; i++)
                {
                    yield return createClient();
                }

                yield return new WaitForSeconds(2);

                foreach (var clientManager in clientManagers)
                {
                    clientManager.gameObject.SetActive(false);
                }

                yield return new WaitForSeconds(20);
                RemoveAllClients();
                yield return new WaitForSeconds(5);
            }
        }

        private static void ToggleGameObject(GameObject go)
        {
            if (GUILayout.Button($"{(go.activeSelf ? "Disable" : "Enable")} GameObject"))
            {
                go.SetActive(!go.activeSelf);
            }
        }
    }
}
