---
id: OnData
title: OnData
---

# Delegate OnData


Delegate for handling incoming data from a connection.
Should only be invoked from within .




##### Syntax

```cs
public delegate void OnData(IConnectionHandle handle, ReadOnlySpan<byte> data);
```

