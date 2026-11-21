# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## When this appears

An override of `NetworkBehaviour.OnSerialize` or `OnDeserialize` is missing its base call.

Skipping the base call can stop SyncVars, SyncObjects, and your base class's custom data from being sent or read. This includes SyncVars declared in the derived class.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-triggering' }}}

## How to fix

- Call the base method once on every serialization path.
- Keep base calls and custom data in matching order on both sides. Pair each added write with its read.
- Do not write SyncVars again; the base call handles them.
- Keep the base return value when custom writes are conditional. Return `true` when custom data is always written.

Mark the behaviour dirty when custom data changes. Returning `true` does not schedule an update; dirty state and `SyncSettings` control when data is sent.

{{{ Path:'Snippets/Analyzers/Mirage1402.cs' Name:'mirage1402-resolved' }}}

If you replace the whole format yourself, suppress the warning only after checking both writer and reader. Ordinary properties are not automatically sent.
