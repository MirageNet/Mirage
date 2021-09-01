# Rpc Examples

Examples of Rpc and generated code

## Example 1

Set players name from client and have it synced to other players

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

Weaver moves the user code into a new function and then replace the body of the Rpc with an Internal send call

RPCs are registered using the classes static constructor with methods that will read all the parameter and then invoke the user code method

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
            UserCode_RpcChangeName(newName);
        }
        else 
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                writer.WriteString(newName);
                this.SendServerRpcInternal(typeof(Player), "RpcChangeName", writer, 0, true);
            }
        }
    }

    public void UserCode_RpcChangeName(string newName)
    {
        PlayerName = newName;
    }
    protected void Skeleton_RpcChangeName(NetworkReader reader, INetworkPlayer senderConnection, int replyId)
    {
        this.UserCode_RpcChangeName(reader.ReadString());
    }
    static Player()
    {
        RemoteCallHelper.RegisterServerRpcDelegate(typeof(Player), "RpcChangeName", new CmdDelegate(null.Skeleton_RpcChangeName), false);
    }
}
```