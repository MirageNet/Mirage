using Mirage;

namespace NetworkBehaviourTests.NetworkBehaviourAbstractBaseValid
{
    public abstract class EntityBase : NetworkBehaviour { }

    public class EntityConcrete : EntityBase
    {
        [SyncVar]
        public int abstractDerivedSync { get; set; }
    }

    public class NetworkBehaviourAbstractBaseValid : EntityConcrete
    {
        [SyncVar]
        public int concreteDerivedSync { get; set; }
    }
}
