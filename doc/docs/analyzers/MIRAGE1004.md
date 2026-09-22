# MIRAGE1004: Invalid SyncVar Hook Method

## When this appears

A `[SyncVar]` names a hook that is missing, invalid, or ambiguous.

Hooks let your code react to a changed value, such as refreshing a health bar. Weaver must be able to find and call the hook to generate a working callback.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

## How to fix

Declare the hook on the same type as the SyncVar and reference its name with `nameof`. Use a nongeneric `void` method or a field-like event.

For a SyncVar of type `T`, supported signatures are:

| Arguments | Method | Field-like event type |
| --- | --- | --- |
| None | `void Hook()` | `System.Action` |
| New value | `void Hook(T newValue)` | `System.Action<T>` |
| Old and new values | `void Hook(T oldValue, T newValue)` | `System.Action<T, T>` |

Parameters must match `T` exactly and be passed by value. Implicit conversions and `ref`/`in`/`out` are unsupported.

If overloads conflict, give the hook a unique name or set `hookType` to choose the method/event kind and argument count.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}

## Other hook cases

- Inherited-only hooks are not found. A virtual or abstract method declared beside a base SyncVar can dispatch to a derived override.
- A generic behaviour's type parameter is allowed; the hook method cannot introduce its own. Static and instance hooks are supported, including private methods and static events.
- Events need a backing field. Custom delegates, ordinary delegate fields, and explicit `add`/`remove` accessors are unsupported.
- `SyncHookType.Automatic` requires one matching signature. Matching methods with different argument counts are ambiguous. Automatic lookup is not C# overload resolution; unrelated overloads can interfere.
- An omitted or empty hook name means no hook. `invokeHookOnServer = true` or `invokeHookOnOwner = true` requires a valid hook.
