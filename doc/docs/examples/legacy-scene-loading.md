---
sidebar_position: 30
---

# Legacy Scene Loading

The **LegacySceneLoading** sample contains the old `NetworkSceneManager` and `CharacterSpawner` components. They used to be part of Mirage, and are kept as a sample for projects that still use them.

:::warning Legacy Components
For new projects use [NetworkSceneLoader](/docs/guides/scene-loading/network-scene-loader) instead, or see [Manual Scene Loading](/docs/guides/scene-loading/manual-scene-loading). None of the other samples use these components.
:::

## What's Included

- **`NetworkSceneManager`**: Keeps the active scene in sync between server and clients, including additive scenes. See [Network Scene Manager](/docs/guides/scene-loading/network-scene-manager) for how to use it.
- **`CharacterSpawner`**: Spawns a player character when a client connects or finishes changing scene, with optional start positions.

## Using It

1. Import the **LegacySceneLoading** sample from the Package Manager.
2. Add `NetworkSceneManager` and `CharacterSpawner` to your network GameObject, the same as in older versions of Mirage.

If you are upgrading a project that had these components on its network GameObject, importing this sample fixes the "missing script" warnings. The scripts kept their original GUIDs when they moved to the sample, so existing scenes find them again.
