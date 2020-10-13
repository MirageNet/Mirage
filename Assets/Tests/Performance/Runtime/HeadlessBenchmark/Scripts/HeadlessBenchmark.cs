using System;
using System.Collections;
using Mirror.Tcp;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mirror.Test.Performance.Runtime.HeadlessBenchmark
{
    public class HeadlessBenchmark : MonoBehaviour
    {
        public NetworkManager networkManager;
        public GameObject MonsterPrefab;
        public GameObject PlayerPrefab;

        //Used for testing in editor
        public bool debugMode;
        public string debugArgs;

        string[] cachedArgs;

        void Start()
        {
            SetArgs();

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || debugMode)
            {
                StartCoroutine(HeadlessStart());
            }
        }

        void SetArgs()
        {
            if (debugMode)
            {
                cachedArgs = debugArgs.Split(' ');
            }
            else
            {
                cachedArgs = Environment.GetCommandLineArgs();
            }
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
                //if (transport.Equals("kcp"))
                //{
                //KcpTransport newTransport = networkManager.gameObject.AddComponent<KcpTransport>();

                //Try to apply port if exists and needed by transport.
                //if (!string.IsNullOrEmpty(port))
                //{
                //newTransport.Port = int.Parse(port);
                //}
                //networkManager.server.transport = newTransport;
                //networkManager.client.Transport = newTransport;
                //}
            }

            //Server mode?
            if (!string.IsNullOrEmpty(GetArg("-server")))
            {
                networkManager.server.Started.AddListener(OnServerStarted);
                networkManager.server.Authenticated.AddListener(conn => networkManager.server.SetClientReady(conn));
                _ = networkManager.server.ListenAsync();
                LogDebug("Starting Server Only Mode");
            }

            //Or client mode?
            string client = GetArg("-client");
            if (!string.IsNullOrEmpty(client))
            {
                string address;
                //network address provided?
                string networkAddress = GetArgValue("address");
                if (!string.IsNullOrEmpty(networkAddress))
                {
                    address = networkAddress;
                }
                else
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

                LogDebug("Starting " + clonesCount + " Clients");

                // connect from a bunch of clients
                for (int i = 0; i < clonesCount; i++)
                    yield return StartClient(i, networkManager.client.Transport, address);
            }

            if (!string.IsNullOrEmpty(GetArg("-help")))
            {
                LogDebug("--==MirrorNG HeadlessClients Benchmark==--");
                LogDebug("Please start your standalone application with the -nographics and -batchmode options");
                LogDebug("Also provide these arguments to control the autostart process:");
                LogDebug("-server (will run in server only mode)");
                LogDebug("-client 1234 (will run the specified number of clients)");
                LogDebug("-transport tcp (transport to be used in test. add more by editing HeadlessBenchmark.cs)");
                LogDebug("-port 1234 (port used by transport)");
                LogDebug("-monster 100 (number of monsters to spawn on the server)");
                LogDebug("-debug (enables verbose logging and GUI mode)");
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

        void LogDebug(string text)
        {
            if(debugMode)
            {
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    Debug.Log(text);
                }
            }
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
