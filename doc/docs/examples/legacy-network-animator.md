---
sidebar_position: 31
---

# Legacy Network Animator

The **LegacyNetworkAnimator** sample contains the old `NetworkAnimator` component. It syncs Animator parameters and states, and is kept for projects that still use it.

:::warning Legacy Component
`NetworkAnimator` is not optimized and is not recommended for new projects. Usually it is better to sync the gameplay state (for example a `[SyncVar]` for "is moving") and let each client drive its own Animator, or use a `[ClientRpc]` for one-off triggers. The [Additive Scenes](/docs/examples/additive-scenes) example does this for its tank's fire animation.
:::

## Using It

1. Import the **LegacyNetworkAnimator** sample from the Package Manager.
2. Add `NetworkAnimator` to an object with a `NetworkIdentity` and an `Animator`.

See [Network Animator](/docs/components/network-animator) for the component's settings.
