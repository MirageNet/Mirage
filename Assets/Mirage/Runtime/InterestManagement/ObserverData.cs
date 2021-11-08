using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    public struct ObserverData
    {
        private readonly INetworkVisibility _system;

        private readonly Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> _observers;

        public INetworkVisibility System => _system;

        public Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> Observers => _observers;

        public ObserverData(INetworkVisibility system, Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> observers)
        {
            _system = system;
            _observers = observers;
        }
        public override string ToString()
        {
            return $"[Visibility System :{nameof(ObserverData)}]";
        }
    }
}
