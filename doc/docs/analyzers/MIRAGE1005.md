# MIRAGE1005: Invalid SyncVar Hook Method

## The Problem
A property marked with `[SyncVar]` specifies a hook name in its attribute, but the hook cannot be resolved, is static (for events), is ambiguous, or does not match the required signature.

Mirage's IL Weaver post-processes compiled assemblies by intercepting property writes to call user-defined hook methods/events. For this injection to succeed, the hook must be a valid method or event in the declaring class and must match the expected signatures. If the Weaver cannot resolve the hook, it will throw a compilation error, halting the build.

Specifically, the rules for SyncVar hooks are:
1. **Resolution:** The method or event must exist in the class.
2. **Signature Matching:** Parameters of the hook method or event must match the type of the SyncVar exactly.
   - For Methods: `0`, `1`, or `2` parameters are allowed (e.g., `void Hook()`, `void Hook(T newValue)`, or `void Hook(T oldValue, T newValue)`).
   - For Events: Must be a `System.Action` (generic or non-generic) with `0`, `1`, or `2` parameters.
3. **Ambiguity:** Under the default `SyncHookType.Automatic` mode, if multiple overloads (e.g., a method and an event, or methods with different parameter counts) exist with the same name, the Weaver cannot automatically determine which to call and will throw an error. Use explicit `hookType` or rename/remove overloads to resolve.
4. **Static Constraints:**
   - **Methods:** Can be `static` (supported by Weaver).
   - **Events:** Cannot be `static` (Weaver requires instance events and will fail to compile).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-triggering' }}}

---

## How to Resolve
Verify the spelling of the hook name, ensure all parameters match the SyncVar's type exactly, avoid static events, and explicitly define the `hookType` if overload resolution is ambiguous.

{{{ Path:'Snippets/Analyzers/Mirage1005.cs' Name:'mirage1005-resolved' }}}
