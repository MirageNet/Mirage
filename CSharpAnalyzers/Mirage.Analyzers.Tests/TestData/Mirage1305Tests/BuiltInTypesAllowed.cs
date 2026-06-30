using Mirage;

public class MessageSender
{
    public void SendMessage(INetworkPlayer player)
    {
        player.Send<int>(42);
        player.Send<string>("hello");
    }
}
