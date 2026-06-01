# MIRAGE1401: Accessing Network State in Awake/Start

## The Problem
Reading or writing network states (such as `IsServer`, `IsClient`, `HasAuthority`, or `SyncVar` values) inside Unity's lifecycle methods `Awake` or `Start`.

Unity's `Awake` and `Start` methods are called during GameObject initialization. At this point, Mirage's network identity is not yet spawned or initialized, meaning properties like `IsServer`, `IsClient`, and authority states are not set, and `SyncVars` have not been initialized with their network values. Accessing them inside `Awake` or `Start` results in incorrect behavior, default values, or race conditions.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

---

## How to Resolve

Override `OnStartServer`, `OnStartClient`, `OnStartLocalPlayer`, or `OnStartAuthority` to run network initialization code when the network state is fully ready.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}
