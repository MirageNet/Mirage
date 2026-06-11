---
sidebar_position: 2
---
# Sync Var
[`SyncVars`](/docs/reference/Mirage/SyncVarAttribute) are properties of classes that inherit from [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour), which are synchronized from the server to clients. When a game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. Use the [[SyncVar]](/docs/reference/Mirage/SyncVarAttribute) custom attribute to specify which variables in your script you want to synchronize.

:::note
The state of SyncVars is applied to game objects on clients before [Identity.OnStartClient](/docs/reference/Mirage/NetworkIdentity#onstartclient) event is invoked, so the state of the object is always up-to-date in subscribed callbacks.
:::


SyncVars can use any [type supported by Mirage](/docs/guides/serialization/data-types). You can have up to 64 SyncVars on a single NetworkBehaviour script, including [SyncLists](/docs/guides/sync/sync-objects/sync-list) and other sync types.

The server automatically sends SyncVar updates when the value of a SyncVar changes, so you do not need to track when they change or send information about the changes yourself. Changing a value in the inspector will not trigger an update.

:::note
SyncVars are not sent right away or in the order they are set. They will be sent as a group in the next sync update.
:::

## Why Properties Instead of Fields?
Mirage uses properties `{ get; set; }` for `[SyncVar]` variables instead of fields to naturally intercept variable reads and writes without rewriting user code at all the access locations. 

When a property is accessed, C# executes standard getter and setter methods under the hood. During compilation, Mirage's Weaver intercepts these methods, clears their compiler-generated bodies, and injects serialization, dirty tracking, and hook dispatching logic. This enables clean, transparent synchronization that behaves identically across all calling contexts.

### Auto-Property Requirement
`[SyncVar]` properties **must** be simple auto-properties. They cannot have custom `get` or `set` accessors. 

Because the Weaver completely clears the compiled method body of the getter and setter to inject the synchronization logic, any custom logic written inside user-defined accessors would be lost. The Weaver verifies that only simple compiler-generated auto-properties (which only read/write directly to their backing field) are used, and will throw a `SyncVarException` at build time if any custom logic is detected.

## Example
Let's have a simple `Player` class with the following code:

{{{ Path:'Snippets/Sync/SyncVarExamples.cs' Name:'SyncVarBasicExample' }}}

In this example, when Player A clicks the left mouse button, he sends a [ServerRpc](/docs/guides/remote-actions/server-rpc) to the server where the `clickCount` SyncVar is incremented. All other visible players will be informed about Player A's new `clickCount` value.

## Class inheritance
SyncVars work with class inheritance. Consider this example:

{{{ Path:'Snippets/Sync/SyncVarExamples.cs' Name:'SyncVarInheritanceExample' }}}

You can attach the Cat component to your cat prefab, and it will synchronize both its `name` and `color`.

:::caution
Both `Cat` and `Pet` should be in the same assembly. If they are in separate assemblies, make sure not to change `name` from inside `Cat` directly, add a method to `Pet` instead. 
:::

## SyncVar hook
The `hook` option of SyncVar attribute can be used to specify a function to be called when the SyncVar changes value on the client and server.

For more information on SyncVar hooks see [Sync Var Hooks](/docs/guides/sync/sync-var-hooks)

### Example Client Only
Below is a simple example of assigning a random color to each player when they're spawned on the server.  All clients will see all players in the correct colors, even if they join later.

{{{ Path:'Snippets/Sync/SyncVarExamples.cs' Name:'SyncVarClientOnlyHookExample' }}}

### Example Client & Server
Below is a simple example of assigning a random color to each player when they're spawned on the server. All clients will see all players in the correct colors, even if they join later, the server will also fire the event.

{{{ Path:'Snippets/Sync/SyncVarExamples.cs' Name:'SyncVarServerClientHookExample' }}}

## SyncVar Initialize Only

Just like regular SyncVars, when a game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. 
With the `initialOnly` flag set to true you will now be able to control the state of the SyncVar manually rather than waiting for Mirage to update them. 

:::note
Make sure you manually update your observable clients with the new state.  
Syncvar Hooks become redundant, as you are setting the state of the Syncvar directly.
:::

### Example

{{{ Path:'Snippets/Sync/SyncVarExamples.cs' Name:'SyncVarInitialOnlyExample' }}}

## Protecting SyncVars from Allocation Attacks

When synchronizing strings or collections (such as custom types with length-restricted reader/writer) inside `SyncVars`, you can restrict their deserialization size using the `[MaxLength(int)]` attribute to protect against memory allocation attacks.

Memory allocation attacks occur when a client receives a sync payload with a maliciously crafted length header, forcing the receiver to pre-allocate a massive array or string, leading to Out of Memory (OOM) crashes.

```cs
using Mirage;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Restricts the display name to a maximum of 32 characters
    [SyncVar, MaxLength(32)]
    public string displayName { get; set; }
}
```

Applying `[MaxLength(N)]` ensures that if a serialized update exceeds the maximum character count for strings or the maximum element count for collections, Mirage throws a `SerializationLimitException` before the allocation occurs. This aborts deserialization, flags the sender with `PlayerErrorFlags.SerializationLimit`, and applies an error rate-limit penalty of 100 on the connection.
