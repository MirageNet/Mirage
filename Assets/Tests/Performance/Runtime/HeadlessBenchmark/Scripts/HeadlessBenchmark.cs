using System;
using Mirror.Tcp;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mirror.Test.Performance.Runtime.HeadlessBenchmark
{
    public class HeadlessBenchmark : MonoBehaviour
    {
        public NetworkManager networkManager;
        public GameObject MonsterPrefab;

        //Used for testing in editor
        public bool debugMode;
        public string debugArgs;

        string[] cachedArgs;

        void Start()
        {
            SetArgs();

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null || debugMode)
            {
                HeadlessStart();
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

        void HeadlessStart()
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
                _ = networkManager.server.ListenAsync();
                LogDebug("Starting Server Only Mode");
            }

            //Or client mode?
            if (!string.IsNullOrEmpty(GetArg("-client")))
            {
                string networkAddress = GetArgValue("address");
                if (!string.IsNullOrEmpty(networkAddress))
                {
                    networkManager.client.ConnectAsync(networkAddress);
                }
                else
                {
                    networkManager.client.ConnectAsync("localhost");
                }
                LogDebug("Starting Client Only Mode");
            }

            if(!string.IsNullOrEmpty(GetArg("-help")))
            {
                Console.WriteLine("--==MirrorNG HeadlessClients Benchmark==--");
                Console.WriteLine("Please start your standalone application with the -nographics and -batchmode options");
                Console.WriteLine("Also provide these arguments to control the autostart process:");
                Console.WriteLine("-server (will run in server only mode)");
                Console.WriteLine("-client (will run in client only mode)");
                Console.WriteLine("-transport tcp (transport to be used in test. add more by editing HeadlessBenchmark.cs)");
                Console.WriteLine("-port 1234 (port used by transport)");
                Console.WriteLine("-monster 100 (number of monsters to spawn on the server)");
                Console.WriteLine("-debug (enables verbose logging and GUI mode)");
            }
        }

        void OnServerStarted()
        {
            string monster = GetArgValue("monster");
            if (!string.IsNullOrEmpty(monster))
            {
                for (int i = 0; i < int.Parse(monster); i++)
                    SpawnMonster(i);
            }
        }

        void SpawnMonster(int i)
        {
            GameObject monster = Instantiate(MonsterPrefab);
            monster.gameObject.name = $"Monster {i}";
            networkManager.server.Spawn(monster.gameObject);
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
