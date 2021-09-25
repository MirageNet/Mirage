using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mirage.InterestManagement
{
    public readonly struct ObserverData
    {
        public sealed class EqualityComparer : IEqualityComparer<ObserverData>
        {
            public bool Equals(ObserverData x, ObserverData y)
            {
                return x._system == y._system;
            }

            public int GetHashCode(ObserverData obj)
            {
                return obj._system.GetHashCode();
            }
        }

        private readonly INetworkVisibility _system;

        private readonly Dictionary<NetworkIdentity, INetworkPlayer> _observers;

        public INetworkVisibility System => _system;

        public Dictionary<NetworkIdentity, INetworkPlayer> Observers => _observers;

        public ObserverData(INetworkVisibility system, Dictionary<NetworkIdentity, INetworkPlayer> observers)
        {
            _system = system;
            _observers = observers;
        }

        public bool Equals(ObserverData other)
        {
            return _system == other._system;
        }

        public override bool Equals(object obj)
        {
            return obj is ObserverData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _system.GetHashCode();
        }

        public override string ToString()
        {
            return $"[Visibility System :{this}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ObserverData x, ObserverData y)
        {
            return x._system == y._system;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ObserverData x, ObserverData y)
        {
            return x._system != y._system;
        }
    }
}
