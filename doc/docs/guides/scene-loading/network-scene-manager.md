---
title: (Legacy) Network Scene Manager
---
# Using Network Scene Manager

:::warning Legacy Component
`NetworkSceneManager` and `CharacterSpawner` are now considered legacy components and have been moved to the Examples folder. 

For modern projects, we recommend using the **[NetworkSceneLoader](/docs/guides/scene-loading/network-scene-loader)** component or the **[Manual Scene Loading](/docs/guides/scene-loading/manual-scene-loading)** guide.
:::

NetworkSceneManager contains methods and logic to help keep the scene in sync between server and client.

### How to use

The Network Scene Manager takes care of most of the grunt work that is needed to load unload and network scenes between server and client. The examples below show exactly how to use
the network scene manager.

## Load Scene Normally

This will load up a new scene on the server and tell all current player's loaded on the server to load the scene up.

{{{ Path:'Snippets/SceneLoading/LoadSceneNormal.cs' Name:'load-scene-normal' }}}

:::note
If you require physics scenes to load up on the server you can override the default parameter like so.
:::

{{{ Path:'Snippets/SceneLoading/LoadSceneNormalParams.cs' Name:'load-scene-normal-params' }}}

## Load Scene Additively

This will load a scene additively on the server and tell specific clients to do the same. Example shows send to everyone.

{{{ Path:'Snippets/SceneLoading/LoadSceneAdditively.cs' Name:'load-scene-additively' }}}

:::note
If you want to send the additive scene to only specific players we can do it like so. You must get the player on your own.
:::

{{{ Path:'Snippets/SceneLoading/LoadSceneAdditivelyPlayer.cs' Name:'load-scene-additively-player' }}}

:::note
Also if you want to load the scene normally to specific players versus additively like the server you can override the parameter to do so also. The server will still
load additively, the reason is if you need fully normal loading you can use the above method instead to do it.
:::

{{{ Path:'Snippets/SceneLoading/LoadSceneAdditivelyPlayer.cs' Name:'load-scene-additively-player-normal' }}}

:::note
Also if you want to load the scene in physic's mode you can override another parameter also to do so. You can also make clients load normally in the example below we keep it false to load
the client side additively too.
:::

{{{ Path:'Snippets/SceneLoading/LoadSceneAdditivelyPlayer.cs' Name:'load-scene-additively-player-physics' }}}

This will unload a scene additively on the server and tell specific clients to do the same. Example shows send to everyone.

{{{ Path:'Snippets/SceneLoading/UnLoadSceneAdditively.cs' Name:'unload-scene-additively' }}}

:::note
If you want to send the additive scene to only specific players we can do it like so. You must get the player on your own.
:::

{{{ Path:'Snippets/SceneLoading/LoadSceneAdditivelyPlayer.cs' Name:'load-scene-additively-player' }}}

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

{{{ Path:'Snippets/SceneLoading/CustomSceneManager.cs' Name:'custom-scene-manager-on-server-authenticated' }}}
### Example - Start

By default, `Start` registers all our listeners for scene management handling. If you need to override it then do this and add your stuff.

{{{ Path:'Snippets/SceneLoading/CustomSceneManager.cs' Name:'custom-scene-manager-start' }}}

### Example - OnDestroy

By default OnDestroy de-registers all our listener's for scene management handling. If you need to override it then do this and add your stuff.

{{{ Path:'Snippets/SceneLoading/CustomSceneManager.cs' Name:'custom-scene-manager-on-destroy' }}}
