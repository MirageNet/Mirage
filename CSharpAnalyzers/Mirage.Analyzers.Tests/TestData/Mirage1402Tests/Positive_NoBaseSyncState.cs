using Mirage;
using Mirage.Serialization;

public class EmptyBase : NetworkBehaviour
{
    // No SyncVars or ISyncObjects here
}

public class DerivedPlayer : EmptyBase
{
    [SyncVar]
    public int HeroId { get; set; }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
    }
}
