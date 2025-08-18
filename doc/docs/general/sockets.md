---
sidebar_position: 3
---

# Sockets
Sockets are low-level objects that are responsible for sending and receiving raw data over the network. 
Mirage has a single transport (called `Mirage.SocketLayer.Peer`) built into its core, which handles all the connection 
states and reliability. Peer then uses the low-level sockets API to send and receive raw data over the network. 
Check [this section](#implementing-new-socket) below to see how to implement your own socket.

## Available sockets
The table below shows a list of commonly used sockets and their features. Please note that not all of these may be included directly with the Mirage package; some might require separate installation.

| | UDP (built-in) | Steam ([Steamworks.NET](https://github.com/MirageNet/FizzySteamyMirror), [Facepunch](https://github.com/MirageNet/SteamyFaceNG)) | [Websocket](https://github.com/James-Frowen/SimpleWebSocket)            | 
| - | :-: | :-: | :-: |
| **CCU** | 1000+ | ? | ? |
| **Protocol** | UDP | UDP | TCP |
| **Unreliable** | :white_check_mark: | :white_check_mark: | :x: |
| **Mobile**     | :white_check_mark: | :x: | :white_check_mark: |
| **WebGL**      | :x: | :x: | :white_check_mark: |
| **CPU**        | LOW | LOW | ? |
| **NAT Punch**  | :x: | :white_check_mark: | :x: |
| **Encryption** | :x: | :white_check_mark: | :white_check_mark: |
| **IPv6**       | :white_check_mark: | ? | ? |
| **Managed**    | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| **Native**     | :white_check_mark: | :x: | :x: |
| **Based on**   | NanoSockets (native), .NET (managed) | Steam Game Networking Sockets                                                                                                      | [SimpleWebTransport](https://github.com/James-Frowen/SimpleWebTransport) |

## Changing a socket
To change a socket, follow the steps below:
1. Install the desired socket. Instructions are available on each socket's repository.
2. Add the socket's `SocketFactory` as a new component on the object where you have the other Mirage components.
3. Assign a reference to this component in the `Socket Factory` field of `NetworkServer` and `NetworkClient` components.
4. Check if the socket has any required additional steps.
5. Done. Mirage should now be using the newly added socket.

## Implementing new socket
To create a new socket, you have to implement two classes - one that implements the `Mirage.SocketLayer.ISocket`
 interface, which will represent the low-level socket and one that extends the `Mirage.SocketLayer.SocketFactory` 
 class, which will act as the MonoBehaviour component creating new instances of the low-level class.

You can check out our built-in UDP socket implementation as an inspiration: `Mirage.Sockets.Udp.UdpSocket` and 
`Mirage.Sockets.Udp.UdpSocketFactory`.