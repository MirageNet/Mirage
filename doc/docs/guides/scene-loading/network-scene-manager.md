---
title: Network Scene Manager
---
# Using Network Scene Manager

[NetworkSceneManager](/docs/reference/Mirage/NetworkSceneManager) contains methods and logic to help keep the scene in sync between server and client.

:::caution Work In Progress
This page is a work in progress
:::

### How to use

The Network Scene Manager takes care of most of the grunt work that is needed to load unload and network scenes between server and client. The examples below show exactly how to use
the network scene manager.

## Load Scene Normally

This will load up a new scene on the server and tell all current player's loaded on the server to load the scene up.

```cs
public class LoadScene : MonoBehaviour
{
    public void Start()
    {
        NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

        sceneManager.ServerLoadSceneNormal("path to scene asset file.")
    }
}
```

:::note
If you require physics scenes to load up on the server you can override the default parameter like so.
:::

```cs
sceneManager.ServerLoadSceneNormal("path to scene asset file.", new LoadSceneParameters { loadSceneMode = LoadSceneMode.Normal, localPhysicsMode = LocalPhysicsMode.Physics2D });
```

## Load Scene Additively

This will load a scene additively on the server and tell specific clients to do the same. Example shows send to everyone.

```cs
public class LoadSceneAdditively : MonoBehaviour
{
    public void Start()
    {
        NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

        sceneManager.ServerLoadSceneAdditively("path to scene asset file.", sceneManager.Server.Players)
    }
}
```

:::note
If you want to send the additive scene to only specific players we can do it like so. You must get the player on your own.
:::

```cs
sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player)
```

:::note
Also if you want to load the scene normally to specific players versus additively like the server you can override the parameter to do so also. The server will still
load additively, the reason is if you need fully normal loading you can use the above method instead to do it.
:::

```cs
sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player, true)
```

:::note
Also if you want to load the scene in physic's mode you can override another parameter also to do so. You can also make clients load normally in the example below we keep it false to load
the client side additively too.
:::

```cs
sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player, false, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additively, localPhysicsMode = LocalPhysicsMode.Physics2D )
```

This will unload a scene additively on the server and tell specific clients to do the same. Example shows send to everyone.

```cs
public class UnLoadSceneAdditively : MonoBehaviour
{
    public void Start()
    {
        NetworkSceneManager sceneManager = GetComponent<NetworkSceneManager>();

        sceneManager.ServerUnloadSceneAdditively("path to scene asset file.", sceneManager.Server.Players)
    }
}
```

:::note
If you want to send the additive scene to only specific players we can do it like so. You must get the player on your own.
:::

```cs
sceneManager.ServerLoadSceneAdditively("path to scene asset file.", Player)
```

## Virtual Methods

Some of the methods in NetworkSceneManager can be overridden to customize how it works

- Start
- OnDestroy
- ClientStartSceneMessage
- ClientFinishedLoadingSceneMessage
- ClientNotReadyMessage
- OnServerAuthenticated
- OnServerPlayerDisconnected

### Example - OnServerAuthenticated

By default OnServerAuthenticated sends the active scene and all additive scenes to the client, It can be overridden to only send the active scene:

```cs 
public class MySceneManager : NetworkSceneManager
{
    protected internal override void OnServerAuthenticated(INetworkPlayer player)
    {
        // just load server's active scene instead of all additive scenes as well
        player.Send(new SceneMessage { MainActivateScene = ActiveScenePath });
        player.Send(new SceneReadyMessage());
    }
}
```
### Example - Start

By default, `Start` registers all our listeners for scene management handling. If you need to override it then do this and add your stuff.

```cs
public class MySceneManager : NetworkSceneManager
{
    protected internal override void Start()
    {
        // add your stuff before.

        base.Start();

        // add your stuff after.
    }
}
```

### Example - OnDestroy

By default OnDestroy de-registers all our listener's for scene management handling. If you need to override it then do this and add your stuff.

```cs
public class MySceneManager : NetworkSceneManager
{
    protected internal override void OnDestroy()
    {
        // add your stuff before.

        base.OnDestroy();

        // add your stuff after.
    }
}
```
