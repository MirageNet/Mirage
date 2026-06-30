using Mirage;
using Mirage.Serialization;
using Mirage.Collections;

public class BasePlayer : NetworkBehaviour
{
    public readonly SyncList<int> Scores = new SyncList<int>();
}

public class HeroPlayer : BasePlayer
{
    public override bool {|#0:OnSerialize|}(NetworkWriter writer, bool initialState)
    {
        return true;
    }
}
