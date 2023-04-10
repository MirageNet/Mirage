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
        private string[] cachedArgs;
        private string port;

        private void Start()
        {
            cachedArgs = Application.isEditor ?
                cachedArgs = editorArgs.Split(' ') :
                Environment.GetCommandLineArgs();

            HeadlessStart();

        }
        private IEnumerator DisplayFramesPerSecons()
        {
            var previousFrameCount = Time.frameCount;
            long previousMessageCount = 0;

            while (true)
            {
                yield return new WaitForSeconds(1);
                var frameCount = Time.frameCount;
                var frames = frameCount - previousFrameCount;

                long messageCount = 0;
                // todo use debug metrics from peer when they are added
                //if (transport is KcpTransport kcpTransport)
                //{
                //    messageCount = kcpTransport.ReceivedMessageCount;
                //}

                var messages = messageCount - previousMessageCount;

                if (Application.isEditor)
                    Debug.LogFormat("{0} FPS {1} messages {2} clients", frames, messages, server.NumberOfPlayers);
                else
                    Console.WriteLine("{0} FPS {1} messages {2} clients", frames, messages, server.NumberOfPlayers);
                previousFrameCount = frameCount;
                previousMessageCount = messageCount;
            }
        }

        private void HeadlessStart()
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

        private void OnServerStarted()
        {
            StartCoroutine(DisplayFramesPerSecons());

            var monster = GetArgValue("-monster");
            if (!string.IsNullOrEmpty(monster))
            {
                for (var i = 0; i < int.Parse(monster); i++)
                    SpawnMonsters(i);
            }
        }

        private void SpawnMonsters(int i)
        {
            var monster = Instantiate(MonsterPrefab);
            monster.gameObject.name = $"Monster {i}";
            serverObjectManager.Spawn(monster.gameObject);
        }

        private void ParseForServerMode()
        {
            if (string.IsNullOrEmpty(GetArg("-server"))) return;

            var serverGo = Instantiate(ServerPrefab);
            serverGo.name = "Server";
            server = serverGo.GetComponent<NetworkServer>();
            server.MaxConnections = 9999;
            server.SocketFactory = socketFactory;
            serverObjectManager = serverGo.GetComponent<ServerObjectManager>();

            var networkSceneManager = serverGo.GetComponent<NetworkSceneManager>();
            networkSceneManager.Server = server;

            serverObjectManager.Server = server;
            serverObjectManager.Setup();

            networkSceneManager.ServerObjectManager = serverObjectManager;

            var spawner = serverGo.GetComponent<CharacterSpawner>();
            spawner.ServerObjectManager = serverObjectManager;
            spawner.Server = server;

            server.Started.AddListener(OnServerStarted);
            server.Authenticated.AddListener(conn => serverObjectManager.SpawnVisibleObjects(conn, true));
            server.StartServer();
            Console.WriteLine("Starting Server Only Mode");
        }

        private async UniTaskVoid StartClients()
        {
            var clientArg = GetArg("-client");
            if (!string.IsNullOrEmpty(clientArg))
            {
                //network address provided?
                var address = GetArgValue("-address");
                if (string.IsNullOrEmpty(address))
                {
                    address = "localhost";
                }

                //nested clients
                var clonesCount = 1;
                var clonesString = GetArgValue("-client");
                if (!string.IsNullOrEmpty(clonesString))
                {
                    clonesCount = int.Parse(clonesString);
                }

                Console.WriteLine("Starting {0} clients", clonesCount);

                // connect from a bunch of clients
                for (var i = 0; i < clonesCount; i++)
                {
                    StartClient(i, address);
                    await UniTask.Delay(500);

                    Debug.LogFormat("Started {0} clients", i + 1);
                }
            }
        }

        private void StartClient(int i, string networkAddress)
        {
            var clientGo = Instantiate(ClientPrefab);
            clientGo.name = $"Client {i}";
            var client = clientGo.GetComponent<NetworkClient>();
            var objectManager = clientGo.GetComponent<ClientObjectManager>();
            var spawner = clientGo.GetComponent<CharacterSpawner>();
            var networkSceneManager = clientGo.GetComponent<NetworkSceneManager>();
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

        private void ParseForHelp()
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

        private void ParseForSocket()
        {
            var socket = GetArgValue("-socket");
            if (string.IsNullOrEmpty(socket) || socket.Equals("udp"))
            {
                var newSocket = gameObject.AddComponent<UdpSocketFactory>();
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

        private string GetArgValue(string name)
        {
            for (var i = 0; i < cachedArgs.Length; i++)
            {
                if (cachedArgs[i] == name && cachedArgs.Length > i + 1)
                {
                    return cachedArgs[i + 1];
                }
            }
            return null;
        }

        private string GetArg(string name)
        {
            for (var i = 0; i < cachedArgs.Length; i++)
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
