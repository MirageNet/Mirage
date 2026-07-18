# MIRAGE1004: Invalid SyncVar Hook Method

## The Problem
A field marked with `[SyncVar]` specifies a hook name in its attribute, but the hook cannot be resolved, is ambiguous, or does not match the required signature.

Mirage's IL Weaver post-processes compiled assemblies by intercepting field writes to call user-defined hook methods/events. For this injection to succeed, the hook must be a valid method or event in the declaring class and must match the expected signatures. If the Weaver cannot resolve the hook, it will throw a compilation error, halting the build.

Specifically, the rules for SyncVar hooks are:
1. **Resolution:** The method or event must exist in the class.
2. **Signature Matching:** Parameters of the hook method or event must match the type of the SyncVar exactly.
   - For Methods: `0`, `1`, or `2` parameters are allowed (e.g., `void Hook()`, `void Hook(T newValue)`, or `void Hook(T oldValue, T newValue)`).
   - For Events: Must be a `System.Action` (generic or non-generic) with `0`, `1`, or `2` parameters.
3. **Ambiguity:** Under the default `SyncHookType.Automatic` mode, if multiple overloads (e.g., a method and an event, or methods with different parameter counts) exist with the same name, the Weaver cannot automatically determine which to call and will throw an error. Use explicit `hookType` or rename/remove overloads to resolve.
4. **Static Constraints:** Both methods and events can be `static` (fully supported by Weaver).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

---

## How to Resolve
Depending on the case, apply the following fix:

- **Case 1 (Missing Hook):** Verify the spelling of the hook name and ensure the method exists within the class.
- **Case 2 (Type Mismatch):** Ensure all parameters match the `SyncVar`'s type exactly (e.g., don't use `float` for a `double` SyncVar).
- **Case 3 (Invalid Delegate):** Ensure hook events use `System.Action` delegates.
- **Case 4 (Ambiguity):** Explicitly define the `hookType` in the `[SyncVar]` attribute if multiple overloads of the hook name exist.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}
