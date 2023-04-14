---
sidebar_position: 5
---

# Troubleshooting

## No Writer found for X

Mirage normally generates readers and writers for any [Data Type](/docs/guides/serialization/data-types)
In order to do so, it needs to know what types you want to read or write.
You are getting this error because Mirage did not know you wanted to read or write this type.

Mirage scans your code looking for calls to `Send`, `ReceiveHandler`, `Write` or `Read`. 
It will also recognize [SyncVars](/docs/guides/sync/) and parameters of [Remote Calls](/docs/guides/remote-actions/). 
If it does not find one, it assumes you are not trying to serialize the type so it does not generate the reader and writer.

For example, you might get this error with this code when trying to sync the [SyncList](/docs/guides/sync/sync-objects/sync-list).

```cs
public struct MyCustomType
{
    public int id;
    public string name;
}

class MyBehaviour : NetworkBehaviour
{
    private readonly SyncList<MyCustomType> myList = new SyncList<MyCustomType>();
}
```

In this case, there is no direct invocation to send or receive.  So Mirage does not know about it. 

**There is a simple workaround:** add an `[NetworkMessage]` attribute to your class or struct.
```cs
[NetworkMessage] // Added attribute
public struct MyCustomType
{
    public int id;
    public string name;
}
```
