# Overview

These core components are included in Mirage:

-   [Network Animator](/docs/components/network-animator)  
    The Network Animator component allows you to synchronize animation states for networked objects. It synchronizes state and parameters from an Animator Controller.
-   [Network Authenticator](/docs/components/authenticators/)  
    Network Authenticators facilitate integration of user accounts and credentials into your application.
-   [Network Discovery](/docs/components/network-discovery)  
    Network Discovery uses a UDP broadcast on the LAN enabling clients to find the running server and connect to it.
-   [Network Identity](/docs/components/network-identity)  
    The Network Identity component is at the heart of the Mirage networking high-level API. It controls a game object’s unique identity on the network, and it uses that identity to make the networking system aware of the game object. It offers two different options for configuration and they are mutually exclusive, which means either one of the options or none can be checked.
-   [Network LogSettings](/docs/components/network-log-settings)  
    Adds logging levels per class for Mirror components
-   [Network Manager](/docs/components/network-manager)  
    The Network Manager is a component for managing the networking aspects of a multiplayer game.
-   [Network Manager HUD](/docs/components/network-manager-hud)  
    The Network Manager HUD is a quick-start tool to help you start building your multiplayer game straight away, without first having to build a user interface for game creation/connection/joining. It allows you to jump straight into your gameplay programming, and means you can build your own version of these controls later in your development schedule.
-   [Network Match Checker](/docs/components/network-match-checker)  
    The Network Match Checker component controls visibility of networked objects based on match id.
-   [Network Ping Display](/docs/components/network-ping-display)
    Network Ping Display shows the Ping time for clients using OnGUI
-   [Network Proximity Checker](/docs/components/network-proximity-checker)  
    The Network Proximity Checker component controls the visibility of game objects for network clients, based on proximity to players.
-   [Network Rigidbody](/docs/components/network-rigidbody)
    The Network Rigidbody synchronizes velocity and other properties of a rigidbody across the network.
-   [Network Room Manager](/docs/components/network-room-manager)  
    The Network Room Manager is an extension component of Network Manager that provides a basic functional room.
-   [Network Room Player](/docs/components/network-room-player)  
    The Network Room Player is a component that's required on Player prefabs used in the Room Scene with the Network Room Manager above.
-   [Network Scene Checker](/docs/components/network-scene-checker)  
    The Network Scene Checker component controls visibility of networked objects between scenes.
-   [Network Start Position](/docs/components/network-start-position)  
    Network Start Position is used by the Network Manager when creating character objects. The position and rotation of the Network Start Position are used to place the newly created character object.
-   [Network Transform](/docs/components/network-transform)  
    The Network Transform component synchronizes the movement and rotation of game objects across the network. Note that the network Transform component only synchronizes spawned networked game objects.
-   [Network Transform Child](/docs/components/network-transform-child)  
    The Network Transform Child component synchronizes the position and rotation of the child game object of a game object with a Network Transform component.
