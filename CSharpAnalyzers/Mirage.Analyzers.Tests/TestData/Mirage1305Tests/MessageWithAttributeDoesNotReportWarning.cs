using Mirage;

[NetworkMessage]
public struct AttributedMessage
{
    public int id;
}

public class MessageSender
{
    public void SendMessage(INetworkPlayer player)
    {
        player.Send<AttributedMessage>(new AttributedMessage());
    }
}
