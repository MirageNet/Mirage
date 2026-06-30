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

    public override void {|#0:OnDeserialize|}(NetworkReader reader, bool initialState)
    {
    }
}
