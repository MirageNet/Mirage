# [8.0.0](https://github.com/MirrorNG/MirrorNG/compare/7.0.0-master...8.0.0-master) (2020-02-06)


### Bug Fixes

* call callback after update dictionary in host ([#1476](https://github.com/MirrorNG/MirrorNG/issues/1476)) ([1736bb0](https://github.com/MirrorNG/MirrorNG/commit/1736bb0c42c0d2ad341e31a645658722de3bfe07))
* port network discovery ([d6a1154](https://github.com/MirrorNG/MirrorNG/commit/d6a1154e98c52e7873411ce9d7b87f7b294dc436))
* remove scriptableobject error Tests ([479b78b](https://github.com/MirrorNG/MirrorNG/commit/479b78bf3cabe93938bf61b7f8fd63ba46da0f4a))
* Telepathy reverted to older version to fix freezes when calling Client.Disconnect on some platforms like Windows 10 ([d0d77b6](https://github.com/MirrorNG/MirrorNG/commit/d0d77b661cd07e25887f0e2f4c2d72b4f65240a2))
* Telepathy updated to latest version. Threads are closed properly now. ([4007423](https://github.com/MirrorNG/MirrorNG/commit/4007423db28f7044f6aa97b108a6bfbe3f2d46a9))


* Renamed localEulerAnglesSensitivity (#1474) ([eee9692](https://github.com/MirrorNG/MirrorNG/commit/eee969201d69df1e1ee1f1623b55a78f6003fbb1)), closes [#1474](https://github.com/MirrorNG/MirrorNG/issues/1474)


### breaking

* Transports can now provide their Uri ([#1454](https://github.com/MirrorNG/MirrorNG/issues/1454)) ([b916064](https://github.com/MirrorNG/MirrorNG/commit/b916064856cf78f1c257f0de0ffe8c9c1ab28ce7)), closes [#38](https://github.com/MirrorNG/MirrorNG/issues/38)


### Features

* Implemented NetworkReaderPool ([#1464](https://github.com/MirrorNG/MirrorNG/issues/1464)) ([9257112](https://github.com/MirrorNG/MirrorNG/commit/9257112c65c92324ad0bd51e4a017aa1b4c9c1fc))
* LAN Network discovery ([#1453](https://github.com/MirrorNG/MirrorNG/issues/1453)) ([e75b45f](https://github.com/MirrorNG/MirrorNG/commit/e75b45f8889478456573ea395694b4efc560ace0)), closes [#38](https://github.com/MirrorNG/MirrorNG/issues/38)
* Mirror Icon for all components ([#1452](https://github.com/MirrorNG/MirrorNG/issues/1452)) ([a7efb13](https://github.com/MirrorNG/MirrorNG/commit/a7efb13e29e0bc9ed695a86070e3eb57b7506b4c))
* supports scriptable objects ([4b8f819](https://github.com/MirrorNG/MirrorNG/commit/4b8f8192123fe0b79ea71f2255a4bbac404c88b1))


### BREAKING CHANGES

* localEulerAnglesSensitivity renamed to localRotationSensitivity
* Make the server uri method mandatory in transports

Co-authored-by: MrGadget <chris@clevertech.net>

# [7.0.0](https://github.com/MirrorNG/MirrorNG/compare/6.0.0-master...7.0.0-master) (2020-01-27)


### Features

* Network Scene Checker Component ([#1271](https://github.com/MirrorNG/MirrorNG/issues/1271)) ([71c0d3b](https://github.com/MirrorNG/MirrorNG/commit/71c0d3b2ee1bbdb29d1c39ee6eca3ef9635d70bf))
* network writer and reader now support uri ([0c2556a](https://github.com/MirrorNG/MirrorNG/commit/0c2556ac64bd93b9e52dae34cf8d84db114b4107))


* Rename NetworkServer.localClientActive ([7cd0894](https://github.com/MirrorNG/MirrorNG/commit/7cd0894853b97fb804ae15b8a75b75c9d7bc04a7))
* Simplify spawning ([c87a38a](https://github.com/MirrorNG/MirrorNG/commit/c87a38a4ff0c350901138b90db7fa8e61b1ab7db))


### BREAKING CHANGES

* rename localClientActive to LocalClientActive
* Spawn no longer receives NetworkClient

# [6.0.0](https://github.com/MirrorNG/MirrorNG/compare/5.0.0-master...6.0.0-master) (2020-01-22)


### Bug Fixes

* compilation error ([df7baa4](https://github.com/MirrorNG/MirrorNG/commit/df7baa4db0d347ee69c17bad9f9e56ccefb54fab))
* compilation error ([dc74256](https://github.com/MirrorNG/MirrorNG/commit/dc74256fc380974ad6df59b5d1dee3884b879bd7))
* Fix Room Slots for clients ([#1439](https://github.com/MirrorNG/MirrorNG/issues/1439)) ([268753c](https://github.com/MirrorNG/MirrorNG/commit/268753c3bd0a9c0695d8d4510a129685be364a11))

# [5.0.0](https://github.com/MirrorNG/MirrorNG/compare/4.0.0-master...5.0.0-master) (2020-01-19)

# [4.0.0](https://github.com/MirrorNG/MirrorNG/compare/3.1.0-master...4.0.0-master) (2020-01-18)

# [3.1.0](https://github.com/MirrorNG/MirrorNG/compare/3.0.4-master...3.1.0-master) (2020-01-16)


### Bug Fixes

* Decouple ChatWindow from player ([#1429](https://github.com/MirrorNG/MirrorNG/issues/1429)) ([42a2f9b](https://github.com/MirrorNG/MirrorNG/commit/42a2f9b853667ef9485a1d4a31979fcf1153c0d7))
* StopHost with offline scene calls scene change twice ([#1409](https://github.com/MirrorNG/MirrorNG/issues/1409)) ([a0c96f8](https://github.com/MirrorNG/MirrorNG/commit/a0c96f85189bfc9b5a936a8a33ebda34b460f17f))
* Telepathy works on .net core again ([cb3d9f0](https://github.com/MirrorNG/MirrorNG/commit/cb3d9f0d08a961b345ce533d1ce64602f7041e1c))


### Features

* Add Sensitivity to NetworkTransform ([#1425](https://github.com/MirrorNG/MirrorNG/issues/1425)) ([f69f174](https://github.com/MirrorNG/MirrorNG/commit/f69f1743c54aa7810c5a218e2059c115760c54a3))

## [3.0.4](https://github.com/MirrorNG/MirrorNG/compare/3.0.3-master...3.0.4-master) (2020-01-12)


### Bug Fixes

* comply with MIT license in upm package ([b879bef](https://github.com/MirrorNG/MirrorNG/commit/b879bef4295e48c19d96a1d45536a11ea47380f3))

## [3.0.3](https://github.com/MirrorNG/MirrorNG/compare/3.0.2-master...3.0.3-master) (2020-01-12)


### Bug Fixes

* auto reference mirrorng assembly ([93f8688](https://github.com/MirrorNG/MirrorNG/commit/93f8688b39822bb30ed595ca36f44a8a556bec85))
* MirrorNG works with 2019.2 ([9f35d6b](https://github.com/MirrorNG/MirrorNG/commit/9f35d6be427843aa7dd140cde32dd843c62147ce))

## [3.0.2](https://github.com/MirrorNG/MirrorNG/compare/3.0.1-master...3.0.2-master) (2020-01-12)


### Bug Fixes

* remove Tests from upm package ([#34](https://github.com/MirrorNG/MirrorNG/issues/34)) ([8d8ea0f](https://github.com/MirrorNG/MirrorNG/commit/8d8ea0f10743044e4a9a3d6c5b9f9973cf48e28b))

## [3.0.1](https://github.com/MirrorNG/MirrorNG/compare/3.0.0-master...3.0.1-master) (2020-01-11)


### Bug Fixes

* remove Tests from UPM ([#33](https://github.com/MirrorNG/MirrorNG/issues/33)) ([8f42af0](https://github.com/MirrorNG/MirrorNG/commit/8f42af0a3992cfa549eb404ad9f9693101895ce9))

# [3.0.0](https://github.com/MirrorNG/MirrorNG/compare/2.0.0-master...3.0.0-master) (2020-01-11)


### Bug Fixes

* [#723](https://github.com/MirrorNG/MirrorNG/issues/723) - NetworkTransform teleport works properly now ([fd7dc5e](https://github.com/MirrorNG/MirrorNG/commit/fd7dc5e226a76b27250fb503a98f23eb579387f8))
* fix release pipeline ([2a3db0b](https://github.com/MirrorNG/MirrorNG/commit/2a3db0b398cd641c3e1ba65a32b34822e9c9169f))
* release job requires node 10 ([3f50e63](https://github.com/MirrorNG/MirrorNG/commit/3f50e63bc32f4942e1c130c681dabd34ae81b117))
* remove tests from npm package ([#32](https://github.com/MirrorNG/MirrorNG/issues/32)) ([5ed9b4f](https://github.com/MirrorNG/MirrorNG/commit/5ed9b4f1235d5d1dc54c3f50bb1aeefd5dbe3038))
* syntax error in release job ([2eeaea4](https://github.com/MirrorNG/MirrorNG/commit/2eeaea41bc81cfe0c191b39da912adc565e11ec7))


### Features

* Network Animator can reset triggers ([#1420](https://github.com/MirrorNG/MirrorNG/issues/1420)) ([dffdf02](https://github.com/MirrorNG/MirrorNG/commit/dffdf02be596db3d35bdd8d19ba6ada7d796a137))
* NetworkAnimator warns if you use it incorrectly ([#1424](https://github.com/MirrorNG/MirrorNG/issues/1424)) ([c30e4a9](https://github.com/MirrorNG/MirrorNG/commit/c30e4a9f83921416f936ef5fd1bb0e2b3a410807))


### Performance Improvements

* Use NetworkWriterPool in NetworkAnimator ([#1421](https://github.com/MirrorNG/MirrorNG/issues/1421)) ([7d472f2](https://github.com/MirrorNG/MirrorNG/commit/7d472f21f9a807357df244a3f0ac259dd431661f))
* Use NetworkWriterPool in NetworkTransform ([#1422](https://github.com/MirrorNG/MirrorNG/issues/1422)) ([a457845](https://github.com/MirrorNG/MirrorNG/commit/a4578458a15e3d2840a49dd029b4c404cadf85a4))

# [2.0.0](https://github.com/MirrorNG/MirrorNG/compare/1.1.2-master...2.0.0-master) (2020-01-09)

## [1.1.2](https://github.com/MirrorNG/MirrorNG/compare/1.1.1-master...1.1.2-master) (2020-01-09)


### Bug Fixes

* [#1241](https://github.com/MirrorNG/MirrorNG/issues/1241) - Telepathy updated to latest version. All tests are passing again. Thread.Interrupt was replaced by Abort+Join. ([228b32e](https://github.com/MirrorNG/MirrorNG/commit/228b32e1da8e407e1d63044beca0fd179f0835b4))
* [#1278](https://github.com/MirrorNG/MirrorNG/issues/1278) - only call initial state SyncVar hooks on clients if the SyncVar value is different from the default one. ([#1414](https://github.com/MirrorNG/MirrorNG/issues/1414)) ([a3ffd12](https://github.com/MirrorNG/MirrorNG/commit/a3ffd1264c2ed2780e6e86ce83077fa756c01154))
* [#1380](https://github.com/MirrorNG/MirrorNG/issues/1380) - NetworkConnection.clientOwnedObjects changed from uint HashSet to NetworkIdentity HashSet for ease of use and to fix a bug where DestroyOwnedObjects wouldn't find a netId anymore in some cases. ([a71ecdb](https://github.com/MirrorNG/MirrorNG/commit/a71ecdba4a020f9f4648b8275ec9d17b19aff55f))
* FinishLoadSceneHost calls FinishStart host which now calls StartHostClient AFTER server online scene was loaded. Previously there was a race condition where StartHostClient was called immediately in StartHost, before the scene change even finished. This was still from UNET. ([df9c29a](https://github.com/MirrorNG/MirrorNG/commit/df9c29a6b3f9d0c8adbaff5a500e54abddb303b3))

## [1.1.1](https://github.com/MirrorNG/MirrorNG/compare/1.1.0-master...1.1.1-master) (2020-01-05)


### Bug Fixes

* add Changelog metadata fix [#31](https://github.com/MirrorNG/MirrorNG/issues/31) ([c67de22](https://github.com/MirrorNG/MirrorNG/commit/c67de2216aa331de10bba2e09ea3f77e6b1caa3c))

# [1.1.0](https://github.com/MirrorNG/MirrorNG/compare/1.0.0-master...1.1.0-master) (2020-01-04)


### Features

* include generated changelog ([#27](https://github.com/MirrorNG/MirrorNG/issues/27)) ([a60f1ac](https://github.com/MirrorNG/MirrorNG/commit/a60f1acd3a544639a5e58a8946e75fd6c9012327))
