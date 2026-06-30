using Mirage;

public struct UnattributedMessage
{
    public int id;
}

public class MessageSender
{
    public void SendMessage(INetworkPlayer player)
    {
        player.Send<{|#0:UnattributedMessage|}>(new UnattributedMessage());
    }
}
