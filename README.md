[![Documentation](https://img.shields.io/badge/documentation-brightgreen.svg)](https://miragenet.github.io/Mirage/)
[![Discord](https://img.shields.io/discord/809535064551456888.svg)](https://discordapp.com/invite/DTBPBYvexy)
[![release](https://img.shields.io/github/release/MirageNet/Mirage.svg)](https://github.com/MirageNet/Mirage/releases/latest)
[![openupm](https://img.shields.io/npm/v/com.miragenet.mirage?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.miragenet.mirage/)

[![Build](https://github.com/MirageNet/Mirage/workflows/CI/badge.svg)](https://github.com/MirageNet/Mirage/actions?query=workflow%3ACI)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=alert_status)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)
[![SonarCloud Coverage](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=coverage)](https://sonarcloud.io/component_measures?id=MirageNet_Mirage&metric=coverage)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=ncloc)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=sqale_index)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=code_smells)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)

## What is Mirage?

Mirage is a rolling-release **high level** API for the Unity Game Engine that provides a **powerful, yet easy to use** networking API. Be it a online MMO, co-op adventure game or a first person shooter, Mirage improves your networked projects' **probability of success** significantly. With its modular structure, Mirage lets you unleash maximum performance out of your networking. 

Networked objects in the client are mirror images of the objects in the server, and the API provides all the tools necessary to keep them in sync and pass messages between them.

Mirage is a rolling-release network stack. With every update, you get the latest features and bug fixes. You are **encouraged** to diagnose, report and help fix bugs that you find: Issue tickets will be investigated, feature requests are considered and pull requests are regularly reviewed.

Mirage is built by passionate network engineers and is backed by a friendly community.

## Installation

To install Mirage, follow these steps:

1) Mirage requires at least Unity 2020 LTS. You may install [Unity 2020 LTS](https://unity.com/) via the Unity website or via the Unity Hub. <br/>
    You may use newer versions, however _LTS versions are strongly recommended_ as newer versions can contain bugs, glitches or just flat out break game projects.
2) Start a new project or open your existing one. If opening an exsiting one, it is **strongly recommended** to back it up before installing Mirage.
4) Add the OpenUPM registry.  Click on the `Edit` menu, then select `Project settings...`, select `Package Manager` and add a scoped registry like so: <br/>
    Name: `OpenUPM` <br/>
    Url: `https://package.openupm.com` <br/>
    Scopes:
    - `com.cysharp.unitask`
    - `com.openupm`
    - `com.miragenet`
   ![Scoped Registry](doc/images/Scoped%20Registry.png)
4) Close the project settings.
5) Open the package manager by clicking on the `Window` menu and selecting `Package Manager`. Then select `Packages`, `My Registries`, select the latest version of Mirage and click install, like so:
   ![Install Mirage](doc/images/Install%20Mirage.png)
6) You may come back to the package manager at any time to uninstall Mirage or upgrade it.

## Migrating from Mirror

If you've got a project already using Mirror and you want to migrate it to Mirage, it's recommended to check out our [Migration Guide](https://miragenet.github.io/Mirage/Articles/Guides/MirrorMigration.html) for a smooth transition. Also check the heading below, as there are some major differences between Mirage and the other network library.

## Comparison with Mirror

Mirage has some notable differences from its distant sister, Mirror. The table below briefly details them:

| Mirage                                              | Mirror                                 |
| --------------------------------------------------- | -------------------------------------- |
| Installs via Unity Package Manager                  | Installs from Asset Store              |
| Errors are thrown as exceptions                     | Errors are logged                      |
| `[ServerRpc]`                                       | `[Command]`                            |
| `[ClientRpc(target = RpcTarget.Owner)]`             | `[TargetRpc]`                          |
| Subscribe to events in `NetworkServer`              | Override methods in `NetworkManager`   |
| Subscribe to events in `NetworkClient`              | Override methods in `NetworkManager`   |
| Subscribe to events in `NetworkIdentity`            | Override methods in `NetworkBehaviour` |
| Methods use PascalCase (C# guidelines)              | No consistency                         |
| `NetworkTime` available in `NetworkBehaviour`       | `NetworkTime` is global static         |
| Send any data as messages                           | Messages must implement NetworkMessage |
| Supports Unity 2020 LTS or later                    | Supports Unity 2019 LTS or later       |
| Offers simple Socket API to implement new protocols | Each protocol requires a new transport |

**Some notable features that Mirage has:**

* [Fast play mode support](https://blogs.unity3d.com/2019/11/05/enter-play-mode-faster-in-unity-2019-3/)
* Clients can connect to multiple servers - for example, be connected to a chat server while connected to a game server
* Components can be added in child objects
* Modular API: You only use the components you need
* Error handling
* [Version defines](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#define-symbols)
* Server Rpcs can [return values](https://miragenet.github.io/Mirage/Articles/Guides/RemoteCalls/ServerRpc.html)
* Bit packing to help compress values and reduce bandwidth

Peeking under the hood, Mirage is built upon fundamental pillars: 

* Mirage avoids singletons and static state in general
* Mirage follows the [SOLID principles](https://en.wikipedia.org/wiki/SOLID)
* Mirage has high [![Test Coverage](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=coverage)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)
* Mirage has low [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=MirageNet_Mirage&metric=sqale_index)](https://sonarcloud.io/dashboard?id=MirageNet_Mirage)

## Development environment

If you want to contribute to Mirage, follow these steps:

### Linux and Mac

1) Install the git client
2) Clone this repo
    ```sh
    cd /path/to/somewhere/on/your/disk
    mkdir MirageNetworking
    git clone https://github.com/MirageNet/Mirage.git MirageNetworking
    ```
3) Open the newly cloned repo in Unity 2020 LTS or later.

### Windows

1) Install [git](https://git-scm.com/download/win) or use your favorite git client (Fork, SourceTree, etc)
2) As administrator, clone this repo with symbolic links support using Git Bash:
    ```sh 
    cd C:\UnityProjects\DontReallyUseThisExamplePath
    mkdir MirageNetworking
    git clone -c core.symlinks=true https://github.com/MirageNet/Mirage.git
    ```
    It you don't want to use administrator, [add symlink support](https://www.joshkel.com/2018/01/18/symlinks-in-windows/) to your account.
    If you don't enable symlinks, you will be able to work on Mirage but Unity will not see the examples.
3) Open in Unity 2020 LTS or later.

## Transport and Sockets

Mirage supports multiple ways of transporting data:
- Native UDP socket (default on Windows, Mac and Linux) with fallback to C# UDP Sockets (default on other platforms)
- Steam ([Facepunch Steamworks](https://github.com/MirageNet/SteamyFaceNG))
- WebSocket for WebGL clients ([SimpleWebSocket](https://github.com/James-Frowen/SimpleWebSocket))

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :smiley:

The team will review it ASAP and give it the stamp of approval, ask for changes or decline it with a detailed explaination. 

Thank you for using Mirage and we hope to see your project be successful!
