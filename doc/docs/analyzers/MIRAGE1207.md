# MIRAGE1207: Missing RateLimit on ServerRpc

## The Problem
A `[ServerRpc]` method lacks a `[RateLimit]` attribute.

Without a rate limit, clients can spam requests, potentially causing server lag, memory issues, or disconnects.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-triggering' }}}

---

## How to Resolve
Add a `[RateLimit]` attribute to the `[ServerRpc]` method.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-resolved' }}}
