using System;
using System.Collections;
using Mirror.Tcp;
using Mirror.KCP;
using UnityEngine;

namespace Mirror.HeadlessBenchmark
{
    public class HeadlessBenchmark : MonoBehaviour
    {
        public NetworkManager networkManager;
        public GameObject MonsterPrefab;
        public GameObject PlayerPrefab;
        public string editorArgs;

        string[] cachedArgs;

        void Start()
        {
            cachedArgs = Environment.GetCommandLineArgs();

#if UNITY_EDITOR
            cachedArgs = editorArgs.Split(' ');
#endif

            StartCoroutine(HeadlessStart());
        }

        IEnumerator HeadlessStart()
        {
            //Try to find port
            string port = GetArgValue("-port");

            //Try to find Transport
            string transport = GetArgValue("-transport");
            if (!string.IsNullOrEmpty(transport))
            {
                if (transport.Equals("tcp"))
                {
                    TcpTransport newTransport = networkManager.gameObject.AddComponent<TcpTransport>();

                    //Try to apply port if exists and needed by transport.
                    if (!string.IsNullOrEmpty(port))
                    {
                        newTransport.Port = int.Parse(port);
                    }

                    networkManager.server.transport = newTransport;
                    networkManager.client.Transport = newTransport;
                }
                if (transport.Equals("kcp"))
                {
                    KcpTransport newTransport = networkManager.gameObject.AddComponent<KcpTransport>();

                    //Try to apply port if exists and needed by transport.
                if (!string.IsNullOrEmpty(port))
                    {
                        newTransport.Port = ushort.Parse(port);
                    }
                    networkManager.server.transport = newTransport;
                    networkManager.client.Transport = newTransport;
                }
            }

            //Server mode?
            if (!string.IsNullOrEmpty(GetArg("-server")))
            {
                networkManager.server.Started.AddListener(OnServerStarted);
                networkManager.server.Authenticated.AddListener(conn => networkManager.server.SetClientReady(conn));
                _ = networkManager.server.ListenAsync();
                Debug.Log("Starting Server Only Mode");
            }

            //Or client mode?
            string client = GetArg("-client");
            if (!string.IsNullOrEmpty(client))
            {
                //string address;
                //network address provided?
                string address = GetArgValue("-address");
                if (string.IsNullOrEmpty(address))
                {
                    address = "localhost";
                }

                //nested clients
                int clonesCount = 1;
                string clonesString = GetArgValue("-client");
                if (!string.IsNullOrEmpty(clonesString))
                {
                    clonesCount = int.Parse(clonesString);
                }

                Debug.Log("Starting " + clonesCount + " Clients");

                // connect from a bunch of clients
                for (int i = 0; i < clonesCount; i++)
                    yield return StartClient(i, networkManager.client.Transport, address);
            }

            if (!string.IsNullOrEmpty(GetArg("-help")))
            {
                Debug.Log("--==MirrorNG HeadlessClients Benchmark==--");
                Debug.Log("Please start your standalone application with the -nographics and -batchmode options");
                Debug.Log("Also provide these arguments to control the autostart process:");
                Debug.Log("-server (will run in server only mode)");
                Debug.Log("-client 1234 (will run the specified number of clients)");
                Debug.Log("-transport tcp (transport to be used in test. add more by editing HeadlessBenchmark.cs)");
                Debug.Log("-address example.com (will run the specified number of clients)");
                Debug.Log("-port 1234 (port used by transport)");
                Debug.Log("-monster 100 (number of monsters to spawn on the server)");
            }
        }

        void OnServerStarted()
        {
            string monster = GetArgValue("-monster");
            if (!string.IsNullOrEmpty(monster))
            {
                for (int i = 0; i < int.Parse(monster); i++)
                    SpawnMonsters(i);
            }
        }

        void SpawnMonsters(int i)
        {
            GameObject monster = Instantiate(MonsterPrefab);
            monster.gameObject.name = $"Monster {i}";
            networkManager.server.Spawn(monster.gameObject);
        }

        IEnumerator StartClient(int i, Transport transport, string networkAddress)
        {
            var clientGo = new GameObject($"Client {i}", typeof(NetworkClient));
            NetworkClient client = clientGo.GetComponent<NetworkClient>();
            client.Transport = transport;

            client.RegisterPrefab(MonsterPrefab);
            client.RegisterPrefab(PlayerPrefab);
            client.ConnectAsync(networkAddress);
            while (!client.IsConnected)
                yield return null;

            client.Send(new AddPlayerMessage());
        }

        string GetArgValue(string name)
        {
            for (int i = 0; i < cachedArgs.Length; i++)
            {
                if (cachedArgs[i] == name && cachedArgs.Length > i + 1)
                {
                    return cachedArgs[i + 1];
                }
            }
            return null;
        }

        string GetArg(string name)
        {
            for (int i = 0; i < cachedArgs.Length; i++)
            {
                if (cachedArgs[i] == name)
                {
                    return cachedArgs[i];
                }
            }
            return null;
        }
    }
}
