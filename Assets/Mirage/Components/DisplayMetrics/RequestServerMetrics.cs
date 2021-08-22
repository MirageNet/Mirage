using System;
using System.Collections;
using System.Collections.Generic;
using Mirage;
using Mirage.SocketLayer;
using UnityEngine;

namespace JamesFrowen.NetworkingBenchmark
{
    public class RequestServerMetrics : MonoBehaviour
    {
        public NetworkServer server;
        public NetworkClient client;
        public DisplayMetricsAverageGui displayMetrics;
        public bool RequestMetrics = false;

        /// <summary>
        /// Connections that are requesting metrics
        /// </summary>
        HashSet<INetworkPlayer> connections;
        private Metrics metrics;
        uint lastSendTick;

        private void Start()
        {
            if (RequestMetrics)
            {
                client.Connected.AddListener((x) => sendRequest());
            }

            server.Started.AddListener(ServerStarted);
            StartCoroutine(Runner());
        }

        private void ServerStarted()
        {
            connections = new HashSet<INetworkPlayer>();

            server.MessageHandler.RegisterHandler<RequestMetricsMessage>(OnRequestMetricsMessage);
            server.Disconnected.AddListener(x => connections.Remove(x));
        }

        private void OnRequestMetricsMessage(INetworkPlayer arg1, RequestMetricsMessage arg2)
        {
            connections.Add(arg1);
            if (metrics == null)
            {
                metrics = server.Metrics;
                lastSendTick = metrics.tick;
            }
        }

        private void sendRequest()
        {
            client.MessageHandler.RegisterHandler<SendMetricsMessage>(OnSendMetricsMessage);
            client.World.LocalPlayer.Send(new RequestMetricsMessage());
            metrics = new Metrics();
            displayMetrics.Metrics = metrics;
        }

        private void OnSendMetricsMessage(INetworkPlayer _, SendMetricsMessage msg)
        {
            for (uint i = 0; i < msg.newFrames.Length; i++)
            {
                ulong seq = metrics.Sequencer.MoveInBounds(i + msg.start);
                metrics.buffer[seq] = msg.newFrames[i];
            }
        }

        [NetworkMessage]
        struct RequestMetricsMessage
        {

        }

        [NetworkMessage]
        struct SendMetricsMessage
        {
            public uint start;
            public Metrics.Frame[] newFrames;
        }

        public IEnumerator Runner()
        {
            while (true)
            {
                try
                {
                    if (server.Active && connections.Count > 0) ServerUpdate();
                    if (RequestMetrics && client.Active) ClientUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
        private void ServerUpdate()
        {
            var msg = new SendMetricsMessage
            {
                start = lastSendTick,
                newFrames = getFrames(lastSendTick, metrics.tick)
            };

            foreach (INetworkPlayer player in connections)
            {
                player.Send(msg);
            }
        }

        private Metrics.Frame[] getFrames(uint start, uint end)
        {
            long count = metrics.Sequencer.Distance(end, start);
            // limit to 100 frames
            if (count > 100) count = 100;

            var frames = new Metrics.Frame[count];
            for (uint i = 0; i < count; i++)
            {
                ulong seq = metrics.Sequencer.MoveInBounds(i + start);
                frames[i] = metrics.buffer[seq];
            }

            lastSendTick = (uint)metrics.Sequencer.MoveInBounds(start + (uint)count);

            return frames;
        }

        private void ClientUpdate()
        {

        }
    }
}
