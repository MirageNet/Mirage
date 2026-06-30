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

    public override void {|#0:OnDeserialize|}(NetworkReader reader, bool initialState)
    {
    }
}
