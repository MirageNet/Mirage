# Using NetworkSceneManager

<xref:Mirage.NetworkSceneManager> contains methods and logic to help keep the scene in sync between server and client.

## How to use

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
class MySceneManager : NetworkSceneManager
{
    protected internal override void OnServerAuthenticated(INetworkPlayer player)
    {
        // just load server's active scene instead of all additive scenes as well
        player.Send(new SceneMessage { MainActivateScene = ActiveScenePath });
        player.Send(new SceneReadyMessage());
    }
}
```