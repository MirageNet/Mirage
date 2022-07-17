---
sidebar_position: 5
---
# RPC Examples

Examples of RPC and generated code.

## Example 1

Set a player's name from client and have it synced to other players.

```cs
public class Player : NetworkBehaviour
{
    [SyncVar] 
    public string PlayerName;

    [ServerRpc]
    public void RpcChangeName(string newName)
    {
        PlayerName = newName;
    }
}
```

### Generated code

Weaver moves the user code into a new function and then replace the body of the RPC with an internal send call.

RPCs are registered using the classes static constructor with methods that will read all the parameter and then invoke the user code method.

```cs
public class Player : NetworkBehaviour
{
    [SyncVar] 
    public string PlayerName;

    [ServerRpc]
    public void RpcChangeName(string newName)
    {
        if (this.IsServer)
        {
            UserCode_RpcChangeName_123456789(newName);
        }
        else 
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString(newName);
                ServerRpcSender.Send(this, 123456789, writer, 0, true);
            }
        }
    }

    public void UserCode_RpcChangeName_123456789(string newName)
    {
        PlayerName = newName;
    }
    
    protected void Skeleton_RpcChangeName_123456789(NetworkReader reader, INetworkPlayer senderConnection, int replyId)
    {
        this.UserCode_RpcChangeName_123456789(reader.ReadString());
    }

    public Player()
    {
        this.remoteCallCollection.Register(0, typeof(Player), "Player.RpcChangeName", RpcInvokeType.ServerRpc, new CmdDelegate(Skeleton_RpcChangeName), true);
    }

    protected override int GetRpcCount()
    {
        return 1;
    }
}
```