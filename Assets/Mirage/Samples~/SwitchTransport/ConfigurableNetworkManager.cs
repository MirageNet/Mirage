using Mirage.SocketLayer;
using Mirage.Sockets.Udp;
using UnityEngine;

namespace Mirage.Examples.SwitchTransport
{
    public enum SocketType
    {
        // Add SocketFactory types you want to add to your game here
        // and then use this enum to set the SocketFactory field before starting server or client 
        Offline,
        UDP,
        Steam,
    }

    public class ConfigurableNetworkManager : NetworkManager
    {
        [Header("Transport Options")]
        // These can be of type SocketFactory or their child class
        public UdpSocketFactory udpTransport;
        public SocketFactory steamTransport;

        public void StartHost(SocketType socketType)
        {
            ConfigureNetwork(socketType);
            Server.StartServer(Client);
        }

        public void StartClient(SocketType socketType)
        {
            ConfigureNetwork(socketType);
            Client.Connect("localhost");
        }

        public void StartServer(SocketType socketType)
        {
            ConfigureNetwork(socketType);
            Server.StartServer();
        }

        private void ConfigureNetwork(SocketType type)
        {
            // It is important that the network is not active when changing the transport
            // or its config. These asserts will show an error if the network is active.
            Debug.Assert(!Server.Active, "Should not be changing socket type while Server is running");
            Debug.Assert(!Client.Active, "Should not be changing socket type while Client is running");

            // The PeerConfig contains settings for the transport layer.
            // Setting the config is optional, but the default limits might be too low for some projects.
            var peerConfig = new Config()
            {
                MaxConnections = 10,

                ConnectAttemptInterval = 0.25f,
                MaxConnectAttempts = 40, // 10 seconds

                TimeoutDuration = 30,

                MaxReliableFragments = 100,
                MaxReliablePacketsInSendBufferPerConnection = 3000,
            };

            // Set the config on the server and client.
            Server.PeerConfig = peerConfig;
            Client.PeerConfig = peerConfig;

            // Setting Listening to false will disable the transport layer
            // This can be used to run single player without a transport
            Server.Listening = type != SocketType.Offline;

            // The SocketFactory is responsible for creating the socket that the
            // transport will use. We can switch between different transport
            // implementations by changing the socket factory.
            switch (type)
            {
                // for Offline we just set udpTransport, even tho it will not be used
                default:
                case SocketType.Offline:
                case SocketType.UDP:
                    Debug.Log("Setting UDP socket");
                    Server.SocketFactory = udpTransport;
                    Client.SocketFactory = udpTransport;
                    break;
                case SocketType.Steam:
                    Debug.Log("Setting Steam socket");
                    Server.SocketFactory = steamTransport;
                    Client.SocketFactory = steamTransport;
                    break;
            }
        }

        public void Stop()
        {
            if (NetworkMode == NetworkManagerMode.Server || NetworkMode == NetworkManagerMode.Host)
                Server.Stop();
            else if (NetworkMode == NetworkManagerMode.Client)
                Client.Disconnect();
        }
    }
}
