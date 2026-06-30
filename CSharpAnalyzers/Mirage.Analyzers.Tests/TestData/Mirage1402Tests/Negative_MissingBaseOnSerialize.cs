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

    public override bool {|#0:OnSerialize|}(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }
}
