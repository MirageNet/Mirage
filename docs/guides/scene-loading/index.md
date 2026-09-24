# Loading Scenes in Mirage

There are several approaches to scene loading in Mirage, depending on how much control you need:

- **[NetworkSceneLoader](/docs/guides/scene-loading/network-scene-loader)** — A ready-made component that implements the "Join Any Time" pattern. Recommended for most projects.
- **[Manual Scene Loading](/docs/guides/scene-loading/manual-scene-loading)** — Full control over the scene loading flow using built-in messages. Use this if you need custom logic beyond what `NetworkSceneLoader` provides.
- **[(Legacy) NetworkSceneManager](/docs/guides/scene-loading/network-scene-manager)** — Legacy component, kept for reference but not recommended for new projects.