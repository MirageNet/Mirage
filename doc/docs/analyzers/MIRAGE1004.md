# MIRAGE1004: Invalid SyncVar Hook Method

## The Problem
A field marked with `[SyncVar]` specifies a hook name, but the hook cannot be resolved, is ambiguous, or does not match the required signature.

During assembly post-processing, Mirage's Weaver intercepts SyncVar field writes to call user-defined hook methods or events. If the hook cannot be resolved or does not match the expected signatures, it causes a compilation error.

SyncVar hook requirements:
1. **Existence:** The method or event must exist in the class.
2. **Signature:** Parameters must match the SyncVar type exactly.
   - **Methods:** 0, 1, or 2 parameters (e.g., `void Hook()`, `void Hook(T newValue)`, or `void Hook(T oldValue, T newValue)`).
   - **Events:** Must use `System.Action` (generic or non-generic) with 0, 1, or 2 parameters.
3. **Ambiguity:** Under `SyncHookType.Automatic` mode, if multiple overloads (e.g., a method and an event, or methods with different parameter counts) exist with the same name, the Weaver cannot choose and throws an error. Explicitly define `hookType` or rename/remove the overloads.
4. **Static:** Both static and instance methods/events are supported.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

---

## How to Resolve
- **Case 1 (Missing Hook):** Ensure the hook method name is spelled correctly and exists in the class.
- **Case 2 (Type Mismatch):** Ensure hook parameters match the SyncVar's type exactly.
- **Case 3 (Invalid Delegate):** Ensure hook events use `System.Action` delegates.
- **Case 4 (Ambiguity):** Explicitly set `hookType` in the `[SyncVar]` attribute, or resolve the overloads.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}
