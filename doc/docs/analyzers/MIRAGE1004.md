# MIRAGE1004: Invalid SyncVar Hook Method

## The Problem
A field marked with `[SyncVar]` specifies a hook that cannot be resolved, is ambiguous, or does not have a supported callable form.

Mirage's Weaver generates hook calls when synchronized values change. Unresolved hooks and parameter mismatches produce Weaver errors. This analyzer also identifies unsupported forms before code generation; not every invalid form has a specific Weaver diagnostic.

SyncVar hook requirements:

1. **Declaration:** The hook must be declared on the same type that declares the SyncVar. An inherited-only method or event is not found for a newly declared SyncVar. A virtual or abstract method declared beside a base SyncVar can dispatch to a derived override.
2. **Methods:** Use a nongeneric `void` method with 0, 1, or 2 by-value parameters: `void Hook()`, `void Hook(T newValue)`, or `void Hook(T oldValue, T newValue)`. Parameter types must match the SyncVar type exactly; implicit conversions and `ref`/`in`/`out` parameters are not supported. A hook on a generic behaviour may use that behaviour's type parameter `T`; the hook method itself must not introduce type parameters.
3. **Events:** Use a field-like `System.Action`, `System.Action<T>`, or `System.Action<T, T>` event. Mirage invokes its backing field. Custom delegates, delegate fields that are not events, and events with explicit `add`/`remove` accessors are not supported.
4. **Selection:** `SyncHookType.Automatic` requires one supported matching signature. Matching methods with different supported parameter counts, such as `Hook()` and `Hook(T, T)`, are ambiguous. Explicit `hookType` selects a method/event kind and parameter count. Automatic lookup is not C# overload resolution; use a distinct name or explicit `hookType` when unrelated overloads exist.
5. **Static and visibility:** Both static and instance hooks are supported, including private methods and static events.

An omitted or empty hook name means no hook. Setting `invokeHookOnServer` or `invokeHookOnOwner` to `true` without a valid hook is also an error.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

---

## How to Resolve
- **Case 1 (Missing Hook):** Declare the hook on the type that declares the SyncVar, and use its exact name, preferably through `nameof`.
- **Case 2 (Type Mismatch):** Ensure hook parameters match the SyncVar's type exactly.
- **Case 3 (Invalid Delegate):** Use a field-like `System.Action` event with the appropriate type arguments.
- **Case 4 (Ambiguity):** Explicitly set the matching `hookType`, or give the intended hook a unique name.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}
