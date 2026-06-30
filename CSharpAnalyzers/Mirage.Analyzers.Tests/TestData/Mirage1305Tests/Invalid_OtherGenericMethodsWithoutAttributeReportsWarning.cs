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
        NetworkServer server,
        MessagePacker packer)
    {
        receiver.UnregisterHandler<{|#0:UnattributedMessage|}>();
        server.SendToAll<{|#1:UnattributedMessage|}>(default);
        server.SendToMany<{|#2:UnattributedMessage|}>(default, default);
        
        var writer = new NetworkWriter();
        MessagePacker.Pack<{|#3:UnattributedMessage|}>(default, writer);
        
        var reader = new NetworkReader();
        MessagePacker.Unpack<{|#4:UnattributedMessage|}>(reader);
        
        MessagePacker.GetId<{|#5:UnattributedMessage|}>();
    }
}
