# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## When this appears

An override of `NetworkBehaviour.OnSerialize` or `OnDeserialize` omits its base call. This can skip SyncObjects, inherited custom data, and generated SyncVars—including those declared on the overriding class. Ordinary properties are not automatically synchronized.

### Triggering example

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-triggering' }}}

## How to fix

- Call the base method once on every serialization path.
- Keep base calls and custom data in matching order on both sides. Pair each additional write with its corresponding read.
- Do not write SyncVars again; the base call handles them.
- Preserve the base return value when custom writes are conditional. Return `true` when custom data is always written.

Mark the behaviour dirty when custom data changes. Returning `true` does not schedule an update; dirty state and `SyncSettings` control synchronization.

Suppress the warning only for a deliberate complete replacement with a verified matching reader and writer.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-resolved' }}}
