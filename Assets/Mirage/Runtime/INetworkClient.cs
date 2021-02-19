using System;
using UnityEngine;

namespace Mirage
{
    public interface INetworkClient
    {
        void Disconnect();

        void Send<T>(T message, int channelId = Channel.Reliable);
    }
}
