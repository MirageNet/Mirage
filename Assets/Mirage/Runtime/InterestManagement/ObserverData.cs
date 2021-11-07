using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    public struct ObserverData
    {
        private readonly INetworkVisibility _system;

        private readonly Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> _observers;

        public INetworkVisibility System => _system;

        public Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> Observers => _observers;

        public ObserverData(INetworkVisibility system, Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> observers)
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
