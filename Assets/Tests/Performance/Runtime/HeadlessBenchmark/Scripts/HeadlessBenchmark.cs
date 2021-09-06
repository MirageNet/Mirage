using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using UnityEngine;

namespace Mirage.HeadlessBenchmark
{
    public class HeadlessBenchmark : MonoBehaviour
    {
        public GameObject ServerPrefab;
        public GameObject ClientPrefab;
        public NetworkServer server;
        public ServerObjectManager serverObjectManager;
        public GameObject MonsterPrefab;
        public GameObject PlayerPrefab;
        public string editorArgs;
        public SocketFactory socketFactory;

        string[] cachedArgs;
        string port;

        void Start()
        {
            cachedArgs = Application.isEditor ?
                cachedArgs = editorArgs.Split(' ') :
                Environment.GetCommandLineArgs();

            HeadlessStart();

        }
        private IEnumerator DisplayFramesPerSecons()
        {
            int previousFrameCount = Time.frameCount;
            long previousMessageCount = 0;

            while (true)
            {
                yield return new WaitForSeconds(1);
                int frameCount = Time.frameCount;
                int frames = frameCount - previousFrameCount;

                long messageCount = 0;
                // todo use debug metrics from peer when they are added
                //if (transport is KcpTransport kcpTransport)
                //{
                //    messageCount = kcpTransport.ReceivedMessageCount;
                //}

                long messages = messageCount - previousMessageCount;

                if (Application.isEditor)
                    Debug.LogFormat("{0} FPS {1} messages {2} clients", frames, messages, server.NumberOfPlayers);
                else
                    Console.WriteLine("{0} FPS {1} messages {2} clients", frames, messages, server.NumberOfPlayers);
                previousFrameCount = frameCount;
                previousMessageCount = messageCount;
            }
        }

        void HeadlessStart()
        {
            //Try to find port
            port = GetArgValue("-port");

            //Try to find Socket
            ParseForSocket();

            //Server mode?
            ParseForServerMode();

            //Or client mode?
            StartClients().Forget();

            ParseForHelp();
        }

        void OnServerStarted()
        {
            StartCoroutine(DisplayFramesPerSecons());

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
            serverObjectManager.Spawn(monster.gameObject);
        }

        void ParseForServerMode()
        {
            if (string.IsNullOrEmpty(GetArg("-server"))) return;

            GameObject serverGo = Instantiate(ServerPrefab);
            serverGo.name = "Server";
            server = serverGo.GetComponent<NetworkServer>();
            server.MaxConnections = 9999;
            server.SocketFactory = socketFactory;
            serverObjectManager = serverGo.GetComponent<ServerObjectManager>();

            NetworkSceneManager networkSceneManager = serverGo.GetComponent<NetworkSceneManager>();
            networkSceneManager.Server = server;

            serverObjectManager.Server = server;
            serverObjectManager.NetworkSceneManager = networkSceneManager;
            serverObjectManager.Start();

            CharacterSpawner spawner = serverGo.GetComponent<CharacterSpawner>();
            spawner.ServerObjectManager = serverObjectManager;
            spawner.Server = server;

            server.Started.AddListener(OnServerStarted);
            server.Authenticated.AddListener(conn => serverObjectManager.SpawnVisibleObjects(conn));
            server.StartServer();
            Console.WriteLine("Starting Server Only Mode");
        }

        async UniTaskVoid StartClients()
        {
            string clientArg = GetArg("-client");
            if (!string.IsNullOrEmpty(clientArg))
            {
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

                Console.WriteLine("Starting {0} clients", clonesCount);

                // connect from a bunch of clients
                for (int i = 0; i < clonesCount; i++)
                {
                    StartClient(i, address);
                    await UniTask.Delay(500);

                    Debug.LogFormat("Started {0} clients", i + 1);
                }
            }
        }

        void StartClient(int i, string networkAddress)
        {
            GameObject clientGo = Instantiate(ClientPrefab);
            clientGo.name = $"Client {i}";
            NetworkClient client = clientGo.GetComponent<NetworkClient>();
            ClientObjectManager objectManager = clientGo.GetComponent<ClientObjectManager>();
            CharacterSpawner spawner = clientGo.GetComponent<CharacterSpawner>();
            NetworkSceneManager networkSceneManager = clientGo.GetComponent<NetworkSceneManager>();
            networkSceneManager.Client = client;

            objectManager.Client = client;
            objectManager.NetworkSceneManager = networkSceneManager;
            objectManager.Start();
            objectManager.RegisterPrefab(MonsterPrefab.GetComponent<NetworkIdentity>());
            objectManager.RegisterPrefab(PlayerPrefab.GetComponent<NetworkIdentity>());

            spawner.Client = client;
            spawner.PlayerPrefab = PlayerPrefab.GetComponent<NetworkIdentity>();
            spawner.ClientObjectManager = objectManager;
            spawner.SceneManager = networkSceneManager;

            client.SocketFactory = socketFactory;

            try
            {
                client.Connect(networkAddress);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void ParseForHelp()
        {
            if (!string.IsNullOrEmpty(GetArg("-help")))
            {
                Console.WriteLine("--==Mirage HeadlessClients Benchmark==--");
                Console.WriteLine("Please start your standalone application with the -nographics and -batchmode options");
                Console.WriteLine("Also provide these arguments to control the autostart process:");
                Console.WriteLine("-server (will run in server only mode)");
                Console.WriteLine("-client 1234 (will run the specified number of clients)");
                Console.WriteLine("-transport tcp (transport to be used in test. add more by editing HeadlessBenchmark.cs)");
                Console.WriteLine("-address example.com (will run the specified number of clients)");
                Console.WriteLine("-port 1234 (port used by transport)");
                Console.WriteLine("-monster 100 (number of monsters to spawn on the server)");

                Application.Quit();
            }
        }

        void ParseForSocket()
        {
            string socket = GetArgValue("-socket");
            if (string.IsNullOrEmpty(socket) || socket.Equals("udp"))
            {
                UdpSocketFactory newSocket = gameObject.AddComponent<UdpSocketFactory>();
                socketFactory = newSocket;

                //Try to apply port if exists and needed by transport.

                //TODO: Uncomment this after the port is made public
                /*if (!string.IsNullOrEmpty(port))
                {
                    newSocket.port = ushort.Parse(port);
                    newSocket.
                }*/
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
