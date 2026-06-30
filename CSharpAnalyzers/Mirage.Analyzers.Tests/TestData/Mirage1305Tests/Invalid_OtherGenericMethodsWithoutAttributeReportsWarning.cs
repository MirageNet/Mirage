using Mirage;
using Mirage.Serialization;

public struct UnattributedMessage
{
    public int id;
}

public class OtherMethodsSender
{
    public void RunOtherMethods(
        IMessageReceiver receiver,
        NetworkServer server
        )
    {
        receiver.UnregisterHandler<{|#0:UnattributedMessage|}>();
        server.SendToAll<{|#1:UnattributedMessage|}>(msg: default, authenticatedOnly: true,  excludeLocalPlayer: false);
        server.SendToMany<{|#2:UnattributedMessage|}>(players: default, msg: default, excludeLocalPlayer: false);
        
        var writer = new NetworkWriter(1200);
        MessagePacker.Pack<{|#3:UnattributedMessage|}>(default, writer);
        
        var data = new byte[1000];
        MessagePacker.Unpack<{|#4:UnattributedMessage|}>(data, default);
        
        MessagePacker.GetId<{|#5:UnattributedMessage|}>();
    }
}
