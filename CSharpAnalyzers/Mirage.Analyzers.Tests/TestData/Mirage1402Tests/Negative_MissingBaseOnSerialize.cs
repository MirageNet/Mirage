using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    public override bool {|#0:OnSerialize|}(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }
}
