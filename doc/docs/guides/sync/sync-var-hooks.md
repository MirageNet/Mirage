---
sidebar_position: 3
---
# Sync Var Hooks

`SyncVar` can have hooks that are invoked when the values changes.

Hooks are set using the `hook` option on the `SyncVar` attribute, the hook needs to be in the same class as the `SyncVar`

```cs
[SyncVar(hook = nameof(HookName))]
```


A hook can be a method or a event, when using an event it should use `System.Action`. 

The hook can have 0, 1 or 2 args.



```cs
void hook0() { }

void hook1(int newValue) { }

void hook2(int oldValue, int newValue) { }

event Action event0;

event Action<int> event1;

event Action<int, int> event2;
```


## When is hook invoked?

The following is a list of rules that SyncVar hooks follows for when and where they are invoked:

- Hooks are only invoked if value is changed and after the value is updated

- When settings SyncVar
  - both flags false
    - invokes if host (both Server AND client active)
  - `invokeHookOnOwner` flag true
    - invokes if owner
  - `invokeHookOnServer` flag true
    - invokes if server (includes host mode)
  - both flags true
    - invokes if owner OR server

- `DeserializeSyncVars` is never called on host sending update to itself, but is called when owner sends update to server

- Hooks are invoked in `DeserializeSyncVars` if values changes 
  - Always invokes if Only client (eg not host mode)
  - Invoked after the variable is updated with the deserialized value.
  - `invokeHookOnServer`
    - Invokes on server (eg when an change is send from owner)
