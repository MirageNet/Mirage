# Overview

These core components are included in Mirage:

## Core & Foundation

-   [Network Identity](/docs/components/network-identity)
    The Network Identity component controls a game object’s unique identity on the network.

-   [Network Manager](/docs/components/network-manager)
    NetworkManager is a helper class with instance references to the other major parts of the Mirage network.

## Visibility & Scene Management

-   [Scene Visibility Checkers](/docs/components/network-scene-checker)
    These components control the visibility of game objects for network clients based on which scene they're in.

-   [Network Proximity Checker](/docs/components/network-proximity-checker)
    The Network Proximity Checker component controls the visibility of game objects for network clients, based on proximity to players.

-   [Network Match Checker](/docs/components/network-match-checker)
    The Network Match Checker component controls the visibility of networked objects based on match id.

-   [Network Scene Manager](/docs/components/network-scene-manager)
    The Network Scene Manager component controls the Unity Scenes running over the network.

## Network Synchronization

-   [Network Animator](/docs/components/network-animator)
    The Network Animator component allows you to synchronize animation states for networked objects. It synchronizes state and parameters from an Animator Controller.

-   [Network Transform](/docs/components/network-transform)
    The Network Transform component synchronizes the movement and rotation of game objects across the network.

-   [Network Transform Child](/docs/components/network-transform-child)
    The Network Transform Child component synchronizes the position and rotation of the child game object of a game object with a Network Transform component.

## Networking Utilities

-   [Network Manager HUD](/docs/components/network-manager-hud)
    The Network Manager HUD is a quick-start tool to help you start building your multiplayer game straight away.

-   [Network Ping Display](/docs/components/network-ping-display)
    Network Ping Display shows the ping time for clients using a UI Text component.

-   [Network Discovery](/docs/components/network-discovery)
    The Network Discovery component allows clients to find and connect to running servers on the local network.

-   [Ready Check](/docs/components/ready-check)
    The Ready Check component manages the ready state of players.

## Authentication

-   [Authentication Guide](/docs/guides/authentication)
    Learn about authentication processes and available authenticators in Mirage.

## Logging & Debugging

-   [Network Log Settings](/docs/components/network-log-settings)
    The Network Log Settings component provides granular control over log levels for different parts of your game.
