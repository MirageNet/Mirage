using System.Collections.Generic;

namespace Mirage.InterestManagement
{
    [System.Obsolete("Dont use", true)]
    public readonly struct VisibilitySystemData
    {
        public INetworkVisibility System { get; }

        public Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> Observers { get; }

        public VisibilitySystemData(INetworkVisibility system, Dictionary<NetworkIdentity, HashSet<INetworkPlayer>> observers)
        {
            System = system;
            Observers = observers;
        }
        public override string ToString()
        {
            return $"[Visibility System :{nameof(System)}]";
        }

        public class Comparer : IEqualityComparer<VisibilitySystemData>
        {
            public bool Equals(VisibilitySystemData x, VisibilitySystemData y)
            {
                return x.System.GetType().Name.GetStableHashCode() == y.System.GetType().Name.GetStableHashCode();
            }

            public int GetHashCode(VisibilitySystemData obj)
            {
                int hash = obj.System.GetType().Name.GetStableHashCode();

                return hash;
            }
        }
    }
}
