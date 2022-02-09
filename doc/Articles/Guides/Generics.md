# Generics In mirage

Mirage supports generic types for [SyncVars](./Sync/SyncVars.md), [Rpcs](./RemoteCalls/index.md), and for fields in [NetworkMessages](./RemoteCalls/NetworkMessages.md).

## NetworkBehaviour

By making a [NetworkBehaviour](./GameObjects/NetworkBehaviour.md) generic you can then use generic SyncVar fields or use the generic in an rpc

```cs
public class MyGenericBehaviour<T> : NetworkBehaviour
{
    [SyncVar]
    public T Value;

    public void MyRpc(T value) 
    {
        // do stuff
    }
}
```

>[!WARNING] 
> making the rpc itself generic does not work. for example `MyRpc<T>(T value)` will not work. This is because the receiver will have no idea what generic to invoke the type as.

## Network Messages

Generic message are partly supported. Generic Instance can be used as messages, For example using `MyMessage<int>` in the example below.

```cs
public struct MyMessage<T>
{
    public int Value;
}

class Manager 
{
    void Start() 
    {
        Server.MessageHandler.RegisterHandler<MyMessage<int>>(HandleMessage);
    }

    void HandleIntMessage(INetworkPlayer player, MyMessage<int> msg)
    {
        // do stuff
    }
}
```

>[!NOTE] 
> generic message should not have `[NetworkMessage]` because this cause Mirage to try to make writer for the generic itself. Only generic instances (eg MyMessage<int>) can have serialize functions 

## Ensure Type has Write and Read functions

In order for a type to work as a generic, it must have Write and Read that Mirage can find. For built in types this is done automatically (see [Serialization](./Serialization.md)).

For custom types Mirage will try to automatically find them and generate functions, however this does not always work. Adding `[NetworkMessage]` to the type will tell Mirage to generate functions for it.

```cs
[NetworkMessage]
public struct MyCustomType
{
    public int Value;
}
```

Alternatively you can manually create Write and Read functions for your type

```cs
public static class MyCustomTypeExtensions 
{
    public static void Write(this NetworkWriter writer, MyCustomType value) {
        // write here
    }

    public static MyCustomType Read(this NetworkReader reader) {
        // read here
    }
}
```