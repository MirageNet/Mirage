---
id: OnDisconnect
title: OnDisconnect
---

# Delegate OnDisconnect


Delegate for handling a disconnection from a connection.
Should only be invoked from within .




##### Syntax

```cs
public delegate void OnDisconnect(IConnectionHandle handle, ReadOnlySpan<byte> data, string reason);
```

