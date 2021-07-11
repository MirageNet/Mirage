using System;
using Mirage.SocketLayer;
using NanoSockets;
using UnityEngine;

namespace Mirage.Sockets.Udp {
    public struct NanoEndPoint : IEndPoint, IEquatable<NanoEndPoint>
    {
        public Address address;

        public NanoEndPoint(string host, ushort port) {
            address = new Address();
            address.port = port;
            UDP.SetIP(ref address, host);
        }

        public NanoEndPoint(Address address)
        {
            this.address = address;
        }

        public IEndPoint CreateCopy()
        {
            return new NanoEndPoint(address);
        }

        public bool Equals(NanoEndPoint other)
        {
            return address.Equals(other.address);
        }

        public override bool Equals(object obj)
        {
            if (obj is NanoEndPoint endPoint) {
                return address.Equals(endPoint.address);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return address.GetHashCode();
        }

        public override string ToString()
        {
            return address.ToString();
        }
    }

    public class NanoSocket : ISocket
    {
        Socket socket;
        NanoEndPoint tmpEndPoint;

        public void Bind(IEndPoint endPoint)
        {
            UDP.Initialize();
            socket = UDP.Create(256 * 1024, 256 * 1024);
            tmpEndPoint = (NanoEndPoint)endPoint;

            Debug.Log("Binding to " + tmpEndPoint.address);

            if (UDP.Bind(socket, ref tmpEndPoint.address) != 0)
            {
                Debug.LogError("Unable to bind socket.");
            }

            if (UDP.SetDontFragment(socket) != Status.OK)
            {
                Debug.LogError("Don't fragment option error!");
            }

            if (UDP.SetNonBlocking(socket) != Status.OK)
            {
               Debug.LogError("Non-blocking option error!");
            }
        }

        public void Close()
        {
            UDP.Destroy(ref socket);
            UDP.Deinitialize();
        }

        public void Connect(IEndPoint endPoint)
        {
            UDP.Initialize();
            socket = UDP.Create(256 * 1024, 256 * 1024);
            tmpEndPoint = (NanoEndPoint)endPoint;

            if (UDP.Connect(socket, ref tmpEndPoint.address) != 0)
            {
                Debug.LogError("Socket connect failed!");
            }

            if (UDP.SetDontFragment(socket) != Status.OK)
            {
                Debug.LogError("Don't fragment option error!");
            }

            if (UDP.SetNonBlocking(socket) != Status.OK)
            {
                Debug.LogError("Non-blocking option error!");
            }
        }

        public bool Poll()
        {
            return UDP.Poll(socket, 0) > 0;
        }

        public int Receive(byte[] buffer, out IEndPoint endPoint)
        {
            int count = UDP.Receive(socket, ref tmpEndPoint.address, buffer, buffer.Length);
            endPoint = tmpEndPoint;

            return count;
        }

        public void Send(IEndPoint endPoint, byte[] packet, int length)
        {
            tmpEndPoint = (NanoEndPoint)endPoint;
            UDP.Send(socket, ref tmpEndPoint.address, packet, length);
        }
    }
}