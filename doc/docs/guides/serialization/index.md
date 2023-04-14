---
sidebar_position: 1
---
# Serialization

This section of the Mirage documentation covers different aspects of how data is serialized, including the types of data that Mirage supports, advanced serialization techniques, the use of generics, and the SyncPrefab struct for synchronizing prefabs over the network.

- [Data Types](/docs/guides/serialization/data-types)
  Information about the different data types supported in Mirage, including basic C# types, Unity math types, NetworkIdentity, and GameObject with NetworkIdentity.e

- [Advanced Serialization](/docs/guides/serialization/advanced)
  In-depth explanation of how serialization works in Mirage, including how it is implemented using Weaver and Mono.Ccil.

- [Generics](/docs/guides/serialization/generics)
  How Mirage supports generic types for SyncVar, Rpcs, and fields in NetworkMessages.

- [SyncPrefab](/docs/guides/serialization/sync-prefab)
  Explains the SyncPrefab struct in Mirage, which is used to synchronize prefabs over the network. It is particularly useful for short-lived objects like visual effects, audio, or projectiles.

