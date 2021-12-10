# ClientRpc Calls

ClientRpc are sent from NetworkBehaviours on the server to Behaviours on the client. They can be sent from any NetworkBehaviour that has been spawned.

To make a function into a ClientRpc add  `[ClientRpc]` directly above the function.

```cs
[ClientRpc]
public void MyRpcFunction() 
{
    // code to invoke on client
}
```

ClientRpc functions can't be static and must return `void`.

## RpcTarget

There are 3 target modes for ClientRpc:
- Observers (default)
- Owner
- Player

### RpcTarget.Observers

This is the default target.

This will send the RPC message to only the observers of an object according to its [Network Visibility](../Visibility.md). If there is no Network Visibility on the object it will send to all players.

### RpcTarget.Owner

This will send the RPC message to only the owner of the object.

### RpcTarget.Player

This will send the RPC message to the NetworkPlayer that is passed into the call.

```cs
[ClientRpc(target = RpcTarget.Player)]
public void MyRpcFunction(NetworkPlayer target) 
{
    // code to invoke on client
}
```

Mirage will use the `NetworkPlayer target` to know where to sent it, but it will not send the `target` value. Because of this its value will always be null on the client.

## Exclude owner

You may want to exclude the owner client when calling a ClientRpc.  This is done with the `excludeOwner` option: `[ClientRpc(excludeOwner = true)]`.


## Channel

RPC can be sent using either the Reliable or Unreliable channels. `[ClientRpc(channel = Channel.Reliable)]`

# Examples 

``` cs
public class Player : NetworkBehaviour
{
    int health;

    public void TakeDamage(int amount)
    {
        if (!isServer) return;

        health -= amount;
        Damage(amount);
    }

    [ClientRpc]
    void Damage(int amount)
    {
        Debug.Log("Took damage:" + amount);
    }
}
```

When running a game as a host with a local client, ClientRpc calls will be invoked on the local client even though it is in the same process as the server. So the behaviours of local and remote clients are the same for ClientRpc calls.

You can also specify which client gets the call with the `target` parameter. 

If you only want the client that owns the object to be called,  use `[ClientRpc(target = RpcTarget.Owner)]` or you can specify which client gets the message by using `[ClientRpc(target = RpcTarget.Player)]` and passing the player as a parameter.  For example:

``` cs
public class Player : NetworkBehaviour
{
    int health;

    [Server]
    void Magic(GameObject target, int damage)
    {
        target.GetComponent<Player>().health -= damage;

        NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
        DoMagic(opponentIdentity.Owner, damage);
    }

    [ClientRpc(target = Client.Player)]
    public void DoMagic(INetworkPlayer target, int damage)
    {
        // This will appear on the opponent's client, not the attacking player's
        Debug.Log($"Magic Damage = {damage}");
    }

    [Server]
    void HealMe()
    {
        health += 10;
        Healed(10);
    }

    [ClientRpc(target = Client.Owner)]
    public void Healed(int amount)
    {
        // No NetworkPlayer parameter, so it goes to owner
        Debug.Log($"Health increased by {amount}");
    }
}
```
