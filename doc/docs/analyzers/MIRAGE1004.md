# MIRAGE1004: Invalid SyncVar Hook Method

## When this appears

A `[SyncVar]` names a hook that is missing, ambiguous, or unsupported.

Declare the hook on the same type as the SyncVar; inherited-only hooks are not found. A virtual or abstract method declared beside a base SyncVar can dispatch to a derived override.

For a SyncVar of type `T`, supported signatures are:

| Arguments | Method | Field-like event type |
| --- | --- | --- |
| None | `void Hook()` | `System.Action` |
| New value | `void Hook(T newValue)` | `System.Action<T>` |
| Old and new values | `void Hook(T oldValue, T newValue)` | `System.Action<T, T>` |

- Methods must be nongeneric. A generic behaviour's type parameter is allowed; the method cannot introduce its own.
- Parameters must match `T` exactly and be passed by value. Implicit conversions and `ref`/`in`/`out` are unsupported.
- Events need a backing field. Custom delegates, ordinary delegate fields, and explicit `add`/`remove` accessors are unsupported.
- Static and instance hooks are supported, including private methods and static events.

`SyncHookType.Automatic` requires one supported matching signature. Matching methods with different argument counts are ambiguous. Automatic lookup is not C# overload resolution; unrelated overloads can interfere.

An omitted or empty hook name means no hook. `invokeHookOnServer = true` or `invokeHookOnOwner = true` requires a valid hook.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

## How to fix

Declare a supported hook and reference its name with `nameof`. Resolve ambiguity or interfering overloads with a unique name or explicit `hookType`, which selects the method/event kind and argument count.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}
