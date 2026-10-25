---
name: mirage
title: Cheat Sheet & AI Skill
sidebar_label: Cheat Sheet (AI Skill)
sidebar_position: 3
description: Read this to get information about Mirage Networking (SyncVars, RPCs, Sync Direction, network events, and rate limits). Always read this before editing netcode.
---

## Full Documentation (Agent Access)

The skill below is a summary. For full details, fetch the online docs:

- **Index** (start here to discover all pages): `https://miragenet.github.io/Mirage/llms.txt`
- **Individual pages**: `https://miragenet.github.io/Mirage/docs/<path>.md`

Fetch `llms.txt` first to find the right page, then fetch the specific `.md` for full detail.

# Mirage Code Guidelines

Mirage is a high-level networking library for Unity, built as a hard fork of UNet/Mirror.

## Enums
### `Mirage.Channel` (use full name to avoid collisions)
- `Mirage.Channel.Reliable` (default)
- `Mirage.Channel.Unreliable`. 

### `RpcTarget` (use full name to avoid collisions)
- `RpcTarget.Owner`
- `RpcTarget.Observers` (default)
- `RpcTarget.Player` (add `INetworkPlayer target` as first method parameter to pass what player to target)

## Attributes & Options

### `[SyncVar]`
Syncs server field to clients.
Options:
- `hook`: Client method/event name called on value change.
- `initialOnly`: Sync only on spawn, ignores future changes.
- `invokeHookOnServer`: Fires hook on server. (Note: In Host mode, the hook always fires regardless of this value).
- `invokeHookOnOwner`: Fires hook on owner client.
- `hookType`: enum `SyncHookType` values: `[Automatic, MethodWith0Arg, MethodWith1Arg, MethodWith2Arg, EventWith0Arg, EventWith1Arg, EventWith2Arg]`
If field is struct/class, treat it as a property. You can't change sub-fields, the whole value must be set again for mirage to detect changes (like `transform.position` and `.y`)
#### Hook Invoke order
- only if value changed
- on host/server: hook invoked in callstack that SyncVar is set
- on client
  - on spawn: (before `OnStartClient`) all SyncVars for that Behaviour are set, then hooks are invoked (if value changed).
    - note: this is per class, base class will have their SyncVar and hooks invoked before child class runs theirs.
  - on change: value is set, then hook is invoke (if value changed), then next syncvar/hook is set/invoked.

### Sync Direction (Inspector / Component Settings)
Configures which side acts as the source of truth for `[SyncVar]` properties on a `NetworkBehaviour`:
- `Server -> Owner, Observers` (Default / Recommended):
  - Server updates `[SyncVar]` values (e.g. inside `[ServerRpc]` or server timers), which replicate down to the Owner client and Observers.
  - Required for server-authoritative abilities, duration timers, or health systems.
- `Owner -> Observers, Server` (Owner Authority):
  - Owner client directly mutates `[SyncVar]` fields on its local client, replicating to Server and Observers.
  - **Warning**: When set to Owner direction, server-side SyncVar assignments will not replicate back to the Owner client, causing Owner/Server desync if the server attempts to manage state.
- `Owner,Server` -> `Owner,Server` (bidirectional):
  - this allows either owner or server to set a syncvar and have to replicated to each other 
  - add `Observers` as well to have server forward changes to other clients
  - this has desync risk if both owner and server set value at the same time, both will send update message causing race condition

### `[ServerRpc]`
Invoked on server from client.
Options:
- `channel`: `Mirage.Channel` enum
- `requireAuthority`: Requires client authority (default `true`).
- `allowServerToCall`: Allows local server execution bypassing authority (default `false`).
Add `INetworkPlayer sender = null` as last parameter if validation is needed (only useful if `requireAuthority=false`)
Can have return value using `UniTask<MyValue>`

### `[ClientRpc]`
Invoked on clients from server.
Options:
- `channel`: `Mirage.Channel` enum
- `target`: `RpcTarget` enum
- `excludeOwner`: Excludes owner client from execution.
- `excludeHost`: Stops execution on host/server.
Can have return value using `UniTask<MyValue>` (if target is Owner or Player)

### Metadata Attributes
- `[NetworkMessage]`: Generates reader/writer for class/struct. Mostly an optional hint.

### **Security** `[RateLimit]`
Applies rate limiting to `[ServerRpc]`
Options:
- `Interval`: Refill Interval, in seconds (default `1f`).
- `Refill`: Refill per interval (default `50`).
- `MaxTokens`: Bucket capacity (default `200`).
- `Penalty`: Error Cost if bucket is empty (default `1`).

### **Security** `[MaxLength]`
Enforces a limit on the size of serialized strings, arrays, or lists to protect against memory allocation attacks
- Apply to `[ServerRpc]` parameters, SyncVar or NetworkMessage fields.
- Enforces char/element count.
- Client does check before sending and throws `SerializationLimitException` is limit is hit to avoid sending
- Example:
  - Field: `[MaxLength(32)] public string PlayerName;`
  - RPC parameters: `public void CmdUpdateName([MaxLength(24)] string name)`


## Events/Callbacks

### NetworkIdentity (Register in `Awake`)
- `OnStartServer`: Invoked while spawning, before `SpawnMessage` is sent
- `OnAuthorityChanged(bool)`: Authority given/removed. Called before right `OnStartClient` when spawning
- `OnStartClient`: Invoked while spawning, after SyncVars (and hooks) are set
- `OnStartLocalPlayer`: Called right after `OnStartClient`, if make character for player
- `OnOwnerChanged(INetworkPlayer)`: Owner assigned (server-only).
- `OnStopClient`: Unspawned/destroyed on client.
- `OnStopServer`: Unspawned on server.

### NetworkServer (Register anywhere)
- `Started`: Called when server starts (in host mode, called before host is connected)
- `Stopped`: Server start/stop. (use)
- `Authenticated(INetworkPlayer player)`: Player fully connected, (use this instead of `Connected`)
- `Disconnected(INetworkPlayer player)`
- `OnStartHost` / `OnStopHost`: Host mode start/stop.

### NetworkClient (Register anywhere)
- `Started`: Called when client starts (in host mode, called before host is connected)
- `Authenticated`: Player fully connected, (use this instead of `Connected`)
- `Disconnected(ClientStoppedReason reason)`: Called when Disconnect.

## Player Error Handling
INetworkPlayer has `SetError(int cost, PlayerErrorFlags flags)` method that can be used to flag errors caused by players.
Players will then be disconnected if they cause too many errors in a short amount of time.
### Enum Flags `PlayerErrorFlags` 
- `None`
Mirage Flags
- `RpcNullException` throw inside RPC
- `RpcException` throw inside RPC
- `DeserializationException` throw inside NetworkReader
- `RpcSync` RPC internal values incorrect, likely out-of-date build
- `RateLimit`
- `NoAuthority` no object authority
- `Unauthenticated` NetworkMessage sent before authentication was finished
- `Critical` other errors, likely cheater, should be kicked right away
- `LikelyCheater`
- `CustomError` custom errors that can be set by game, starting at `1 << 16`
