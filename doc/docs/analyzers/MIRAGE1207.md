# MIRAGE1207: Missing RateLimit on ServerRpc

## The Problem
A `[ServerRpc]` method is declared without a `[RateLimit]` attribute.

To prevent denial of service (DoS) attacks, server CPU strain, and memory bloat from client RPC spam, it is highly recommended to apply a `[RateLimit]` attribute to every `[ServerRpc]` method. Without a rate limit, a malicious client could flood the server with requests, leading to server performance degradation or player disconnects.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-triggering' }}}

---

## How to Resolve

Add a `[RateLimit]` attribute to the `[ServerRpc]` method with appropriate parameters for the expected rate of call.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-resolved' }}}
