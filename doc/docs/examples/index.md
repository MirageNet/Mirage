---
sidebar_position: 1
---

# Samples Overview

Mirage includes several samples to help you learn how to use various features and how to set things up so they work together.

## Import Samples

Samples can be imported via the Unity Package Manager in the **Samples** section of the Mirage package. Each sample is imported on its own into `Assets/Samples/Mirage/<version>/<SampleName>/`.

![Sample dropdown](/img/examples/UPM-samples.png)

## Playable Samples

Scenes you can open, press Play, and connect to with a host and clients.

- [Basic](/docs/examples/basic)  
  The simplest setup: players connect, get a player number, and show live data synced from the server with SyncVars.
- [Coin Grab Arena](/docs/examples/coin-grab-arena)  
  A complete top-down arena game with round states, synced countdown timer via `NetworkTime`, server-authoritative coin pickups, and a live scoreboard.
- [Pong](/docs/examples/pong)  
  2-player Pong with server-side ball physics, left/right paddle spawning, and client-authority paddles.
- [ChangeScene](/docs/examples/change-scene)  
  Switching scenes for all players with `NetworkSceneLoader`, and loading an extra scene additively.
- [Chat](/docs/examples/chat)  
  A chat room using a custom network message to create the player, and RPCs to send chat messages.
- [Additive Scenes](/docs/examples/additive-scenes)  
  Loading a sub-scene for each player when they enter a zone, with visibility that only shows sub-scene objects to players who have loaded it.
- [Multiple Additive Scenes](/docs/examples/multiple-additive-scenes)  
  Several isolated game instances on one server, each in its own additive physics scene.
- [Tanks](/docs/examples/tanks)  
  A small tank battle with ready-up, server-spawned projectiles, health and score.
- [Interest Management](/docs/examples/interest-management)  
  Hundreds of objects, but each player only receives the ones near them, using `NetworkProximityChecker`.
- [Match Scenes](/docs/examples/match-scenes)  
  2-player matches on one server, each match in its own scene.
- [Rigidbody Physics](/docs/examples/rigidbody-physics)  
  Server-side rigidbody physics synced to clients with `NetworkTransform`.

## Code Samples

Scripts that show how to do one thing, to add to your own project. They don't include a scene.

- [Spawn Custom Player](/docs/examples/spawn-custom-player)  
  Letting each client choose their character (race, name, colors) before the server spawns it.
- [Send Prefab](/docs/examples/send-prefab)  
  Letting a client ask the server to spawn one of an allowed list of prefabs, with validation and rate limiting.
- [Notify](/docs/examples/notify)  
  Sending changes over the unreliable channel, using Notify to know which messages were delivered.
- [Switch Transport](/docs/examples/configurable-network-manager)  
  A `NetworkManager` that can switch between UDP, Steam or offline before starting.

## Legacy Samples

Old components that are no longer part of the main package, kept for projects that still use them.

- [Legacy Scene Loading](/docs/examples/legacy-scene-loading)  
  `NetworkSceneManager` and `CharacterSpawner`.
- [Legacy Network Animator](/docs/examples/legacy-network-animator)  
  `NetworkAnimator`.
