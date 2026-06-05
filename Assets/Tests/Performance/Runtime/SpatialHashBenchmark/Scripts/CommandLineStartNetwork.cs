using System;
using System.Collections.Generic;
using System.Linq;
using Mirage;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests.Performance.Runtime.SpatialHashBenchmark
{
    public class CommandLineStartNetwork : MonoBehaviour
    {
#if UNITY_SERVER || UNITY_EDITOR
        [NonSerialized] private float previous;
        private void Update()
        {
            if (Time.time > previous + 10)
            {
                previous = Time.time;
                if (networkManager.Server.Active)
                {
                    var server = networkManager.Server;
                    var count = server.AllPlayers.Count;
                    Debug.Log($"PlayerCount: {count}");
                }
                else if (networkManager.Client.Active)
                {
                    // all identities with PlayerCharacter attached
                    var count = networkManager.Client.World.SpawnedIdentities.Where(x => x != null && x.TryGetComponent<PlayerCharacter>(out var _)).Count();
                    Debug.Log($"PlayerCount: {count}");
                }
            }
        }
#endif


        public NetworkManager networkManager;

        // only let 1 StartNetwork setup, incase it gets copied
        private static bool hasSetup = false;
        private void Start()
        {
            if (hasSetup) return;
            hasSetup = true;

            if (networkManager.Server.SocketFactory == null)
                networkManager.Server.SocketFactory = networkManager.Server.GetComponent<SocketFactory>();
            if (networkManager.Client.SocketFactory == null)
                networkManager.Client.SocketFactory = networkManager.Client.GetComponent<SocketFactory>();

            Application.targetFrameRate = 60;

            // to lower because we dont care about case
            var args = Environment.GetCommandLineArgs().Select(x => x.ToLower()).ToArray();
            var commands = getArgCommands();
            foreach (var command in commands)
            {
                var index = Array.IndexOf(args, command.start);
                if (index != -1)
                {
                    command.handler.Invoke(index, args);
                }
            }
        }

        public delegate void HandleArgDelate(int index, IReadOnlyList<string> args);
        private struct ArgCommand
        {
            public string start;
            public HandleArgDelate handler;

            public ArgCommand(string start, HandleArgDelate handler)
            {
                this.start = (start ?? throw new ArgumentNullException(nameof(start))).ToLower();
                this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }
        }

        private List<ArgCommand> getArgCommands()
        {
            var args = new List<ArgCommand>();
            args.Add(new ArgCommand("-transport", parseTransport));
            args.Add(new ArgCommand("-address", parseAddress));
            args.Add(new ArgCommand("-port", parsePort));
            args.Add(new ArgCommand("-server", parseServer));
            args.Add(new ArgCommand("-client", parseClient));

            return args;
        }

        private void parseTransport(int index, IReadOnlyList<string> args)
        {
            var newType = getNewTransportType(args[index + 1]);

            var old = networkManager.Server.SocketFactory;
            GameObject.Destroy(old);

            var factory = networkManager.gameObject.AddComponent(newType) as SocketFactory;

            networkManager.Server.SocketFactory = factory;
            networkManager.Client.SocketFactory = factory;
        }

        private Type getNewTransportType(string v)
        {
            throw new NotImplementedException();
        }

        private void parseAddress(int index, IReadOnlyList<string> args)
        {
            if (networkManager.Server.SocketFactory is IHasAddress hasAddress)
            {
                hasAddress.Address = args[index + 1];
            }
            else
            {
                Debug.LogError($"Socket Factory does not support Address, Type:{networkManager.Server.SocketFactory?.GetType()}");
            }
        }

        private void parsePort(int index, IReadOnlyList<string> args)
        {
            if (networkManager.Server.SocketFactory is IHasPort hasPort)
            {
                hasPort.Port = int.Parse(args[index + 1]);
            }
            else
            {
                Debug.LogError($"Socket Factory does not support Port, Type:{networkManager.Server.SocketFactory?.GetType()}");
            }
        }

        private void parseServer(int index, IReadOnlyList<string> args)
        {
            networkManager.Server.StartServer();
        }

        private void parseClient(int index, IReadOnlyList<string> args)
        {
            var count = GetClientCount(index, args);
            var clients = new NetworkClient[count];
            clients[0] = networkManager.Client;
            for (var i = 1; i < count; i++)
            {
                // copy manager
                clients[i] = Instantiate(networkManager).Client;
            }

            foreach (var client in clients)
            {
                client.Connect();
            }
        }

        /// <summary>
        /// Returns the number after the -client arg, or 1 if there is none
        /// </summary>
        private static int GetClientCount(int index, IReadOnlyList<string> args)
        {
            // not enough args
            if (index + 1 >= args.Count)
                return 1;

            var next = args[index + 1];
            // is another Command
            if (next.StartsWith("-"))
            {
                return 1;
            }

            var count = int.Parse(next);
            if (count < 1) throw new ArgumentOutOfRangeException("Client Count", count, "Client count should be atleast 1");
            return count;
        }
    }
}
