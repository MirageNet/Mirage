# Mirage Networking

**Mirage is an open source, easy to use and modular networking library for Unity.**

## Key Features
- **Modularity:** Mirage is built on top of modular components and is easily extensible. Use only the features that you need.
- **Shared codebase:** Both server and client can share the same code in a single project which improves productivity and simplifies development.
- **No static state:** Run multiple servers/clients from a single Unity instance.
- **Low bandwidth usage:** Use our [BitPacking](Articles/Guides/BitPacking/index.md) features to greatly reduce bandwidth.
- **State synchronization:** Use [SyncVars](Articles/Guides/Sync/index.md) to easily synchronize your game state over the network.
- **Server authoritative:** Utilize the [Server RPCs](Articles/Guides/Communications/RemoteActions.md) to do server-authoritative tasks.
- **Message system:** Send classes or structs as network messages for low-level operations.
- **Simple socket API:** Easy to implement new protocols or services. High performance UDP socket built in.
- **Unity's Fast domain reload support**
- **Package Manager support:** Use Unity Package Manager (UPM) to easily install Mirage and any of its modules.
- **Great community:** Visit our [Discord server](https://discord.gg/DTBPBYvexy) to discuss your ideas or problems.

## Getting started
Check out our [Getting Started](Articles/General/Start.md) guide.

## Requirements
Mirage requires Unity 2019.3 or higher.