# ClientRpc Calls

ClientRpc calls are sent from objects on the server to objects on clients. They can be sent from any server object with a NetworkIdentity that has been spawned. Since the server has authority, then there no security issues with server objects being able to send these calls. To make a function into a ClientRpc call, add the [ClientRpc] custom attribute to it. This function will now be run on clients when it is called on the server. Any parameters of [allowed data type](../DataTypes.md) will automatically be passed to the clients with the ClientRpc call..

ClientRpc functions cannot be static.  They must return `void`

ClientRpc messages are only sent to observers of an object according to its [Network Visibility](../Visibility.md). character objects are always obeservers of themselves. In some cases, you may want to exclude the owner client when calling a ClientRpc.  This is done with the `excludeOwner` option: `[ClientRpc(excludeOwner = true)]`.

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
