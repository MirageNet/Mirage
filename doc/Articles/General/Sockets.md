# Sockets
Sockets are low level objects that are responsible for sending and receiving raw data over the network. Mirage has a single transport (called <xref:Mirage.SocketLayer.Peer>) built into its core, which handles all the connection states and reliability. Peer then uses the low level sockets API to send and receive raw data over the network. Check [this section](#implementing-new-socket) below to see how to implement your own socket.

## Available sockets
The table below shows a list of available sockets and their features.

|                | UDP (built-in)                       | Steam ([SteamWorks.NET](https://github.com/MirageNet/FizzySteamyMirror), [Facepunch](https://github.com/MirageNet/SteamyFaceNG))   |
| -------------- | :----------------------------------: | :--------------------------------------------------------------------------------------------------------------------------------: |
| **CCU**        | 1000+                                | ?                                                                                                                                  |
| **Protocol**   | UDP                                  | UDP                                                                                                                                |
| **Unreliable** | :white_check_mark:                   | :white_check_mark:                                                                                                                 |
| **Mobile**     | :white_check_mark:                   |                                                                                                                                    |
| **CPU**        | LOW                                  | LOW                                                                                                                                |
| **NAT Punch**  |                                      | :white_check_mark:                                                                                                                 |
| **Encryption** |                                      | :white_check_mark:                                                                                                                 |
| **IPv6**       | :white_check_mark:                   | ?                                                                                                                                  |
| **Managed**    | :white_check_mark:                   | :white_check_mark:                                                                                                                 |
| **Native**     | :white_check_mark:                   |                                                                                                                                    |
| **Based on**   | NanoSockets (native), .NET (managed) | Steam Game Networking Sockets                                                                                                      |

## Changing a socket
To change a socket, follow the steps below:
1. Install the desired socket. Instructions are available on each socket's repository.
2. Add the socket's `SocketFactory` as a new component on the object where you have the other Mirage components.
3. Assign a reference to this component in the `Socket Factory` field of [NetworkServer](xref:Mirage.NetworkServer) and [NetworkClient](xref:Mirage.NetworkClient) components.
4. Check if the socket has any required additional steps.
5. Done. Mirage should now be using the newly added socket.

## Implementing new socket
To create a new socket, you have to implement two classes - one that implements the <xref:Mirage.SocketLayer.ISocket> interface, which will represent the low-level socket and one that extends the <xref:Mirage.SocketLayer.SocketFactory> class, which will act as the MonoBehaviour component creating new instances of the low-level class.

You can check out our built-in UDP socket implementation as an inspiration: <xref:Mirage.Sockets.Udp.UdpSocket> and <xref:Mirage.Sockets.Udp.UdpSocketFactory>.