using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName;
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId;

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        bool baseDirty = base.OnSerialize(writer, initialState);
        writer.WritePackedInt32(HeroId);
        return baseDirty || true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }
}
