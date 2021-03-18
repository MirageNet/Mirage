> [!IMPORTANT]
> ! This page is work in process

### Concepts
World
- List of NetworkIdentities
- List of Player
- Handles message

InterestManagerment (AOI)
- uses List of NetworkIdentities and List of Player
- decides which player can see which Identity

Player
- owns NetworkIdentities
- views NetworkIdentities (controlled by AOI/Visbility)
- has a connection (optional)

Peer 
- gate keeper
- has list of connections

ISocket
- gate to outside World

connections
- has list of players
- can send and receive data (raw byte)
    - can be reliable/unreliable/notify

messages
- unpacked/deserialized form of raw byte
- has message Id
- messages are sent between Machines, no sending remote message to local handlers

Machines
- unity instance 

RPC
- function that exists on GameObjects
- Client RPC: 
    - invoked by the server
    - Rpc target players 
- Server RPC:
    - invoked by player
    - rpc targets server
- if player has no connection, invoke locally
- if player has connection, invoke remotely
- if "send to all" repeat above for each player




- invoked by players
- 
