---
sidebar_position: 4
---
# Generics

Mirage supports generic types for [SyncVar](/docs/guides/sync/sync-var), [Rpcs](/docs/guides/remote-actions/), and fields in [NetworkMessages](/docs/guides/remote-actions/network-messages).

## NetworkBehaviour

By making a [NetworkBehaviour](/docs/guides/game-objects/network-behaviour) generic you can then use generic SyncVar fields or use the generic in an RPC.

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

:::warning
Making the RPC itself generic does not work. For example, `MyRpc<T>(T value)` will not work. This is because the receiver will have no idea what generic to invoke the type as.
:::

## Ensure Type has Write and Read functions

For a type to work as a generic, it must have a write and read that Mirage can find. For built-in types, this is done automatically (see [Serialization](/docs/guides/serialization)).

For custom types Mirage will try to automatically find them and generate functions, however, this does not always work. Adding `[NetworkMessage]` to the type will tell Mirage to generate functions for it.

```cs
[NetworkMessage]
public struct MyCustomType
{
    public int Value;
}
```

Alternatively, you can manually create Write and Read functions for your type

```cs
public static class MyCustomTypeExtensions 
{
    public static void Write(this NetworkWriter writer, MyCustomType value) 
    {
        // write here
    }

    public static MyCustomType Read(this NetworkReader reader) 
    {
        // read here
    }
}
```

## Network Messages and other types

Generic messages are partly supported. Generic instances can be used as messages, For example, using `MyMessage<int>` in the example below.

This also includes using generic types in RPC or inside other types as long they are generic instances.

```cs
public struct MyMessage<T>
{
    public T Value;
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

:::note
Generic message should not have `[NetworkMessage]` because this cause Mirage to try to make a writer for the generic itself. Only generic instances (eg `MyMessage<int>`) can have serialize functions 
:::

## SyncList, SyncDictionary, SyncSet

SyncList, SyncDictionary, and SyncSet can have generic types as their element type as long as it is a generic instance (eg `MyType<int>` not `MyType<T>`).

```cs 
public struct MyType<T>
{
    public bool Option;
    public T Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public SyncList<MyType<float>> myList;
}
```