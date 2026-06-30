using Mirage;

public struct UnattributedMessage
{
    public int id;
}

public class MessageListener
{
    public void Setup(NetworkServer server)
    {
        server.RegisterHandler<{|#0:UnattributedMessage|}>(OnMessage);
    }

    private void OnMessage(INetworkPlayer player, UnattributedMessage message)
    {
    }
}
