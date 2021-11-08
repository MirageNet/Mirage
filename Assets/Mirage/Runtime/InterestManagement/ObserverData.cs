using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    public readonly struct ObserverData
    {
        public INetworkVisibility System { get; }

        public Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> Observers { get; }

        public ObserverData(INetworkVisibility system, Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> observers)
        {
            System = system;
            Observers = observers;
        }
        public override string ToString()
        {
            return $"[Visibility System :{nameof(ObserverData)}]";
        }
    }
}
