## [154.1.1](https://github.com/MirageNet/Mirage/compare/v154.1.0...v154.1.1) (2025-12-10)


### Bug Fixes

* adding check to SendSpawnMessage to avoid server sending invalid SpawnMessage ([eb794e4](https://github.com/MirageNet/Mirage/commit/eb794e4f4a79e0b3839cb26889d913371b03b86f))

# [154.1.0](https://github.com/MirageNet/Mirage/compare/v154.0.0...v154.1.0) (2025-12-04)


### Bug Fixes

* using pools instead of static cache incase function is called 2nd time ([0fa39d9](https://github.com/MirageNet/Mirage/commit/0fa39d9897fe95dd349ae04660cf283604e92ac5))


### Features

* **ObjectManager:** adding optional filter for scene objects ([bb8294a](https://github.com/MirageNet/Mirage/commit/bb8294a2c66c0b1e227d1412600f8ba36e0564d9))

# [154.0.0](https://github.com/MirageNet/Mirage/compare/v153.3.2...v154.0.0) (2025-11-13)


* fix!: stopping _networkBehavioursCache from being automatically cleared on destroy ([9cd8c94](https://github.com/MirageNet/Mirage/commit/9cd8c94a9790fd7cd61eac09d84e72d2a425f23c))


### BREAKING CHANGES

* NetworkBehaviour list no longer cleared in OnDestroy or NetworkReset

## [153.3.2](https://github.com/MirageNet/Mirage/compare/v153.3.1...v153.3.2) (2025-10-26)


### Bug Fixes

* **Examples:** fixing UI in basic example ([7cd9ccb](https://github.com/MirageNet/Mirage/commit/7cd9ccb221c4f851d3f43808205f2cd1eaa5c542))

## [153.3.1](https://github.com/MirageNet/Mirage/compare/v153.3.0...v153.3.1) (2025-09-25)


### Bug Fixes

* swapping AttachExternalCancellation order ([7dfe8f7](https://github.com/MirageNet/Mirage/commit/7dfe8f7b9fdafaf847eac31175f1817a829580cb))

# [153.3.0](https://github.com/MirageNet/Mirage/compare/v153.2.6...v153.3.0) (2025-09-11)


### Bug Fixes

* adding check to SocketLayer config for MaxReliableFragments ([8b4a6ea](https://github.com/MirageNet/Mirage/commit/8b4a6ea7df515e5063578b2b4a87fd870e9e6a18))
* fixing max length check SendUnreliable ([5aa89c0](https://github.com/MirageNet/Mirage/commit/5aa89c0074a2aef29701a8184d88432591b0c476))


### Features

* adding helper methods to get max message size for each channel ([8d623fb](https://github.com/MirageNet/Mirage/commit/8d623fb4b24da0e21bf4157fa8ffea5a87ec119b))

## [153.2.6](https://github.com/MirageNet/Mirage/compare/v153.2.5...v153.2.6) (2025-09-11)


### Bug Fixes

* adding byte count to HandleMessage log ([6627d74](https://github.com/MirageNet/Mirage/commit/6627d74591ce3f243cb9f200bc68bb9b31f2604a))

## [153.2.5](https://github.com/MirageNet/Mirage/compare/v153.2.4...v153.2.5) (2025-08-31)


### Bug Fixes

* adding compile error when [NetworkMessage] is incorrectly added to generic class ([e9b3a39](https://github.com/MirageNet/Mirage/commit/e9b3a39220d37b31d40967dc9632eb2e8ba6c141))

## [153.2.4](https://github.com/MirageNet/Mirage/compare/v153.2.3...v153.2.4) (2025-08-27)


### Bug Fixes

* ensuring that SpawnVisibleObjects spawns NetworkIdentity in netId order ([de7a657](https://github.com/MirageNet/Mirage/commit/de7a657e0a65f34e785343201ada72a8aa651edb))

## [153.2.3](https://github.com/MirageNet/Mirage/compare/v153.2.2...v153.2.3) (2025-08-27)


### Bug Fixes

* improving logs of prefabHash to always be hex ([a6fdb94](https://github.com/MirageNet/Mirage/commit/a6fdb942e66b87208d702d163767fbc9cdf6d60d))

## [153.2.2](https://github.com/MirageNet/Mirage/compare/v153.2.1...v153.2.2) (2025-08-24)


### Bug Fixes

* fixing throw in NetworkIdentity.ToString ([585ae30](https://github.com/MirageNet/Mirage/commit/585ae305e68cd68a8881909a72beebe9166a490c))

## [153.2.1](https://github.com/MirageNet/Mirage/compare/v153.2.0...v153.2.1) (2025-08-24)


### Bug Fixes

* calling RemoveDestroyedObjects when PrepareToSpawnSceneObjects is called ([8750260](https://github.com/MirageNet/Mirage/commit/875026072ca023a50af0e1c7d34fbd979d2dc271))
* checking world is empty after ObjectManagers unspawn all identities ([73e0153](https://github.com/MirageNet/Mirage/commit/73e015378eec9ca7fc15794cdea476c98c7a78c8))
* fixing onUnspawn not called if client object was destroyed ([637c6d0](https://github.com/MirageNet/Mirage/commit/637c6d0e31f46f70b84a8d179f32f34257acc8ae))

# [153.2.0](https://github.com/MirageNet/Mirage/compare/v153.1.0...v153.2.0) (2025-08-24)


### Features

* making NetworkWorld.RemoveDestroyedObjects public ([a1beda3](https://github.com/MirageNet/Mirage/commit/a1beda3cf375d84d196ce00fcedc0fb70a2a10df))

# [153.1.0](https://github.com/MirageNet/Mirage/compare/v153.0.0...v153.1.0) (2025-08-22)


### Features

* adding NonAlloc read methods for collections ([535b347](https://github.com/MirageNet/Mirage/commit/535b3472325b5c1f16f73c9a135519b57d35b058))

# [153.0.0](https://github.com/MirageNet/Mirage/compare/v152.0.0...v153.0.0) (2025-08-01)


### Bug Fixes

* changing back to UniTaskCompletionSource so task completes same a frame earlier ([3669a6d](https://github.com/MirageNet/Mirage/commit/3669a6d783d1b614407f37099d31197b8b8da587))
* fixing pending dictionary changed in ServerStopped ([86df5ef](https://github.com/MirageNet/Mirage/commit/86df5ef1a0a897954a5c9b895e02bf2bf7f50d9a))


* feat!: improving NetworkAuthenticator to include CancellationToken for AuthenticateAsync ([98ed8f1](https://github.com/MirageNet/Mirage/commit/98ed8f129389dc7a19d0a6bb613704e3b81f647a))


### BREAKING CHANGES

* AuthenticateAsync override now needs CancellationToken cancellationToken argument

# [152.0.0](https://github.com/MirageNet/Mirage/compare/v151.3.3...v152.0.0) (2025-07-04)


* build!: Drop Unity 2020.3 LTS support ([989a784](https://github.com/MirageNet/Mirage/commit/989a7842bced580d059345d389613bc56787d68a))


### BREAKING CHANGES

* Unity 2020.3 LTS is no longer supported due to end-of-life and incompatibility with recent fixes. The minimum supported version is now 2021.3 LTS.

## [151.3.3](https://github.com/MirageNet/Mirage/compare/v151.3.2...v151.3.3) (2025-06-30)


### Bug Fixes

* fixing desync issue caused by `ClearShouldSyncDirtyOnly` using wrong time ([321badc](https://github.com/MirageNet/Mirage/commit/321badc845b5e0f5035028930513e651631eec70))
* fixing NetworkTime Ping not using unscaled time to check ([0dd327a](https://github.com/MirageNet/Mirage/commit/0dd327add09d133524a5b41d429f85c63dc9b5da))
* fixing SocketLayer not using unscaled time ([137fd79](https://github.com/MirageNet/Mirage/commit/137fd798217a017919113ab4d2728ac82e99421f))

## [151.3.2](https://github.com/MirageNet/Mirage/compare/v151.3.1...v151.3.2) (2025-06-08)


### Bug Fixes

* fixing syncvar not sending while timescale is zero ([869e756](https://github.com/MirageNet/Mirage/commit/869e756ee48167730fa48063257e263792c7cf8f))

## [151.3.1](https://github.com/MirageNet/Mirage/compare/v151.3.0...v151.3.1) (2025-05-28)


### Bug Fixes

* removing SpawnOrActivate and fixing StartHostClientObjects setup for objects spawned before host client is connected ([a5e1eb7](https://github.com/MirageNet/Mirage/commit/a5e1eb791aaf480125a53439ba5bf5f2964ed56c))

# [151.3.0](https://github.com/MirageNet/Mirage/compare/v151.2.11...v151.3.0) (2025-05-17)


### Features

* storing message for Async spawning and applying them after ([23c6fa8](https://github.com/MirageNet/Mirage/commit/23c6fa8363e16cf9711d31743556fd7b1e333318))

## [151.2.11](https://github.com/MirageNet/Mirage/compare/v151.2.10...v151.2.11) (2025-04-13)


### Bug Fixes

* fixing compile error from use of logger ([c98e5b0](https://github.com/MirageNet/Mirage/commit/c98e5b076a8822cabe32748c6ed1b8598159f2c7))
* fixing connection state in test setup ([750cb26](https://github.com/MirageNet/Mirage/commit/750cb263f43ef731bb71b50b9bdc1b424d1f7a88))
* making sure messageHandler isn't given message after disconnect ([f0376e1](https://github.com/MirageNet/Mirage/commit/f0376e1afc3dac29b01ca55f8ae95e3aeee7b198))
* making sure that AuthMessage can't be received twice ([c07a662](https://github.com/MirageNet/Mirage/commit/c07a662d5549ed5e0138c8cbf40f0884120950c7))

## [151.2.10](https://github.com/MirageNet/Mirage/compare/v151.2.9...v151.2.10) (2025-04-12)


### Bug Fixes

* **AuthenticatorSettings:** Fix missing semicolon ([f3cac70](https://github.com/MirageNet/Mirage/commit/f3cac702fb8dfd7b883caae64e2e450a350f0737))
* **AuthenticatorSettings:** fix NullReferenceException ([0073c27](https://github.com/MirageNet/Mirage/commit/0073c2755f85f0754e3039f72c838147b1ad2c14))

## [151.2.9](https://github.com/MirageNet/Mirage/compare/v151.2.8...v151.2.9) (2025-04-09)


### Bug Fixes

* fixing not supported version of NanoSocket to accept int bufferSize ([a067a70](https://github.com/MirageNet/Mirage/commit/a067a701b230a61381e36d71dcaa051c929d67e4)), closes [#1185](https://github.com/MirageNet/Mirage/issues/1185)

## [151.2.8](https://github.com/MirageNet/Mirage/compare/v151.2.7...v151.2.8) (2025-03-20)


### Bug Fixes

* catching BufferFullException from flush ([5dd80ea](https://github.com/MirageNet/Mirage/commit/5dd80ea3d7572839b40ba209086f68e92c9dcec8))
* using SendBufferFull from NetworkPlayer when disconnecting ([b6c98f9](https://github.com/MirageNet/Mirage/commit/b6c98f90ae894a1407296e0a8c2937e606744069))

## [151.2.7](https://github.com/MirageNet/Mirage/compare/v151.2.6...v151.2.7) (2025-03-17)


### Bug Fixes

* Auth success message wording in NetworkClient ([#1183](https://github.com/MirageNet/Mirage/issues/1183)) ([ff4873d](https://github.com/MirageNet/Mirage/commit/ff4873db57ad6bfab96c605f0020109f2638afda))

## [151.2.6](https://github.com/MirageNet/Mirage/compare/v151.2.5...v151.2.6) (2025-02-16)


### Bug Fixes

* fixing nullref from _logger ([c3a618c](https://github.com/MirageNet/Mirage/commit/c3a618c33de4b182bd86c287e64d71e793447deb))
* passing bufferSize to nanosocket class instead of UdpSocketFactory ([ea2791c](https://github.com/MirageNet/Mirage/commit/ea2791cd1d5947355942af0fb088502e5590fb16))

## [151.2.5](https://github.com/MirageNet/Mirage/compare/v151.2.4...v151.2.5) (2025-02-12)


### Bug Fixes

* Adding warning to old NetworkTransformBase ([e944f59](https://github.com/MirageNet/Mirage/commit/e944f597cde11ac91be16c9e083dee0d24c56b29))

## [151.2.4](https://github.com/MirageNet/Mirage/compare/v151.2.3...v151.2.4) (2025-02-10)


### Bug Fixes

* logging message name for Unauthenticated Message ([bd1b12b](https://github.com/MirageNet/Mirage/commit/bd1b12bbfa065a6f67045b46a653bb55e571cb4a))

## [151.2.3](https://github.com/MirageNet/Mirage/compare/v151.2.2...v151.2.3) (2025-02-04)


### Bug Fixes

* clearing references to help with GC collect ([c6d5596](https://github.com/MirageNet/Mirage/commit/c6d5596e88fc310de3907c2f2923979e4bd273ec))

## [151.2.2](https://github.com/MirageNet/Mirage/compare/v151.2.1...v151.2.2) (2025-02-04)


### Bug Fixes

* increasing logging when failing to connect to server ([d77c240](https://github.com/MirageNet/Mirage/commit/d77c24056e56c66fbfd62aba20d99e0bf65b05eb))

## [151.2.1](https://github.com/MirageNet/Mirage/compare/v151.2.0...v151.2.1) (2025-02-01)


### Bug Fixes

* clearing references to help with GC collection ([9dad099](https://github.com/MirageNet/Mirage/commit/9dad099f49bd1bfc09a544a1a0b934e2b193b9c8))

# [151.2.0](https://github.com/MirageNet/Mirage/compare/v151.1.1...v151.2.0) (2025-01-21)


### Features

* adding allowServerToCall option to ServerRpc ([01721fe](https://github.com/MirageNet/Mirage/commit/01721feeeb2c795e4fcd665d2235f62d844273cc))

## [151.1.1](https://github.com/MirageNet/Mirage/compare/v151.1.0...v151.1.1) (2025-01-07)


### Bug Fixes

* avoiding throw from ResendMessages when send queue is full ([a14a6fc](https://github.com/MirageNet/Mirage/commit/a14a6fce1eab262ab15666e0a5d842553591aad7))
* moving ResendMessages to update ([8bdb6c1](https://github.com/MirageNet/Mirage/commit/8bdb6c1c193f5c3222bfee99ea5320eb0013d23a))

# [151.1.0](https://github.com/MirageNet/Mirage/compare/v151.0.0...v151.1.0) (2024-12-28)


### Features

* adding custom inspector for NetworkIdentity ([8cc9179](https://github.com/MirageNet/Mirage/commit/8cc9179683f62a4c7916857986f5e652a8637590))

# [151.0.0](https://github.com/MirageNet/Mirage/compare/v150.0.0...v151.0.0) (2024-12-27)


### Bug Fixes

* fixing SetDirtyBit being called with mask equals 0 ([56d07cd](https://github.com/MirageNet/Mirage/commit/56d07cd2a4b558f03221faaa84c4c8e54a42cc13))
* improving error message for spawning scene objects ([91f89ea](https://github.com/MirageNet/Mirage/commit/91f89ea1af2dd809a2f52279e7002f8a09b3dec4))


* feat!: SpawnVisibleObjects now calls RemoveAllVisibleObjects ([fe14235](https://github.com/MirageNet/Mirage/commit/fe14235a720b32cf1d6e9606e6da7628a6496f4a))
* feat!: SpawnVisibleObjects no longer skips disabled gameObject ([d160e77](https://github.com/MirageNet/Mirage/commit/d160e77381b43fac33c3941e6f863366cd0cf510))


### BREAKING CHANGES

* SpawnVisibleObjects now calls RemoveAllVisibleObjects before spawning objects
* SpawnVisibleObjects no longer checks `identity.gameObject.activeSelf` before spawning objects

# [150.0.0](https://github.com/MirageNet/Mirage/compare/v149.8.0...v150.0.0) (2024-12-06)


* feat!: clearing SceneId when spawning object with PrefabHash ([d638088](https://github.com/MirageNet/Mirage/commit/d638088a5e920edf8fff7dcb56309d9074696b66))


### Features

* adding ClearSceneId function ([a492705](https://github.com/MirageNet/Mirage/commit/a492705f9716350d95eb4007d213000f904b2b37))


### BREAKING CHANGES

* SceneId is now cleared when calling Spawn with PrefabHash

# [149.8.0](https://github.com/MirageNet/Mirage/compare/v149.7.0...v149.8.0) (2024-12-02)


### Features

* making SceneId property public ([53de2a0](https://github.com/MirageNet/Mirage/commit/53de2a01e0ddb9aeda0aba824ae0071ead1337cd))

# [149.7.0](https://github.com/MirageNet/Mirage/compare/v149.6.0...v149.7.0) (2024-12-01)


### Features

* adding try catch for return RPC ([6b7e683](https://github.com/MirageNet/Mirage/commit/6b7e68326f29485edf3858695d4615a0b7083802))

# [149.6.0](https://github.com/MirageNet/Mirage/compare/v149.5.2...v149.6.0) (2024-11-22)


### Features

* adding UnscaledTime as log option ([ab21fbb](https://github.com/MirageNet/Mirage/commit/ab21fbbb36f33ffc71337dcc7d70990dd779a30a))

## [149.5.2](https://github.com/MirageNet/Mirage/compare/v149.5.1...v149.5.2) (2024-10-29)


### Bug Fixes

* fixing incorrect log message ([e05ab96](https://github.com/MirageNet/Mirage/commit/e05ab9642ac7776872b37fbdda2791bfb0751fce))

## [149.5.1](https://github.com/MirageNet/Mirage/compare/v149.5.0...v149.5.1) (2024-09-04)


### Bug Fixes

* fixing NRE when DestroyObject is called on unspawned object ([ce00f0d](https://github.com/MirageNet/Mirage/commit/ce00f0de3f50900ded3e52103381adbfb50449d3))

# [149.5.0](https://github.com/MirageNet/Mirage/compare/v149.4.3...v149.5.0) (2024-08-31)


### Bug Fixes

* fixing using in welcomeWindow for unity2021 ([c056f9e](https://github.com/MirageNet/Mirage/commit/c056f9eff6b0b81dc3b84b1d3a14e08e48e2e49e))


### Features

* adding "dont show" toggle to welcome window ([f3eae96](https://github.com/MirageNet/Mirage/commit/f3eae9657f9dce698bc7679782c6c5b82e0fdc2f))

## [149.4.3](https://github.com/MirageNet/Mirage/compare/v149.4.2...v149.4.3) (2024-08-31)


### Bug Fixes

* logging warning after disconnect instead of invoking other handler ([05a72fa](https://github.com/MirageNet/Mirage/commit/05a72facf10fada55b8e30ec3e26173801ce9c99))

## [149.4.2](https://github.com/MirageNet/Mirage/compare/v149.4.1...v149.4.2) (2024-08-21)


### Bug Fixes

* making RemoteCallCollection public ([7945bd1](https://github.com/MirageNet/Mirage/commit/7945bd1513509e026f2a49b8d87ce059a3324ce7))


### Performance Improvements

* adding IEquatable to SyncVar wrappers ([5bb203f](https://github.com/MirageNet/Mirage/commit/5bb203fd695dcaa2714629f6be3678f4ba19add6))
* using SendToMany for AddNewObservers ([69459f5](https://github.com/MirageNet/Mirage/commit/69459f54b22923c6c15228ee055567f101471419))

## [149.4.1](https://github.com/MirageNet/Mirage/compare/v149.4.0...v149.4.1) (2024-08-07)


### Bug Fixes

* **Weaver:** fixing AssemblyResolver failing to find system dlls ([0551a60](https://github.com/MirageNet/Mirage/commit/0551a602803798a0db1bcc79b4242e0df25ac640))

# [149.4.0](https://github.com/MirageNet/Mirage/compare/v149.3.1...v149.4.0) (2024-07-09)


### Bug Fixes

* disconnecting player if send buffer is full ([3ea2d5d](https://github.com/MirageNet/Mirage/commit/3ea2d5d9e615b6d5cf2f42a9b92ff32701fdb7e4))


### Features

* adding SocketLayerException to better handle errors ([c76b88a](https://github.com/MirageNet/Mirage/commit/c76b88a3eb123e0e1295c43416706fe4f8f5ecea))
* IsConnecting and IsConnected helper methods to NetworkPlayer ([d722758](https://github.com/MirageNet/Mirage/commit/d722758741dfff2e017866148382b735a34432ff))

## [149.3.1](https://github.com/MirageNet/Mirage/compare/v149.3.0...v149.3.1) (2024-07-02)


### Bug Fixes

* fixing SpawnVisibleObjects throwing if SpawnedIdentities is modified ([6cf96e6](https://github.com/MirageNet/Mirage/commit/6cf96e63ebe4d05ce3283cd7cac7a9889c9f0800))

# [149.3.0](https://github.com/MirageNet/Mirage/compare/v149.2.0...v149.3.0) (2024-06-10)


### Features

* making PadToByte public ([43363ba](https://github.com/MirageNet/Mirage/commit/43363baa44d00864cd533ac94bc0f1a3673d3dbf))

# [149.2.0](https://github.com/MirageNet/Mirage/compare/v149.1.1...v149.2.0) (2024-05-11)


### Features

* adding attribute to show SyncSettings in networkBehaviour if no syncvars ([dd6a6d6](https://github.com/MirageNet/Mirage/commit/dd6a6d68bfb7ae8cd8bde28577adec0e8361fad3))

## [149.1.1](https://github.com/MirageNet/Mirage/compare/v149.1.0...v149.1.1) (2024-05-06)


### Bug Fixes

* fixing initial send to observers if from server is false ([0ffa316](https://github.com/MirageNet/Mirage/commit/0ffa3169a479e516e838365127900cbc15bb59ec))

# [149.1.0](https://github.com/MirageNet/Mirage/compare/v149.0.1...v149.1.0) (2024-05-06)


### Features

* adding authenticatedOnly to SendToAll ([8b1117b](https://github.com/MirageNet/Mirage/commit/8b1117b4c64b529d0ea412e3539ef2b284083012))
* adding list for AuthenticatedPlayers in NetworkServer ([dc07c38](https://github.com/MirageNet/Mirage/commit/dc07c384d2998f1afa544fa631bb5d2293b6d76f))

## [149.0.1](https://github.com/MirageNet/Mirage/compare/v149.0.0...v149.0.1) (2024-05-03)


### Bug Fixes

* **SocketLayer:** fixing socket receive trying to handle message when length is negative ([b078387](https://github.com/MirageNet/Mirage/commit/b0783879cd68c6881b5b32e0d32a5e725f5895ab))

# [149.0.0](https://github.com/MirageNet/Mirage/compare/v148.4.3...v149.0.0) (2024-04-28)


* fix!: changing all time fields to be double ([432af2e](https://github.com/MirageNet/Mirage/commit/432af2e1afde117d2a8c91fbf2507374578a2dbb))


### Features

* adding VarDoublePacker ([3e9e2a3](https://github.com/MirageNet/Mirage/commit/3e9e2a3ac4ce35d49b6e9ad96b4dafc59d257da0))


### BREAKING CHANGES

* time fields are now double instead of float

## [148.4.3](https://github.com/MirageNet/Mirage/compare/v148.4.2...v148.4.3) (2024-04-26)


### Bug Fixes

* fixing possible NRE ([dc4d6e6](https://github.com/MirageNet/Mirage/commit/dc4d6e68bc1d97c419032dc38f4e80215586c8d9))

## [148.4.2](https://github.com/MirageNet/Mirage/compare/v148.4.1...v148.4.2) (2024-04-26)


### Performance Improvements

* adding batching to unreliable messages ([c77bf78](https://github.com/MirageNet/Mirage/commit/c77bf7805d808024cc08f567f608b8660c608a37))

## [148.4.1](https://github.com/MirageNet/Mirage/compare/v148.4.0...v148.4.1) (2024-04-19)


### Bug Fixes

* fixing return RPC not generating serialize functions ([3d162b8](https://github.com/MirageNet/Mirage/commit/3d162b8692c69a961feef1432f3981241f0de7a3))

# [148.4.0](https://github.com/MirageNet/Mirage/compare/v148.3.0...v148.4.0) (2024-04-16)


### Features

* adding callback that can be used to handle Authentication Failed ([1cdbfb6](https://github.com/MirageNet/Mirage/commit/1cdbfb69caeea2270bb774b4c2df2983dd79953a))

# [148.3.0](https://github.com/MirageNet/Mirage/compare/v148.2.0...v148.3.0) (2024-04-15)


### Features

* making NetworkIdentity.PrefabHash public ([0ec3753](https://github.com/MirageNet/Mirage/commit/0ec3753c09f5dd9857194e14e8b337b0d7dcc744))

# [148.2.0](https://github.com/MirageNet/Mirage/compare/v148.1.1...v148.2.0) (2024-04-14)


### Features

* adding generic NetworkBehaviorSyncvar for ease of use ([5caabd9](https://github.com/MirageNet/Mirage/commit/5caabd9bf60798a3112a359646d9d1af2b96c957))

## [148.1.1](https://github.com/MirageNet/Mirage/compare/v148.1.0...v148.1.1) (2024-04-03)


### Bug Fixes

* removing disconnected player from observer lists ([ece4500](https://github.com/MirageNet/Mirage/commit/ece4500d9c6b004ce2a3f9749b8d4a5935154ec3))

# [148.1.0](https://github.com/MirageNet/Mirage/compare/v148.0.2...v148.1.0) (2024-03-31)


### Bug Fixes

* adding error message if attribute is used on static method ([dc66a54](https://github.com/MirageNet/Mirage/commit/dc66a544586f3f3ded4ca20f0c6753529db74a32))
* trying to fix import issues after file renames ([7a0b3f3](https://github.com/MirageNet/Mirage/commit/7a0b3f347df91043e82ecd2ec0e6fb26d70e56c3))


### Features

* support for jagged arrays ([2ec6d3a](https://github.com/MirageNet/Mirage/commit/2ec6d3afc16f7560844697585d92b33a8239019f))

## [148.0.2](https://github.com/MirageNet/Mirage/compare/v148.0.1...v148.0.2) (2024-03-29)


### Bug Fixes

* fixing AddLateEvent tmp list ([619b91d](https://github.com/MirageNet/Mirage/commit/619b91d49aa2c0aba0d2605d00c8ae4d0d1a27f1))

## [148.0.1](https://github.com/MirageNet/Mirage/compare/v148.0.0...v148.0.1) (2024-03-28)


### Bug Fixes

* fixing use of GO/NI/NB inside Constructor ([2f5db3d](https://github.com/MirageNet/Mirage/commit/2f5db3de45cd985ba6af428c9cf521dfd4b3cfc3))

# [148.0.0](https://github.com/MirageNet/Mirage/compare/v147.4.2...v148.0.0) (2024-03-25)


* refactor!: renaming AddLateEvent_new to AddLateEvent ([5d92eba](https://github.com/MirageNet/Mirage/commit/5d92eba1e3775b1be06f6f79b2a60c6dd1a769dc))
* refactor!: renaming AddLateEvent to AddLateEventUnity ([169720c](https://github.com/MirageNet/Mirage/commit/169720ca024bd9a40c2b2bd312d26f101e344822))
* perf!: changing NetworkIdentity events to use new c# only events ([3aaab68](https://github.com/MirageNet/Mirage/commit/3aaab68b93ab610f88b15f09a9cd00a33bc8a794))


### BREAKING CHANGES

* renaming from AddLateEvent_new placeholder to just AddLateEvent
* renaming AddLateEvent which contains UnityEvent and UnityAction to AddLateEventUnity
* Inspector events removed from NetworkIdentity, use Mirage v147.4.0 first to convert before updating to next version

## [147.4.2](https://github.com/MirageNet/Mirage/compare/v147.4.1...v147.4.2) (2024-03-25)


### Bug Fixes

* fixing file name ([d6de4cb](https://github.com/MirageNet/Mirage/commit/d6de4cb51c9954bd7bd5a8bb3c3a2502718a18a4))

## [147.4.1](https://github.com/MirageNet/Mirage/compare/v147.4.0...v147.4.1) (2024-03-25)


### Bug Fixes

* changing button text ([0f55b2b](https://github.com/MirageNet/Mirage/commit/0f55b2ba207ed64c1eb41cdd4bdd75b71dfcfcf9))

# [147.4.0](https://github.com/MirageNet/Mirage/compare/v147.3.0...v147.4.0) (2024-03-25)


### Bug Fixes

* fixing misplaced #if ([f663543](https://github.com/MirageNet/Mirage/commit/f663543a50c3889fd6c11790f42256b1500b2177))
* new events should not be abstract ([e1072d0](https://github.com/MirageNet/Mirage/commit/e1072d0dda0818bc6a02632fac36d68b920a7851))


### Features

* create new c# only events ([1c9f2d8](https://github.com/MirageNet/Mirage/commit/1c9f2d84ca77b9ef7859cfa13870677b952a6ec4))
* new NetworkInspectorCallbacks component ([a8051c4](https://github.com/MirageNet/Mirage/commit/a8051c4e4097ab85dfffe029f08938b5456074fc))

# [147.3.0](https://github.com/MirageNet/Mirage/compare/v147.2.1...v147.3.0) (2024-03-23)


### Features

* adding public OwnedObjects list and RemoveAllOwnedObject helper method to NetworkPlayer ([5a611ba](https://github.com/MirageNet/Mirage/commit/5a611baefc363f1afaec03dc04f17b3c2aadf4f5))

## [147.2.1](https://github.com/MirageNet/Mirage/compare/v147.2.0...v147.2.1) (2024-03-18)


### Bug Fixes

* releasing buffers that are in acksystem when disconnecting ([42691a6](https://github.com/MirageNet/Mirage/commit/42691a6089d7753d60efdc6b7bc1597b675cb7e3))
* stopping error when adding handlers for prefab that is already registered ([002ec48](https://github.com/MirageNet/Mirage/commit/002ec4819d1859da5b9538b4f758194759b1886e))

# [147.2.0](https://github.com/MirageNet/Mirage/compare/v147.1.0...v147.2.0) (2024-03-12)


### Bug Fixes

* fixing span support UNITY_2021_3_OR_NEWER ([7bf28ed](https://github.com/MirageNet/Mirage/commit/7bf28ed1537fe4fc4e3c389e1086149ea6d970eb))


### Features

* adding support for Span<T> ([3c48db8](https://github.com/MirageNet/Mirage/commit/3c48db8d32880c827dcd7fe7254534bf5a265b60))

# [147.1.0](https://github.com/MirageNet/Mirage/compare/v147.0.3...v147.1.0) (2024-03-12)


### Features

* adding excludeHost to ClientRpc ([a35ed9d](https://github.com/MirageNet/Mirage/commit/a35ed9df2238e282309b3b3f4b5aa026185d5ba4))

## [147.0.3](https://github.com/MirageNet/Mirage/compare/v147.0.2...v147.0.3) (2024-02-16)


### Bug Fixes

* **ClientObjectManager:** add prefab to SpawnHandler when registering ([#1171](https://github.com/MirageNet/Mirage/issues/1171)) ([7a8ff46](https://github.com/MirageNet/Mirage/commit/7a8ff466f2fb227a78c2f8e98e70a80ec66d0f2c))

## [147.0.2](https://github.com/MirageNet/Mirage/compare/v147.0.1...v147.0.2) (2024-02-13)


### Bug Fixes

* fixing unspawn handlers not being used in disconnect cleanup ([915f307](https://github.com/MirageNet/Mirage/commit/915f307a3e51b4dacea79e57b8db549d9a47230a))

## [147.0.1](https://github.com/MirageNet/Mirage/compare/v147.0.0...v147.0.1) (2024-02-05)


### Bug Fixes

* fixing SocketFactory errors when Listening is false ([7ead256](https://github.com/MirageNet/Mirage/commit/7ead25678c38edda4a513addff19014c875c96c2))

# [147.0.0](https://github.com/MirageNet/Mirage/compare/v146.7.0...v147.0.0) (2024-01-31)


### Bug Fixes

* fixing scene object spawning in host mode for 1 scene setup ([2736fe0](https://github.com/MirageNet/Mirage/commit/2736fe0def7d22c18e878bcdd4cc845bd8f3e746))
* fixing use of is not in unity 2020 ([61b7f68](https://github.com/MirageNet/Mirage/commit/61b7f68e2d834c301e0a6ea4ca5cdf2859ed14f0))


* fix!: fixing spawning for unauthenticated code ([b25f2f9](https://github.com/MirageNet/Mirage/commit/b25f2f98f11d8ea7930e823ae5c05a86ee41dd7f))


### BREAKING CHANGES

* removing ServerObjectManager.OnlySpawnOnAuthenticated field

# [146.7.0](https://github.com/MirageNet/Mirage/compare/v146.6.4...v146.7.0) (2023-12-28)


### Features

* renaming IsLocalClient to IsHost ([39557cd](https://github.com/MirageNet/Mirage/commit/39557cdc16c9487589bfa990f03e37994c70440b))

## [146.6.4](https://github.com/MirageNet/Mirage/compare/v146.6.3...v146.6.4) (2023-12-20)


### Bug Fixes

* fixing owner not being excluded when host ([787e55a](https://github.com/MirageNet/Mirage/commit/787e55a79f9a67cd2073da33934ff8b2a9cf58b6))

## [146.6.3](https://github.com/MirageNet/Mirage/compare/v146.6.2...v146.6.3) (2023-12-03)


### Bug Fixes

* fixing time not being sent on connect ([66e0978](https://github.com/MirageNet/Mirage/commit/66e0978f84bc783750818ad4c1ccd40205984547))

## [146.6.2](https://github.com/MirageNet/Mirage/compare/v146.6.1...v146.6.2) (2023-12-02)


### Bug Fixes

* **SocketLayer:** increase default fragment size to 50 ([1fdaa43](https://github.com/MirageNet/Mirage/commit/1fdaa43ab685c96e1911f6bc114e4dc8d8981c9f))

## [146.6.1](https://github.com/MirageNet/Mirage/compare/v146.6.0...v146.6.1) (2023-11-20)


### Bug Fixes

* **Weaver:** fixing lookup for collection methods ([b530360](https://github.com/MirageNet/Mirage/commit/b53036088d1668d9cd537b0d42e1655998537608))

# [146.6.0](https://github.com/MirageNet/Mirage/compare/v146.5.1...v146.6.0) (2023-11-05)


### Features

* adding writer for dictionary ([f89d596](https://github.com/MirageNet/Mirage/commit/f89d59687d9dd4d9b74212e2312b837437740b29))

## [146.5.1](https://github.com/MirageNet/Mirage/compare/v146.5.0...v146.5.1) (2023-11-05)


### Bug Fixes

* removing debug log ([28d0447](https://github.com/MirageNet/Mirage/commit/28d04476b7c3da7f857ed9158ba329d195b49408))

# [146.5.0](https://github.com/MirageNet/Mirage/compare/v146.4.0...v146.5.0) (2023-11-05)


### Features

* adding WeaverSerializeCollection that can be added to generic writers ([00d476b](https://github.com/MirageNet/Mirage/commit/00d476b4b9784e53140eb0dff54b09ed61eaa9a0))

# [146.4.0](https://github.com/MirageNet/Mirage/compare/v146.3.2...v146.4.0) (2023-10-20)


### Features

* sending secure hash of connect key ([f8901c2](https://github.com/MirageNet/Mirage/commit/f8901c2608edf9e01ff30fc446a4702eeff163b8))

## [146.3.2](https://github.com/MirageNet/Mirage/compare/v146.3.1...v146.3.2) (2023-10-14)


### Bug Fixes

* fixing syncDirection for syncObjects in host mode ([ec45074](https://github.com/MirageNet/Mirage/commit/ec45074cb303fc200baa91a7397adad5650de7b3))

## [146.3.1](https://github.com/MirageNet/Mirage/compare/v146.3.0...v146.3.1) (2023-10-05)


### Bug Fixes

* fixing null ref when using syncObject ([362044a](https://github.com/MirageNet/Mirage/commit/362044ab6d265303eb61e2dacc7c9dc28f54ff44))

# [146.3.0](https://github.com/MirageNet/Mirage/compare/v146.2.2...v146.3.0) (2023-10-02)


### Features

* **Example:** adding scene per match example ([5e7911f](https://github.com/MirageNet/Mirage/commit/5e7911ff8b951bb039a341726633c70693995a30))

## [146.2.2](https://github.com/MirageNet/Mirage/compare/v146.2.1...v146.2.2) (2023-08-21)


### Bug Fixes

* **Weaver:** fixing serialize methods for when type is not resolved ([21f1aee](https://github.com/MirageNet/Mirage/commit/21f1aee3ce2a2c000f8952cf12ad17ccbab5a6ce))


### Performance Improvements

* using cached Id for value types ([dccdbf3](https://github.com/MirageNet/Mirage/commit/dccdbf335a13a4c853f8e56616169cab2d376578))

## [146.2.1](https://github.com/MirageNet/Mirage/compare/v146.2.0...v146.2.1) (2023-08-02)


### Bug Fixes

* also removing character when it is removed as owned ([b96ed68](https://github.com/MirageNet/Mirage/commit/b96ed68a85b2f362212c71c1294904d1e65f8da3))
* updating syncObject ReadOnly before StartServer/Client ([67a1d06](https://github.com/MirageNet/Mirage/commit/67a1d067b621241e9cee3fcb899a068b108ba8a7))

# [146.2.0](https://github.com/MirageNet/Mirage/compare/v146.1.1...v146.2.0) (2023-07-27)


### Features

* adding emit methods for weaver extensions ([6b57cd7](https://github.com/MirageNet/Mirage/commit/6b57cd708bf2be6b32fda12b9b3384a165df4770))
* making ackSystem public so const fields can be used ([63f62bf](https://github.com/MirageNet/Mirage/commit/63f62bfba32b9c458253a86ac530a40a6a99aaaf))

## [146.1.1](https://github.com/MirageNet/Mirage/compare/v146.1.0...v146.1.1) (2023-07-24)


### Reverts

* "pref: using deferred mode for reading, this will speed up other Weavers which dont need to read everything" ([6bff2e3](https://github.com/MirageNet/Mirage/commit/6bff2e3cd433c37682427215a644d69c50320b31))

# [146.1.0](https://github.com/MirageNet/Mirage/compare/v146.0.0...v146.1.0) (2023-07-21)


### Features

* adding attribute that allows for combination of checks ([18852f6](https://github.com/MirageNet/Mirage/commit/18852f647e7b67258d5a84b1bbf01a2420ed324c))

# [146.0.0](https://github.com/MirageNet/Mirage/compare/v145.3.0...v146.0.0) (2023-07-20)


### Features

* INetworkPlayer now has a IsHost property ([5d7e5b7](https://github.com/MirageNet/Mirage/commit/5d7e5b79bffcfab9509d3425caaba8a7b5ca748b))


### BREAKING CHANGES

* NetworkPlayer constructor now requires isHost parameter

# [145.3.0](https://github.com/MirageNet/Mirage/compare/v145.2.0...v145.3.0) (2023-07-17)


### Features

* **Cecil:** adding AddProperty to cecil extensions ([328912a](https://github.com/MirageNet/Mirage/commit/328912ad6707f18aab8ba6f050c5255c4340beba))

# [145.2.0](https://github.com/MirageNet/Mirage/compare/v145.1.0...v145.2.0) (2023-07-16)


### Features

* adding code gen extensions asmdef ([1d7363d](https://github.com/MirageNet/Mirage/commit/1d7363d59aa0d63c3d7c6364c83a7cfaf3f5711c))

# [145.1.0](https://github.com/MirageNet/Mirage/compare/v145.0.4...v145.1.0) (2023-07-09)


### Bug Fixes

* add missing CompMenu for CharacterSpawner ([87c6204](https://github.com/MirageNet/Mirage/commit/87c6204aa7b6d16294b689f02d46d60f68648ea6))
* add missing HelpUrl for CharacterSpawner ([5cd704a](https://github.com/MirageNet/Mirage/commit/5cd704ac42249ffd89742bc88a53c8d8e34624e5))
* add missing HelpUrl for ClientObjectManager ([1186cf6](https://github.com/MirageNet/Mirage/commit/1186cf690256eb87f5e654d7e28cdec5443eefcb))
* add missing HelpUrl for NetworkClient ([daec3a1](https://github.com/MirageNet/Mirage/commit/daec3a1a1e46422dda798a5ba0c9a1f818b532d2))
* add missing HelpUrl for NetworkSceneManager ([cd5c753](https://github.com/MirageNet/Mirage/commit/cd5c7531d88c1c3acc0e3823df5ed3e4b3c4dbcd))
* add missing HelpUrl for NetworkServer ([35d217e](https://github.com/MirageNet/Mirage/commit/35d217ea613281cf3cd196d2bd5c256ab1fdd820))
* add missing HelpUrl for ServerObjectManager ([127a883](https://github.com/MirageNet/Mirage/commit/127a883a4967116ecd95f36f2d9b5bee476e7477))
* add missing HelpUrl for SocketFactory ([a29b1b7](https://github.com/MirageNet/Mirage/commit/a29b1b7446578d5d732e3ca33c28a6f34a5fa9e9))
* measuring time not distance. add s ([fbcdf1e](https://github.com/MirageNet/Mirage/commit/fbcdf1ef7e7260a88ef30fb9925d12506aec0239))


### Features

* adding function to skip bits ([0bcbc7f](https://github.com/MirageNet/Mirage/commit/0bcbc7f945415e7d073615c35509fda686715761))

## [145.0.4](https://github.com/MirageNet/Mirage/compare/v145.0.3...v145.0.4) (2023-07-07)


### Bug Fixes

* Cleaning up name used for generic types ([d846f13](https://github.com/MirageNet/Mirage/commit/d846f137651a5375eb77042b6f81426e695fd562))

## [145.0.3](https://github.com/MirageNet/Mirage/compare/v145.0.2...v145.0.3) (2023-07-06)


### Bug Fixes

* adding warning if spawned Identity already has netid ([22478f4](https://github.com/MirageNet/Mirage/commit/22478f4b7b5b6f665a73ac97b0864fb621759f98))

## [145.0.2](https://github.com/MirageNet/Mirage/compare/v145.0.1...v145.0.2) (2023-07-05)


### Bug Fixes

* calling NetworkReset with custom unspawn handlers ([94a9199](https://github.com/MirageNet/Mirage/commit/94a919970b8ec33f5b5c59aaa708dce75bfafdbb))
* checking if scene object before destroying owned object ([d6ced44](https://github.com/MirageNet/Mirage/commit/d6ced44793d4704dde51bf7f585fb8eacf70a42b))

## [145.0.1](https://github.com/MirageNet/Mirage/compare/v145.0.0...v145.0.1) (2023-06-29)


### Bug Fixes

* logging for notify send ([929686c](https://github.com/MirageNet/Mirage/commit/929686c9f6718ffffcd2fc2317c64874749a4e87))

# [145.0.0](https://github.com/MirageNet/Mirage/compare/v144.0.0...v145.0.0) (2023-06-27)


### Features

* adding function to SyncObjects so that they can use the NetworkBehaviour that are a part of ([b6f3526](https://github.com/MirageNet/Mirage/commit/b6f35264d282ebacbc6d24e54808872cebc50c4a))


### BREAKING CHANGES

* ISyncObject now needs to implement the SetNetworkBehaviour function

# [144.0.0](https://github.com/MirageNet/Mirage/compare/v143.2.3...v144.0.0) (2023-06-26)


### Bug Fixes

* adding check to SyncObjects to make sure they are IEnumerable before drawing them as list. ([907d8ce](https://github.com/MirageNet/Mirage/commit/907d8cece89774cba21511478e4c675e6f60d49e))
* logging prefab hash as hex ([8289f20](https://github.com/MirageNet/Mirage/commit/8289f2074c2681a4cefaad8001b8f59c94970d4c))
* removing debug log from welcomewindow ([3be3f42](https://github.com/MirageNet/Mirage/commit/3be3f423689c91e9fd75b1fd62db5bc84c42c9ee))


### Code Refactoring

* moving SyncsAnything to InspectorHelper ([f3755d9](https://github.com/MirageNet/Mirage/commit/f3755d9b296b11e96a206ed01c1bfb4644cbcf7f))


### BREAKING CHANGES

* SyncsAnything for editor scripts moved to InspectorHelper

## [143.2.3](https://github.com/MirageNet/Mirage/compare/v143.2.2...v143.2.3) (2023-06-23)


### Bug Fixes

* use of await AsyncOperation in unity 2023 ([#1151](https://github.com/MirageNet/Mirage/issues/1151)) ([5af1435](https://github.com/MirageNet/Mirage/commit/5af14359f07f50c848fc784c201ac86ca43a0262))

## [143.2.2](https://github.com/MirageNet/Mirage/compare/v143.2.1...v143.2.2) (2023-06-21)


### Bug Fixes

* fixing host authentication using Connected event ([6bb6bed](https://github.com/MirageNet/Mirage/commit/6bb6bed48d7facf5683c501df364c0d0f39ce4e1))

## [143.2.1](https://github.com/MirageNet/Mirage/compare/v143.2.0...v143.2.1) (2023-06-20)


### Bug Fixes

* fixing path of change log ([#1150](https://github.com/MirageNet/Mirage/issues/1150)) ([7716809](https://github.com/MirageNet/Mirage/commit/77168097c755d4d3778b87822464b042305d03eb))

# [143.2.0](https://github.com/MirageNet/Mirage/compare/v143.1.0...v143.2.0) (2023-06-19)


### Features

* adding script that will make sure object is always visible ([0b763cf](https://github.com/MirageNet/Mirage/commit/0b763cf6209f2570750140795171549ab6781a16))

# [143.1.0](https://github.com/MirageNet/Mirage/compare/v143.0.0...v143.1.0) (2023-06-19)


### Features

* adding attribute that instructs Weaver to write a type as generic ([1b1e4e6](https://github.com/MirageNet/Mirage/commit/1b1e4e6de6e7a0137e1796eccf2d92059c512d88))

# [143.0.0](https://github.com/MirageNet/Mirage/compare/v142.0.0...v143.0.0) (2023-06-18)


### Features

* adding INetworkPlayer to Authenticate ([ba54dd2](https://github.com/MirageNet/Mirage/commit/ba54dd27ab5da796a2f3d39e6fc891abffeec4fd))


### BREAKING CHANGES

* Authenticate overrides now require INetworkPlayer argument

# [142.0.0](https://github.com/MirageNet/Mirage/compare/v141.2.0...v142.0.0) (2023-06-17)


### Features

* adding support for Reply Rpc on server side ([dc56f27](https://github.com/MirageNet/Mirage/commit/dc56f2761745438d2d1ae883c1204c5a5ed8503c))


### BREAKING CHANGES

* Rpc messages renamed

# [141.2.0](https://github.com/MirageNet/Mirage/compare/v141.1.0...v141.2.0) (2023-06-17)


### Features

* adding syncList methods to set an index as dirty ([4dfe8df](https://github.com/MirageNet/Mirage/commit/4dfe8df162cba4bdeb6c1331afba81d5065c5cf0))

# [141.1.0](https://github.com/MirageNet/Mirage/compare/v141.0.2...v141.1.0) (2023-06-16)


### Features

* **NetworkManagerGUI:** overhaul the debug controls ([#1149](https://github.com/MirageNet/Mirage/issues/1149)) ([8b97f36](https://github.com/MirageNet/Mirage/commit/8b97f36399a8c1e1165e47011f9334df3bdcf28b))

## [141.0.2](https://github.com/MirageNet/Mirage/compare/v141.0.1...v141.0.2) (2023-06-13)


### Bug Fixes

* fixing scene objects not being removed from NetworkWorld ([546dd3c](https://github.com/MirageNet/Mirage/commit/546dd3c0f1b025d76e5ccc2deb8790025b6a5dbe))

## [141.0.1](https://github.com/MirageNet/Mirage/compare/v141.0.0...v141.0.1) (2023-06-12)


### Bug Fixes

* fixing RPC with multiple components ([2ae9ddc](https://github.com/MirageNet/Mirage/commit/2ae9ddc00fb008d969e5f3c1172f182049152fd4))

# [141.0.0](https://github.com/MirageNet/Mirage/compare/v140.3.0...v141.0.0) (2023-06-12)


* feat!: adding extra SendTo functions on NetworkServer ([c5ba561](https://github.com/MirageNet/Mirage/commit/c5ba5617aa53c723d168bf48581d285aebd1474a))


### Features

* adding overload to create pools without requiring buffer size ([9842b40](https://github.com/MirageNet/Mirage/commit/9842b406232df6f1d997d4225465ea7c5b3a0389))


### BREAKING CHANGES

* Server.SendToMany functions reworked

# [140.3.0](https://github.com/MirageNet/Mirage/compare/v140.2.0...v140.3.0) (2023-06-11)


### Features

* adding option to rethrow exception throw message handler ([15f27a4](https://github.com/MirageNet/Mirage/commit/15f27a4b6931fb5c87af0d8cdf2f5d66bd5ebf56))


### Performance Improvements

* moving RPC collection to NetworkIdentity ([773910c](https://github.com/MirageNet/Mirage/commit/773910cd31f0f034ede9930470262307c935ba5e))

# [140.2.0](https://github.com/MirageNet/Mirage/compare/v140.1.0...v140.2.0) (2023-06-10)


### Features

* option to invoke hook on owner when they are sending syncvar ([36c1b37](https://github.com/MirageNet/Mirage/commit/36c1b373711349fdd9533c8f59a02fc6c2615d5b))

# [140.1.0](https://github.com/MirageNet/Mirage/compare/v140.0.0...v140.1.0) (2023-06-09)


### Features

* adding syncvar hook with 0 args ([d17576a](https://github.com/MirageNet/Mirage/commit/d17576a6e6168fd2fcfcc7e8963231d11124b879))

# [140.0.0](https://github.com/MirageNet/Mirage/compare/v139.0.0...v140.0.0) (2023-06-04)


### Features

* updating syncLists to use new SyncDirection ([f6f78a8](https://github.com/MirageNet/Mirage/commit/f6f78a8e642d72df17de5a85b204dbbc45b9ffe2))


### BREAKING CHANGES

* SyncObjects now need to implement SetShouldSyncFrom

# [139.0.0](https://github.com/MirageNet/Mirage/compare/v138.0.0...v139.0.0) (2023-05-24)


### Bug Fixes

* avoiding disconnect being called twice ([ce36e58](https://github.com/MirageNet/Mirage/commit/ce36e5839779eaf140bb68ea27aaf5bbd0e0af05))


* feat!: new authentication ([b9b490f](https://github.com/MirageNet/Mirage/commit/b9b490fe9eaba47376fed369c08610ed724f4f1b))


### Features

* adding flag to set if message is only allowed if authenticated ([d710f45](https://github.com/MirageNet/Mirage/commit/d710f455b134eee157d013cddc89a73d949ce24c))
* adding log for receiving message ([e73c9d5](https://github.com/MirageNet/Mirage/commit/e73c9d5fe7779b0367274da98d264ceac3c6d39a))
* adding log handler that adds label and color to messages ([bb26471](https://github.com/MirageNet/Mirage/commit/bb264718a97a50a68aa3aeda99612d2cb1de6238))
* adding send log to network player ([af7104c](https://github.com/MirageNet/Mirage/commit/af7104c434e8153825d24562a5ffee86e99a45a1))
* adding session authenticator ([ae1ef69](https://github.com/MirageNet/Mirage/commit/ae1ef69ec7ceba406df1ccaf9cb9989c58b58996))
* improving log for RegisterHandler ([c6d4efd](https://github.com/MirageNet/Mirage/commit/c6d4efdf1a7b103e7b593971b74f6be8dd645b8b))
* updating Authenticators ([b355604](https://github.com/MirageNet/Mirage/commit/b35560496a85974f3fd068d4c48b290082a4bc96))


### BREAKING CHANGES

* complete NetworkAuthenticator rework see docs for changes
* by default message will disconnect player if unauthenticated. Use allowUnauthenticated flag to avoid this

# [138.0.0](https://github.com/MirageNet/Mirage/compare/v137.0.3...v138.0.0) (2023-05-23)


### Bug Fixes

* stopping protected fields from being written by weaver ([9e2d74a](https://github.com/MirageNet/Mirage/commit/9e2d74a48505ae9d096086febcdfac1c67ee947f)), closes [/github.com/MirrorNetworking/Mirror/issues/3485#issuecomment-1559005650](https://github.com//github.com/MirrorNetworking/Mirror/issues/3485/issues/issuecomment-1559005650)


### Features

* making NetworkManagerGUI automatically find NetworkManager when added ([c600b4c](https://github.com/MirageNet/Mirage/commit/c600b4c644c1440eb5be0f4fa820791003696901))


### BREAKING CHANGES

* internal fields are not longer automatically written by Weaver generated functions

## [137.0.3](https://github.com/MirageNet/Mirage/compare/v137.0.2...v137.0.3) (2023-05-17)


### Bug Fixes

* fixing serialize writing to owner writer when no owner ([c6f203f](https://github.com/MirageNet/Mirage/commit/c6f203f137c6d39fce578c39212c5c0309aa4c70))
* fixing server setting HasAuthority to true ([a6fa26d](https://github.com/MirageNet/Mirage/commit/a6fa26d07542be950e1df7403066654d4ab3f61b))

## [137.0.2](https://github.com/MirageNet/Mirage/compare/v137.0.1...v137.0.2) (2023-05-09)


### Bug Fixes

* fixing guard functions for generic and array ([b453d36](https://github.com/MirageNet/Mirage/commit/b453d3678bf6699d66cfd5d7cbddb3d9e99aab74))

## [137.0.1](https://github.com/MirageNet/Mirage/compare/v137.0.0...v137.0.1) (2023-05-05)


### Bug Fixes

* fixing message size being too small with DisableReliableLayer ([50b4093](https://github.com/MirageNet/Mirage/commit/50b40937c76e327d9104ccc1aaf21602d5ec72ab))

# [137.0.0](https://github.com/MirageNet/Mirage/compare/v136.2.0...v137.0.0) (2023-05-05)


### Bug Fixes

* adding OnValidate to NetworkManager ([5a54b3a](https://github.com/MirageNet/Mirage/commit/5a54b3ae4b068b7df3d965e350cde26c85000b03))
* fixing null ref when ObjectManager not set ([7333749](https://github.com/MirageNet/Mirage/commit/7333749f95177f43346f0119f3ed022f8d7d941f))
* fixing race condition with ServerObjectManager ([f2d2cf3](https://github.com/MirageNet/Mirage/commit/f2d2cf33df07f09e52ede16194d1ab17ee4253b7))


### Code Refactoring

* changing clientObjectManager to be used by other classes ([2a69409](https://github.com/MirageNet/Mirage/commit/2a69409d326129f88b51e4775027a425f5b17bee))


### BREAKING CHANGES

* NetworkClient and NetworkSceneManager now need a reference to ClientObjectManager
* NetworkServer now need a reference to ServerObjectManager

# [136.2.0](https://github.com/MirageNet/Mirage/compare/v136.1.0...v136.2.0) (2023-05-04)


### Features

* adding helper function to get AuthenticationData as a type ([a2d7382](https://github.com/MirageNet/Mirage/commit/a2d738205240302ea49f38806360450113f39f64))

# [136.1.0](https://github.com/MirageNet/Mirage/compare/v136.0.0...v136.1.0) (2023-05-04)


### Features

* components to sync if gameObject is active ([873db7f](https://github.com/MirageNet/Mirage/commit/873db7f5cad0be4521179cdedf32570daef37f0c))

# [136.0.0](https://github.com/MirageNet/Mirage/compare/v135.1.0...v136.0.0) (2023-05-01)


### Bug Fixes

* fixing typo for Spawn settings ([2d8a35a](https://github.com/MirageNet/Mirage/commit/2d8a35a6ad418bb2edb48a834e341feaeddb4c0e))


### Code Refactoring

* adding headers to Character spawner fields ([3ed2134](https://github.com/MirageNet/Mirage/commit/3ed2134771e53e98fa377ef12c4ff4ceeabc87d3))
* Changing default spawn settings ([89564f4](https://github.com/MirageNet/Mirage/commit/89564f49c8df7a937369925b66c5ec9f1c08b749))
* moving Spawn Settings too its own file ([5bdc7a3](https://github.com/MirageNet/Mirage/commit/5bdc7a3f1bf0f47f25d08b49c323160629f7897d))


### Features

* adding Name and GameObjectActive to Spawn settings ([fd12390](https://github.com/MirageNet/Mirage/commit/fd123909c0b00f084f40e80fb1f4aa57de5284c3))
* adding option to always enable client object ([686a904](https://github.com/MirageNet/Mirage/commit/686a9048cc4f27cf7e96ba18f4c6d3bcb6910c69))
* adding option to disable Setting name by CharacterSpawner ([796a7bd](https://github.com/MirageNet/Mirage/commit/796a7bd9344e98e1e537a10e41f9c834e916e5a0))


### BREAKING CHANGES

* NetworkIdentity.TransformSpawnSettings moved to NetworkSpawnSettings
* Spawning a prefab will no longer automatically enable it. Spawn settings must have SendGameObjectActive set to true and object on server to be enabled as well
* SpawnMessage now has SpawnValues struct to store values in. SpawnMessage also has new ToString message
* removing FormerlySerializedAs from CharacterSpawner

# [135.1.0](https://github.com/MirageNet/Mirage/compare/v135.0.0...v135.1.0) (2023-04-30)


### Bug Fixes

* fixing SendNotify deliver ([3f9ceb9](https://github.com/MirageNet/Mirage/commit/3f9ceb9ac83917515b43518d7eac9f1d15c1bb30))


### Features

* adding option to disable reliable layer ([6618b5d](https://github.com/MirageNet/Mirage/commit/6618b5d233804a4742b122f2c98c7ab9e9807915))
* adding option to use UniTaskVoid for message handlers ([64b9cc7](https://github.com/MirageNet/Mirage/commit/64b9cc70083e48a3fd4684f9453aa1e6053d28e7))

# [135.0.0](https://github.com/MirageNet/Mirage/compare/v134.0.0...v135.0.0) (2023-04-29)


### Code Refactoring

* making add remove connections private ([0cac9a3](https://github.com/MirageNet/Mirage/commit/0cac9a36af183af9c0820e8d5a1592d1b8c01906))


### BREAKING CHANGES

* NetworkServer methods AddConnection and RemoveConnection are no longer public

# [134.0.0](https://github.com/MirageNet/Mirage/compare/v133.0.0...v134.0.0) (2023-04-28)


### Code Refactoring

* removing NumberOfPlayers ([cafbe50](https://github.com/MirageNet/Mirage/commit/cafbe50a591a464acb30f4a59037f1e28bdb553e))
* removing Obsolete functions ([a3932f2](https://github.com/MirageNet/Mirage/commit/a3932f241ee5129efe4ec9d677f9c622a68ebccf))


### Features

* adding SpawnInstantiate helper methods ([58f14cd](https://github.com/MirageNet/Mirage/commit/58f14cd43b33dc0e812863fcf274dc98d1105d22))


### BREAKING CHANGES

* removing Obsolete functions, see commit for details
* NumberOfPlayers removed

# [133.0.0](https://github.com/MirageNet/Mirage/compare/v132.0.3...v133.0.0) (2023-04-24)


### Features

* refactoring LobbyReady to use new features ([b9e13e3](https://github.com/MirageNet/Mirage/commit/b9e13e36060bcb696c0b3a70ab99c592a66454c3))


### BREAKING CHANGES

* LobbyReady and ReadyCheck components changed

fix: using Start instead so Server can be added by test

test: trying to fix lobby test

fix: fixing lobby ready

setting syncvar in another asm doesn't work all the time. Unity ILPP is weird

## [132.0.3](https://github.com/MirageNet/Mirage/compare/v132.0.2...v132.0.3) (2023-04-14)


### Bug Fixes

* SyncPrefab now uses Prefab field to write even if hash is not zero ([57eea65](https://github.com/MirageNet/Mirage/commit/57eea65b675480c7e6a76e522a67bee76859bb55))

## [132.0.2](https://github.com/MirageNet/Mirage/compare/v132.0.1...v132.0.2) (2023-04-11)


### Bug Fixes

* fixing hostmode not sending syncvar to remote owner ([925d57d](https://github.com/MirageNet/Mirage/commit/925d57db73df5bb0211235355c94afe1247c29f0))

## [132.0.1](https://github.com/MirageNet/Mirage/compare/v132.0.0...v132.0.1) (2023-04-10)


### Bug Fixes

* adding angle packer ([fcbd007](https://github.com/MirageNet/Mirage/commit/fcbd0070a1d393e5808a9de2c5d4816057a2536c))

# [132.0.0](https://github.com/MirageNet/Mirage/compare/v131.1.3...v132.0.0) (2023-04-10)


### Bug Fixes

* fixing sample ([7d1d6f7](https://github.com/MirageNet/Mirage/commit/7d1d6f7c29cb846e2522c8ecd1e2198617632819))
* renaming SpawnObjects to SpawnSceneObjects ([6b89619](https://github.com/MirageNet/Mirage/commit/6b89619fb1958243803fa0b425ae4089daaa74ae))


### Code Refactoring

* renaming Start to Setup for public methods to start ServerObjectManager late ([d5f0647](https://github.com/MirageNet/Mirage/commit/d5f0647bdb02c6d0cb968db2db56b7abc177473b))


* refactor!: moving scene code out of ServerObjectManager ([f2b3938](https://github.com/MirageNet/Mirage/commit/f2b3938b120850cf6840c300adaa6c7a0b89440b))
* refactor!: adding INetworkVisibility ([e47d4a3](https://github.com/MirageNet/Mirage/commit/e47d4a371b331e168d46608703c585847a2f8052))


### Features

* adding to string override for network identity. ([279e207](https://github.com/MirageNet/Mirage/commit/279e2075411359a60ad4b1d5ad198d20508b30c1))


### BREAKING CHANGES

* NetworkSceneManager now requires a reference to ServerObjectManager in the inspector
* ServerObjectManager.Start renamed to Setup
* NetworkIdentity.Visibility can now throw if called before Object is spawned

## [131.1.3](https://github.com/MirageNet/Mirage/compare/v131.1.2...v131.1.3) (2023-04-10)


### Bug Fixes

* setting reason too None if packet length is not 3 ([dcc4fa9](https://github.com/MirageNet/Mirage/commit/dcc4fa920d908837c29274ce76762f698e7466c1))

## [131.1.2](https://github.com/MirageNet/Mirage/compare/v131.1.1...v131.1.2) (2023-04-05)


### Bug Fixes

* fixing incorrect Undo.RecordObject target ([0db933a](https://github.com/MirageNet/Mirage/commit/0db933a124d99aead1937b4f10ae65624918b425))
* fixing register button not setting holder as dirty ([762e295](https://github.com/MirageNet/Mirage/commit/762e295a7a1c36117aa5f49ff0e7802f73c13afc))

## [131.1.1](https://github.com/MirageNet/Mirage/compare/v131.1.0...v131.1.1) (2023-04-04)


### Bug Fixes

* fixing compile error  with inspector in 2022 ([cb7fa26](https://github.com/MirageNet/Mirage/commit/cb7fa26182a5c8d9811a8b12d3554152072ccda8))
* improving host syncing for new direction ([645570c](https://github.com/MirageNet/Mirage/commit/645570c852561c6658a0e1bf94bf0b35e2d884c6))

# [131.1.0](https://github.com/MirageNet/Mirage/compare/v131.0.2...v131.1.0) (2023-04-01)


### Bug Fixes

* fixing sync direction drawer showing incorrect warning ([11a7ab6](https://github.com/MirageNet/Mirage/commit/11a7ab601463061cbe21aad435a979d884b6da83))


### Features

* adding struct to sync a prefab over network using its hash ([15e7bbe](https://github.com/MirageNet/Mirage/commit/15e7bbe44220005d287b12ace9edb7e0d365900a))

## [131.0.2](https://github.com/MirageNet/Mirage/compare/v131.0.1...v131.0.2) (2023-04-01)


### Bug Fixes

* not sending to owner if they are host player ([1c5eed5](https://github.com/MirageNet/Mirage/commit/1c5eed5a247023e1652e0a83cda6c9aa00edb54e))

## [131.0.1](https://github.com/MirageNet/Mirage/compare/v131.0.0...v131.0.1) (2023-03-31)


### Bug Fixes

* hiding SyncSettings when there is nothing to sync ([53f3c62](https://github.com/MirageNet/Mirage/commit/53f3c623a2c2af252ff73fb9a1840ce4c77e5cc8))

# [131.0.0](https://github.com/MirageNet/Mirage/compare/v130.4.1...v131.0.0) (2023-03-31)


### Bug Fixes

* adding safety when returning to pool ([22e2990](https://github.com/MirageNet/Mirage/commit/22e2990c5a1093fd5cd0c6f71584cdbeadfbf7b2))
* fixing drawer to indent fields ([a28a5cf](https://github.com/MirageNet/Mirage/commit/a28a5cf14eaf0d763625bd3fcaf7c965bc8b26d5))
* fixing errors when owner is set before world is ([01ad248](https://github.com/MirageNet/Mirage/commit/01ad248d64eaa5916d3e15bdf9c8333f43b7e6d9))
* fixing IsValidDirection method ([e437a10](https://github.com/MirageNet/Mirage/commit/e437a1087007c16396d781aec3dc716b19e03242))
* RegisterPrefabs now has option to skip over existing handlers ([ec80990](https://github.com/MirageNet/Mirage/commit/ec80990563793eae26f03a71cb1aa01514f45d60))


* refactor!: converting Channel to enum ([9142513](https://github.com/MirageNet/Mirage/commit/9142513a48831793308c35d9dc0ad9a2be086270))
* refactor!: renaming functions used to clear dirty bits ([775698c](https://github.com/MirageNet/Mirage/commit/775698cc3f983f061997ec04cf5d00450834a052))
* refactor!: starting to use syncSettings ([f673f9b](https://github.com/MirageNet/Mirage/commit/f673f9b90087ded3211e8304f933fc144c821607))
* refactor!: deleting Experimental components ([fb5c2a6](https://github.com/MirageNet/Mirage/commit/fb5c2a6d9497453d73ea0d5ea8b4ebbec82a91c2))


### Features

* adding angle and vec2 packer ([039e9cd](https://github.com/MirageNet/Mirage/commit/039e9cddf7a65d7c4b3ddde6d0655e58da9db0e2))
* adding drawer for sync settings ([10ab69e](https://github.com/MirageNet/Mirage/commit/10ab69e7a8e87b0e0b994a78b9c65b1c17c64a0c))
* adding event to world that is invoked when authority changes ([5739296](https://github.com/MirageNet/Mirage/commit/573929658612ffcdf0a392276d6f3de9d4291005))
* adding helper methods for networkWorld ([5ad1f83](https://github.com/MirageNet/Mirage/commit/5ad1f837d22f0710574d0b98e3e1f403084f73f7))
* adding methods to write from pointer ([0b3ae5e](https://github.com/MirageNet/Mirage/commit/0b3ae5e8cd3b873b8dab61ba8bcee7291b573865))
* adding OwnerAndObservers option so it shows in inspector ([26ec10d](https://github.com/MirageNet/Mirage/commit/26ec10d838799a81dd95407a93b8f1f813cf032d))
* adding static version of update time so that other classes can use it ([d96ed04](https://github.com/MirageNet/Mirage/commit/d96ed04e7fbf1b4ae63fa2c6501c0b1904cbdc33))
* adding sync settings ([53ceb2a](https://github.com/MirageNet/Mirage/commit/53ceb2ac06bc96347af2fafd996cd9a72df1d00f))
* SyncStack ([bef8514](https://github.com/MirageNet/Mirage/commit/bef85141ac6b954baa633873fc0f64ecc6229607))


### BREAKING CHANGES

* RegisterPrefabs now has an extra dontAddIfExist bool argument
* Channel is not an enum instead of an int
* ClearAllDirtyBits renamed to ClearShouldSync
* Renaming and Obsolete of custom serialize methods
* deleting Experimental components

## [130.4.1](https://github.com/MirageNet/Mirage/compare/v130.4.0...v130.4.1) (2023-03-21)


### Bug Fixes

* fixing nanosockets build error on some platforms ([fbd136c](https://github.com/MirageNet/Mirage/commit/fbd136cd03bb3aac8bd283d2bc0eb8c622db3b56))
* fixing typo causing Prefabs header being drawn twice ([2c86fc0](https://github.com/MirageNet/Mirage/commit/2c86fc028f8d15e2cb387316444f7a33d9c0d0d9))

# [130.4.0](https://github.com/MirageNet/Mirage/compare/v130.3.0...v130.4.0) (2023-03-16)


### Features

* adding bool for InitialState ([cce01f8](https://github.com/MirageNet/Mirage/commit/cce01f805b15a36493efd38c0b2c0d1c5cfd0b83))

# [130.3.0](https://github.com/MirageNet/Mirage/compare/v130.2.0...v130.3.0) (2023-02-10)


### Features

* making SetHostVisibility virtual ([aef174c](https://github.com/MirageNet/Mirage/commit/aef174c5f24b2edc20de2e4c471fc5193eb4f440))

# [130.2.0](https://github.com/MirageNet/Mirage/compare/v130.1.0...v130.2.0) (2023-02-10)


### Bug Fixes

* making HostRendererVisibility hide objects when it is spawned if not visible ([03976a5](https://github.com/MirageNet/Mirage/commit/03976a538d3d35f838c509924dd7b49698f1546d))


### Features

* adding a function for server to tell clients to load scene ([14283b1](https://github.com/MirageNet/Mirage/commit/14283b1489430d21e12fe75e44f0621f214462a3))
* adding component that can be used to disable renderers on host player ([64c09bf](https://github.com/MirageNet/Mirage/commit/64c09bfb532afc29e9956839d7715fb9f3d8016c))
* adding event that is invoked when object visibility changes ([d82cd76](https://github.com/MirageNet/Mirage/commit/d82cd7638f7175cb9e5e8a16c59fb9aa19342652))
* adding option to not send additive scenes when client connects ([f39414b](https://github.com/MirageNet/Mirage/commit/f39414be4f072981dd75612ef526cd0eb8aa3f91))

# [130.1.0](https://github.com/MirageNet/Mirage/compare/v130.0.0...v130.1.0) (2023-02-06)


### Bug Fixes

* adding exitgui to stop error ([df719b2](https://github.com/MirageNet/Mirage/commit/df719b2ab86311f66e874716ef3fa612e111e868))


### Features

* add networked prefab attribute ([9fdd2ec](https://github.com/MirageNet/Mirage/commit/9fdd2ec872f6c603efaa49a27ca7b28a41adf464))
* making NetworkedPrefab attribute work with new NetworkPrefabs SO ([37c4877](https://github.com/MirageNet/Mirage/commit/37c4877d09f352f2648c67e71d441ca73ebc6cd8))

# [130.0.0](https://github.com/MirageNet/Mirage/compare/v129.6.2...v130.0.0) (2023-02-06)


### Bug Fixes

* **Examples:** updating Paddlespawner awake function to be protected ([241a421](https://github.com/MirageNet/Mirage/commit/241a421e88d4357c39c01fdf3af6c8c52d0d6367))
* marking scene as not ready when player first joins server ([f523143](https://github.com/MirageNet/Mirage/commit/f523143d59f7e14b8571a3bec676b1fb0b6c297d))


* feat(CharacterSpawner)!: making Awake and OnDestroy protected virtual ([14ed80a](https://github.com/MirageNet/Mirage/commit/14ed80adbc2003e2817a2eddfae37c057b78def7))


### BREAKING CHANGES

* CharacterSpawner.Awake is now protected instead of public

Co-authored-by: James Frowen <jamesfrowen5@gmail.com>

## [129.6.2](https://github.com/MirageNet/Mirage/compare/v129.6.1...v129.6.2) (2023-02-03)


### Bug Fixes

* stopping exception when creating new SO ([f55e929](https://github.com/MirageNet/Mirage/commit/f55e9295cbfb15146cbbae927e4468d7665dec4c))

## [129.6.1](https://github.com/MirageNet/Mirage/compare/v129.6.0...v129.6.1) (2023-02-03)


### Bug Fixes

* fixing typo in DisallowMultipleComponent ([715aa33](https://github.com/MirageNet/Mirage/commit/715aa33f7050ffada635232da834479562aade32))
* **NetworkManagerGUI:** prevent potential NRE spam if reference is lost and tidy up ([#1130](https://github.com/MirageNet/Mirage/issues/1130)) ([86b5c3d](https://github.com/MirageNet/Mirage/commit/86b5c3d75e45ab17e83f12b1458bf617e2c4e17c))

# [129.6.0](https://github.com/MirageNet/Mirage/compare/v129.5.0...v129.6.0) (2023-02-02)


### Bug Fixes

* fixing networkprefab drawer height ([8884070](https://github.com/MirageNet/Mirage/commit/888407035284074def8a885dd456f2365a64fd25))


### Features

* adding buttons that create gameobject with NetworkIdentity ([0115818](https://github.com/MirageNet/Mirage/commit/01158185d64ab3e03f227dd553a5e3401a4be488))
* adding custom drawer for networkprefab field ([bb34c04](https://github.com/MirageNet/Mirage/commit/bb34c04a0871103a806bf510049232dab3623a8f))
* drawing prefab list under NetworkPrefab reference ([8c63726](https://github.com/MirageNet/Mirage/commit/8c637263cff3270badc8bfcaf6d6bce5f9291b49))
* hiding COM prefab list when it is empty and NetworkPrefab is set ([5088d33](https://github.com/MirageNet/Mirage/commit/5088d33b5d2a5375ec5549c63372cdf83425eb83))
* making RegisterPrefabs public ([77c8a48](https://github.com/MirageNet/Mirage/commit/77c8a48049eae823c4b9496e0fe4dc50ad22d14b))
* use scriptable object for spawnable prefabs ([#1127](https://github.com/MirageNet/Mirage/issues/1127)) ([1973e76](https://github.com/MirageNet/Mirage/commit/1973e76e3abed43ffc47b57f117772f2627cf803))

# [129.5.0](https://github.com/MirageNet/Mirage/compare/v129.4.0...v129.5.0) (2023-01-31)


### Bug Fixes

* ui toolkit list fields not being reorderable ([1c77772](https://github.com/MirageNet/Mirage/commit/1c77772046a7db0f77fbd3515812ddc6a702444c))


### Features

* adding async versions to NetworkSceneManager functions ([#1128](https://github.com/MirageNet/Mirage/issues/1128)) ([abae4c8](https://github.com/MirageNet/Mirage/commit/abae4c80d5acba540b6fd5ac130b50d0493b0331))

# [129.4.0](https://github.com/MirageNet/Mirage/compare/v129.3.2...v129.4.0) (2023-01-06)


### Features

* ui toolkit network behaviour editor for unity 2022.2 and newer ([#1121](https://github.com/MirageNet/Mirage/issues/1121)) ([f626c77](https://github.com/MirageNet/Mirage/commit/f626c77304b03ce7a177d00b0283754407bf1531))

## [129.3.2](https://github.com/MirageNet/Mirage/compare/v129.3.1...v129.3.2) (2022-12-22)


### Bug Fixes

* errors and warnings on 2023.1 alpha ([#1120](https://github.com/MirageNet/Mirage/issues/1120)) ([37fa113](https://github.com/MirageNet/Mirage/commit/37fa1132828ea72515d9351b7acc8110225e6da3))

## [129.3.1](https://github.com/MirageNet/Mirage/compare/v129.3.0...v129.3.1) (2022-12-21)


### Bug Fixes

* throw InvalidOperationException if network client attempts to send messages while disconnected ([#1118](https://github.com/MirageNet/Mirage/issues/1118)) ([b7ae4de](https://github.com/MirageNet/Mirage/commit/b7ae4deab1fe197def84d81e81cab257f25010c9))

# [129.3.0](https://github.com/MirageNet/Mirage/compare/v129.2.1...v129.3.0) (2022-12-16)


### Features

* adding filter to log settings ([d50e754](https://github.com/MirageNet/Mirage/commit/d50e7543be2179db0674e456f23ec0afa30df89f))

## [129.2.1](https://github.com/MirageNet/Mirage/compare/v129.2.0...v129.2.1) (2022-12-15)


### Bug Fixes

* marking RemoteCallCollection as NonSerialized ([c6ef84b](https://github.com/MirageNet/Mirage/commit/c6ef84ba8fb20cf589ab6c4f50147b263711ea64))

# [129.2.0](https://github.com/MirageNet/Mirage/compare/v129.1.4...v129.2.0) (2022-12-11)


### Features

* adding functions to manually update server and client ([d18ef5a](https://github.com/MirageNet/Mirage/commit/d18ef5a94ec3b2c59dbc59605f37c448e2658712))

## [129.1.4](https://github.com/MirageNet/Mirage/compare/v129.1.3...v129.1.4) (2022-12-11)


### Bug Fixes

* fixing string encoding from GetIP IntPtr ([5073a5f](https://github.com/MirageNet/Mirage/commit/5073a5f92086f73c0de536ded1f4a39b676e0379))

## [129.1.3](https://github.com/MirageNet/Mirage/compare/v129.1.2...v129.1.3) (2022-12-10)


### Bug Fixes

* setting field value ([f9b1f65](https://github.com/MirageNet/Mirage/commit/f9b1f651ab1c6a29d201a3d9c96a5d5f2f42944d))

## [129.1.2](https://github.com/MirageNet/Mirage/compare/v129.1.1...v129.1.2) (2022-12-10)


### Bug Fixes

* adding null check for failing to resolve assembly ([13ce8a9](https://github.com/MirageNet/Mirage/commit/13ce8a9f7a3bfa6d142defa2b336a738d635c69c))

## [129.1.1](https://github.com/MirageNet/Mirage/compare/v129.1.0...v129.1.1) (2022-12-10)


### Bug Fixes

* il2cpp linux runtime SIGABRT crash with nanosockets (unity 2021.3.15) ([#1116](https://github.com/MirageNet/Mirage/issues/1116)) ([ff7148b](https://github.com/MirageNet/Mirage/commit/ff7148b47eb55e4211db22bc2bfaf6a9c3315e7e))

# [129.1.0](https://github.com/MirageNet/Mirage/compare/v129.0.3...v129.1.0) (2022-12-05)


### Features

* adding option to exclude NetworkBehaviourInspector ([99e9519](https://github.com/MirageNet/Mirage/commit/99e9519c59bc73d9f4bbdd02ae105178e09fb5c4))

## [129.0.3](https://github.com/MirageNet/Mirage/compare/v129.0.2...v129.0.3) (2022-11-27)


### Bug Fixes

* fixing scene events not firing when host connects ([598057e](https://github.com/MirageNet/Mirage/commit/598057eda86bc0463aba54b89a106650467421c9))

## [129.0.2](https://github.com/MirageNet/Mirage/compare/v129.0.1...v129.0.2) (2022-11-27)


### Bug Fixes

* fixing COM throwing when registering same prefab twice ([5005daf](https://github.com/MirageNet/Mirage/commit/5005dafd82ca1c023f1fab5a16d9f3142dd5ef8f))

## [129.0.1](https://github.com/MirageNet/Mirage/compare/v129.0.0...v129.0.1) (2022-11-26)


### Bug Fixes

* fixing unity version in package.json ([920493e](https://github.com/MirageNet/Mirage/commit/920493e1caf2c44f890e4b647658347cb78f32f8))

# [129.0.0](https://github.com/MirageNet/Mirage/compare/v128.7.0...v129.0.0) (2022-11-14)


### Bug Fixes

* dont seend scene message on authenticate to host ([94ab113](https://github.com/MirageNet/Mirage/commit/94ab113c2fbe66e553ae268c0ca2ece246f8c3be))
* fixing client networkplayer being given to host player ([7331eb0](https://github.com/MirageNet/Mirage/commit/7331eb05eae3056d0aaf1de6741b183c221e1a79))
* fixing loading scenes in host mode ([9caeea8](https://github.com/MirageNet/Mirage/commit/9caeea8b144db9ca0c8bb1dfddef502ade587fa3))
* not sending unload message to host player ([f1ace90](https://github.com/MirageNet/Mirage/commit/f1ace90f5f26593e65624a5b58386341fc944bb1))


### BREAKING CHANGES

* host no longer invokes scene start/finish events on when host player Authenticates
* SceneMessage should no longer be sent to host player

# [128.7.0](https://github.com/MirageNet/Mirage/compare/v128.6.0...v128.7.0) (2022-11-09)


### Features

* adding send method that will exclude a single player ([f930bd0](https://github.com/MirageNet/Mirage/commit/f930bd01add2b8260705360da6b2096335d1ef71))

# [128.6.0](https://github.com/MirageNet/Mirage/compare/v128.5.0...v128.6.0) (2022-11-02)


### Bug Fixes

* making MaxStringLength re-size the internal buffer to allow for bigger strings ([ed00f34](https://github.com/MirageNet/Mirage/commit/ed00f347d86bc0ab89eb78296a747ab5dae75cb0)), closes [#1109](https://github.com/MirageNet/Mirage/issues/1109)


### Features

* adding extra string methods to use different encoding ([586717b](https://github.com/MirageNet/Mirage/commit/586717b81370d26459bf1d7e989e6871edafa5d6))

# [128.5.0](https://github.com/MirageNet/Mirage/compare/v128.4.1...v128.5.0) (2022-10-08)


### Bug Fixes

* checking for custom visibility in new assert ([f4b9597](https://github.com/MirageNet/Mirage/commit/f4b959789bc6505e782bfbcf12a1a09605ed26fe))


### Features

* adding option to stop spanwn on unauthenticated ([629fab8](https://github.com/MirageNet/Mirage/commit/629fab8a05a6d43f5e213aebf07f86a2743458ac))

## [128.4.1](https://github.com/MirageNet/Mirage/compare/v128.4.0...v128.4.1) (2022-10-08)


### Bug Fixes

* miscellanous NanoSocket bits and pieces, add Mirage Standalone support ([#1106](https://github.com/MirageNet/Mirage/issues/1106)) ([1b7e097](https://github.com/MirageNet/Mirage/commit/1b7e0972160955a0cf972c289f82264614f7ba8b))

# [128.4.0](https://github.com/MirageNet/Mirage/compare/v128.3.1...v128.4.0) (2022-09-25)


### Features

* adding Id to NetworkBehaviour ([8eafae5](https://github.com/MirageNet/Mirage/commit/8eafae5ad57ccde175c87ea4e5817a4ec8573bcc))

## [128.3.1](https://github.com/MirageNet/Mirage/compare/v128.3.0...v128.3.1) (2022-09-21)


### Bug Fixes

* **AckSystem:** fixing fragmented message having incorrect order ([1fb4970](https://github.com/MirageNet/Mirage/commit/1fb4970fc8b8bbdc65d64aa4a16d84903494f7b0))

# [128.3.0](https://github.com/MirageNet/Mirage/compare/v128.2.0...v128.3.0) (2022-09-16)


### Bug Fixes

* adding try/catch for spawn async ([9cad50a](https://github.com/MirageNet/Mirage/commit/9cad50a9eaf5d3fa476d470d22515440de25b7cc))
* fixing async payload not being held ([2eafb46](https://github.com/MirageNet/Mirage/commit/2eafb4693f772ee8c48825fedf6d9620e53c5e3c))
* improving log for server.spawn ([4cdb606](https://github.com/MirageNet/Mirage/commit/4cdb60611e71e36a4cb64caa6c2b7292339f56b6))


### Features

* adding way to return spawnHandler from prefabHash ([4d1d552](https://github.com/MirageNet/Mirage/commit/4d1d552737f3771405172200d4fe29efbc3724cb))
* allowing PrefabHash to be set even if it already has value ([c764c1f](https://github.com/MirageNet/Mirage/commit/c764c1ff7e67627992270198b3350e67ae1d217f))

# [128.2.0](https://github.com/MirageNet/Mirage/compare/v128.1.1...v128.2.0) (2022-09-14)


### Features

* adding pack as int functions to QuaternionPacker ([79ccc25](https://github.com/MirageNet/Mirage/commit/79ccc2556dce66d1d24b3baaf0705fb24165e167))


### Performance Improvements

* making QuaternionPacker pack to 0 for Quaternion.identity ([7c2bfe4](https://github.com/MirageNet/Mirage/commit/7c2bfe43b2ee45c640aaaf93f7ec37aa4fda7b80))

## [128.1.1](https://github.com/MirageNet/Mirage/compare/v128.1.0...v128.1.1) (2022-09-01)


### Bug Fixes

* fixing SetDisplayMetrics for host mode ([ce26e49](https://github.com/MirageNet/Mirage/commit/ce26e493491872332497b25dc582d6a57312f167))

# [128.1.0](https://github.com/MirageNet/Mirage/compare/v128.0.0...v128.1.0) (2022-08-25)


### Features

* adding SpawnVisibleObjects method that can skip objects ([#1100](https://github.com/MirageNet/Mirage/issues/1100)) ([21bf049](https://github.com/MirageNet/Mirage/commit/21bf0499d4d238f97758effc2f85e4f29f9f9681))

# [128.0.0](https://github.com/MirageNet/Mirage/compare/v127.0.0...v128.0.0) (2022-08-18)


* refactor!: removing INetworkServer ([aaaba98](https://github.com/MirageNet/Mirage/commit/aaaba98d22cb7ba08f71b7782cfe0834af0128d1))
* refactor!: removing INetworkClient ([15eb6d1](https://github.com/MirageNet/Mirage/commit/15eb6d1b872cfaa1abe9d02f19b4213864bf87a6))
* refactor!: removing INetworkSceneManager ([e1fdf86](https://github.com/MirageNet/Mirage/commit/e1fdf86618bcbc401dd944f7c3a7a77155e9ab07))
* refactor!: removing IServerObjectManager ([cf9f746](https://github.com/MirageNet/Mirage/commit/cf9f746e6160652ae0e2ce61bc77b9188d67ffa8))
* refactor!: removing IClientObjectManager ([4559573](https://github.com/MirageNet/Mirage/commit/4559573d5063ea1f113fe499cf1fda14501ad226))


### BREAKING CHANGES

* interface for NetworkServer removed
* interface for NetworkClient removed
* interface for NetworkSceneManager removed
* interface for ServerObjectManager removed
* interface for ClientObjectManager removed

# [127.0.0](https://github.com/MirageNet/Mirage/compare/v126.1.0...v127.0.0) (2022-08-18)


* refactor(ClientObjectManager)!: renaming function that registers handlers ([b8b07ee](https://github.com/MirageNet/Mirage/commit/b8b07ee295a0942f9c9c1d5cf5ab23f348596485))
* refactor(ClientObjectManager)!: merging prefab and handlers into class ([229aa4b](https://github.com/MirageNet/Mirage/commit/229aa4bf9396205cea3414d21cf310754d8030c7))


### Features

* adding async spawn handler ([993f425](https://github.com/MirageNet/Mirage/commit/993f425c6f81f0b84c7b7d03f1e2e8beff6a41db))


### BREAKING CHANGES

* RegisterPrefab that registers handlers is now called RegisterSpawnHandler
* adding new exceptions to registering prefabs and handles

# [126.1.0](https://github.com/MirageNet/Mirage/compare/v126.0.1...v126.1.0) (2022-08-17)


### Bug Fixes

* removing dependency on JetBrains.Annotations ([2e1263e](https://github.com/MirageNet/Mirage/commit/2e1263edce5f581978c7e397ca50ade1f19aff6d))


### Features

* adding more public methods for NetworkPlayer VisList ([ccc7ef5](https://github.com/MirageNet/Mirage/commit/ccc7ef55a3726616d4043edce38d053eabadf0ca))

## [126.0.1](https://github.com/MirageNet/Mirage/compare/v126.0.0...v126.0.1) (2022-08-08)


### Bug Fixes

* fixing compile error in unity 2019.4 ([864a144](https://github.com/MirageNet/Mirage/commit/864a144745e4121580550989af7d34b78394feb1))

# [126.0.0](https://github.com/MirageNet/Mirage/compare/v125.0.0...v126.0.0) (2022-08-07)


### Features

* making RemoteCallCollection public ([2906f18](https://github.com/MirageNet/Mirage/commit/2906f18c70b026ecac9297dea7877d4845c4f00e))


### Performance Improvements

* removing allocations for validating network identity in debug mode ([1909749](https://github.com/MirageNet/Mirage/commit/190974954f0a8e803b1be99d4a9b0a21ec59b965))


### BREAKING CHANGES

* remoteCallCollection renamed to RemoteCallCollection (may require unity restart after import)

# [125.0.0](https://github.com/MirageNet/Mirage/compare/v124.0.0...v125.0.0) (2022-08-05)


### Bug Fixes

* improving error handling for Client spawning ([02ca962](https://github.com/MirageNet/Mirage/commit/02ca962c9a64b415386c0e03a0a456ea3ba9245a))


### Features

* adding spawn overload for NetworkIdentity and PrefabHash ([9c51fef](https://github.com/MirageNet/Mirage/commit/9c51fef552d5ae9affb7109e8528576703ec1231))


### BREAKING CHANGES

* ClientObjectManager.GetPrefab now throws instead of returning null

# [124.0.0](https://github.com/MirageNet/Mirage/compare/v123.4.0...v124.0.0) (2022-08-01)


### Bug Fixes

* adding error when target rpc is called with null ([73bbcc2](https://github.com/MirageNet/Mirage/commit/73bbcc206e78744d73e7d1e9858aac4ba47e75d3))
* adding more validate for invoking ServerRpc in host mode ([26b3318](https://github.com/MirageNet/Mirage/commit/26b3318ea5aaa0b5b8d8ff643cb7b46aa69c7992))
* fixing target rpc being called in host modified ([fa052d6](https://github.com/MirageNet/Mirage/commit/fa052d6c2bf1bbc176de5da4fce5af1e1fc94faa)), closes [#1095](https://github.com/MirageNet/Mirage/issues/1095)
* fixing use of incorrect exception in rpc validate ([948c3a4](https://github.com/MirageNet/Mirage/commit/948c3a461ffe281bda052d340dbd39d33f96d2b5))
* making rpc validate methods public ([3b7a5ae](https://github.com/MirageNet/Mirage/commit/3b7a5aeb15251edffb7f9e325921c315c91c6659))


### Code Refactoring

* moving the invoke check to ClientRpcSender ([3465258](https://github.com/MirageNet/Mirage/commit/346525809f54e8394fd3ccb79bebc37f4a64e81a))


### BREAKING CHANGES

* ServerRpc are now only invoked locally if in host mode
* ServerRpc now throws InvalidOperationException if authority is required
* ClientRpc are now only invoked locally if in host mode

# [123.4.0](https://github.com/MirageNet/Mirage/compare/v123.3.3...v123.4.0) (2022-07-27)


### Bug Fixes

* updating HelpUrl for new docs ([#1094](https://github.com/MirageNet/Mirage/issues/1094)) ([e31a5db](https://github.com/MirageNet/Mirage/commit/e31a5dbccb9ec7d08943c9fb7c530a7dfd37f252))


### Features

* adding example script for sending prefab in rpc ([c0dc98e](https://github.com/MirageNet/Mirage/commit/c0dc98e018b0f8a5b22f4fe62f0951558cacdadd))

## [123.3.3](https://github.com/MirageNet/Mirage/compare/v123.3.2...v123.3.3) (2022-07-23)


### Bug Fixes

* **Weaver:** finding extension methods in mirage manully ([b3ada19](https://github.com/MirageNet/Mirage/commit/b3ada19b49b3940c66538e58450807e7dbee08dc))

## [123.3.2](https://github.com/MirageNet/Mirage/compare/v123.3.1...v123.3.2) (2022-07-16)


### Bug Fixes

* fixing warning created from id generator ([eaee748](https://github.com/MirageNet/Mirage/commit/eaee7486e1d180f38273379f5f656c1d11d0d8f6))
* **Weaver:** fixing extension methods for unity2021 ([7f35778](https://github.com/MirageNet/Mirage/commit/7f3577815bf289c152a8521270be9c348b028a25))
* **Weaver:** fixing generic check on extension methods ([fddf9ea](https://github.com/MirageNet/Mirage/commit/fddf9eaa15f68ab2489a7bb640ce81e935cde284)), closes [#1066](https://github.com/MirageNet/Mirage/issues/1066)
* **Weaver:** fixing WeaverDiagnosticsTimer for when directory is not found ([eb880d5](https://github.com/MirageNet/Mirage/commit/eb880d5ceb0eca1f833c213da7434df833e5b126))


### Performance Improvements

* avoiding call to find NetworkIdentity ([7947e9e](https://github.com/MirageNet/Mirage/commit/7947e9ec8a88bc7942606be79096b18aabaf0606))

## [123.3.1](https://github.com/MirageNet/Mirage/compare/v123.3.0...v123.3.1) (2022-06-29)


### Bug Fixes

* making MessageInfo public instead of internal ([e6a4413](https://github.com/MirageNet/Mirage/commit/e6a4413a99d08600a9595926ee9ce10791974567))

# [123.3.0](https://github.com/MirageNet/Mirage/compare/v123.2.4...v123.3.0) (2022-06-21)


### Features

* adding option for unsigned floats in floatpacker ([dda61e1](https://github.com/MirageNet/Mirage/commit/dda61e17f3f22be2589b98a551bd06d09fc5aba3))

## [123.2.4](https://github.com/MirageNet/Mirage/compare/v123.2.3...v123.2.4) (2022-06-03)


### Bug Fixes

* enable runInBackground so that connections dont timeout ([cb1b869](https://github.com/MirageNet/Mirage/commit/cb1b86925d7c6adf64168c21c82314726671916c))
* updating tool tip for MaxConnections ([938f34c](https://github.com/MirageNet/Mirage/commit/938f34c36ceb8b67f0fbb66cbe7f45f3d3ebada5))

## [123.2.3](https://github.com/MirageNet/Mirage/compare/v123.2.2...v123.2.3) (2022-05-31)


### Bug Fixes

* fixing define in UdpSocketFactory ([a402953](https://github.com/MirageNet/Mirage/commit/a402953c8a4ae6246ce972387d7e4dabeaa2305f))

## [123.2.2](https://github.com/MirageNet/Mirage/compare/v123.2.1...v123.2.2) (2022-05-31)


### Bug Fixes

* incorrect unity 2021 compile define ([6d9c566](https://github.com/MirageNet/Mirage/commit/6d9c566ceea1c4a268d14d2e0dff329d4f7eae47))

## [123.2.1](https://github.com/MirageNet/Mirage/compare/v123.2.0...v123.2.1) (2022-05-29)


### Bug Fixes

* fix the fix that fixed the CS0104 error about CollectionExtensions ([9657bdb](https://github.com/MirageNet/Mirage/commit/9657bdb07188839fe70223af2d288a179a03a00a))
* network writer test CollectionExtensions causing CS0104 in 2021.3 ([1ed750a](https://github.com/MirageNet/Mirage/commit/1ed750a7c5c49b91ab56d6348474084e6ca2f811))

# [123.2.0](https://github.com/MirageNet/Mirage/compare/v123.1.5...v123.2.0) (2022-05-26)


### Bug Fixes

* adding missing interface method to PipePeerConnection ([90e5179](https://github.com/MirageNet/Mirage/commit/90e5179fca4834e4eedaf9ed2785f8ec712155b8))


### Features

* **SocketLayer:** adding FlushBatch to Connection ([476156a](https://github.com/MirageNet/Mirage/commit/476156a16f3143ee89e70bfef74294b836b6ffbe))

## [123.1.5](https://github.com/MirageNet/Mirage/compare/v123.1.4...v123.1.5) (2022-05-25)


### Bug Fixes

* adding sequence point to hookException ([47ec337](https://github.com/MirageNet/Mirage/commit/47ec33710b69831ac9e0c69898040ebcc2dd3bc1))

## [123.1.4](https://github.com/MirageNet/Mirage/compare/v123.1.3...v123.1.4) (2022-05-25)


### Bug Fixes

* avoid using NanoSockets on Mac OS (codesigning issues) ([c9c17ab](https://github.com/MirageNet/Mirage/commit/c9c17abb94e0caf36e69a7f8f2a500257cfd6ca1))

## [123.1.3](https://github.com/MirageNet/Mirage/compare/v123.1.2...v123.1.3) (2022-05-24)


### Bug Fixes

* adding ability to start the Client & Server from interfaces ([#1079](https://github.com/MirageNet/Mirage/issues/1079)) ([61e8d6b](https://github.com/MirageNet/Mirage/commit/61e8d6baa9635aa4fe10738bc71f6f5e0e080081))

## [123.1.2](https://github.com/MirageNet/Mirage/compare/v123.1.1...v123.1.2) (2022-05-20)


### Bug Fixes

* fixing ResizeBuffer ([2232c11](https://github.com/MirageNet/Mirage/commit/2232c11b71784585b63f4ec5b1f90484d343e1a5))

## [123.1.1](https://github.com/MirageNet/Mirage/compare/v123.1.0...v123.1.1) (2022-05-19)


### Bug Fixes

* adding defines for nanosocket ([#1078](https://github.com/MirageNet/Mirage/issues/1078)) ([b4523a0](https://github.com/MirageNet/Mirage/commit/b4523a09d25b15937509d3ab5ee265971b531cff))

# [123.1.0](https://github.com/MirageNet/Mirage/compare/v123.0.6...v123.1.0) (2022-05-12)


### Features

* adding OwnerChanged to networkIdentity ([#1077](https://github.com/MirageNet/Mirage/issues/1077)) ([7c94ab7](https://github.com/MirageNet/Mirage/commit/7c94ab7a487501b5b538986d4d9af1debc293c91))

## [123.0.6](https://github.com/MirageNet/Mirage/compare/v123.0.5...v123.0.6) (2022-05-12)


### Bug Fixes

* replacing const with attribute ([#1076](https://github.com/MirageNet/Mirage/issues/1076)) ([de6c97c](https://github.com/MirageNet/Mirage/commit/de6c97caf94138b86028edde4c425fda26bcc952))

## [123.0.5](https://github.com/MirageNet/Mirage/compare/v123.0.4...v123.0.5) (2022-05-09)


### Bug Fixes

* **SocketLayer:** throwing if Peer is given a null endpoint ([3e3c737](https://github.com/MirageNet/Mirage/commit/3e3c737b9c8718a751bbb8795238823690b74e1d))

## [123.0.4](https://github.com/MirageNet/Mirage/compare/v123.0.3...v123.0.4) (2022-05-09)


### Bug Fixes

* **Weaver:** fixing crash from missing attribute ([e54a3a2](https://github.com/MirageNet/Mirage/commit/e54a3a26be577f60d2d6135df11187f8671fcd05))

## [123.0.3](https://github.com/MirageNet/Mirage/compare/v123.0.2...v123.0.3) (2022-05-06)


### Bug Fixes

* **Weaver:** making const fields static ([09c3b3b](https://github.com/MirageNet/Mirage/commit/09c3b3b116a466f55bbfee2771c2b9ca60b2dad4))

## [123.0.2](https://github.com/MirageNet/Mirage/compare/v123.0.1...v123.0.2) (2022-04-27)


### Performance Improvements

* removing allocation from SendToAll ([6166244](https://github.com/MirageNet/Mirage/commit/616624491c2bf6e77c9d74641931c15359116df4))

## [123.0.1](https://github.com/MirageNet/Mirage/compare/v123.0.0...v123.0.1) (2022-04-24)


### Bug Fixes

* fixing NRE in log when handler is null ([013b03c](https://github.com/MirageNet/Mirage/commit/013b03c17adbdb08292f671aa22820422f9b0840))

# [123.0.0](https://github.com/MirageNet/Mirage/compare/v122.1.0...v123.0.0) (2022-04-22)


### Bug Fixes

* peer now sends invalid key even if key is shorter than correct key ([1139527](https://github.com/MirageNet/Mirage/commit/1139527ea8442e5dd47226560bb4872fc3949ca4))


### Code Refactoring

* forcing GetReader to be given objectLocator ([2f3c4b5](https://github.com/MirageNet/Mirage/commit/2f3c4b58c54fd1e24f2ee460d44721292d83d792))


### BREAKING CHANGES

* NetworkReaderPool.GetReader now has IObjectLocator argument (can be null). Use MirageNetworkReader instead of NetworkReader if you need to read NetworkIdentity

# [122.1.0](https://github.com/MirageNet/Mirage/compare/v122.0.2...v122.1.0) (2022-04-14)


### Bug Fixes

* fixing double types showing up in log settings if no namespace given ([5ea84d5](https://github.com/MirageNet/Mirage/commit/5ea84d5ca3f92f334f1bf0239907f70ecabd9447))
* fixing logger for id generator using nameof instead of typeof ([3665b80](https://github.com/MirageNet/Mirage/commit/3665b801fed8d201c1f6cbb598ea10a652fffbd1))


### Features

* syncvar hook with 1 arg ([#1070](https://github.com/MirageNet/Mirage/issues/1070)) ([6e21877](https://github.com/MirageNet/Mirage/commit/6e21877090dbc9c2eab82565b258efabfcc138f6))

## [122.0.2](https://github.com/MirageNet/Mirage/compare/v122.0.1...v122.0.2) (2022-04-14)


### Bug Fixes

* increasing log to warning when receiving known type without handler ([#1072](https://github.com/MirageNet/Mirage/issues/1072)) ([05db6cf](https://github.com/MirageNet/Mirage/commit/05db6cf1008526ec6f5969765a4ca6735b3f7445))


### Performance Improvements

* using plus 1 count for other collection types ([#1073](https://github.com/MirageNet/Mirage/issues/1073)) ([25ab6f3](https://github.com/MirageNet/Mirage/commit/25ab6f321d76d2298bb69f63e6f2c9575d176a9e))

## [122.0.1](https://github.com/MirageNet/Mirage/compare/v122.0.0...v122.0.1) (2022-04-13)


### Performance Improvements

* stopping allocations from log in MessageHandler when not enabled ([d5b3292](https://github.com/MirageNet/Mirage/commit/d5b329231ce256904e03542bfc584d6637bc4e98))

# [122.0.0](https://github.com/MirageNet/Mirage/compare/v121.0.1...v122.0.0) (2022-04-12)


### Bug Fixes

* adding end of stream check to ReadList and ReadBytes as well ([9dc69dc](https://github.com/MirageNet/Mirage/commit/9dc69dc6a8d801a0d882af49f0ead7eedb0678a5))
* fixing OnlineOfflineScene to work with NetworkSceneManager ([bb916f3](https://github.com/MirageNet/Mirage/commit/bb916f348629aa5b2adb10f33ef5a37f164e49bf))
* fixing StopAuthority not being called in host mode on destroy ([f331875](https://github.com/MirageNet/Mirage/commit/f331875ec98f27d2fc66f4c8f5e924976dbc3dde))
* stopping `DestroyAllClientObjects` being called in host mode. ([582c20b](https://github.com/MirageNet/Mirage/commit/582c20b84517690efad88f957dd0de7a8172055e))


### BREAKING CHANGES

* OnlineOfflineScene uses NetworkSceneManager instead of just loading locally using client start

## [121.0.1](https://github.com/MirageNet/Mirage/compare/v121.0.0...v121.0.1) (2022-04-06)


### Bug Fixes

* adding const to type when writer is generated ([f3bcc89](https://github.com/MirageNet/Mirage/commit/f3bcc895654345442fae47fe4e58c1ae192b7a9f))
* throwing if SceneManager returns null ([833634a](https://github.com/MirageNet/Mirage/commit/833634a75274b52f7f4bbb309ad4b72c880b6098))

# [121.0.0](https://github.com/MirageNet/Mirage/compare/v120.1.2...v121.0.0) (2022-03-31)


* fix!: fixing removing RequireComponent from NetworkManager ([81d8603](https://github.com/MirageNet/Mirage/commit/81d8603e64f98fa3a47f4b384b42c0762cf16be1))


### Features

* adding event to NetworkPlayer when Identity is changed ([9e22ff4](https://github.com/MirageNet/Mirage/commit/9e22ff4c313923b9b4990c5dcc59a95d33249303))
* adding methods to create NetworkMamger using other socket facories ([aaba9bc](https://github.com/MirageNet/Mirage/commit/aaba9bc5824c79bc81569b5423844ba468ab2ae6))


### BREAKING CHANGES

* RequireComponent from NetworkManager. NetworkServer and NetworkClient will now need to be added manaully.

## [120.1.2](https://github.com/MirageNet/Mirage/compare/v120.1.1...v120.1.2) (2022-03-25)


### Bug Fixes

* **NetworkIdentity:** fix more missing things ([7420bd1](https://github.com/MirageNet/Mirage/commit/7420bd128c505044d452c12e034ded4f43ab31cb))
* **NetworkIdentity:** fix one quote on a log string to unbreak it all ([339ab4c](https://github.com/MirageNet/Mirage/commit/339ab4c02bfd963e3f1beeb49a0a929a78d03c80))
* **tests:** networkidentity test failing due to changed exception text ([877eb12](https://github.com/MirageNet/Mirage/commit/877eb12e63db914cdbd4f1211c58e77f211adc76))

## [120.1.1](https://github.com/MirageNet/Mirage/compare/v120.1.0...v120.1.1) (2022-03-23)


### Bug Fixes

* **NetworkClient:** checking if client is already active before connecting host ([bc89211](https://github.com/MirageNet/Mirage/commit/bc892115495d4bfde3c8395c66268b36e3e8f0ae))

# [120.1.0](https://github.com/MirageNet/Mirage/compare/v120.0.0...v120.1.0) (2022-03-22)


### Features

* adding SetCharacterName to character spawner ([1b051c7](https://github.com/MirageNet/Mirage/commit/1b051c76e793d5731dddec33f993b5fdc82816ed))

# [120.0.0](https://github.com/MirageNet/Mirage/compare/v119.1.4...v120.0.0) (2022-03-20)


* refactor(SocketLayer)!: moving MaxPacketSize to SocketFactory ([49c7f41](https://github.com/MirageNet/Mirage/commit/49c7f41bedf6ffd1a3655037a74d14884cf1b23e))


### BREAKING CHANGES

* socket factories now have to override MaxPacketSize property

## [119.1.4](https://github.com/MirageNet/Mirage/compare/v119.1.3...v119.1.4) (2022-03-17)


### Bug Fixes

* Don't create a socket if not listening (should fix [#1054](https://github.com/MirageNet/Mirage/issues/1054)) ([f33c6eb](https://github.com/MirageNet/Mirage/commit/f33c6eb2525919a4304e39f1309552c75c788abf))
* networkserver shouldn't create socket if we're not listening (addresses ticket [#1054](https://github.com/MirageNet/Mirage/issues/1054)) ([5117f49](https://github.com/MirageNet/Mirage/commit/5117f49cfc7c5750388f248db2ec1b6296510503))

## [119.1.3](https://github.com/MirageNet/Mirage/compare/v119.1.2...v119.1.3) (2022-03-02)


### Bug Fixes

* fixing pong example ([0b28fe4](https://github.com/MirageNet/Mirage/commit/0b28fe47e746a5a2fc5e65e8565c83671ce8560a))

## [119.1.2](https://github.com/MirageNet/Mirage/compare/v119.1.1...v119.1.2) (2022-03-01)


### Bug Fixes

* fixing udp socket for macOS ([#1049](https://github.com/MirageNet/Mirage/issues/1049)) ([67baff1](https://github.com/MirageNet/Mirage/commit/67baff1e072eca93629744c954c5edb3bf49f952))

## [119.1.1](https://github.com/MirageNet/Mirage/compare/v119.1.0...v119.1.1) (2022-02-19)


### Bug Fixes

* fixing ArgumentOutOfRangeException when no NetworkIdentity on object ([d8e7830](https://github.com/MirageNet/Mirage/commit/d8e7830ef709a968cf27d35557b48235b4572d0c))

# [119.1.0](https://github.com/MirageNet/Mirage/compare/v119.0.1...v119.1.0) (2022-02-17)


### Features

* support for generic network messages ([#1040](https://github.com/MirageNet/Mirage/issues/1040)) ([2d8990d](https://github.com/MirageNet/Mirage/commit/2d8990dc02dc945ec072fdccf40e63c1c5b8cd43))

## [119.0.1](https://github.com/MirageNet/Mirage/compare/v119.0.0...v119.0.1) (2022-02-16)


### Performance Improvements

* moving replyId to its own message ([e8e1829](https://github.com/MirageNet/Mirage/commit/e8e18296d3b5efb7742b074dd56a9e9b0915bce1))

# [119.0.0](https://github.com/MirageNet/Mirage/compare/v118.0.0...v119.0.0) (2022-02-16)


* feat!(SocketLayer): adding connection key based on mirage version ([ff5a308](https://github.com/MirageNet/Mirage/commit/ff5a308470d6d46b24dd731a7d7639d55795ec9b))


### BREAKING CHANGES

* Mismatched server/client versions will no longer be able to connect to each other

# [118.0.0](https://github.com/MirageNet/Mirage/compare/v117.2.0...v118.0.0) (2022-02-08)


### Features

* support for generic syncvar ([057e177](https://github.com/MirageNet/Mirage/commit/057e177a101b69683969f7f8550a96fbaa49d47e))


### Performance Improvements

* using index for rpcs instead of hash ([88c4cd3](https://github.com/MirageNet/Mirage/commit/88c4cd3e678f805afb87dc4e889a812d07616c6d))


### BREAKING CHANGES

* removed RemoteCallHelper and adding RemoteCallCollection instead NetworkBehaviour

# [117.2.0](https://github.com/MirageNet/Mirage/compare/v117.1.1...v117.2.0) (2022-02-07)


### Bug Fixes

* fixing generic syncvar hooks ([90e9f24](https://github.com/MirageNet/Mirage/commit/90e9f248d44fb1aa0b212e33668b11720fceb73a))
* **Weaver:** avoiding short instructions for methods with unknown length ([f605c46](https://github.com/MirageNet/Mirage/commit/f605c46c5c2637e605d387d9d220fcc873e0388b))


### Features

* **RPC:** adding support for generic syncvars ([b6984ca](https://github.com/MirageNet/Mirage/commit/b6984ca6a56390aedde023e8f361be82feadb51c))

## [117.1.1](https://github.com/MirageNet/Mirage/compare/v117.1.0...v117.1.1) (2022-02-07)


### Bug Fixes

* fixing rpc cast for generic types ([111fce6](https://github.com/MirageNet/Mirage/commit/111fce6a0eb276bbe68c2dff0129b5289d2752a3))
* rpcs for fixing IL2CPP ([e574a80](https://github.com/MirageNet/Mirage/commit/e574a80cae66df440edc28ab46b6ac3bda87bd00))

# [117.1.0](https://github.com/MirageNet/Mirage/compare/v117.0.0...v117.1.0) (2022-02-06)


### Bug Fixes

* **RPC:** fixing call when type is generic ([84e2dd6](https://github.com/MirageNet/Mirage/commit/84e2dd66523da92cc98ef3bd559a3aac0de32e36))


### Features

* making setter for NetworkReader.ObjectLocator public ([50ab932](https://github.com/MirageNet/Mirage/commit/50ab93246d89421498bc10f1c4043727d038b7ae))

# [117.0.0](https://github.com/MirageNet/Mirage/compare/v116.2.0...v117.0.0) (2022-02-02)


### Bug Fixes

* **RPC:** fixing rpc calls to base methods and overloads ([8bc165d](https://github.com/MirageNet/Mirage/commit/8bc165deb6432e881340f82c63a3a4d0a6039263))
* **RPC:** fixing ServerRpc that could be called without Authority ([51411fb](https://github.com/MirageNet/Mirage/commit/51411fb068d2a44330d85e740d4c36b3f74c3ce6))


### Code Refactoring

* **RPC:** moving send rpc functions to their own classes ([eaadd62](https://github.com/MirageNet/Mirage/commit/eaadd62266c9c0d9ce8a34afebfdae7130e68a7c))
* **RPC:** renaming methods for RemoteCallHelper ([186f228](https://github.com/MirageNet/Mirage/commit/186f22866207e1af6bb1713479034db1fdbe136e))


### Features

* **RPC:** adding rpc overloads ([5ca30f1](https://github.com/MirageNet/Mirage/commit/5ca30f1a05fbcb4fd880fd59d74386a19ab47d5c))


### Performance Improvements

* **RPC:** generating hash at compile time not runtime ([4f81402](https://github.com/MirageNet/Mirage/commit/4f814024293f7f0e0988003f622d43a52cf7b414))


### BREAKING CHANGES

* **RPC:** removed RemoteCallHelper.GetDelegate, use GetCall or TryGetCall instead
* **RPC:** Send Rpc methods are now found in ServerRpcSender and ClientRpcSender and may have been renamed

# [116.2.0](https://github.com/MirageNet/Mirage/compare/v116.1.1...v116.2.0) (2022-02-02)


### Features

* calling OnAuthorityChanged with false when an object is unspawned ([#1034](https://github.com/MirageNet/Mirage/issues/1034)) ([d8334e8](https://github.com/MirageNet/Mirage/commit/d8334e8902998981001040b0601fcee5a9e68955))

## [116.1.1](https://github.com/MirageNet/Mirage/compare/v116.1.0...v116.1.1) (2022-02-01)


### Bug Fixes

* fixing calling base methods for generic types in IL2CPP ([cf91e1d](https://github.com/MirageNet/Mirage/commit/cf91e1d54796866d2cf87f8e919bb5c681977e45))

# [116.1.0](https://github.com/MirageNet/Mirage/compare/v116.0.0...v116.1.0) (2022-01-29)


### Bug Fixes

* adding missing MessageHandler to interfaces ([370c5ab](https://github.com/MirageNet/Mirage/commit/370c5ab47bb58980c109f2814c933fd3ed26bfa9))


### Features

* adding notify send to NetworkPlayer ([ad699fc](https://github.com/MirageNet/Mirage/commit/ad699fcdc9f5d4fb7a83f02b88240e92bed8246e))

# [116.0.0](https://github.com/MirageNet/Mirage/compare/v115.0.0...v116.0.0) (2022-01-23)


### Bug Fixes

* **NetworkReader:** fixing PadAndCopy function ([8e6c516](https://github.com/MirageNet/Mirage/commit/8e6c516e4608e8d661ac9e6c5f8577a6e87ff96e))


### BREAKING CHANGES

* **NetworkReader:** NetworkReader.PadAndCopy no longer needs the byte size argument

# [115.0.0](https://github.com/MirageNet/Mirage/compare/v114.1.1...v115.0.0) (2022-01-22)


### Bug Fixes

* fix welcome window packages tab displaying actual modules ([#1024](https://github.com/MirageNet/Mirage/issues/1024)) ([5930281](https://github.com/MirageNet/Mirage/commit/593028177a7f55e0d80bc94984f6c2634d010e18))
* **NetworkSceneManager:** fixing exception when scene is null ([a366843](https://github.com/MirageNet/Mirage/commit/a36684397923110b8336652978d20837a4a17345))


* feat(NetworkSceneManager)!: return scene instead of strings from events (#1028) ([106e714](https://github.com/MirageNet/Mirage/commit/106e714bb5adcb6d35712d31fd682dd1a9dcd3d9)), closes [#1028](https://github.com/MirageNet/Mirage/issues/1028) [#1026](https://github.com/MirageNet/Mirage/issues/1026)


### BREAKING CHANGES

* - Scene finished loading events now return scene
- SceneChangeEvent class renamed to SceneChangeStartedEvent and SceneChangeFinishedEvent

## [114.1.1](https://github.com/MirageNet/Mirage/compare/v114.1.0...v114.1.1) (2022-01-19)


### Bug Fixes

* fixing array and generic write for NetworkBehaviours ([d2ee8ac](https://github.com/MirageNet/Mirage/commit/d2ee8acd16a494c6a39fb12977850c98c6e2d9e3))

# [114.1.0](https://github.com/MirageNet/Mirage/compare/v114.0.3...v114.1.0) (2022-01-15)


### Features

* allow end user's to create there own net id generator ([#1019](https://github.com/MirageNet/Mirage/issues/1019)) ([d2e8834](https://github.com/MirageNet/Mirage/commit/d2e88343b939d12d15f9acae0c06b14386c6dd74))

## [114.0.3](https://github.com/MirageNet/Mirage/compare/v114.0.2...v114.0.3) (2022-01-13)


### Bug Fixes

* fixing calls to interface for target RPCs ([#1021](https://github.com/MirageNet/Mirage/issues/1021)) ([780123d](https://github.com/MirageNet/Mirage/commit/780123d6bb5602632e029d1c5fdc4e73ed477f0c))

## [114.0.2](https://github.com/MirageNet/Mirage/compare/v114.0.1...v114.0.2) (2022-01-03)


### Bug Fixes

* removing un-used using ([0e7bd82](https://github.com/MirageNet/Mirage/commit/0e7bd82be0cae06d1d54eada68c452f5dcfe1342))

## [114.0.1](https://github.com/MirageNet/Mirage/compare/v114.0.0...v114.0.1) (2021-12-31)


### Bug Fixes

* **WelcomeWindow:** fixed parsing of change logs ([#1017](https://github.com/MirageNet/Mirage/issues/1017)) ([5da522d](https://github.com/MirageNet/Mirage/commit/5da522d950113470573bbd406f622772d398336e))

# [114.0.0](https://github.com/MirageNet/Mirage/compare/v113.3.4...v114.0.0) (2021-12-31)


### Bug Fixes

* **NetworkTime:** renaming PingFrequency to PingInterval ([b729cdc](https://github.com/MirageNet/Mirage/commit/b729cdcf6afaa5ec544d9ef89f1a2040dfeaeaa4))


### Code Refactoring

* replacing MessagePacker.GetMessageType with property ([#1016](https://github.com/MirageNet/Mirage/issues/1016)) ([106c47c](https://github.com/MirageNet/Mirage/commit/106c47c93a62f2b1b47da14aadaa918fb2e45d17))


### BREAKING CHANGES

* MessagePacker.GetMessageType replaced with property
* **NetworkTime:** NetworkTime PingFrequency remamed to PingInterval

## [113.3.4](https://github.com/MirageNet/Mirage/compare/v113.3.3...v113.3.4) (2021-12-31)


### Bug Fixes

* **LogFactory:** fixing clear and find all buttons in log factory ([6f7e5d5](https://github.com/MirageNet/Mirage/commit/6f7e5d538f8da2e635796632c1a8d946cfa22e61))

## [113.3.3](https://github.com/MirageNet/Mirage/compare/v113.3.2...v113.3.3) (2021-12-29)


### Bug Fixes

* **Logging:** fixing full name not being loaded ([caece7e](https://github.com/MirageNet/Mirage/commit/caece7e3fcac6a7569f84970cee6ea9b4dd5a29a))

## [113.3.2](https://github.com/MirageNet/Mirage/compare/v113.3.1...v113.3.2) (2021-12-29)


### Bug Fixes

* adding try/catch for nanosocket ([#1010](https://github.com/MirageNet/Mirage/issues/1010)) ([88badd6](https://github.com/MirageNet/Mirage/commit/88badd61fc0f1f8c377b2299cb647f6e70a1ef7b))
* fixing log settings that have no namespace ([#1014](https://github.com/MirageNet/Mirage/issues/1014)) ([ead317f](https://github.com/MirageNet/Mirage/commit/ead317f7ea35c3faaa538326145ed56bd0a3f438))
* **NetworkBehavior:** removing NB that belong to another NI from list ([#970](https://github.com/MirageNet/Mirage/issues/970)) ([4738d29](https://github.com/MirageNet/Mirage/commit/4738d2938e3881481612465cff27680189c6442b))
* **NetworkWorld:** fixing add identity when object is destroyed client side ([b5a765e](https://github.com/MirageNet/Mirage/commit/b5a765e7010da2f9daf47c12b81a6616d32c0250))
* Setting client not ready to stop character spawning before scene change ([#1009](https://github.com/MirageNet/Mirage/issues/1009)) ([fcbe10d](https://github.com/MirageNet/Mirage/commit/fcbe10d3383b28da88b167da004c69578c165d72))

## [113.3.1](https://github.com/MirageNet/Mirage/compare/v113.3.0...v113.3.1) (2021-12-29)


### Bug Fixes

* making sure server/client stops if gameobject is destroyed ([9cdc27d](https://github.com/MirageNet/Mirage/commit/9cdc27d8d5e0f5a9231cdfe21725ae1378c3e067))

# [113.3.0](https://github.com/MirageNet/Mirage/compare/v113.2.0...v113.3.0) (2021-12-22)


### Bug Fixes

* **NetworkBehaviour:** can now found NetworkIdentity in parent when gameobject is disabled ([#1006](https://github.com/MirageNet/Mirage/issues/1006)) ([d54537a](https://github.com/MirageNet/Mirage/commit/d54537a8c0a136130d4f8cf91a4e779bdb992221))


### Features

* **SyncVar:** option to invoke hooks on server too ([#1012](https://github.com/MirageNet/Mirage/issues/1012)) ([8c12c28](https://github.com/MirageNet/Mirage/commit/8c12c280467c8f68e9e96344bf6143db9b032a9f))

# [113.2.0](https://github.com/MirageNet/Mirage/compare/v113.1.3...v113.2.0) (2021-12-20)


### Bug Fixes

* **Serialization:** fixing ReadArray when reading items that are bitpacked ([6dfbf84](https://github.com/MirageNet/Mirage/commit/6dfbf846c5456b7a80f4d5c8d3977426e4bff4c5))


### Features

* **Serialization:** adding CanReadBits function to network reader ([3aae955](https://github.com/MirageNet/Mirage/commit/3aae955c8e30b87e9b005323febc9f1831c45bb7))

## [113.1.3](https://github.com/MirageNet/Mirage/compare/v113.1.2...v113.1.3) (2021-12-20)


### Bug Fixes

* fix a typo with NetworkServer disconnection logs, improve comments, fix formatting ([#1005](https://github.com/MirageNet/Mirage/issues/1005)) ([adcf3f6](https://github.com/MirageNet/Mirage/commit/adcf3f659c70c2fe8d425546d6018915370db736))

## [113.1.2](https://github.com/MirageNet/Mirage/compare/v113.1.1...v113.1.2) (2021-12-08)


### Bug Fixes

* fixing error message in message handler ([b38a24c](https://github.com/MirageNet/Mirage/commit/b38a24c8bd0d0c31245742b038e837f5d70bf979))

## [113.1.1](https://github.com/MirageNet/Mirage/compare/v113.1.0...v113.1.1) (2021-12-07)


### Bug Fixes

* adding meta files ([cca1ae2](https://github.com/MirageNet/Mirage/commit/cca1ae2ebd94eab366e4dadb080199b5fc872b56))

# [113.1.0](https://github.com/MirageNet/Mirage/compare/v113.0.4...v113.1.0) (2021-12-07)


### Features

* adding packers for variable length floats ([4c38c09](https://github.com/MirageNet/Mirage/commit/4c38c095461fc4fa1b823563cdb8075abe45dfe3))

## [113.0.4](https://github.com/MirageNet/Mirage/compare/v113.0.3...v113.0.4) (2021-12-04)


### Performance Improvements

* limit when pool logs when creating over max ([#1001](https://github.com/MirageNet/Mirage/issues/1001)) ([ef5c426](https://github.com/MirageNet/Mirage/commit/ef5c426e1adf1d6ab8b4524d341a256fbae024ad)), closes [#998](https://github.com/MirageNet/Mirage/issues/998)

## [113.0.3](https://github.com/MirageNet/Mirage/compare/v113.0.2...v113.0.3) (2021-12-04)


### Bug Fixes

* fixing rpc return values for unity 2021.2 ([#1000](https://github.com/MirageNet/Mirage/issues/1000)) ([a6ee5e8](https://github.com/MirageNet/Mirage/commit/a6ee5e8d618398b171c4698878a2ced8140621aa))

## [113.0.2](https://github.com/MirageNet/Mirage/compare/v113.0.1...v113.0.2) (2021-12-04)


### Bug Fixes

* fixing Return rpc for values that require ObjectLocator ([11b2fb3](https://github.com/MirageNet/Mirage/commit/11b2fb3473b74b7b46447c0e449fa9b5dbf3bc1d))

## [113.0.1](https://github.com/MirageNet/Mirage/compare/v113.0.0...v113.0.1) (2021-12-04)


### Bug Fixes

* fixing ReadNetworkBehaviour when NI is not found ([cb20ad9](https://github.com/MirageNet/Mirage/commit/cb20ad9a62fb22e158d2a97ff8c883bee2123b93))

# [113.0.0](https://github.com/MirageNet/Mirage/compare/v112.0.2...v113.0.0) (2021-11-29)


### Bug Fixes

* making NetworkServer.players a readonly collection ([f1b4512](https://github.com/MirageNet/Mirage/commit/f1b4512f00bde3be1e2da6c222e037939e442f8f))


### Features

* adding interfaces for SocketFactory to use so it is easier to get/set address and port ([#996](https://github.com/MirageNet/Mirage/issues/996)) ([e969e6d](https://github.com/MirageNet/Mirage/commit/e969e6dc4c85337e2deaa40202d557da6ac51f69))
* allowing events to be used with syncvar hook ([#991](https://github.com/MirageNet/Mirage/issues/991)) ([f455a2d](https://github.com/MirageNet/Mirage/commit/f455a2d8991cb88c8ea69764c5926e3626a590b6))


### Performance Improvements

* splitting peer update into 2 functions ([#993](https://github.com/MirageNet/Mirage/issues/993)) ([0d6d34b](https://github.com/MirageNet/Mirage/commit/0d6d34bd4d03800dc9edfc0adda1cd2d4b483d44))


### BREAKING CHANGES

* NetworkServer.Players is now a IReadOnlyCollection<INetworkPlayer>

## [112.0.2](https://github.com/MirageNet/Mirage/compare/v112.0.1...v112.0.2) (2021-11-22)


### Bug Fixes

* **NetworkReader:** fixing reset putting reader back into pool ([52253b6](https://github.com/MirageNet/Mirage/commit/52253b6e5e9de8131e37849b82da9e712707e86e))

## [112.0.1](https://github.com/MirageNet/Mirage/compare/v112.0.0...v112.0.1) (2021-11-07)


### Bug Fixes

* wrapping NetworkIdentityIdGenerator in #if editor ([f058f7f](https://github.com/MirageNet/Mirage/commit/f058f7f1a2177185742b36e034064d19bf66ea17))

# [112.0.0](https://github.com/MirageNet/Mirage/compare/v111.1.1...v112.0.0) (2021-11-07)


### Bug Fixes

* adding a check to make sure asset id isn't created from empty string ([26a5bbc](https://github.com/MirageNet/Mirage/commit/26a5bbc8269c6c4ff67a07bd9d62c95a8270cae5))
* fixing defines for Unity2021.2 ([#977](https://github.com/MirageNet/Mirage/issues/977)) ([15ffa5b](https://github.com/MirageNet/Mirage/commit/15ffa5bbb1343addd1294318f2489bc7797e3255))
* fixing id generation for negative numbers ([f5e1b9a](https://github.com/MirageNet/Mirage/commit/f5e1b9a72b37bf5ebbd43de85d4efd8567a2866d))
* renameing identity.Reset so it doesn't override unity's reset function ([bc5da6a](https://github.com/MirageNet/Mirage/commit/bc5da6a766c53e477421795030dd3a84d40777ff))
* using hex for logging prefabhash and sceneid ([f0c335f](https://github.com/MirageNet/Mirage/commit/f0c335f8ffb5a028e4ee38c3687f1c0f508f0969))


### Code Refactoring

* **NetworkIdentity:** moving id generation to its own file ([51795ce](https://github.com/MirageNet/Mirage/commit/51795ce2e48c1b19be044ac069b2846b26ae8094))


### Features

* **NetworkIdentity:** adding helper methods for Id ([0ddfdb8](https://github.com/MirageNet/Mirage/commit/0ddfdb835e9f6c8c06652d6a7533da36ca925876))
* **ServerObjectManager:** adding AddCharacter method that takes Identity and hash ([629036a](https://github.com/MirageNet/Mirage/commit/629036a000f9a2df1356a9f12b7eff8c315cfc42))
* **ServerObjectManager:** adding ReplaceCharacter method  that takes Identity and hash ([4320c05](https://github.com/MirageNet/Mirage/commit/4320c054ea07d0a9de332efed7aeaae9cd3ff5ed))


### Performance Improvements

* making transform values option to send with spawn message ([#972](https://github.com/MirageNet/Mirage/issues/972)) ([22e7dcd](https://github.com/MirageNet/Mirage/commit/22e7dcd8cd2cec20f2ec6f34a48312014726d4cf))
* no longer sending PrefabHash if the object is a scene id ([46e29a6](https://github.com/MirageNet/Mirage/commit/46e29a66c01e819feb9007a799e1f2cafa6a4f98))


### BREAKING CHANGES

* Scene objects based on a prefab will no longer have their PrefabHash set on spawn on the client
* **NetworkIdentity:** Scene Id generation is no longer public

## [111.1.1](https://github.com/MirageNet/Mirage/compare/v111.1.0...v111.1.1) (2021-11-06)


### Bug Fixes

* weaver error on guard attributes put on awake ([#986](https://github.com/MirageNet/Mirage/issues/986)) ([2f89372](https://github.com/MirageNet/Mirage/commit/2f893724d9b8cb04063a3c296ce54c81eee75cbf))

# [111.1.0](https://github.com/MirageNet/Mirage/compare/v111.0.0...v111.1.0) (2021-11-02)


### Features

* **NetworkVisibility:** adding new visibility scripts for checking scenes ([#958](https://github.com/MirageNet/Mirage/issues/958)) ([6725625](https://github.com/MirageNet/Mirage/commit/67256253934b34fec84fd644a11c34a982980ea8))

# [111.0.0](https://github.com/MirageNet/Mirage/compare/v110.0.0...v111.0.0) (2021-11-01)


### Code Refactoring

* **NetworkIdentity:** removing clientAuthorityCallback ([#971](https://github.com/MirageNet/Mirage/issues/971)) ([8adf83d](https://github.com/MirageNet/Mirage/commit/8adf83db2cba4c12c75ca39ea1e5d0059cc71bff))


### BREAKING CHANGES

* **NetworkIdentity:** NetworkIdentity.clientAuthorityCallback removed

# [110.0.0](https://github.com/MirageNet/Mirage/compare/v109.0.3...v110.0.0) (2021-11-01)


### Bug Fixes

* adding method to remove character without destroying the object ([19cad00](https://github.com/MirageNet/Mirage/commit/19cad00a00bd85536d36ec826d1e849c31c7bb58)), closes [#883](https://github.com/MirageNet/Mirage/issues/883)
* fixing remove authority for host ([528f66b](https://github.com/MirageNet/Mirage/commit/528f66bf1af6c2fb339532f0ac80ad6be12c1f32))


### BREAKING CHANGES

* RemovePlayerForConnection removed, use RemoveCharacter or DestroyCharacter instead. Note for RemoveCharacter destroyServerObject now defaults to true

## [109.0.3](https://github.com/MirageNet/Mirage/compare/v109.0.2...v109.0.3) (2021-10-31)


### Bug Fixes

* fixing spawn prefab when handler is null ([eb0fd1c](https://github.com/MirageNet/Mirage/commit/eb0fd1c6956032692cea171b8829b9dc5428c2f5))

## [109.0.2](https://github.com/MirageNet/Mirage/compare/v109.0.1...v109.0.2) (2021-10-29)


### Bug Fixes

* **NetworkIdentity:** not sending whole spawn message when removing authority ([#976](https://github.com/MirageNet/Mirage/issues/976)) ([127bcbf](https://github.com/MirageNet/Mirage/commit/127bcbfa29432c92e3a50fb53791a4f6ff817b7d))

## [109.0.1](https://github.com/MirageNet/Mirage/compare/v109.0.0...v109.0.1) (2021-10-27)


### Bug Fixes

* fixing using for editor ([27c9522](https://github.com/MirageNet/Mirage/commit/27c9522bdedf9083b73248e11f06611835dc2c28))
* fixing validate for new prefab id ([f66c3b6](https://github.com/MirageNet/Mirage/commit/f66c3b653b91a759f2687329791fe1e251e44a41))

# [109.0.0](https://github.com/MirageNet/Mirage/compare/v108.0.1...v109.0.0) (2021-10-23)


### Performance Improvements

* replacing assetid with prefab hash ([b14c692](https://github.com/MirageNet/Mirage/commit/b14c692ae71bb16955b6d58e1f27024c14d10401))


### BREAKING CHANGES

* prefab Id is now an int instead of a guid

## [108.0.1](https://github.com/MirageNet/Mirage/compare/v108.0.0...v108.0.1) (2021-10-21)


### Bug Fixes

* **NetworkSceneChecker:** fixing awake for networkscenechecker ([fb4321e](https://github.com/MirageNet/Mirage/commit/fb4321eea0b86fb5ed3f979ff10e36533d24c88e)), closes [#754](https://github.com/MirageNet/Mirage/issues/754)

# [108.0.0](https://github.com/MirageNet/Mirage/compare/v107.1.3...v108.0.0) (2021-10-21)


### Bug Fixes

* **LogSettings:** allowing multi object editing ([5c83dc6](https://github.com/MirageNet/Mirage/commit/5c83dc6e6a8dfa975bbf1c24cae057b4ed5a5057))
* **LogSettings:** improving warning when log settings has no reference ([c2ed26f](https://github.com/MirageNet/Mirage/commit/c2ed26f02e764ac41a9a3a58fb282dce1e7973a7))
* **LogSettings:** removing log settings component from samples ([a72d777](https://github.com/MirageNet/Mirage/commit/a72d777d2e368da0bb68d0a487c49988db421f5b))
* removing redundant property ([f8bda6e](https://github.com/MirageNet/Mirage/commit/f8bda6e2b62fc6c676a579a31b1d535043244e44))


### Code Refactoring

* **NetworkIdentity:** removing NetworkIdentity.GetSceneIdentity ([34699ae](https://github.com/MirageNet/Mirage/commit/34699aee1f3427cca691c69f74ba73a32a1695a7))


### BREAKING CHANGES

* **NetworkIdentity:** `NetworkIdentity.GetSceneIdentity` has been removed

## [107.1.3](https://github.com/MirageNet/Mirage/compare/v107.1.2...v107.1.3) (2021-10-20)


### Bug Fixes

* **LogSettings:** making labels clickable ([7f65a8a](https://github.com/MirageNet/Mirage/commit/7f65a8a03f57024663ce8eaa4f34e5fa62a7f076))

## [107.1.2](https://github.com/MirageNet/Mirage/compare/v107.1.1...v107.1.2) (2021-10-19)


### Bug Fixes

* **LogSettings:** making reset exit if settings is already set ([a227593](https://github.com/MirageNet/Mirage/commit/a227593f158502e7fa2e9684343445be626bee21))

## [107.1.1](https://github.com/MirageNet/Mirage/compare/v107.1.0...v107.1.1) (2021-10-12)


### Bug Fixes

* **NetworkBehaviour:** fixing find identity if parent gameobject is disabled ([04700f9](https://github.com/MirageNet/Mirage/commit/04700f9bb664dae6ac306b236464e5427ff7d09e))

# [107.1.0](https://github.com/MirageNet/Mirage/compare/v107.0.1...v107.1.0) (2021-10-10)


### Bug Fixes

* this fixes issues with multi scene loading on server and client ([#965](https://github.com/MirageNet/Mirage/issues/965)) ([e725a41](https://github.com/MirageNet/Mirage/commit/e725a417be530a3b617305d59a563339fe9fb93f))


### Features

* added a method to do physic scene loads on server and tell clients about it ([f215cef](https://github.com/MirageNet/Mirage/commit/f215cefaf594c5b0bac4223dfe837623bd480b4b))

## [107.0.1](https://github.com/MirageNet/Mirage/compare/v107.0.0...v107.0.1) (2021-10-09)


### Bug Fixes

* **ServerRpc:** fixing sender for server rpc in host mode ([#961](https://github.com/MirageNet/Mirage/issues/961)) ([41b6b79](https://github.com/MirageNet/Mirage/commit/41b6b79156cce14382e7c700aba1c1381c4eb136))

# [107.0.0](https://github.com/MirageNet/Mirage/compare/v106.2.4...v107.0.0) (2021-10-08)


### Performance Improvements

* **BitPacking:** using quaternion compression by default ([#957](https://github.com/MirageNet/Mirage/issues/957)) ([e9fedf1](https://github.com/MirageNet/Mirage/commit/e9fedf10e57ba0b043ef62c4fa9be06c80de9c72))


### BREAKING CHANGES

* **BitPacking:** Pack extension methods renamed to WriteQuaternion and ReadQuaternion

## [106.2.4](https://github.com/MirageNet/Mirage/compare/v106.2.3...v106.2.4) (2021-10-06)


### Bug Fixes

* **CharacterSpawner:** respawning character if one exists ([203e487](https://github.com/MirageNet/Mirage/commit/203e487e686ba499e4a03c66586676d871146bfb))

## [106.2.3](https://github.com/MirageNet/Mirage/compare/v106.2.2...v106.2.3) (2021-10-06)


### Bug Fixes

* **NetworkServer:** making listening disable server peer ([#959](https://github.com/MirageNet/Mirage/issues/959)) ([528698b](https://github.com/MirageNet/Mirage/commit/528698b10ced354aeb155c6a09b30ba1228f535a))

## [106.2.2](https://github.com/MirageNet/Mirage/compare/v106.2.1...v106.2.2) (2021-10-05)


### Performance Improvements

* **NetworkSceneManager:** using hashset contains to check for player ([e3df3b5](https://github.com/MirageNet/Mirage/commit/e3df3b5403b135491e2879f5f5f8bd29cb02d3d2))

## [106.2.1](https://github.com/MirageNet/Mirage/compare/v106.2.0...v106.2.1) (2021-10-02)


### Bug Fixes

* **LogGUI:** fixing exception in find all loggers for generic types ([11755e7](https://github.com/MirageNet/Mirage/commit/11755e7bf61d08908b7dcf95d65a45fd9b228696))
* **LogGUI:** fixing exception in find all loggers for generic types ([6221965](https://github.com/MirageNet/Mirage/commit/622196562696902e75abe23795d07ea5dc8a74ba))

# [106.2.0](https://github.com/MirageNet/Mirage/compare/v106.1.0...v106.2.0) (2021-10-01)


### Bug Fixes

* **BitPacking:** setting default QuaternionPack to 9 ([bcc44a7](https://github.com/MirageNet/Mirage/commit/bcc44a7cc43f2a2787c02c757141d183c7af9b78))


### Features

* **Metrics:** allowing background to be updated at runtime ([1974a76](https://github.com/MirageNet/Mirage/commit/1974a7618cd4de1a973d67bf70fcf1e0c0188dfb))

# [106.1.0](https://github.com/MirageNet/Mirage/compare/v106.0.0...v106.1.0) (2021-10-01)


### Features

* adding sequence size of metrics to inspector ([c7a21dd](https://github.com/MirageNet/Mirage/commit/c7a21dd9976b0263f790c3c537398c89dc6057c7))

# [106.0.0](https://github.com/MirageNet/Mirage/compare/v105.1.3...v106.0.0) (2021-10-01)


### Bug Fixes

* fixing use of network identity in network message ([#955](https://github.com/MirageNet/Mirage/issues/955)) ([bc1b82a](https://github.com/MirageNet/Mirage/commit/bc1b82a339ba1d67620c07d060a5588100d50d96))


### Features

* better log settings ([#951](https://github.com/MirageNet/Mirage/issues/951)) ([6395251](https://github.com/MirageNet/Mirage/commit/639525107cb3e780f46d882a83538a526a97f76c))
* **Peer:** updating peer metrics to have more data ([#940](https://github.com/MirageNet/Mirage/issues/940)) ([512d916](https://github.com/MirageNet/Mirage/commit/512d916d33d338a9e02017e65f02eb7e53e98dd1))


### BREAKING CHANGES

* log settings are now saved to a SO file instead of EditorPrefs

## [105.1.3](https://github.com/MirageNet/Mirage/compare/v105.1.2...v105.1.3) (2021-10-01)


### Bug Fixes

* fixing errror meessage for server client attributes ([#953](https://github.com/MirageNet/Mirage/issues/953)) ([53cf60b](https://github.com/MirageNet/Mirage/commit/53cf60b234e71ccda00df7a37bf75ee2b8bc833a))

## [105.1.2](https://github.com/MirageNet/Mirage/compare/v105.1.1...v105.1.2) (2021-09-30)


### Bug Fixes

* **ServerRpc:** fixing error message when client is not set ([f9c8033](https://github.com/MirageNet/Mirage/commit/f9c8033b1ad4e137d439336bd515d3ae96df1cdc))

## [105.1.1](https://github.com/MirageNet/Mirage/compare/v105.1.0...v105.1.1) (2021-09-25)


### Bug Fixes

* Character spawner was registering the player prefab in awake and during scene loads this wont ever register again. Need to change it on scene changes. ([d6cdc86](https://github.com/MirageNet/Mirage/commit/d6cdc864616878ebed2f11fe2054aa2b49819626))
* updates to SpawnObject.md and SpawnObjects.PNG files. ([02e07fa](https://github.com/MirageNet/Mirage/commit/02e07fab67f344bf6d7a3e0c67a7c0ea21c5eff1))

# [105.1.0](https://github.com/MirageNet/Mirage/compare/v105.0.1...v105.1.0) (2021-09-23)


### Bug Fixes

* fixing order that extension methods are found ([#917](https://github.com/MirageNet/Mirage/issues/917)) ([aa8fe87](https://github.com/MirageNet/Mirage/commit/aa8fe87d94a8b20ec9de882c731eb54f314e4b84))


### Features

* **Peer:** adding send buffer limit ([#939](https://github.com/MirageNet/Mirage/issues/939)) ([b4666cb](https://github.com/MirageNet/Mirage/commit/b4666cbd890d2c1cde45dc5950a215394adf773b))

## [105.0.1](https://github.com/MirageNet/Mirage/compare/v105.0.0...v105.0.1) (2021-09-22)


### Bug Fixes

* fixing assert when loading scene using its name ([72ac3a6](https://github.com/MirageNet/Mirage/commit/72ac3a64508b73cc168ecdd7192724841a362249))
* logging error if no handler is registered ([54c8520](https://github.com/MirageNet/Mirage/commit/54c8520f9665362f47608038f8e383aa47a69049))
* **NetworkSceneManager:** registering ready and not ready message for host client ([6a97ae9](https://github.com/MirageNet/Mirage/commit/6a97ae95a29267907c6435c2e28e33aed41a0d3d))


### Reverts

* "fix: logging error if no handler is registered" ([aca8c5a](https://github.com/MirageNet/Mirage/commit/aca8c5a79bd870d68ad0e979120b11b5999d6c93))

# [105.0.0](https://github.com/MirageNet/Mirage/compare/v104.3.0...v105.0.0) (2021-09-22)


### Bug Fixes

* adding client active check in network ping display ([1d43243](https://github.com/MirageNet/Mirage/commit/1d43243ba72e1b4ca96e8bd9b3f55fc9f0980b9d))


### Code Refactoring

* move network time to world ([6b10ba2](https://github.com/MirageNet/Mirage/commit/6b10ba2098ee4fd4e924c64ae96f513eaad8cf87))


### BREAKING CHANGES

* NetworkTime move from NetworkServer/NetworkClient to NetworkWorld

# [104.3.0](https://github.com/MirageNet/Mirage/compare/v104.2.0...v104.3.0) (2021-09-22)


### Features

* **Syncvar:** adding InitialOnly to syncvar ([#941](https://github.com/MirageNet/Mirage/issues/941)) ([abf4637](https://github.com/MirageNet/Mirage/commit/abf463709c8115dc3e039b288661d9ca2c18b7c7))

# [104.2.0](https://github.com/MirageNet/Mirage/compare/v104.1.0...v104.2.0) (2021-09-20)


### Bug Fixes

* **Peer:** throwing if sequence size is too big ([6c7c5ad](https://github.com/MirageNet/Mirage/commit/6c7c5ad0a5837f9b4e147dcb06b410297214bf84))


### Features

* **Weaver:** adding constant values for max value for number of bits ([#934](https://github.com/MirageNet/Mirage/issues/934)) ([2f7c322](https://github.com/MirageNet/Mirage/commit/2f7c322e1c76404fc172ed78c7d95de27b81fcc3))


### Performance Improvements

* **Peer:** updating header size for ipv6 ([a8798f0](https://github.com/MirageNet/Mirage/commit/a8798f08a6c9947c84c4b26fc97a4d752bd9de88))

# [104.1.0](https://github.com/MirageNet/Mirage/compare/v104.0.4...v104.1.0) (2021-09-20)


### Features

* **Weaver:** allowing bit packing attributes to work on structs and rpcs ([#933](https://github.com/MirageNet/Mirage/issues/933)) ([dca6b54](https://github.com/MirageNet/Mirage/commit/dca6b54b704f70e71aec9e0bb17a4d8eaa18ba22))

## [104.0.4](https://github.com/MirageNet/Mirage/compare/v104.0.3...v104.0.4) (2021-09-16)


### Performance Improvements

* **Weaver:** passing in format string ([6c40fd6](https://github.com/MirageNet/Mirage/commit/6c40fd6a3e1c813ee3e3881fc7cd9551360cea87))

## [104.0.3](https://github.com/MirageNet/Mirage/compare/v104.0.2...v104.0.3) (2021-09-14)


### Performance Improvements

* **Weaver:** increasing performance of PostProcessorReflectionImporter ([#931](https://github.com/MirageNet/Mirage/issues/931)) ([13e6d1f](https://github.com/MirageNet/Mirage/commit/13e6d1fdc7f0c47900e5bfb2a7830cde7ed1b56c))

## [104.0.2](https://github.com/MirageNet/Mirage/compare/v104.0.1...v104.0.2) (2021-09-13)


### Performance Improvements

* **Weaver:** caching NetworkBehaviour properties ([#927](https://github.com/MirageNet/Mirage/issues/927)) ([1f2c53b](https://github.com/MirageNet/Mirage/commit/1f2c53b5e368acc688e51a6d5bba2df2ae92379f))

## [104.0.1](https://github.com/MirageNet/Mirage/compare/v104.0.0...v104.0.1) (2021-09-13)


### Performance Improvements

* **Weaver:** optimizing find file in assembly resolver ([#925](https://github.com/MirageNet/Mirage/issues/925)) ([68d6749](https://github.com/MirageNet/Mirage/commit/68d67497abbec0d69428b4b36b4aaba3576429ce))

# [104.0.0](https://github.com/MirageNet/Mirage/compare/v103.1.1...v104.0.0) (2021-09-08)


### Bug Fixes

* fixing DestroyOwnedObjects when Identity is null ([4c12efe](https://github.com/MirageNet/Mirage/commit/4c12efe95f84cf55714ce155b3ecb934e0fd411a))
* fixing ReplaceCharacter when identity has no character ([b685e11](https://github.com/MirageNet/Mirage/commit/b685e1153d34c46e08420f7f298e345d61f73b0a))


### Code Refactoring

* changing OnServerAuthenticated to protected ([b0da955](https://github.com/MirageNet/Mirage/commit/b0da9559b7bd993f1a24a7ec9e0805147f0c8c0e))
* moving ready methods to NetworkSceneManager ([5dade34](https://github.com/MirageNet/Mirage/commit/5dade34fb8961b50124291e1761634b730e0907e))
* networkplayer sceneis ready notw defaults to true ([319e8d8](https://github.com/MirageNet/Mirage/commit/319e8d87049c78899dcfc8928e26f00f4a829ee5))
* renaming NotReadyMessage ([77f7777](https://github.com/MirageNet/Mirage/commit/77f7777f94c2428947866c76c994cb6ea3d48973))
* renaming remove observers ([12ffce7](https://github.com/MirageNet/Mirage/commit/12ffce74221e6b18a508ae3bbd6cfaf558c5322d))
* renaming SceneObjectManager.SetClientReady ([3f8d2bc](https://github.com/MirageNet/Mirage/commit/3f8d2bc56f4d526ce0a04943a5c5a5e17b563c3d))
* replacing ReadyMessage with SceneReadyMessage ([beb4ed9](https://github.com/MirageNet/Mirage/commit/beb4ed9bb8a74ca5e50faead99c5c237e8ab4d31))


### Features

* adding HasCharacter property to network player ([445081a](https://github.com/MirageNet/Mirage/commit/445081a54bf1784da7c243f19601c221fb859525))
* adding OnPlayerSceneReady event ([e59c93c](https://github.com/MirageNet/Mirage/commit/e59c93c46ccc39ca6863d6e9dfd4b03abe9e0d6a))
* adding option to ignore character check for spawning objects ([9234eb4](https://github.com/MirageNet/Mirage/commit/9234eb4957c83b2eda050d577d9f1d9365ea8878))


### BREAKING CHANGES

* NetworkServerManager.OnServerAuthenticated is now protected instead of public
* NetworkPlayer.SceneIsReady now default to true
* moving SetAllClientsNotReady and SetClientNotReady from ServerObjectManager to NetworkSceneManager
* Renaming SceneObjectManager.SetClientReady to SpawnVisibleObjects
* Removing ReadyMessage, Use SceneReadyMessage instead
* Renaming NotReadyMessage to SceneNotReadyMessage
* NetworkPlayer.RemoveObservers renamed to RemoveAllVisibleObjects

## [103.1.1](https://github.com/MirageNet/Mirage/compare/v103.1.0...v103.1.1) (2021-09-08)


### Bug Fixes

* allow to set specific number of logs to be used from changelog. Need to change it atm through code. ([d8d28f9](https://github.com/MirageNet/Mirage/commit/d8d28f973a4c0b7a76879b6fdeb74794b9594b80))
* did not realize there was changes not pushed to the branch got from master now and fixed it all up again. ([988641f](https://github.com/MirageNet/Mirage/commit/988641f040f88fd33bf18c878f368c1165d41521))
* paths for welcome window changelog. ([a1ec86d](https://github.com/MirageNet/Mirage/commit/a1ec86db6c13ca02cc90e9764faa78c03d69297c))
* proper fix using current welcome window script path to get changelog path. ([6868453](https://github.com/MirageNet/Mirage/commit/686845314fb3def8a613c2d8071bee39deb807de))
* this fixes styling and error on 2019+ editors. ([3b58a7b](https://github.com/MirageNet/Mirage/commit/3b58a7bef15bc4943aab65257feeb8bf85b9ace6))
* Welcome window now searches for existing file of the changelog to know which mirage install was done. ([ab96915](https://github.com/MirageNet/Mirage/commit/ab96915366704577d4aecbb39b1fd7af96ce5cf9))

# [103.1.0](https://github.com/MirageNet/Mirage/compare/v103.0.1...v103.1.0) (2021-09-08)


### Features

* attributes to use new var int bit packers ([#895](https://github.com/MirageNet/Mirage/issues/895)) ([1da5c42](https://github.com/MirageNet/Mirage/commit/1da5c426503cfb90a94fd41bb8c6a6b2f02e7a3f))

## [103.0.1](https://github.com/MirageNet/Mirage/compare/v103.0.0...v103.0.1) (2021-09-06)


### Bug Fixes

* fixing syncvar hook not being called in host mode ([#918](https://github.com/MirageNet/Mirage/issues/918)) ([7accba7](https://github.com/MirageNet/Mirage/commit/7accba707127ba57bf3c2d3f3382b0decfa466b8))

# [103.0.0](https://github.com/MirageNet/Mirage/compare/v102.0.0...v103.0.0) (2021-09-05)


### Code Refactoring

* NetIdentity to Identity ([dc00532](https://github.com/MirageNet/Mirage/commit/dc005327928e57618d503d893390a43450d086d0))
* renaming Client enum to RpcTarget ([bc32d06](https://github.com/MirageNet/Mirage/commit/bc32d0682c8673af8b4d6fc0327c2fdacd952e94))
* renaming ConnectionToClient to Owner ([5493eae](https://github.com/MirageNet/Mirage/commit/5493eae810a596ffeb4b2c021ecb163e59753644))


### BREAKING CHANGES

* NetIdentity renamed to Identity
* ConnectionToClient renamed to Owner
* enum used in ClientRpc has been renamed to RpcTarget

# [102.0.0](https://github.com/MirageNet/Mirage/compare/v101.10.0...v102.0.0) (2021-09-02)


### Features

* new improved scene manager  ([#892](https://github.com/MirageNet/Mirage/issues/892)) ([2a9bdec](https://github.com/MirageNet/Mirage/commit/2a9bdec6887bc67e7f53bb46f13592607b5c72b8))


### BREAKING CHANGES

* NetworkSceneManager has been re-written, many events and methods now have new names.

# [101.10.0](https://github.com/MirageNet/Mirage/compare/v101.9.2...v101.10.0) (2021-08-31)


### Features

* attributes to use new vector bit packers ([#905](https://github.com/MirageNet/Mirage/issues/905)) ([149bf5a](https://github.com/MirageNet/Mirage/commit/149bf5adc44f60ad810c2233afceea32c5af2e2a))

## [101.9.2](https://github.com/MirageNet/Mirage/compare/v101.9.1...v101.9.2) (2021-08-29)


### Bug Fixes

* fixing namespace for display metrics ([9d3f056](https://github.com/MirageNet/Mirage/commit/9d3f0568943e531474f56852453233e173e3508f))

## [101.9.1](https://github.com/MirageNet/Mirage/compare/v101.9.0...v101.9.1) (2021-08-29)


### Bug Fixes

* improving warning message for taking too many objects from pool ([2bc42c5](https://github.com/MirageNet/Mirage/commit/2bc42c512432ece82bf1eabc53ccf65d27e54556))

# [101.9.0](https://github.com/MirageNet/Mirage/compare/v101.8.0...v101.9.0) (2021-08-26)


### Bug Fixes

* **NanoSocket:** adding dispose and finalize ([#904](https://github.com/MirageNet/Mirage/issues/904)) ([ca949ea](https://github.com/MirageNet/Mirage/commit/ca949ea15b900ee7d35d180b73e44103ed54d064))


### Features

* attributes to use new float bit packers ([#896](https://github.com/MirageNet/Mirage/issues/896)) ([273d27c](https://github.com/MirageNet/Mirage/commit/273d27ce03efee81f507def913ce9a906b1a38f2))

# [101.8.0](https://github.com/MirageNet/Mirage/compare/v101.7.0...v101.8.0) (2021-08-24)


### Features

* attributes to calculate bit count of a given range ([#902](https://github.com/MirageNet/Mirage/issues/902)) ([1c22ea6](https://github.com/MirageNet/Mirage/commit/1c22ea63217a3206cb0eb41174135e8fc0133138))

# [101.7.0](https://github.com/MirageNet/Mirage/compare/v101.6.0...v101.7.0) (2021-08-24)


### Features

* attributes to use zig zag encoding ([#897](https://github.com/MirageNet/Mirage/issues/897)) ([ccef5fb](https://github.com/MirageNet/Mirage/commit/ccef5fb0302ff97d897f17698e53967e014a9a95))

# [101.6.0](https://github.com/MirageNet/Mirage/compare/v101.5.1...v101.6.0) (2021-08-22)


### Features

* **serialization:** attribute to set bit size for ints base syncvars ([#882](https://github.com/MirageNet/Mirage/issues/882)) ([1660ca6](https://github.com/MirageNet/Mirage/commit/1660ca690bee0ef58e398d36511233ebd9975188))

## [101.5.1](https://github.com/MirageNet/Mirage/compare/v101.5.0...v101.5.1) (2021-08-22)


### Performance Improvements

* adding throw helper methods so that AggressiveInlining works ([#894](https://github.com/MirageNet/Mirage/issues/894)) ([de12166](https://github.com/MirageNet/Mirage/commit/de1216690a1823724fbb6c717d55a80e44784fa5))

# [101.5.0](https://github.com/MirageNet/Mirage/compare/v101.4.2...v101.5.0) (2021-08-22)


### Features

* **NetworkVisibility:** adding default implementation for OnRebuildObservers ([2bcf22f](https://github.com/MirageNet/Mirage/commit/2bcf22f26c1e76f0ed28a066a8c8384f8396077a))


### Performance Improvements

* sending reply id as nullable ([a756389](https://github.com/MirageNet/Mirage/commit/a7563890c49ca0958332f8fb9fedcca5111ed85f))

## [101.4.2](https://github.com/MirageNet/Mirage/compare/v101.4.1...v101.4.2) (2021-08-21)


### Bug Fixes

* fixing log message for checking observers for connected player ([99c31c8](https://github.com/MirageNet/Mirage/commit/99c31c8df337c363a2b4a0be9843dcf8fe24913c))
* fixing mistake in log change ([b090fa2](https://github.com/MirageNet/Mirage/commit/b090fa2f619ec87de4bc1534343e977247de12f5))

## [101.4.1](https://github.com/MirageNet/Mirage/compare/v101.4.0...v101.4.1) (2021-08-19)


### Bug Fixes

* **weaver:** fixing dirty bit for syncvar ([b4a837d](https://github.com/MirageNet/Mirage/commit/b4a837dde40da17ce7947b835da7e6b747d14e32))

# [101.4.0](https://github.com/MirageNet/Mirage/compare/v101.3.0...v101.4.0) (2021-08-17)


### Bug Fixes

* fixing pong example ([5ccb42e](https://github.com/MirageNet/Mirage/commit/5ccb42e2c9fdc94bc0e2edec7f669a075f89b604))


### Features

* adding helper classes for packing uint values ([#878](https://github.com/MirageNet/Mirage/issues/878)) ([3c24f67](https://github.com/MirageNet/Mirage/commit/3c24f67dc6140c9c711b26837ea5b9c8220c1cc1))

# [101.3.0](https://github.com/MirageNet/Mirage/compare/v101.2.0...v101.3.0) (2021-08-16)


### Bug Fixes

* preparing client objects on connect ([#876](https://github.com/MirageNet/Mirage/issues/876)) ([9789c0b](https://github.com/MirageNet/Mirage/commit/9789c0b5851c8377e13f799f4a437197e801ecac))
* **SocketLayer:** moving endpoint copy outside of connection ([c19929f](https://github.com/MirageNet/Mirage/commit/c19929f19b110406b85ac11b6c49a365f5d7da39))


### Features

* adding helper classes for packing float, vector and quaternion ([#847](https://github.com/MirageNet/Mirage/issues/847)) ([410bcd6](https://github.com/MirageNet/Mirage/commit/410bcd6475e851dfd0a63944051f6522662f8e85))

# [101.2.0](https://github.com/MirageNet/Mirage/compare/v101.1.0...v101.2.0) (2021-08-10)


### Bug Fixes

* only invoking unspawn even if item was removed from dictionary ([259e8d0](https://github.com/MirageNet/Mirage/commit/259e8d0dc736bf262fd6bde552618bb37daba31e))
* **NetworkReader:** checking offset when moving bit position ([641b2b0](https://github.com/MirageNet/Mirage/commit/641b2b005ea7fae6afaba98393344e067f530abf))


### Features

* adding destroy function that takes network identity ([e91f6d3](https://github.com/MirageNet/Mirage/commit/e91f6d34189159cb8d632949d7fceb7071c950f2))

# [101.1.0](https://github.com/MirageNet/Mirage/compare/v101.0.2...v101.1.0) (2021-08-06)


### Features

* **peer:** adding scripts to display metrics from peer ([#872](https://github.com/MirageNet/Mirage/issues/872)) ([e7ac06f](https://github.com/MirageNet/Mirage/commit/e7ac06f972e089f53adc4a90ab90ade1fb309f05))
* adding way to call SendNotify without allocations and example ([#875](https://github.com/MirageNet/Mirage/issues/875)) ([16b3000](https://github.com/MirageNet/Mirage/commit/16b300002a28eff27bbc2d880993fd61271a4ab6))

## [101.0.2](https://github.com/MirageNet/Mirage/compare/v101.0.1...v101.0.2) (2021-08-01)


### Bug Fixes

* fixing error message for disconnect ([0b603fe](https://github.com/MirageNet/Mirage/commit/0b603fe9e15fe2d333c869c8ff9cc51a4c27a823))
* fixing resize buffer so that it uses byte capacity ([927fe95](https://github.com/MirageNet/Mirage/commit/927fe9574e6a28aac15e90bf4ecc8d3172d423b4))
* fixing resize buffer when new size is greater than double ([972b6d2](https://github.com/MirageNet/Mirage/commit/972b6d2b811e8082504d9da26b1dd36f377c05eb))
* removing debug logs from resize buffer ([5f3524c](https://github.com/MirageNet/Mirage/commit/5f3524cf92fcc975928288e31bd456b608ca67f1))

## [101.0.1](https://github.com/MirageNet/Mirage/compare/v101.0.0...v101.0.1) (2021-07-29)


### Bug Fixes

* making weaver generate serialize functions for nested messages ([#873](https://github.com/MirageNet/Mirage/issues/873)) ([a351222](https://github.com/MirageNet/Mirage/commit/a351222a3a7d2f75404bfebda049ea270f2f4e63))

# [101.0.0](https://github.com/MirageNet/Mirage/compare/v100.0.2...v101.0.0) (2021-07-29)


### Code Refactoring

* removing old version of sequencer ([0efe4ba](https://github.com/MirageNet/Mirage/commit/0efe4babede7726a9dc10590b27194924e4ad7b5))


### BREAKING CHANGES

* removing old version of Sequencer, use version in socket layer instead

## [100.0.2](https://github.com/MirageNet/Mirage/compare/v100.0.1...v100.0.2) (2021-07-25)


### Performance Improvements

* removing alloc from reliable sends ([00945f3](https://github.com/MirageNet/Mirage/commit/00945f3f310453f5e57e1b6495be125282d3b8c3))
* removing allocations from SendToMany ([c57f64d](https://github.com/MirageNet/Mirage/commit/c57f64d7695e1bddefb39fa151ac21b5d2176f35))

## [100.0.1](https://github.com/MirageNet/Mirage/compare/v100.0.0...v100.0.1) (2021-07-19)


### Bug Fixes

* **NanoSocket:** adding Exception when nanosocket bind fails ([a3028ec](https://github.com/MirageNet/Mirage/commit/a3028ecb06084574cbbb0b10bbd33394d44d94d2))


### Performance Improvements

* adding native UDP socket (NanoSockets) for supported platforms ([#860](https://github.com/MirageNet/Mirage/issues/860)) ([3f34863](https://github.com/MirageNet/Mirage/commit/3f34863b65325a54d6a4542c7b767fedc1abf406))
* removing allocations from assert ([5c216de](https://github.com/MirageNet/Mirage/commit/5c216de5411f6ae9e8a7bb14e52323e214be2793))

# [100.0.0](https://github.com/MirageNet/Mirage/compare/v99.1.0...v100.0.0) (2021-07-15)


### Code Refactoring

* moving message handling out of networkplayer ([#818](https://github.com/MirageNet/Mirage/issues/818)) ([b2e9d96](https://github.com/MirageNet/Mirage/commit/b2e9d9693471097ac86fabd725bad8aa6b444983))


### BREAKING CHANGES

* RegisterHandler functions now exist on MessageHandler On Server and Client
* NetworkAuthenticator now use Setup methods that should be used to register messages

# [99.1.0](https://github.com/MirageNet/Mirage/compare/v99.0.2...v99.1.0) (2021-07-15)


### Features

* **NetworkWriter:** adding method to move position and docs comments ([#861](https://github.com/MirageNet/Mirage/issues/861)) ([35cf3ec](https://github.com/MirageNet/Mirage/commit/35cf3ecbffbb582bad3022b93b6d5c09ab266f48))

## [99.0.2](https://github.com/MirageNet/Mirage/compare/v99.0.1...v99.0.2) (2021-07-05)


### Bug Fixes

* fixing syncvar reading when using bools ([31aca8e](https://github.com/MirageNet/Mirage/commit/31aca8e95752628ed3ad0c6d8e415fcce296b817))

## [99.0.1](https://github.com/MirageNet/Mirage/compare/v99.0.0...v99.0.1) (2021-07-03)


### Bug Fixes

* adding assembly version to editor asmdef ([5969236](https://github.com/MirageNet/Mirage/commit/5969236f7a0d6e57848e51cb084f824233e367a4))


### Performance Improvements

* **WelcomeWindow:** using string builder for change log ([4bc10d8](https://github.com/MirageNet/Mirage/commit/4bc10d840890345f79c24082d283b06302f86b82))

# [99.0.0](https://github.com/MirageNet/Mirage/compare/v98.0.1...v99.0.0) (2021-07-03)


### Performance Improvements

* replacing network writer with a faster version that can do bit packing ([#805](https://github.com/MirageNet/Mirage/issues/805)) ([3cffa66](https://github.com/MirageNet/Mirage/commit/3cffa662fee2b09fb54f549d42d820300c61ecda))


### BREAKING CHANGES

* NetworkWriter and NetworkReader have been completely re-written to support bitpacking

## [98.0.1](https://github.com/MirageNet/Mirage/compare/v98.0.0...v98.0.1) (2021-07-02)


### Performance Improvements

* replacing network writer with a faster version that can do bit packing ([#805](https://github.com/MirageNet/Mirage/issues/805)) ([773c58f](https://github.com/MirageNet/Mirage/commit/773c58f75eb77be2a893398d1e27012ee9ba83e6))

# [98.0.0](https://github.com/MirageNet/Mirage/compare/v97.1.2...v98.0.0) (2021-07-02)


### Performance Improvements

* improving how socketlayer handles endpoints ([#856](https://github.com/MirageNet/Mirage/issues/856)) ([59ce7e0](https://github.com/MirageNet/Mirage/commit/59ce7e097d46d7f2cb7eaa01736dde36066396c5))


### BREAKING CHANGES

* Socket functions now use an interface instead of the EndPoint class, Socket Implementations should create a custom Endpoint class for their socket.

## [97.1.2](https://github.com/MirageNet/Mirage/compare/v97.1.1...v97.1.2) (2021-07-01)


### Performance Improvements

* **SocketLayer:** adding IEquatable to struct ([f935786](https://github.com/MirageNet/Mirage/commit/f93578641956c17d84f32ab6356887f45974b07d))

## [97.1.1](https://github.com/MirageNet/Mirage/compare/v97.1.0...v97.1.1) (2021-06-28)


### Bug Fixes

* **NetworkManagerHud:** adding null check before using server or client ([e4200e0](https://github.com/MirageNet/Mirage/commit/e4200e07b9c424eccda9e8bfb75a743af753ad78))
* **NetworkManagerHud:** returning to offline menu when server or client is stopped ([6b4c988](https://github.com/MirageNet/Mirage/commit/6b4c98889777cad31d27d8211f8858e4d81bde09))

# [97.1.0](https://github.com/MirageNet/Mirage/compare/v97.0.1...v97.1.0) (2021-06-27)


### Features

* **SocketLayer:** adding fragmentation to reliable sending ([#851](https://github.com/MirageNet/Mirage/issues/851)) ([4764294](https://github.com/MirageNet/Mirage/commit/4764294b18a6e20780fe05626bd001f8c63790f8))

## [97.0.1](https://github.com/MirageNet/Mirage/compare/v97.0.0...v97.0.1) (2021-06-27)


### Bug Fixes

* fixing first notify not being returned ([#854](https://github.com/MirageNet/Mirage/issues/854)) ([84b7d2e](https://github.com/MirageNet/Mirage/commit/84b7d2e4cf938763772ebedbb5a55e2db0034ff4))

# [97.0.0](https://github.com/MirageNet/Mirage/compare/v96.5.2...v97.0.0) (2021-06-27)


### Code Refactoring

* simplifying packet size in config ([#852](https://github.com/MirageNet/Mirage/issues/852)) ([6bc5ab8](https://github.com/MirageNet/Mirage/commit/6bc5ab8e87c9ec6d2a35a836119a4c891a5a173c))


### BREAKING CHANGES

* BufferSize and MTU replaced by MaxPacketSize

## [96.5.2](https://github.com/MirageNet/Mirage/compare/v96.5.1...v96.5.2) (2021-06-27)


### Bug Fixes

* fixing typo in error ([063e3b4](https://github.com/MirageNet/Mirage/commit/063e3b472af003eda400b1fcbbe863f63b3de422))

## [96.5.1](https://github.com/MirageNet/Mirage/compare/v96.5.0...v96.5.1) (2021-06-27)


### Bug Fixes

* fixing typo in error ([6195108](https://github.com/MirageNet/Mirage/commit/6195108dd84d5e83be3ac25d824d50125fa9510f))

# [96.5.0](https://github.com/MirageNet/Mirage/compare/v96.4.3...v96.5.0) (2021-06-23)


### Bug Fixes

* fixing length used for sending unreliable ([821e2d2](https://github.com/MirageNet/Mirage/commit/821e2d28b2a764f834b96696ed6a438af2535f60))


### Features

* adding send methods for array segment ([ea09c61](https://github.com/MirageNet/Mirage/commit/ea09c61e0c6c67990d05a58529a800a6948728c0))


### Performance Improvements

* using array segments for sending ([c990952](https://github.com/MirageNet/Mirage/commit/c990952246463282687c2968852dbe1ae36fdb6e))
* using pool for send notify ([ac000eb](https://github.com/MirageNet/Mirage/commit/ac000eb6c2db50c81831d092c358f9e707954876))

## [96.4.3](https://github.com/MirageNet/Mirage/compare/v96.4.2...v96.4.3) (2021-06-23)


### Bug Fixes

* checking if disconnected before packing message ([a76caf1](https://github.com/MirageNet/Mirage/commit/a76caf148540a2c15cc5622362391acfe20b2388))

## [96.4.2](https://github.com/MirageNet/Mirage/compare/v96.4.1...v96.4.2) (2021-06-21)


### Bug Fixes

* null checks when types can't be resolved ([#848](https://github.com/MirageNet/Mirage/issues/848)) ([677c792](https://github.com/MirageNet/Mirage/commit/677c7924b85a890f159ee0abfef44363b266f49a))

## [96.4.1](https://github.com/MirageNet/Mirage/compare/v96.4.0...v96.4.1) (2021-06-21)


### Bug Fixes

* welcome window fixes ([#845](https://github.com/MirageNet/Mirage/issues/845)) ([a187844](https://github.com/MirageNet/Mirage/commit/a18784451fb8b4711964ee9ef6c19c161a02059b))

# [96.4.0](https://github.com/MirageNet/Mirage/compare/v96.3.1...v96.4.0) (2021-06-20)


### Features

* adding attribute to ignore extension method for read writer ([#841](https://github.com/MirageNet/Mirage/issues/841)) ([9494500](https://github.com/MirageNet/Mirage/commit/94945006f48c486482a67a8c114a2fbe32c2aba4))

## [96.3.1](https://github.com/MirageNet/Mirage/compare/v96.3.0...v96.3.1) (2021-06-17)


### Bug Fixes

* improving error for failed deserialize ([2e1601b](https://github.com/MirageNet/Mirage/commit/2e1601bfccb94f734f80bb6bca3e483cf451436b))
* improving error for message handler ([7ab73b6](https://github.com/MirageNet/Mirage/commit/7ab73b66e167c06edc2d94290f815b74549fc3de))

# [96.3.0](https://github.com/MirageNet/Mirage/compare/v96.2.1...v96.3.0) (2021-06-04)


### Bug Fixes

* adding action to pipe connection so client events can be called on stop ([#838](https://github.com/MirageNet/Mirage/issues/838)) ([eebe63a](https://github.com/MirageNet/Mirage/commit/eebe63ada737ff5e4d15f0c5231ed192f9d76079)), closes [#837](https://github.com/MirageNet/Mirage/issues/837)


### Features

* adding host mode stopped as reason for client disconnect ([0054dd5](https://github.com/MirageNet/Mirage/commit/0054dd5fc36010f924a9bf5eb2fd98439d6eaddd))

## [96.2.1](https://github.com/MirageNet/Mirage/compare/v96.2.0...v96.2.1) (2021-06-04)


### Bug Fixes

* adding warning if extension method is overwriting existing method ([#836](https://github.com/MirageNet/Mirage/issues/836)) ([aee89dc](https://github.com/MirageNet/Mirage/commit/aee89dced62c9dc1a944e9dc79f8629b863fd393))

# [96.2.0](https://github.com/MirageNet/Mirage/compare/v96.1.2...v96.2.0) (2021-06-02)


### Features

* making enums for socket layer public instead of internal ([bb9f209](https://github.com/MirageNet/Mirage/commit/bb9f2090f5c26e1a556d7ac6fb5a513067965572))

## [96.1.2](https://github.com/MirageNet/Mirage/compare/v96.1.1...v96.1.2) (2021-06-02)


### Bug Fixes

* fixing order of host setup ([#832](https://github.com/MirageNet/Mirage/issues/832)) ([3951a40](https://github.com/MirageNet/Mirage/commit/3951a40f4a7a7b04cf66ea4b9c10d067a7e96782))

## [96.1.1](https://github.com/MirageNet/Mirage/compare/v96.1.0...v96.1.1) (2021-06-02)


### Bug Fixes

* closing socket should give by local peer as reason ([993933f](https://github.com/MirageNet/Mirage/commit/993933f5a2575a795250207ff5b1191b717e2a13))

# [96.1.0](https://github.com/MirageNet/Mirage/compare/v96.0.0...v96.1.0) (2021-06-02)


### Bug Fixes

* moving syncvar sender to networkserver so it gets intilized earlier ([8b2b828](https://github.com/MirageNet/Mirage/commit/8b2b828a6cfd0407ece01d707294cac8ef5ce94c))
* stopping Server.Stop being called twice ([b950d39](https://github.com/MirageNet/Mirage/commit/b950d395717c69da778fb0702ddf35cd067ca1d8))


### Features

* adding is authenticated bool to network player ([#828](https://github.com/MirageNet/Mirage/issues/828)) ([372fd70](https://github.com/MirageNet/Mirage/commit/372fd709fe931ac5656ca7365310d65895c2b986))

# [96.0.0](https://github.com/MirageNet/Mirage/compare/v95.1.1...v96.0.0) (2021-06-01)


### Features

* reworking network authenticator ([#827](https://github.com/MirageNet/Mirage/issues/827)) ([a3c61d8](https://github.com/MirageNet/Mirage/commit/a3c61d87911aadcede7f789cf05455fbca5526e8))


### BREAKING CHANGES

* - BasicAuthenticator now uses single string field instead of 2
- Renaming methods from OnServerAuthenticate to ServerAuthenticate
- Renaming methods from OnClientAuthenticate to ClientAuthenticate

## [95.1.1](https://github.com/MirageNet/Mirage/compare/v95.1.0...v95.1.1) (2021-05-30)


### Bug Fixes

* fixing errors in tanks sample ([ed99d05](https://github.com/MirageNet/Mirage/commit/ed99d050fdd4af28d85d277704f4b9870bae394d))

# [95.1.0](https://github.com/MirageNet/Mirage/compare/v95.0.0...v95.1.0) (2021-05-29)


### Features

* adding started event to client ([#825](https://github.com/MirageNet/Mirage/issues/825)) ([3360b7c](https://github.com/MirageNet/Mirage/commit/3360b7c45cabef8677f831282f9d2a86fcfb8c58))

# [95.0.0](https://github.com/MirageNet/Mirage/compare/v94.0.0...v95.0.0) (2021-05-28)


### Code Refactoring

* remove channel from handler ([#824](https://github.com/MirageNet/Mirage/issues/824)) ([f11ef9a](https://github.com/MirageNet/Mirage/commit/f11ef9ac4db982334c889fd3fccf901fcc3de90f))


### Features

* adding disconnect reason to client disconnect ([#820](https://github.com/MirageNet/Mirage/issues/820)) ([e597570](https://github.com/MirageNet/Mirage/commit/e597570bab913c025a019ce82acf309a1be4f647))
* adding Peer config properties ([9fd8a05](https://github.com/MirageNet/Mirage/commit/9fd8a0540cc04242820ea3cdc3d781ccb29ed1ad))


### Performance Improvements

* NetworkAnimator parameters use ArraySegment instead of Arrays ([#822](https://github.com/MirageNet/Mirage/issues/822)) ([00f4833](https://github.com/MirageNet/Mirage/commit/00f4833c121f13f0c5208bb23006fb58bcf294c5))
* remove redundant transform calls on NT ([#823](https://github.com/MirageNet/Mirage/issues/823)) ([2d10305](https://github.com/MirageNet/Mirage/commit/2d10305c608c544a3fbf930af17b9651931d5ac1))


### BREAKING CHANGES

* NetworkDiagnostics no longer tracks channel
* Client.Disconnected now has a Reason argument

# [94.0.0](https://github.com/MirageNet/Mirage/compare/v93.0.2...v94.0.0) (2021-05-24)


### Bug Fixes

* removing other uses of old Notify ([25a0503](https://github.com/MirageNet/Mirage/commit/25a0503619404b5a0f52dc374b9607199e4cc177))


### Code Refactoring

* removing Obsolete Notify code from networkplayer ([b2e5531](https://github.com/MirageNet/Mirage/commit/b2e5531f95ecaddc9ea23460a7e06bba201cbfc6))


### BREAKING CHANGES

* removing notify code from networkplayer, notify is now part of peer

## [93.0.2](https://github.com/MirageNet/Mirage/compare/v93.0.1...v93.0.2) (2021-05-24)


### Bug Fixes

* closing socket on application quit ([c37fe7d](https://github.com/MirageNet/Mirage/commit/c37fe7d28e74f0e3b771146542c98f669af43381))
* fixing SocketException after closing remote applcation ([4ed12ba](https://github.com/MirageNet/Mirage/commit/4ed12badc104c53919b13d1465ce8e0832e86b39))
* fixing udp socket exception on linux ([#809](https://github.com/MirageNet/Mirage/issues/809)) ([a4e8689](https://github.com/MirageNet/Mirage/commit/a4e8689cc15157ff83b045944e1f516937365134))
* removing unnecessary check ([e8a93a4](https://github.com/MirageNet/Mirage/commit/e8a93a49d0b230c20434392d3ae506938ddc5b9b))
* stopping null ref in disconnect ([4d0f092](https://github.com/MirageNet/Mirage/commit/4d0f0922ea1fec83b2d6b14148c039d6ebf9d024))

## [93.0.1](https://github.com/MirageNet/Mirage/compare/v93.0.0...v93.0.1) (2021-05-23)


### Bug Fixes

* stopping null ref caused by hud stopping non-active server ([8aa561a](https://github.com/MirageNet/Mirage/commit/8aa561ace45fdc6df8df9238b13e3af5d3358e75))

# [93.0.0](https://github.com/MirageNet/Mirage/compare/v92.0.0...v93.0.0) (2021-05-23)


### Features

* foldout for events on network server ([#806](https://github.com/MirageNet/Mirage/issues/806)) ([cbb12d1](https://github.com/MirageNet/Mirage/commit/cbb12d13d295c048f0a913ddb91203f4ed9f66f5))
* replacing transport with peer ([#780](https://github.com/MirageNet/Mirage/issues/780)) ([66b2315](https://github.com/MirageNet/Mirage/commit/66b231565c019be49f8da2af8b5e8e17822ecd8f))


### BREAKING CHANGES

* - All Transports are obsolete.
- Transports Are replaced with ISocket. Custom Transports should now implement ISocket and SocketFactory instead
- Message handlers are now invoked in Update instead of in an Async Coroutine
- Send Notify moved to SocketLayer
- Server.StartAsync is no longer Async
- Server.StartAsync is now called Server.StartServer
- Client.Connect is no longer async
- Local message in host mode invoke handlers immediately instead of waiting till next update
- NetworkPlayer now has a Disconnect method. This means user does not need a reference to SocketLayer asmdef.
- Disconnected players are blocked from sending messages

# [92.0.0](https://github.com/MirageNet/Mirage/compare/v91.2.0...v92.0.0) (2021-05-23)


### Code Refactoring

* moving networkmanager gui and hud to components folder ([#802](https://github.com/MirageNet/Mirage/issues/802)) ([7612bb6](https://github.com/MirageNet/Mirage/commit/7612bb6efe030ffa6e0baab8640bf9f1772dc780))


### Features

* adding weaver support for nullable types ([#800](https://github.com/MirageNet/Mirage/issues/800)) ([14af628](https://github.com/MirageNet/Mirage/commit/14af62854c1d0aa957e232bc43ae39609b010604))


### BREAKING CHANGES

* NetworkManagerHud is now in the Mirage.Components asmdef

# [91.2.0](https://github.com/MirageNet/Mirage/compare/v91.1.0...v91.2.0) (2021-05-18)


### Features

* simplifying some checks in ServerObjectManager ([#801](https://github.com/MirageNet/Mirage/issues/801)) ([fe9a07a](https://github.com/MirageNet/Mirage/commit/fe9a07a9a2a24d03ac7d0a2239dbf51291dbe21d))

# [91.1.0](https://github.com/MirageNet/Mirage/compare/v91.0.1...v91.1.0) (2021-05-17)


### Bug Fixes

* fixing compile in new gui ([d2800be](https://github.com/MirageNet/Mirage/commit/d2800be08fd355077e8be9fdf3195cdf9cbc3c83))


### Features

* add optional imgui support to network manager hud ([#789](https://github.com/MirageNet/Mirage/issues/789)) ([7841794](https://github.com/MirageNet/Mirage/commit/7841794df1d210b316f5d3a9fb9f8e268ddb0fc2))

## [91.0.1](https://github.com/MirageNet/Mirage/compare/v91.0.0...v91.0.1) (2021-05-04)


### Bug Fixes

* unspawning all server object on server stop ([5041a06](https://github.com/MirageNet/Mirage/commit/5041a06de8506574add2795dc075b71245e39d88))

# [91.0.0](https://github.com/MirageNet/Mirage/compare/v90.0.0...v91.0.0) (2021-05-01)


### Code Refactoring

* renaming listen to start and merging it with start host ([#795](https://github.com/MirageNet/Mirage/issues/795)) ([3d4e091](https://github.com/MirageNet/Mirage/commit/3d4e0916b14b3b1b494b9bfba366844f209f2414))


### BREAKING CHANGES

* - ListenAsync renamed to StartAsync
- StartHost removed, use StartAsync with localClient parameter instead
- OnStartHost is now always called after Started

# [90.0.0](https://github.com/MirageNet/Mirage/compare/v89.0.0...v90.0.0) (2021-04-30)


### Code Refactoring

* removing stop host ([#794](https://github.com/MirageNet/Mirage/issues/794)) ([55536fc](https://github.com/MirageNet/Mirage/commit/55536fc6fb674b164bf599367022428df3b8ed63))


### BREAKING CHANGES

* NetworkServer.StopHost removed, use NetworkServer.Disconnect instead

# [89.0.0](https://github.com/MirageNet/Mirage/compare/v88.1.3...v89.0.0) (2021-04-28)


### Code Refactoring

* renaming network server disconnect to stop ([#793](https://github.com/MirageNet/Mirage/issues/793)) ([634139c](https://github.com/MirageNet/Mirage/commit/634139c8a6dde8f6b83d72a54ef779631005050c))


### BREAKING CHANGES

* NetworkServer.Disconnect is now called Stop

## [88.1.3](https://github.com/MirageNet/Mirage/compare/v88.1.2...v88.1.3) (2021-04-28)


### Bug Fixes

* removing quitting handler on cleanup ([1132d92](https://github.com/MirageNet/Mirage/commit/1132d9246e4640036acdb10bac4d2c8a2e426b78))
* server offline does not need a specific object ([#788](https://github.com/MirageNet/Mirage/issues/788)) ([9ca7639](https://github.com/MirageNet/Mirage/commit/9ca7639818d311d72f56f035c58dd516cbe73c27))

## [88.1.2](https://github.com/MirageNet/Mirage/compare/v88.1.1...v88.1.2) (2021-04-27)


### Bug Fixes

* using null propagation to stop null ref being throw ([5da2c6d](https://github.com/MirageNet/Mirage/commit/5da2c6dce2094db534da1b375036460bb37044c8))

## [88.1.1](https://github.com/MirageNet/Mirage/compare/v88.1.0...v88.1.1) (2021-04-21)


### Bug Fixes

* class name did not match file name ([5eb58eb](https://github.com/MirageNet/Mirage/commit/5eb58eb9ba0a04e88ce348b7541364c7e17d9db1))
* test was not correctly checking for offline server ([e913004](https://github.com/MirageNet/Mirage/commit/e913004986cf9e08618184f5066fadaf1d4af7bc))

# [88.1.0](https://github.com/MirageNet/Mirage/compare/v88.0.0...v88.1.0) (2021-04-20)


### Features

* new socket layer ([#749](https://github.com/MirageNet/Mirage/issues/749)) ([fb84452](https://github.com/MirageNet/Mirage/commit/fb844522fc4974d09194af8f1cdac4b167e161d8))

# [88.0.0](https://github.com/MirageNet/Mirage/compare/v87.2.4...v88.0.0) (2021-04-17)


### Code Refactoring

* remove unspawn as its redundant with destroy ([#760](https://github.com/MirageNet/Mirage/issues/760)) ([6a8497c](https://github.com/MirageNet/Mirage/commit/6a8497c309dfb71caf533dc35efc337745b324a4))
* removing GetNewPlayer and sealing networkplayer ([#781](https://github.com/MirageNet/Mirage/issues/781)) ([16a6ba0](https://github.com/MirageNet/Mirage/commit/16a6ba00920423d07b75aa5054da1d6b8b41dce8))


### BREAKING CHANGES

* removed Unspawn method, use with Destroy with destroyServerObject flag instead
* no longer possible to create custom INetworkPlayer to be used inside mirage

## [87.2.4](https://github.com/MirageNet/Mirage/compare/v87.2.3...v87.2.4) (2021-04-16)


### Bug Fixes

* add some exception documentation ([829f10a](https://github.com/MirageNet/Mirage/commit/829f10a506f1ea9dcf7e664d672202538057a88b))

## [87.2.3](https://github.com/MirageNet/Mirage/compare/v87.2.2...v87.2.3) (2021-04-16)


### Bug Fixes

* param no longer exists. updating summary ([c9a919f](https://github.com/MirageNet/Mirage/commit/c9a919f9d5e3a672793f6db228b8eb720abf5af6))

## [87.2.2](https://github.com/MirageNet/Mirage/compare/v87.2.1...v87.2.2) (2021-04-16)


### Bug Fixes

* **WelcomeWindow:** re-adding logger and if 2020.1 or newer ([288ed0f](https://github.com/MirageNet/Mirage/commit/288ed0f62e9f0855995e623db0317b84948059bb))

## [87.2.1](https://github.com/MirageNet/Mirage/compare/v87.2.0...v87.2.1) (2021-04-14)


### Bug Fixes

* clientchangedscene event called too early ([#776](https://github.com/MirageNet/Mirage/issues/776)) ([82dda04](https://github.com/MirageNet/Mirage/commit/82dda04882e80ed2e7e32f25c431ac5a6a1fbba8))

# [87.2.0](https://github.com/MirageNet/Mirage/compare/v87.1.2...v87.2.0) (2021-04-12)


### Features

* adding change log parsing to welcome window. ([#771](https://github.com/MirageNet/Mirage/issues/771)) ([e5409ff](https://github.com/MirageNet/Mirage/commit/e5409ffac6f7494da78ee9e1a36165e5788e30e8))
* dark mode ([#750](https://github.com/MirageNet/Mirage/issues/750)) ([e3f1d26](https://github.com/MirageNet/Mirage/commit/e3f1d26c2f2d783b0b26c911c3b37ce7988fffaf))

## [87.1.2](https://github.com/MirageNet/Mirage/compare/v87.1.1...v87.1.2) (2021-04-12)


### Bug Fixes

* bug with scene object spawning ([#773](https://github.com/MirageNet/Mirage/issues/773)) ([b02c13d](https://github.com/MirageNet/Mirage/commit/b02c13d4115fac58905c5b8aeb3f41b790a0b4cc))

## [87.1.1](https://github.com/MirageNet/Mirage/compare/v87.1.0...v87.1.1) (2021-04-10)


### Bug Fixes

* moving using out of #if ([00517bc](https://github.com/MirageNet/Mirage/commit/00517bc31091c89a56c614440ffd52ca0afbcf17))

# [87.1.0](https://github.com/MirageNet/Mirage/compare/v87.0.1...v87.1.0) (2021-04-08)


### Features

* add NetworkManagerMode back to NetMan ([#756](https://github.com/MirageNet/Mirage/issues/756)) ([f1f8f57](https://github.com/MirageNet/Mirage/commit/f1f8f57b64e49a81225916db981f5ae1d6809e51))

## [87.0.1](https://github.com/MirageNet/Mirage/compare/v87.0.0...v87.0.1) (2021-04-08)


### Bug Fixes

* add icons to all mirage monobehaviours ([#769](https://github.com/MirageNet/Mirage/issues/769)) ([3a9673f](https://github.com/MirageNet/Mirage/commit/3a9673ffb4b34d638817c008cec629316f381072))

# [87.0.0](https://github.com/MirageNet/Mirage/compare/v86.0.2...v87.0.0) (2021-04-08)


### Bug Fixes

* using add late event for authority and combining start and stop events ([#767](https://github.com/MirageNet/Mirage/issues/767)) ([8903f00](https://github.com/MirageNet/Mirage/commit/8903f00653b1ba527eb87af4dba106ba0cd9544a))
* using AddLateEvent to stop race condition for client events  ([#768](https://github.com/MirageNet/Mirage/issues/768)) ([681875b](https://github.com/MirageNet/Mirage/commit/681875b814e79bc17e4bdeb8e58a124ddab2fe72))
* using AddLateEvent to stop race condition for network identity events ([#766](https://github.com/MirageNet/Mirage/issues/766)) ([4f8bf11](https://github.com/MirageNet/Mirage/commit/4f8bf110d4144d41da9a5d80aaba49d46a7f3b54))
* using AddLateEvent to stop race condition for server events ([#765](https://github.com/MirageNet/Mirage/issues/765)) ([a1ec84c](https://github.com/MirageNet/Mirage/commit/a1ec84c714cefc63ce458733df464ab48f8c2913))


### Features

* adding RemoveListener and RemoveAllListeners to AddLateEvent ([#764](https://github.com/MirageNet/Mirage/issues/764)) ([d67c96c](https://github.com/MirageNet/Mirage/commit/d67c96cc5e8cfe9720cdd8909fe1533f27834f4d))
* adding RemoveListener and RemoveAllListeners to AddLateEvent ([#764](https://github.com/MirageNet/Mirage/issues/764)) ([0ca2804](https://github.com/MirageNet/Mirage/commit/0ca2804b95287fcfa445d2148cc6263cc8f851f4))


### BREAKING CHANGES

* - NetworkClient.Connected event is now type of IAddLateEvent
- NetworkClient.Authenticated event is now type of IAddLateEvent
- NetworkClient.Disconnected event is now type of IAddLateEvent

* refactor: removing NetworkConnectionEvent use NetworkPlayerEvent instead
* NetworkConnectionEvent renamed to NetworkPlayerEvent

* reverting clean up change
* - Identity.OnStartAuthority and IdentityOnStopAuthority are now Identity.OnAuthorityChanged and are type of IAddLateEvent<bool>
* - Server.Started event is now type of IAddLateEvent
- Server.Stoped event is now type of IAddLateEvent
- Server.OnStartHost event is now type of IAddLateEvent
- Server.OnStopHost event is now type of IAddLateEvent
- inspector values for changed events will need to be re-assigned
* - Identity.OnStartServer event is now type of IAddLateEvent
- Identity.OnStopServer event is now type of IAddLateEvent
- Identity.OnStartClient event is now type of IAddLateEvent
- Identity.OnStopClient event is now type of IAddLateEvent
- Identity.OnStartLocalPlayer event is now type of IAddLateEvent
- inspector values for changed events will need to be re-assigned
* AddLateEvent Reset no longer removes listeners
* AddLateEvent Reset no longer removes listeners

## [86.0.2](https://github.com/MirageNet/Mirage/compare/v86.0.1...v86.0.2) (2021-04-05)


### Bug Fixes

* removing redundant null checks ([909b668](https://github.com/MirageNet/Mirage/commit/909b668d7508fd3022ba309c6b441feb04e48ba1))

## [86.0.1](https://github.com/MirageNet/Mirage/compare/v86.0.0...v86.0.1) (2021-04-02)


### Bug Fixes

* changed icon to new mirage icons. ([#758](https://github.com/MirageNet/Mirage/issues/758)) ([04edac0](https://github.com/MirageNet/Mirage/commit/04edac0d1bce1f7b5f3dec23dde4dae11f422db3))

# [86.0.0](https://github.com/MirageNet/Mirage/compare/v85.0.0...v86.0.0) (2021-04-01)


### Bug Fixes

* better name to stop namespace conflict with Unity ([#755](https://github.com/MirageNet/Mirage/issues/755)) ([69784e7](https://github.com/MirageNet/Mirage/commit/69784e70f8a0f6b2d5e0d4734358688088044abd))


### BREAKING CHANGES

* networkManager.SceneManager removed to networkManager.NetworkSceneManager

# [85.0.0](https://github.com/MirageNet/Mirage/compare/v84.3.1...v85.0.0) (2021-03-30)


### Code Refactoring

* moving syncvar sending to its own class ([9699e03](https://github.com/MirageNet/Mirage/commit/9699e03c75973342e72375ac4a424abb1181bd17))


### Features

* adding class that will invoke late handlers ([631adce](https://github.com/MirageNet/Mirage/commit/631adceee9ea3d9a4f1ff0f885bfcec4b8e21502))


### BREAKING CHANGES

* Dirty object collection is now inside SyncVarSender

## [84.3.1](https://github.com/MirageNet/Mirage/compare/v84.3.0...v84.3.1) (2021-03-29)


### Bug Fixes

* adding FormerlySerializedAs to networkanimator ([39f8fbd](https://github.com/MirageNet/Mirage/commit/39f8fbd0c8b1450f15b33ddba7965f147433ff11))

# [84.3.0](https://github.com/MirageNet/Mirage/compare/v84.2.1...v84.3.0) (2021-03-27)


### Features

* not listening can now be toggled at runtime. fixes host spawning ([#728](https://github.com/MirageNet/Mirage/issues/728)) ([256b16c](https://github.com/MirageNet/Mirage/commit/256b16c12c1550f66ccbdf6d289ec75e6abab315))

## [84.2.1](https://github.com/MirageNet/Mirage/compare/v84.2.0...v84.2.1) (2021-03-27)


### Bug Fixes

* invoke client rpc only once in host mode ([#744](https://github.com/MirageNet/Mirage/issues/744)) ([ee6e55e](https://github.com/MirageNet/Mirage/commit/ee6e55e6dc7376308e702e431387ebdf12051a98))

# [84.2.0](https://github.com/MirageNet/Mirage/compare/v84.1.1...v84.2.0) (2021-03-27)


### Features

* configurable wait time for tests ([#729](https://github.com/MirageNet/Mirage/issues/729)) ([b10f3e8](https://github.com/MirageNet/Mirage/commit/b10f3e8a6b2097e7550f74426b3051464b6b9d23))

## [84.1.1](https://github.com/MirageNet/Mirage/compare/v84.1.0...v84.1.1) (2021-03-26)


### Bug Fixes

* make sure resolved typedef isn't null ([#731](https://github.com/MirageNet/Mirage/issues/731)) ([2f6414f](https://github.com/MirageNet/Mirage/commit/2f6414f7f676b250786c5caffaf83d8fa16daffb))

# [84.1.0](https://github.com/MirageNet/Mirage/compare/v84.0.0...v84.1.0) (2021-03-25)


### Features

* sample for interest management ([#727](https://github.com/MirageNet/Mirage/issues/727)) ([0f4cdc5](https://github.com/MirageNet/Mirage/commit/0f4cdc50910e3ec63a3b93c12633b026039cd696))

# [84.0.0](https://github.com/MirageNet/Mirage/compare/v83.0.0...v84.0.0) (2021-03-24)


### Code Refactoring

* using interface instead of network server ([#722](https://github.com/MirageNet/Mirage/issues/722)) ([7312bd8](https://github.com/MirageNet/Mirage/commit/7312bd8cd83f2133f9d0a6162dea9689630d88f2))


### BREAKING CHANGES

* fields and parameters using NetworkServer are now using INetworkServer Instead

# [83.0.0](https://github.com/MirageNet/Mirage/compare/v82.0.0...v83.0.0) (2021-03-24)


### Code Refactoring

* using interface instead of network client ([#721](https://github.com/MirageNet/Mirage/issues/721)) ([703596a](https://github.com/MirageNet/Mirage/commit/703596aa4678eeef6e6e89b8877fceb8062e4476))


### BREAKING CHANGES

* fields and parameters using NetworkClient are now using INetworkClient Instead

# [82.0.0](https://github.com/MirageNet/Mirage/compare/v81.0.2...v82.0.0) (2021-03-24)


### Code Refactoring

* changing indexer to try get ([#720](https://github.com/MirageNet/Mirage/issues/720)) ([01ca9bb](https://github.com/MirageNet/Mirage/commit/01ca9bb48ce14446052cd45d5c41a9a299a1efcf))
* removing connection to server ([#703](https://github.com/MirageNet/Mirage/issues/703)) ([ff95634](https://github.com/MirageNet/Mirage/commit/ff95634c8c08be20574f8c73530926725944ecae))


### BREAKING CHANGES

* ObjectLocator now has TryGet method instead of indexer that returns null

* updating uses of objectLocator

* fixing names not being the same
* Removed ConnectionToServer property

* removing uses of ConnectionToServer

* removing use in test

* removing ClientRpc player target

* creating null

* changing tests to expect null

* fixing docs

* using client player for target rpc

Co-authored-by: Paul Pacheco <paulpach@gmail.com>

* fixing tests for rpc target

Co-authored-by: Paul Pacheco <paulpach@gmail.com>

## [81.0.2](https://github.com/MirageNet/Mirage/compare/v81.0.1...v81.0.2) (2021-03-22)


### Bug Fixes

* show syncvar label ([c32a940](https://github.com/MirageNet/Mirage/commit/c32a9403be7be30a5e56b0f4c71ddc05263a746a))

## [81.0.1](https://github.com/MirageNet/Mirage/compare/v81.0.0...v81.0.1) (2021-03-20)


### Performance Improvements

* clientrpc in host mode bypasses network ([#714](https://github.com/MirageNet/Mirage/issues/714)) ([edb0705](https://github.com/MirageNet/Mirage/commit/edb0705e5038684e3c3565c38e3529578832fb1d))

# [81.0.0](https://github.com/MirageNet/Mirage/compare/v80.0.1...v81.0.0) (2021-03-19)


### Performance Improvements

* serverrpc bypasses network on host mode ([#708](https://github.com/MirageNet/Mirage/issues/708)) ([695eb46](https://github.com/MirageNet/Mirage/commit/695eb4686fb5c0d6d1db3e3f3d8f7803226c009f))


### BREAKING CHANGES

* ServerRpc execute synchronous in host mode

## [80.0.1](https://github.com/MirageNet/Mirage/compare/v80.0.0...v80.0.1) (2021-03-17)


### Bug Fixes

* compilation issue on standalone build ([d6bea93](https://github.com/MirageNet/Mirage/commit/d6bea93c6885185f546aaf6b6e7c29abbe420012))

# [80.0.0](https://github.com/MirageNet/Mirage/compare/v79.0.0...v80.0.0) (2021-03-15)


### Code Refactoring

* rename connection to player ([#706](https://github.com/MirageNet/Mirage/issues/706)) ([03e8cfa](https://github.com/MirageNet/Mirage/commit/03e8cfab37184ebbf137ea3bddea217da6b45c95))


### BREAKING CHANGES

* Connection renamed to player

# [79.0.0](https://github.com/MirageNet/Mirage/compare/v78.0.0...v79.0.0) (2021-03-15)


### Code Refactoring

* move serialization into Mirage.Serialization ([#700](https://github.com/MirageNet/Mirage/issues/700)) ([5dc037d](https://github.com/MirageNet/Mirage/commit/5dc037dbb6db48c1ad94d20a63aa5f953ade90c2))


### BREAKING CHANGES

* NetworkReader and NetworkWriter moved to Mirage.Serialization namespace

# [78.0.0](https://github.com/MirageNet/Mirage/compare/v77.0.0...v78.0.0) (2021-03-14)


### Code Refactoring

* move collections to Mirage.Collections ([#698](https://github.com/MirageNet/Mirage/issues/698)) ([e22f765](https://github.com/MirageNet/Mirage/commit/e22f765c39b2d7b8c835357f530cc597664858db))


### BREAKING CHANGES

* collections moved to Mirage.Collections

# [77.0.0](https://github.com/MirageNet/Mirage/compare/v76.0.0...v77.0.0) (2021-03-14)


### Code Refactoring

* move logging into a folder and namespace ([#697](https://github.com/MirageNet/Mirage/issues/697)) ([814653f](https://github.com/MirageNet/Mirage/commit/814653f06654f96c68f762dfcebd3e86cc8c92dc))


### BREAKING CHANGES

* Logging moved into a namespace, use Mirror.Logging

# [76.0.0](https://github.com/MirageNet/Mirage/compare/v75.1.0...v76.0.0) (2021-03-13)


### Code Refactoring

* remove local visibility hacks ([#696](https://github.com/MirageNet/Mirage/issues/696)) ([df499ab](https://github.com/MirageNet/Mirage/commit/df499abb8b4e29f8b7067adedcab576eea564a29))


### BREAKING CHANGES

* NetworkVisibility no longer disables renderers in host mode

# [75.1.0](https://github.com/MirageNet/Mirage/compare/v75.0.0...v75.1.0) (2021-03-13)


### Features

* 0 is not lossy when compressing quaternions ([#695](https://github.com/MirageNet/Mirage/issues/695)) ([c1552c0](https://github.com/MirageNet/Mirage/commit/c1552c0daed8209a7af1a2b3194ff21df5488484))

# [75.0.0](https://github.com/MirageNet/Mirage/compare/v74.0.0...v75.0.0) (2021-03-13)


### Bug Fixes

* adding ISceneLoader to INetworkPlayer ([e8ab7a4](https://github.com/MirageNet/Mirage/commit/e8ab7a41b9f29007c7bab8132b34d715e0a8b5d2))
* compression of 90 degrees angle ([#689](https://github.com/MirageNet/Mirage/issues/689)) ([2c0bac6](https://github.com/MirageNet/Mirage/commit/2c0bac6e2e4fab7d055be7ec095eb2297ec457e2))
* fixing uses of message ([f2a5522](https://github.com/MirageNet/Mirage/commit/f2a55222ab7ec9916aa1ba77f8a6c93d27632d23))


### Code Refactoring

* moving static send to NetworkServer ([#692](https://github.com/MirageNet/Mirage/issues/692)) ([5b19dc3](https://github.com/MirageNet/Mirage/commit/5b19dc3214e034c331dd9906f53195eb968d1165))
* removing address property from player ([#691](https://github.com/MirageNet/Mirage/issues/691)) ([d772e53](https://github.com/MirageNet/Mirage/commit/d772e53d9d5f65eb4053782a066b1fcfab9fe8d6))
* removing disconnect method from player ([#688](https://github.com/MirageNet/Mirage/issues/688)) ([e1daf92](https://github.com/MirageNet/Mirage/commit/e1daf92934b0de03ceada857a969b6aed717752c))
* rename PlayerSpawner to CharacterSpawner ([#686](https://github.com/MirageNet/Mirage/issues/686)) ([1db3498](https://github.com/MirageNet/Mirage/commit/1db3498120d18d8270e149b6af9df52fd3d92e90))
* renaming NetworkConnection to NetworkPlayer ([#684](https://github.com/MirageNet/Mirage/issues/684)) ([3ecb659](https://github.com/MirageNet/Mirage/commit/3ecb6593ea05928df40e66f3a33b3b3ccb4a5283))


### Features

* adding Connection property to NetworkPlayer ([#687](https://github.com/MirageNet/Mirage/issues/687)) ([5e1c4ba](https://github.com/MirageNet/Mirage/commit/5e1c4bad4c800b96d3ecc75cbde5c0c1e0f22da9))
* adding logger.Assert that doesn't require a message ([8c213e3](https://github.com/MirageNet/Mirage/commit/8c213e30532feee8a4e8f0ec32c58e26cd8f7afb))


### BREAKING CHANGES

* moving NetworkPlayer.Send to NetworkServer.SendToMany
* Address replaced with Connection.GetEndPointAddress

* updating uses of Address
* Disconnect replaced with Connection.Disconnect

* fixing uses in Mirage

* updating uses in authenticators
* Renamed PlayerSpawner to CharacterSpawner
* renaming NetworkConnection to NetworkPlayer

* renaming types in weaver tests

* fixing test message

* fixing xref in docs

# [74.0.0](https://github.com/MirageNet/Mirage/compare/v73.0.0...v74.0.0) (2021-03-08)


### Bug Fixes

* invoking started event when Listening is false ([#675](https://github.com/MirageNet/Mirage/issues/675)) ([afef2d4](https://github.com/MirageNet/Mirage/commit/afef2d4a552a348d3bf91703c188bab6c8967b1e))


### Code Refactoring

* **transports:** removing sendAsync from transports ([#673](https://github.com/MirageNet/Mirage/issues/673)) ([42b165f](https://github.com/MirageNet/Mirage/commit/42b165ff0b267c67c93407db5c5a36c647301126))
* removing sendasync from networkconnection ([#672](https://github.com/MirageNet/Mirage/issues/672)) ([e79b00e](https://github.com/MirageNet/Mirage/commit/e79b00eefd77ac8982cf23acf872dbcd3e4e9b31))


### BREAKING CHANGES

* **transports:** Removed SendAsync from transport,  use Send instead
* Removed SendAsync from NetworkConnection.  Use Send instead

# [73.0.0](https://github.com/MirageNet/Mirage/compare/v72.0.1...v73.0.0) (2021-03-06)


### Code Refactoring

* replacing version enum with assembly version ([#663](https://github.com/MirageNet/Mirage/issues/663)) ([d8facb7](https://github.com/MirageNet/Mirage/commit/d8facb7c4cf06033935f7e7386a25cb3ec855737))


### BREAKING CHANGES

* Version.Current is no longer an enum and now returns Mirage's assembly version

## [72.0.1](https://github.com/MirageNet/Mirage/compare/v72.0.0...v72.0.1) (2021-03-06)


### Bug Fixes

* disabling welcome window before unity 2020.1 ([#662](https://github.com/MirageNet/Mirage/issues/662)) ([a527af4](https://github.com/MirageNet/Mirage/commit/a527af4455c366d5b0ad5826c557dd52f1f3728f))

# [72.0.0](https://github.com/MirageNet/Mirage/compare/v71.0.0...v72.0.0) (2021-03-05)


### Code Refactoring

* removing un-used INetworkManager interface ([#661](https://github.com/MirageNet/Mirage/issues/661)) ([347bf6c](https://github.com/MirageNet/Mirage/commit/347bf6c0aaa3e69a4b9027defda0e999541f8a7c))


### BREAKING CHANGES

* removing INetworkManager

# [71.0.0](https://github.com/MirageNet/Mirage/compare/v70.0.0...v71.0.0) (2021-03-05)


### Styles

* renaming NetworkScenePath to ActiveScenePath ([#647](https://github.com/MirageNet/Mirage/issues/647)) ([7a26360](https://github.com/MirageNet/Mirage/commit/7a26360b9d11e7d76ef73628f593503d0d785380))


### BREAKING CHANGES

* Use NetworkSceneManager.ActiveScenePath instead of NetworkSceneManager.NetworkScenePath

* removing cref till docs are fixed

# [70.0.0](https://github.com/MirageNet/Mirage/compare/v69.1.2...v70.0.0) (2021-03-04)


### Code Refactoring

* spawnobjects throws exception instead of returning false ([#639](https://github.com/MirageNet/Mirage/issues/639)) ([4cb8afb](https://github.com/MirageNet/Mirage/commit/4cb8afb6e33be80b1c2e5fa3db57a246be203b74))


### BREAKING CHANGES

* SpawnObjects throws Exception instead of returning false

## [69.1.2](https://github.com/MirageNet/Mirage/compare/v69.1.1...v69.1.2) (2021-03-03)


### Bug Fixes

* **weaver:** adding missing errors when Attributes are used in monobehaviour ([64b580b](https://github.com/MirageNet/Mirage/commit/64b580bb15048e60ba96de843ba47ffdc1a9fd0b))

## [69.1.1](https://github.com/MirageNet/Mirage/compare/v69.1.0...v69.1.1) (2021-03-02)


### Bug Fixes

* multi scene example nre and event errors ([#649](https://github.com/MirageNet/Mirage/issues/649)) ([8c4c352](https://github.com/MirageNet/Mirage/commit/8c4c352bd226e12324f851218d6a9d56be894662))

# [69.1.0](https://github.com/MirageNet/Mirage/compare/v69.0.0...v69.1.0) (2021-03-02)


### Features

* adding assert extension method to logger ([#642](https://github.com/MirageNet/Mirage/issues/642)) ([1df6081](https://github.com/MirageNet/Mirage/commit/1df6081e5d1244a1f780b9945f5bd3c0a10387f0))

# [69.0.0](https://github.com/MirageNet/Mirage/compare/v68.0.3...v69.0.0) (2021-03-02)


### Styles

* renaming NumPlayers to NumberOfPlayers ([#646](https://github.com/MirageNet/Mirage/issues/646)) ([27b99be](https://github.com/MirageNet/Mirage/commit/27b99be272ef4f6df73717d877385981e7565259))


### BREAKING CHANGES

* Use NetworkServer.NumberOfPlayers instead of NetworkServer.NumPlayers

## [68.0.3](https://github.com/MirageNet/Mirage/compare/v68.0.2...v68.0.3) (2021-02-25)


### Bug Fixes

* welcome window layout ([#634](https://github.com/MirageNet/Mirage/issues/634)) ([2bceedb](https://github.com/MirageNet/Mirage/commit/2bceedb0baa0dd5d5767e440deea74077469fd11))

## [68.0.2](https://github.com/MirageNet/Mirage/compare/v68.0.1...v68.0.2) (2021-02-24)


### Bug Fixes

* mirage icon missing in welcome window ([#637](https://github.com/MirageNet/Mirage/issues/637)) ([6c4dc9e](https://github.com/MirageNet/Mirage/commit/6c4dc9ee9639503631da43f74f92f0394fadab29))

## [68.0.1](https://github.com/MirageNet/Mirage/compare/v68.0.0...v68.0.1) (2021-02-23)


### Bug Fixes

* welcome window icon not being found ([#635](https://github.com/MirageNet/Mirage/issues/635)) ([263a7b2](https://github.com/MirageNet/Mirage/commit/263a7b21c36357f8e6a7fa48f9ea74d1e6233216))

# [68.0.0](https://github.com/MirageNet/Mirage/compare/v67.4.0...v68.0.0) (2021-02-22)


### Code Refactoring

* move LocalPlayer to ClientObjectManager ([#619](https://github.com/MirageNet/Mirage/issues/619)) ([df1e379](https://github.com/MirageNet/Mirage/commit/df1e379e064cdea01309fbf9ada0bab1f3bbd7dd))


### Features

* add more control over player spawning ([#626](https://github.com/MirageNet/Mirage/issues/626)) ([e0dd626](https://github.com/MirageNet/Mirage/commit/e0dd626d5bd23e6c555730c6a4c3517694ea3bba))


### BREAKING CHANGES

* removed NetworkClient.LocalPlayer,  use ClientObjectManager.LocalPlayer instead

# [67.4.0](https://github.com/MirageNet/Mirage/compare/v67.3.2...v67.4.0) (2021-02-21)


### Features

* support generic network behaviors ([#574](https://github.com/MirageNet/Mirage/issues/574)) ([715642c](https://github.com/MirageNet/Mirage/commit/715642ceb5de02dc500d8ba3f4cda883431decb7))

## [67.3.2](https://github.com/MirageNet/Mirage/compare/v67.3.1...v67.3.2) (2021-02-21)


### Bug Fixes

* passing NetworkBehaviors in syncvars work with il2pp ([#631](https://github.com/MirageNet/Mirage/issues/631)) ([cd7317f](https://github.com/MirageNet/Mirage/commit/cd7317f368bd65c6bd594c337de4bdc676fe2805)), closes [#630](https://github.com/MirageNet/Mirage/issues/630) [#629](https://github.com/MirageNet/Mirage/issues/629)

## [67.3.1](https://github.com/MirageNet/Mirage/compare/v67.3.0...v67.3.1) (2021-02-21)


### Bug Fixes

* passing NetworkBehaviors in RPC works with IL2PP ([#630](https://github.com/MirageNet/Mirage/issues/630)) ([87becee](https://github.com/MirageNet/Mirage/commit/87becee8fdc028ca86abd6aa13a55396ea202567)), closes [#629](https://github.com/MirageNet/Mirage/issues/629)

# [67.3.0](https://github.com/MirageNet/Mirage/compare/v67.2.7...v67.3.0) (2021-02-21)


### Features

* install/uninstall modules from welcome window ([#593](https://github.com/MirageNet/Mirage/issues/593)) ([53ac404](https://github.com/MirageNet/Mirage/commit/53ac40492ade8059338436ad15b69e3573ad1482))

## [67.2.7](https://github.com/MirageNet/Mirage/compare/v67.2.6...v67.2.7) (2021-02-21)


### Bug Fixes

* show selected tab when welcome screen is opened ([#628](https://github.com/MirageNet/Mirage/issues/628)) ([f6cae98](https://github.com/MirageNet/Mirage/commit/f6cae984f1dfb2941a35cd4bfd1dd0050ce06873))

## [67.2.6](https://github.com/MirageNet/Mirage/compare/v67.2.5...v67.2.6) (2021-02-20)


### Bug Fixes

* bug in tanks sample ([#575](https://github.com/MirageNet/Mirage/issues/575)) ([d82efea](https://github.com/MirageNet/Mirage/commit/d82efea9015cf2db9436f12deaa9a7dd65ba862d))

## [67.2.5](https://github.com/MirageNet/Mirage/compare/v67.2.4...v67.2.5) (2021-02-20)


### Bug Fixes

* revert preprocessor change ([b89c454](https://github.com/MirageNet/Mirage/commit/b89c45489755a9fa598ec804e8d71b35cfc8f438))

## [67.2.4](https://github.com/MirageNet/Mirage/compare/v67.2.3...v67.2.4) (2021-02-20)


### Bug Fixes

* add comments and backing field for time ([#618](https://github.com/MirageNet/Mirage/issues/618)) ([da74e11](https://github.com/MirageNet/Mirage/commit/da74e11066f1a6782ec1304f855b79840c7058f3))

## [67.2.3](https://github.com/MirageNet/Mirage/compare/v67.2.2...v67.2.3) (2021-02-19)


### Bug Fixes

* add summaries to event classes ([#623](https://github.com/MirageNet/Mirage/issues/623)) ([e6b9354](https://github.com/MirageNet/Mirage/commit/e6b9354665294dc11140a560bfc661ef6bfbb3b9))
* no longer true. spawnableobjects populated by FindObjectsOfTypeAll ([#622](https://github.com/MirageNet/Mirage/issues/622)) ([5692709](https://github.com/MirageNet/Mirage/commit/5692709b62095a08e25172d6a4dfba51e47355f5))

## [67.2.2](https://github.com/MirageNet/Mirage/compare/v67.2.1...v67.2.2) (2021-02-19)


### Bug Fixes

* drop support for older versions ([5dc12b5](https://github.com/MirageNet/Mirage/commit/5dc12b5e8553f5689833af5b71a936615514b021))

## [67.2.1](https://github.com/MirageNet/Mirage/compare/v67.2.0...v67.2.1) (2021-02-19)


### Bug Fixes

* add missing properties to interface ([#617](https://github.com/MirageNet/Mirage/issues/617)) ([e45920e](https://github.com/MirageNet/Mirage/commit/e45920ebd375b382f48e11bba7e73549efb58c6c))

# [67.2.0](https://github.com/MirageNet/Mirage/compare/v67.1.0...v67.2.0) (2021-02-19)


### Features

* add events to interfaces ([#614](https://github.com/MirageNet/Mirage/issues/614)) ([4d1a772](https://github.com/MirageNet/Mirage/commit/4d1a77263dfbca0f631ded85a5f772ce6e4e4343))

# [67.1.0](https://github.com/MirageNet/Mirage/compare/v67.0.2...v67.1.0) (2021-02-19)


### Features

* welcome window shows active tab ([#616](https://github.com/MirageNet/Mirage/issues/616)) ([1411d0a](https://github.com/MirageNet/Mirage/commit/1411d0ae4dd3b83c44ce27800e4117f00211f092))

## [67.0.2](https://github.com/MirageNet/Mirage/compare/v67.0.1...v67.0.2) (2021-02-19)


### Bug Fixes

* clientObjectManager was not using its interface ([#615](https://github.com/MirageNet/Mirage/issues/615)) ([d2b07ea](https://github.com/MirageNet/Mirage/commit/d2b07ea46ef1e5e0dbc4685b39c4cd24a7c2fc36))

## [67.0.1](https://github.com/MirageNet/Mirage/compare/v67.0.0...v67.0.1) (2021-02-19)


### Bug Fixes

* move interfaces into their own files ([#613](https://github.com/MirageNet/Mirage/issues/613)) ([432005b](https://github.com/MirageNet/Mirage/commit/432005bea62a83d50416aa7760ee15881ac68d00))

# [67.0.0](https://github.com/MirageNet/Mirage/compare/v66.2.2...v67.0.0) (2021-02-19)


### Code Refactoring

* remove unnecessary assembly definition ([#599](https://github.com/MirageNet/Mirage/issues/599)) ([89ffc7c](https://github.com/MirageNet/Mirage/commit/89ffc7c9cc9a37650c998381bf97c148b2f1cfd9))


### BREAKING CHANGES

* removed MirageNG.asmdef,  change your assembly to use Mirage.asmdef instead

## [66.2.2](https://github.com/MirageNet/Mirage/compare/v66.2.1...v66.2.2) (2021-02-19)


### Bug Fixes

* simplify checking if client is host ([#602](https://github.com/MirageNet/Mirage/issues/602)) ([dbf5784](https://github.com/MirageNet/Mirage/commit/dbf5784884012bd08a46d823452f8f9d95009721))
* static not needed ([#604](https://github.com/MirageNet/Mirage/issues/604)) ([879ba01](https://github.com/MirageNet/Mirage/commit/879ba018feed89d12b217e273f66b0fe75a447d5))
* stopping `: ` being at the start of all log messages ([#606](https://github.com/MirageNet/Mirage/issues/606)) ([8efe7ce](https://github.com/MirageNet/Mirage/commit/8efe7cea0eb8536335f46bf412a8ba71be74ae96))

## [66.2.1](https://github.com/MirageNet/Mirage/compare/v66.2.0...v66.2.1) (2021-02-17)


### Bug Fixes

* fix serializing NetworkBehavior and GameObjects ([b74fcf9](https://github.com/MirageNet/Mirage/commit/b74fcf9feb949769f3ec7df851cd9e02aca24565))

# [66.2.0](https://github.com/MirageNet/Mirage/compare/v66.1.0...v66.2.0) (2021-02-17)


### Features

* make networkscenemanager optional again ([#595](https://github.com/MirageNet/Mirage/issues/595)) ([ed180ff](https://github.com/MirageNet/Mirage/commit/ed180ff3f9230f4676a8b9a07351dd8302acb906))

# [66.1.0](https://github.com/MirageNet/Mirage/compare/v66.0.0...v66.1.0) (2021-02-17)


### Features

* optional dontdestroyonload for networkscenemanager ([#596](https://github.com/MirageNet/Mirage/issues/596)) ([67e41bd](https://github.com/MirageNet/Mirage/commit/67e41bd66c15d5951b1b7bd8a0bd302e21182655))

# [66.0.0](https://github.com/MirageNet/Mirage/compare/v65.1.0...v66.0.0) (2021-02-16)


### Bug Fixes

* update package json for new name and url ([#594](https://github.com/MirageNet/Mirage/issues/594)) ([e915aeb](https://github.com/MirageNet/Mirage/commit/e915aeb1ebcaa099c2d7a964b60194471b1f79c3))


### Code Refactoring

* change to Mirage namespace ([#590](https://github.com/MirageNet/Mirage/issues/590)) ([bafe18c](https://github.com/MirageNet/Mirage/commit/bafe18c379fd9c06ad7483e5421fa86dbef715e6))


### BREAKING CHANGES

* replaced Mirror namespace with Mirage

# [65.1.0](https://github.com/MirageNet/Mirage/compare/v65.0.0...v65.1.0) (2021-02-15)


### Features

* move spawned dictionary to com/som ([#568](https://github.com/MirageNet/Mirage/issues/568)) ([1ad8f3d](https://github.com/MirageNet/Mirage/commit/1ad8f3de951cd65a0da067bac8d078059d89ee87))

# [65.0.0](https://github.com/MirageNet/Mirage/compare/v64.0.0...v65.0.0) (2021-02-14)


### Code Refactoring

* NetworkReader no longer depends on NetworkClient and NetworkServer ([15f27c4](https://github.com/MirageNet/Mirage/commit/15f27c4bde191106c830077c136b018e1f053430))
* readers no longer depends on NetworkClient and NetworkServer ([#583](https://github.com/MirageNet/Mirage/issues/583)) ([177c307](https://github.com/MirageNet/Mirage/commit/177c307dfd69816844565f0f3aeb6d1c71663652))


### BREAKING CHANGES

* NetworkReader no longer have .Client and .Server, it has a .ObjectLocator instead
* NetworkReader no longer have .Client and .Server, it has a .ObjectLocator instead

# [64.0.0](https://github.com/MirageNet/Mirage/compare/v63.5.0...v64.0.0) (2021-02-14)


### Code Refactoring

* separate player ownership from NetworkServer ([#580](https://github.com/MirageNet/Mirage/issues/580)) ([8d7efa6](https://github.com/MirageNet/Mirage/commit/8d7efa6a3d02df8b5cef8399f0efcc286c2cbf5f))


### BREAKING CHANGES

* SendToClientOfPlayer removed. Use identity.ConnectionToClient.Send() instead

# [63.5.0](https://github.com/MirageNet/Mirage/compare/v63.4.0...v63.5.0) (2021-02-10)


### Features

* Notify acks messages in one way messages ([07ca15d](https://github.com/MirageNet/Mirage/commit/07ca15df5765528b7ff478cc13fc327a86ca9968))


### Performance Improvements

* no need to send initial empty notify ([8fbe346](https://github.com/MirageNet/Mirage/commit/8fbe346012d58c8ab0f3be0cbaa5dc43a8cbc7b1))

# [63.4.0](https://github.com/MirageNet/Mirage/compare/v63.3.5...v63.4.0) (2021-02-09)


### Features

* open changelog page when user downloaded a new version ([#571](https://github.com/MirageNet/Mirage/issues/571)) ([d9ed96e](https://github.com/MirageNet/Mirage/commit/d9ed96e118d2da0d5ffe6aac755f6da00d7204e7))

## [63.3.5](https://github.com/MirageNet/Mirage/compare/v63.3.4...v63.3.5) (2021-02-07)


### Bug Fixes

* nre issues when trying to reload back in the same instance to a … ([#570](https://github.com/MirageNet/Mirage/issues/570)) ([158ea2f](https://github.com/MirageNet/Mirage/commit/158ea2ff61b4a6fca27a6b12588f279fe02c131d))

## [63.3.4](https://github.com/MirageNet/Mirage/compare/v63.3.3...v63.3.4) (2021-02-07)


### Bug Fixes

* increase log level of full server ([#572](https://github.com/MirageNet/Mirage/issues/572)) ([c4ce239](https://github.com/MirageNet/Mirage/commit/c4ce23978d362969ed9d491d88c85f5ce7a48470))

## [63.3.3](https://github.com/MirageNet/Mirage/compare/v63.3.2...v63.3.3) (2021-02-05)


### Reverts

* Revert "fix: support legacy Weaver (#546)" ([d58725f](https://github.com/MirageNet/Mirage/commit/d58725f1d1a5a96262cc0ecd007663983de354d4)), closes [#546](https://github.com/MirageNet/Mirage/issues/546)

## [63.3.2](https://github.com/MirageNet/Mirage/compare/v63.3.1...v63.3.2) (2021-02-04)


### Bug Fixes

* support legacy Weaver ([#546](https://github.com/MirageNet/Mirage/issues/546)) ([e1bbc03](https://github.com/MirageNet/Mirage/commit/e1bbc035169a7d0f15293afabf2b362f949e8a78))

## [63.3.1](https://github.com/MirageNet/Mirage/compare/v63.3.0...v63.3.1) (2021-02-04)


### Bug Fixes

* client throwing exception when force disconnecting ([#567](https://github.com/MirageNet/Mirage/issues/567)) ([a69b498](https://github.com/MirageNet/Mirage/commit/a69b4989309fc00520be394e71bcb49d67d9889b))

# [63.3.0](https://github.com/MirageNet/Mirage/compare/v63.2.1...v63.3.0) (2021-02-03)


### Bug Fixes

* remove last start call ([b8fc97d](https://github.com/MirageNet/Mirage/commit/b8fc97d1b22f18447849929d056305b34859face))


### Features

* explicit declaration of network messages ([#565](https://github.com/MirageNet/Mirage/issues/565)) ([b0610e2](https://github.com/MirageNet/Mirage/commit/b0610e2c6fb8fb99d9accc02a3f464cae24b6f85))

## [63.2.1](https://github.com/MirageNet/Mirage/compare/v63.2.0...v63.2.1) (2021-02-03)


### Bug Fixes

* built in messages get an id ([fe71bcd](https://github.com/MirageNet/Mirage/commit/fe71bcdfe4a4af3b3018d05a8e2b37b452868931))

# [63.2.0](https://github.com/MirageNet/Mirage/compare/v63.1.0...v63.2.0) (2021-02-03)


### Features

* raise event when spawning objects ([#564](https://github.com/MirageNet/Mirage/issues/564)) ([60725d9](https://github.com/MirageNet/Mirage/commit/60725d951cd6dc9718dd2801b8cf648b86b474fc))

# [63.1.0](https://github.com/MirageNet/Mirage/compare/v63.0.1...v63.1.0) (2021-02-03)


### Features

* spawn an object by network identity ref ([#561](https://github.com/MirageNet/Mirage/issues/561)) ([87a520d](https://github.com/MirageNet/Mirage/commit/87a520df3361bf030af6734f955571f36bab6f40))

## [63.0.1](https://github.com/MirageNet/Mirage/compare/v63.0.0...v63.0.1) (2021-02-03)


### Bug Fixes

* better benchmark ([#562](https://github.com/MirageNet/Mirage/issues/562)) ([731f13b](https://github.com/MirageNet/Mirage/commit/731f13bdc23f93a8a5808cfc59d263115609e3c4))

# [63.0.0](https://github.com/MirageNet/Mirage/compare/v62.10.0...v63.0.0) (2021-02-02)


### Bug Fixes

* object references in examples ([06d89c0](https://github.com/MirageNet/Mirage/commit/06d89c07262b2562bbef33de37615a32356cd1ee))


### Styles

* Use PascalCase for public fields ([9dc94c1](https://github.com/MirageNet/Mirage/commit/9dc94c10e05396a14514ea3bf43276830fe061f1))
* Use PascalCase for public fields ([8a195f6](https://github.com/MirageNet/Mirage/commit/8a195f607f671f05ffaa9cf9b9cc9067e46cba35))
* Use PascalCase for public fields ([8366e80](https://github.com/MirageNet/Mirage/commit/8366e80e7887112a23f0b03a97bae47893be3bf4))
* Use PascalCase for public fields ([2717957](https://github.com/MirageNet/Mirage/commit/2717957b13523f4d93199f7afda46a5f41ec53ce))
* Use PascalCase for public fields ([f1758d3](https://github.com/MirageNet/Mirage/commit/f1758d306e4c300e1f357f7fb3c242b34df421fc))
* Use PascalCase for public fields ([e478d8d](https://github.com/MirageNet/Mirage/commit/e478d8d2df6611fc572f49693cc66eb0bf89a856))
* Use PascalCase for public fields ([5f88032](https://github.com/MirageNet/Mirage/commit/5f880329539eebaa84bed45dfb6e5ed3aedb7dec))


### BREAKING CHANGES

* HeadlessAutoStart.* renamed to follow PascalCase
* OnlineOfflineScene to use PascalCase
* PlayerSpawner.* renamed to follow PascalCase
* NetworkManager fields renamed to PascalCase
* ClientObjectManager.client renamed to .Client
* ClientObjectManager.networkSceneManager renamed to .NetworkSceneManager
* ServerObjectManager.server renamed to .Server
* ServerObjectManager.networkSceneManager renamed to .NetworkSceneManager
* NetworkSceneManager.client renamed to .Client
* NetworkSceneManager.server renamed to .Server

# [62.10.0](https://github.com/MirageNet/Mirage/compare/v62.9.8...v62.10.0) (2021-02-01)


### Features

* recommend Fast3 as default for performance ([#559](https://github.com/MirageNet/Mirage/issues/559)) ([8cbf6e6](https://github.com/MirageNet/Mirage/commit/8cbf6e69a0075310d720e21897fe22a66b6e28e8))

## [62.9.8](https://github.com/MirageNet/Mirage/compare/v62.9.7...v62.9.8) (2021-02-01)


### Bug Fixes

* remove GC from demo ([2474a0c](https://github.com/MirageNet/Mirage/commit/2474a0cad61261160a361ddf4c64be7a3343ccd5))

## [62.9.7](https://github.com/MirageNet/Mirage/compare/v62.9.6...v62.9.7) (2021-02-01)


### Bug Fixes

* add ignorance support with defines for test ([35634d5](https://github.com/MirageNet/Mirage/commit/35634d599c4d8a7b71a1866491200745bcf5928f))
* make test transport agnostic ([cbb00b3](https://github.com/MirageNet/Mirage/commit/cbb00b3501c7d5fe83f42deec5dfe964fdea7592))

## [62.9.6](https://github.com/MirageNet/Mirage/compare/v62.9.5...v62.9.6) (2021-01-31)


### Bug Fixes

* move method not firing and too slow ([56ce2fd](https://github.com/MirageNet/Mirage/commit/56ce2fd12a8801139c7fcbd38d421abb46fc00da))
* nre in headless example ([9e8dccc](https://github.com/MirageNet/Mirage/commit/9e8dcccea5c2f40d947dfeddb285c05ebc607405))

## [62.9.5](https://github.com/MirageNet/Mirage/compare/v62.9.4...v62.9.5) (2021-01-30)


### Bug Fixes

* bug cleaning up networkscenemanager ([8db98db](https://github.com/MirageNet/Mirage/commit/8db98db50268e478b52fd78642e9c34a29796cfc))
* fast domain reload with Kcp ([bd3676d](https://github.com/MirageNet/Mirage/commit/bd3676d4d93c3d113241f95577c4ac581133696b)), closes [#547](https://github.com/MirageNet/Mirage/issues/547)

## [62.9.4](https://github.com/MirageNet/Mirage/compare/v62.9.3...v62.9.4) (2021-01-29)


### Bug Fixes

* running weaver for editor scripts ([#557](https://github.com/MirageNet/Mirage/issues/557)) ([12a1d73](https://github.com/MirageNet/Mirage/commit/12a1d73061766d3c8d1bfb406a20ba4f44d18d50)), closes [#537](https://github.com/MirageNet/Mirage/issues/537) [#552](https://github.com/MirageNet/Mirage/issues/552)

## [62.9.3](https://github.com/MirageNet/Mirage/compare/v62.9.2...v62.9.3) (2021-01-27)


### Bug Fixes

* use logger in KcpConnection ([#553](https://github.com/MirageNet/Mirage/issues/553)) ([90df76d](https://github.com/MirageNet/Mirage/commit/90df76d590306cbbaa96089d83892f8a2f50c0ce))

## [62.9.2](https://github.com/MirageNet/Mirage/compare/v62.9.1...v62.9.2) (2021-01-27)


### Bug Fixes

* add nullable to event invokes ([#554](https://github.com/MirageNet/Mirage/issues/554)) ([8236614](https://github.com/MirageNet/Mirage/commit/823661456a78b05bd5642167fdbbf1ea925f93e6))

## [62.9.1](https://github.com/MirageNet/Mirage/compare/v62.9.0...v62.9.1) (2021-01-26)


### Performance Improvements

* cache component index in network behavior ([#550](https://github.com/MirageNet/Mirage/issues/550)) ([e566545](https://github.com/MirageNet/Mirage/commit/e566545c544c218a7c4c9d8df78c7914e47b051a))

# [62.9.0](https://github.com/MirageNet/Mirage/compare/v62.8.0...v62.9.0) (2021-01-26)


### Features

* notify algorithm in connection ([#549](https://github.com/MirageNet/Mirage/issues/549)) ([64e4bbc](https://github.com/MirageNet/Mirage/commit/64e4bbcc5f218e3bac8c8d9f74b3a95cc26de77f))

# [62.8.0](https://github.com/MirageNet/Mirage/compare/v62.7.2...v62.8.0) (2021-01-25)


### Features

* sequence generator ([#548](https://github.com/MirageNet/Mirage/issues/548)) ([1137865](https://github.com/MirageNet/Mirage/commit/113786562272efbfb79438bb0733061cec82b718))

## [62.7.2](https://github.com/MirageNet/Mirage/compare/v62.7.1...v62.7.2) (2021-01-24)


### Bug Fixes

* use unity's cecil ([#545](https://github.com/MirageNet/Mirage/issues/545)) ([4da7a45](https://github.com/MirageNet/Mirage/commit/4da7a45d16d8262046a6ae4f5cb2015189d6e019))

## [62.7.1](https://github.com/MirageNet/Mirage/compare/v62.7.0...v62.7.1) (2021-01-22)


### Bug Fixes

* simplified SetHostVisibilityExceptionNetworkBehaviour test ([#544](https://github.com/MirageNet/Mirage/issues/544)) ([b7465a5](https://github.com/MirageNet/Mirage/commit/b7465a5ba139884157b3d63bd11710d68c9abb9a))

# [62.7.0](https://github.com/MirageNet/Mirage/compare/v62.6.1...v62.7.0) (2021-01-22)


### Features

* transports can report bandwidth ([#542](https://github.com/MirageNet/Mirage/issues/542)) ([d84b3bb](https://github.com/MirageNet/Mirage/commit/d84b3bbc9e0cee127c2fc2c0ee8aefc8fcce2250))

## [62.6.1](https://github.com/MirageNet/Mirage/compare/v62.6.0...v62.6.1) (2021-01-20)


### Bug Fixes

* additive scene example fixes ([#540](https://github.com/MirageNet/Mirage/issues/540)) ([81c6d95](https://github.com/MirageNet/Mirage/commit/81c6d95a253825922dd127e32a393c2288b1e823))

# [62.6.0](https://github.com/MirageNet/Mirage/compare/v62.5.5...v62.6.0) (2021-01-20)


### Features

* add Online and Offline scene support via optional component ([#505](https://github.com/MirageNet/Mirage/issues/505)) ([a83dd5f](https://github.com/MirageNet/Mirage/commit/a83dd5fa263900eae7882b2434940e20393fd674))

## [62.5.5](https://github.com/MirageNet/Mirage/compare/v62.5.4...v62.5.5) (2021-01-20)


### Bug Fixes

* additive scene example not working ([#534](https://github.com/MirageNet/Mirage/issues/534)) ([1580627](https://github.com/MirageNet/Mirage/commit/1580627f8de7ba521c3b105074e293f92e8fbee0))

## [62.5.4](https://github.com/MirageNet/Mirage/compare/v62.5.3...v62.5.4) (2021-01-19)


### Bug Fixes

* don't destroy network scene objects on server stop ([#518](https://github.com/MirageNet/Mirage/issues/518)) ([c9d0387](https://github.com/MirageNet/Mirage/commit/c9d03870210a130fa47237d14144a033585de481))

## [62.5.3](https://github.com/MirageNet/Mirage/compare/v62.5.2...v62.5.3) (2021-01-19)


### Performance Improvements

* avoid weaving editor scripts ([#537](https://github.com/MirageNet/Mirage/issues/537)) ([42967f3](https://github.com/MirageNet/Mirage/commit/42967f3b407d0fcfbcd7b1bd92f6b1a525ccf307))

## [62.5.2](https://github.com/MirageNet/Mirage/compare/v62.5.1...v62.5.2) (2021-01-19)


### Bug Fixes

* error saving field name SYNC_VAR_COUNT ([#536](https://github.com/MirageNet/Mirage/issues/536)) ([cbc52e5](https://github.com/MirageNet/Mirage/commit/cbc52e5fecc2c9aaadc3b7e2a0d886b2a6353afa))

## [62.5.1](https://github.com/MirageNet/Mirage/compare/v62.5.0...v62.5.1) (2021-01-19)


### Bug Fixes

* invalid IL with NI syncvars with hooks ([#535](https://github.com/MirageNet/Mirage/issues/535)) ([49f6141](https://github.com/MirageNet/Mirage/commit/49f614169b5deae4a4dd93e22b003f3f11b7ea03))

# [62.5.0](https://github.com/MirageNet/Mirage/compare/v62.4.1...v62.5.0) (2021-01-18)


### Features

* Add ConnectAsync overload with port. ([#516](https://github.com/MirageNet/Mirage/issues/516)) ([0a9558f](https://github.com/MirageNet/Mirage/commit/0a9558f1d474e3463fa5b79f38c6044da7bf856b))

## [62.4.1](https://github.com/MirageNet/Mirage/compare/v62.4.0...v62.4.1) (2021-01-15)


### Bug Fixes

* unbound allocation ([1fe1bf6](https://github.com/MirageNet/Mirage/commit/1fe1bf6905eb1e860736781e01f309689051ee38))

# [62.4.0](https://github.com/MirageNet/Mirage/compare/v62.3.0...v62.4.0) (2021-01-13)


### Features

* set the assembly version in all assemblies ([#531](https://github.com/MirageNet/Mirage/issues/531)) ([da381bd](https://github.com/MirageNet/Mirage/commit/da381bd575298ddc44a7f3ac9c5291f704ff5e54))

# [62.3.0](https://github.com/MirageNet/Mirage/compare/v62.2.0...v62.3.0) (2021-01-12)


### Features

* writer generation errors link to code ([b76e873](https://github.com/MirageNet/Mirage/commit/b76e87311aaf23f7292a14887a5d5bc2cb319239))

# [62.2.0](https://github.com/MirageNet/Mirage/compare/v62.1.0...v62.2.0) (2021-01-12)


### Features

* reader generation errors link to the code ([99a70fe](https://github.com/MirageNet/Mirage/commit/99a70fe695cdabd70d9a484667f20c4c1af0a1dd))

# [62.1.0](https://github.com/MirageNet/Mirage/compare/v62.0.1...v62.1.0) (2021-01-11)


### Features

* send NI, NB and GO in RPC ([#528](https://github.com/MirageNet/Mirage/issues/528)) ([428ca63](https://github.com/MirageNet/Mirage/commit/428ca63711020cac9accf555ac63feb2495fe965))

## [62.0.1](https://github.com/MirageNet/Mirage/compare/v62.0.0...v62.0.1) (2021-01-11)


### Bug Fixes

* Use Guid.Empty in the tests ([e900d33](https://github.com/MirageNet/Mirage/commit/e900d33f23486d0acb0794a9ff3932caae06ced0))

# [62.0.0](https://github.com/MirageNet/Mirage/compare/v61.1.2...v62.0.0) (2021-01-11)


### Features

* use ILPostProcessor for weaver ([#525](https://github.com/MirageNet/Mirage/issues/525)) ([def64cd](https://github.com/MirageNet/Mirage/commit/def64cd1db525398738f057b3d1eb1fe8afc540c)), closes [/forum.unity.com/threads/how-does-unity-do-codegen-and-why-cant-i-do-it-myself.853867/#post-5646937](https://github.com//forum.unity.com/threads/how-does-unity-do-codegen-and-why-cant-i-do-it-myself.853867//issues/post-5646937)


### BREAKING CHANGES

* Mirage assembly no longer contains the components.  Reference Mirror.Components instead.
* Editor scripts are no longer weaved

## [61.1.2](https://github.com/MirageNet/Mirage/compare/v61.1.1...v61.1.2) (2021-01-09)


### Bug Fixes

* NullReferenceException destroying objects ([#526](https://github.com/MirageNet/Mirage/issues/526)) ([3ad2608](https://github.com/MirageNet/Mirage/commit/3ad2608e9e5223f72aba2458b97e5ec45ca15d94))

## [61.1.1](https://github.com/MirageNet/Mirage/compare/v61.1.0...v61.1.1) (2021-01-08)


### Bug Fixes

* warning with missing Transport folder ([be5aa69](https://github.com/MirageNet/Mirage/commit/be5aa693fc439f309e257e202cc77172597218c0))

# [61.1.0](https://github.com/MirageNet/Mirage/compare/v61.0.2...v61.1.0) (2021-01-05)


### Features

* allow for generic NetworkBehaviors ([#519](https://github.com/MirageNet/Mirage/issues/519)) ([2858ff4](https://github.com/MirageNet/Mirage/commit/2858ff4cbb32cf22015389d57cdff182034624a1))

## [61.0.2](https://github.com/MirageNet/Mirage/compare/v61.0.1...v61.0.2) (2021-01-02)


### Bug Fixes

* method access exception with serverrpc that return something ([8cb00e9](https://github.com/MirageNet/Mirage/commit/8cb00e9f3253932008cf165636c4b5427be9aebe))

## [61.0.1](https://github.com/MirageNet/Mirage/compare/v61.0.0...v61.0.1) (2020-12-31)


### Bug Fixes

* Icon in welcome window ([eb97cef](https://github.com/MirageNet/Mirage/commit/eb97cefb5880372e353b45bff5084a7ebe92da3c))

# [61.0.0](https://github.com/MirageNet/Mirage/compare/v60.4.1...v61.0.0) (2020-12-31)


* Removed NetworkDiscovery ([0df3afc](https://github.com/MirageNet/Mirage/commit/0df3afc2616d53785aa6d2d676473251ead811b2))


### BREAKING CHANGES

* Moved NetworkDiscovery to a separate repo

## [60.4.1](https://github.com/MirageNet/Mirage/compare/v60.4.0...v60.4.1) (2020-12-30)


### Bug Fixes

* NRE in welcome window ([e4e2fa5](https://github.com/MirageNet/Mirage/commit/e4e2fa52691c201e7b051af8bbc1dfdce4be84e5))

# [60.4.0](https://github.com/MirageNet/Mirage/compare/v60.3.0...v60.4.0) (2020-12-29)


### Features

* SyncVar support arbitrary NetworkBehavior ([#514](https://github.com/MirageNet/Mirage/issues/514)) ([67b0c9f](https://github.com/MirageNet/Mirage/commit/67b0c9f16dc9a8e7065d1bcf0140a479494eb76c))

# [60.3.0](https://github.com/MirageNet/Mirage/compare/v60.2.0...v60.3.0) (2020-12-29)


### Features

* Support gameobjects in syncvars ([#513](https://github.com/MirageNet/Mirage/issues/513)) ([29fb101](https://github.com/MirageNet/Mirage/commit/29fb1018fad55a6c392be002906da5cc0d79163c))

# [60.2.0](https://github.com/MirageNet/Mirage/compare/v60.1.1...v60.2.0) (2020-12-27)


### Features

* UIElements Welcome Window ([#510](https://github.com/MirageNet/Mirage/issues/510)) ([654c5e1](https://github.com/MirageNet/Mirage/commit/654c5e1a91a994d4c763281c1c22e91298826fb4))

## [60.1.1](https://github.com/MirageNet/Mirage/compare/v60.1.0...v60.1.1) (2020-12-27)


### Bug Fixes

* runtime version ([10e7d6e](https://github.com/MirageNet/Mirage/commit/10e7d6e4d87b923962f9f8bed713db28a21f0354))
* runtime version ([51b925f](https://github.com/MirageNet/Mirage/commit/51b925f924d6ed0877c2a5f72ebf753f284b4ca2))

# [60.1.0](https://github.com/MirageNet/Mirage/compare/v60.0.1...v60.1.0) (2020-12-27)


### Features

* Provide mirrorng version at runtime ([#511](https://github.com/MirageNet/Mirage/issues/511)) ([b2df972](https://github.com/MirageNet/Mirage/commit/b2df97248a845a412f27004b43bc75f99a58ae1e))

## [60.0.1](https://github.com/MirageNet/Mirage/compare/v60.0.0...v60.0.1) (2020-12-19)


### Bug Fixes

* network objects not destroyed on server stop ([#468](https://github.com/MirageNet/Mirage/issues/468)) ([abf5f2f](https://github.com/MirageNet/Mirage/commit/abf5f2f4fb4375c7fece06d3e11ce79408ac9666))

# [60.0.0](https://github.com/MirageNet/Mirage/compare/v59.2.1...v60.0.0) (2020-12-19)


* remove serverOnly (#496) ([0ef5c33](https://github.com/MirageNet/Mirage/commit/0ef5c33418bf1550d664ed942eede165752fca7e)), closes [#496](https://github.com/MirageNet/Mirage/issues/496) [#389](https://github.com/MirageNet/Mirage/issues/389) [#389](https://github.com/MirageNet/Mirage/issues/389)


### Bug Fixes

* prevent DoS attacks with invalid array length ([#500](https://github.com/MirageNet/Mirage/issues/500)) ([78e6077](https://github.com/MirageNet/Mirage/commit/78e60777fae6ba63e34406b75ccfe074363ed593))


### BREAKING CHANGES

* Remove serverOnly option in NetworkIdentity

## [59.2.1](https://github.com/MirageNet/Mirage/compare/v59.2.0...v59.2.1) (2020-11-30)


### Bug Fixes

* font color in basic example ([a9dfb3f](https://github.com/MirageNet/Mirage/commit/a9dfb3fdac79b68f900d71c48cdc9dc07cffba1e))


### Performance Improvements

* Ping now goes over unreliable channel ([#507](https://github.com/MirageNet/Mirage/issues/507)) ([096d62d](https://github.com/MirageNet/Mirage/commit/096d62dd6c6536358939614ac61c258f6f4e0a8d))

# [59.2.0](https://github.com/MirageNet/Mirage/compare/v59.1.0...v59.2.0) (2020-11-28)


### Features

* Quaternion compression ([#501](https://github.com/MirageNet/Mirage/issues/501)) ([c67f873](https://github.com/MirageNet/Mirage/commit/c67f8737bd20f61a4c403c0726d27d56f0464b07))

# [59.1.0](https://github.com/MirageNet/Mirage/compare/v59.0.8...v59.1.0) (2020-11-25)


### Features

* User can now configure window size ([ec0b839](https://github.com/MirageNet/Mirage/commit/ec0b8397d958f0317b3354f0f83d85f496495c49))

## [59.0.8](https://github.com/MirageNet/Mirage/compare/v59.0.7...v59.0.8) (2020-11-23)


### Bug Fixes

* check for log level for warnings ([#445](https://github.com/MirageNet/Mirage/issues/445)) ([90013ea](https://github.com/MirageNet/Mirage/commit/90013eaaaab4668cbb99f8ffa2b463f136253006))

## [59.0.7](https://github.com/MirageNet/Mirage/compare/v59.0.6...v59.0.7) (2020-11-23)


### Bug Fixes

* use OnDestroy to Unsubscribe in comps like [#480](https://github.com/MirageNet/Mirage/issues/480) ([#481](https://github.com/MirageNet/Mirage/issues/481)) ([3dd66c0](https://github.com/MirageNet/Mirage/commit/3dd66c079fbd2ad2b7c57c5d39421a85755c2e30))

## [59.0.6](https://github.com/MirageNet/Mirage/compare/v59.0.5...v59.0.6) (2020-11-20)


### Bug Fixes

* script not found error with NetworkDiscoveryHud ([#494](https://github.com/MirageNet/Mirage/issues/494)) ([8e39e21](https://github.com/MirageNet/Mirage/commit/8e39e219aed3ca62b9f5766059e711da7540a629))

## [59.0.5](https://github.com/MirageNet/Mirage/compare/v59.0.4...v59.0.5) (2020-11-14)


### Bug Fixes

* generate reader for types in other assemblies ([b685226](https://github.com/MirageNet/Mirage/commit/b685226bbc9442d5901d4968492b63dc852e2704))
* generate writer for types in other assemblies ([8385c29](https://github.com/MirageNet/Mirage/commit/8385c29c23a2b702e2c0d7f156a803d57efc0f5d))

## [59.0.4](https://github.com/MirageNet/Mirage/compare/v59.0.3...v59.0.4) (2020-11-14)


### Bug Fixes

* using mathematics in commands and rpcs ([#492](https://github.com/MirageNet/Mirage/issues/492)) ([ee27841](https://github.com/MirageNet/Mirage/commit/ee278415652ff6087fd8e45f15be3cf7f01181b8))

## [59.0.3](https://github.com/MirageNet/Mirage/compare/v59.0.2...v59.0.3) (2020-11-10)


### Bug Fixes

* calling base command in other assemblies ([e49fda1](https://github.com/MirageNet/Mirage/commit/e49fda13bdb227c80185d3bd02062649bc2124b1))
* error importing scriptable object from another module ([6cdd112](https://github.com/MirageNet/Mirage/commit/6cdd1127b88117765953c74d49071e19af471817))

## [59.0.2](https://github.com/MirageNet/Mirage/compare/v59.0.1...v59.0.2) (2020-11-10)


### Bug Fixes

* logs now save properly when reloading ([19c86e7](https://github.com/MirageNet/Mirage/commit/19c86e7d5c68ff044f5a92f374a2a1944c05895e))

## [59.0.1](https://github.com/MirageNet/Mirage/compare/v59.0.0...v59.0.1) (2020-11-07)


### Bug Fixes

* don't consume so much memory registering prefabs ([#486](https://github.com/MirageNet/Mirage/issues/486)) ([d451782](https://github.com/MirageNet/Mirage/commit/d451782f60a9f7066686f2dff2b68cc60fa8c725))

# [59.0.0](https://github.com/MirageNet/Mirage/compare/v58.0.1...v59.0.0) (2020-11-06)


* Transport now has connected and started events. (#479) ([3e7f688](https://github.com/MirageNet/Mirage/commit/3e7f688d05a0252aed8af8f058441b904cd13531)), closes [#479](https://github.com/MirageNet/Mirage/issues/479)


### BREAKING CHANGES

* Add Connected event to Transport API
* Add Started event to Transport API
* ListenAsync returns a task that completes when the transport stops
* Remove AcceptAsync from transports

## [58.0.1](https://github.com/MirageNet/Mirage/compare/v58.0.0...v58.0.1) (2020-11-05)


### Bug Fixes

* Restarting host does not start player ([#480](https://github.com/MirageNet/Mirage/issues/480)) ([11cb7f2](https://github.com/MirageNet/Mirage/commit/11cb7f2da4b6eda9c19bbd3844523396a713d648))

# [58.0.0](https://github.com/MirageNet/Mirage/compare/v57.0.0...v58.0.0) (2020-11-05)


* ClientObjectManager now requires NetworkIdentity (#475) ([103593b](https://github.com/MirageNet/Mirage/commit/103593bdb3ffb78ad27714f68424e2250ed99008)), closes [#475](https://github.com/MirageNet/Mirage/issues/475)


### BREAKING CHANGES

* Now you can only assign prefabs with NetworkIdentity to the ClientObjectManager

# [57.0.0](https://github.com/MirageNet/Mirage/compare/v56.5.0...v57.0.0) (2020-11-05)


* Remove redundant spawn handler (#476) ([9bbf0dc](https://github.com/MirageNet/Mirage/commit/9bbf0dc824efb068b5c181ce0ee3fd519da0380f)), closes [#476](https://github.com/MirageNet/Mirage/issues/476)


### BREAKING CHANGES

* Removed redundant spawn handler

# [56.5.0](https://github.com/MirageNet/Mirage/compare/v56.4.4...v56.5.0) (2020-11-04)


### Features

* ClientObjectManager is available in network behaviors ([#466](https://github.com/MirageNet/Mirage/issues/466)) ([d0d0b2a](https://github.com/MirageNet/Mirage/commit/d0d0b2a4c910fa89065b0f0144f63ed3a2ce9dac))

## [56.4.4](https://github.com/MirageNet/Mirage/compare/v56.4.3...v56.4.4) (2020-11-04)


### Bug Fixes

* do not cache lastReceived in release ([9a15863](https://github.com/MirageNet/Mirage/commit/9a15863565259888dca5570c64df8fbbba476988))

## [56.4.3](https://github.com/MirageNet/Mirage/compare/v56.4.2...v56.4.3) (2020-11-03)


### Bug Fixes

* PlayerSpawner depends on ClientObjectManager throw if missing ([#472](https://github.com/MirageNet/Mirage/issues/472)) ([0ab0a70](https://github.com/MirageNet/Mirage/commit/0ab0a7040c2809cab111557345bb4f56c5cccda9))

## [56.4.2](https://github.com/MirageNet/Mirage/compare/v56.4.1...v56.4.2) (2020-11-03)


### Bug Fixes

* better transport checks on NS and NC ([#464](https://github.com/MirageNet/Mirage/issues/464)) ([7703d80](https://github.com/MirageNet/Mirage/commit/7703d8042875521177e5c5fa2755091563148601))
* old refs to NetMan ([0df8c89](https://github.com/MirageNet/Mirage/commit/0df8c897330199933a309248ef286b2cb652ea14))

## [56.4.1](https://github.com/MirageNet/Mirage/compare/v56.4.0...v56.4.1) (2020-11-03)


### Bug Fixes

* DoS vector in kcp accept ([#469](https://github.com/MirageNet/Mirage/issues/469)) ([6964bc6](https://github.com/MirageNet/Mirage/commit/6964bc6a34147a916ff49fa1fabcd933f9efce42))

# [56.4.0](https://github.com/MirageNet/Mirage/compare/v56.3.4...v56.4.0) (2020-11-03)


### Features

* return values from [ServerRpc] ([#454](https://github.com/MirageNet/Mirage/issues/454)) ([0d076a7](https://github.com/MirageNet/Mirage/commit/0d076a72c30daea0d343523bdf38dc1f9e14739d))

## [56.3.4](https://github.com/MirageNet/Mirage/compare/v56.3.3...v56.3.4) (2020-11-03)


### Bug Fixes

* added missing UniTask.asmdef reference to Mirror.Weaver.asmdef ([#463](https://github.com/MirageNet/Mirage/issues/463)) ([bcb8ae7](https://github.com/MirageNet/Mirage/commit/bcb8ae73f4ebc949d10d8abec6e57a2737bfd276))

## [56.3.3](https://github.com/MirageNet/Mirage/compare/v56.3.2...v56.3.3) (2020-11-02)


### Bug Fixes

* prevent NRE if client is not present on server ([#461](https://github.com/MirageNet/Mirage/issues/461)) ([357da87](https://github.com/MirageNet/Mirage/commit/357da87d530e8bdb01e2eb0967d75a7a354c92b1))

## [56.3.2](https://github.com/MirageNet/Mirage/compare/v56.3.1...v56.3.2) (2020-11-02)


### Bug Fixes

* **NetworkAnimator:** fixing trigger not applied on host ([49b5325](https://github.com/MirageNet/Mirage/commit/49b532546f22074ff1478bf76ecd78f2406a1b55))

## [56.3.1](https://github.com/MirageNet/Mirage/compare/v56.3.0...v56.3.1) (2020-11-02)


### Bug Fixes

* accept after disconnect ([3d06e8a](https://github.com/MirageNet/Mirage/commit/3d06e8aa9b21c667b231a609cf194e517b75438c))
* add missing reset ([4f75b92](https://github.com/MirageNet/Mirage/commit/4f75b925fb72077c9e2d0ee56b9b99cc14e44281))

# [56.3.0](https://github.com/MirageNet/Mirage/compare/v56.2.0...v56.3.0) (2020-11-02)


### Features

* throw exception if invalid rpc ([#456](https://github.com/MirageNet/Mirage/issues/456)) ([3cef90d](https://github.com/MirageNet/Mirage/commit/3cef90d3a059dd0311b59ce6561fe9abe798b9e4))

# [56.2.0](https://github.com/MirageNet/Mirage/compare/v56.1.2...v56.2.0) (2020-11-02)


### Features

* server sends list of additive scenes upon connect ([#451](https://github.com/MirageNet/Mirage/issues/451)) ([3d0b6c5](https://github.com/MirageNet/Mirage/commit/3d0b6c5ad2401798cfd19fdba7f71d0817c67854))

## [56.1.2](https://github.com/MirageNet/Mirage/compare/v56.1.1...v56.1.2) (2020-10-31)


### Bug Fixes

* client NRE for objects spawned in .Started in hostmode  ([#453](https://github.com/MirageNet/Mirage/issues/453)) ([918504c](https://github.com/MirageNet/Mirage/commit/918504c1b7a16f44dcb6c348150ac078869766c3))

## [56.1.1](https://github.com/MirageNet/Mirage/compare/v56.1.0...v56.1.1) (2020-10-30)


### Bug Fixes

* disconnect transport when play mode exits ([#449](https://github.com/MirageNet/Mirage/issues/449)) ([e741809](https://github.com/MirageNet/Mirage/commit/e74180947103fd41c2f3ecfad917825c16c18627))

# [56.1.0](https://github.com/MirageNet/Mirage/compare/v56.0.2...v56.1.0) (2020-10-30)


### Features

* add quick access ref to NetIdentity.ServerObjectManager ([#444](https://github.com/MirageNet/Mirage/issues/444)) ([d691ca4](https://github.com/MirageNet/Mirage/commit/d691ca4e2c7c7abd6d746718ceca577d41c3b606))

## [56.0.2](https://github.com/MirageNet/Mirage/compare/v56.0.1...v56.0.2) (2020-10-30)


### Bug Fixes

* third try to prevent double load ([#447](https://github.com/MirageNet/Mirage/issues/447)) ([347d176](https://github.com/MirageNet/Mirage/commit/347d176c83f9e43c177b450688899191f4221fe7))

## [56.0.1](https://github.com/MirageNet/Mirage/compare/v56.0.0...v56.0.1) (2020-10-29)


### Bug Fixes

* host wasnt loading with additive fix ([cd6110a](https://github.com/MirageNet/Mirage/commit/cd6110a1350387a915a07bf348418713717d8816))
* wrong text in exception ([d0d5581](https://github.com/MirageNet/Mirage/commit/d0d55811fb8e143b9d745cc73f2a0ee65332878e))

# [56.0.0](https://github.com/MirageNet/Mirage/compare/v55.0.1...v56.0.0) (2020-10-29)


### breaking

* add ServerObjectManager for object spawning ([#443](https://github.com/MirageNet/Mirage/issues/443)) ([7abf355](https://github.com/MirageNet/Mirage/commit/7abf3556284d831e5e85993f7f264c28c97df458)), closes [#438](https://github.com/MirageNet/Mirage/issues/438)


### BREAKING CHANGES

* NetworkServer no longer spawns objects, add a ServerObjectManager for that

## [55.0.1](https://github.com/MirageNet/Mirage/compare/v55.0.0...v55.0.1) (2020-10-27)


### Bug Fixes

* **weaver:** NRE with basic authenticator ([#440](https://github.com/MirageNet/Mirage/issues/440)) ([68480db](https://github.com/MirageNet/Mirage/commit/68480db93762b0f08f9e28c22512a546109bf7bc))

# [55.0.0](https://github.com/MirageNet/Mirage/compare/v54.1.1...v55.0.0) (2020-10-27)


* no need for NM here ([78b3b10](https://github.com/MirageNet/Mirage/commit/78b3b1061eb9183a54f3c5ea9614804f5474716e))


### BREAKING CHANGES

* Authenticator does not have dependency on NM anymore

## [54.1.1](https://github.com/MirageNet/Mirage/compare/v54.1.0...v54.1.1) (2020-10-26)


### Bug Fixes

* local client loading additive scene twice. ([5a74fb0](https://github.com/MirageNet/Mirage/commit/5a74fb0573b254e8c0cbb9c86d38e3bbe2724f77))

# [54.1.0](https://github.com/MirageNet/Mirage/compare/v54.0.0...v54.1.0) (2020-10-26)


### Features

* log settings component ([#439](https://github.com/MirageNet/Mirage/issues/439)) ([9f06f2e](https://github.com/MirageNet/Mirage/commit/9f06f2e527a816994f16903575bb11ec14d24ce6))

# [54.0.0](https://github.com/MirageNet/Mirage/compare/v53.0.1...v54.0.0) (2020-10-25)


* ReceiveAsync throws EndOfStreamException (#435) ([faf2e54](https://github.com/MirageNet/Mirage/commit/faf2e54abe07beb0405657f41e166212f3ae00ff)), closes [#435](https://github.com/MirageNet/Mirage/issues/435)


### BREAKING CHANGES

* External transports will need an update

## [53.0.1](https://github.com/MirageNet/Mirage/compare/v53.0.0...v53.0.1) (2020-10-24)


### Performance Improvements

* faster component serialization ([#430](https://github.com/MirageNet/Mirage/issues/430)) ([b675027](https://github.com/MirageNet/Mirage/commit/b67502711798bebe15ae58c96d6b09145fe0e8ed)), closes [#2331](https://github.com/MirageNet/Mirage/issues/2331)

# [53.0.0](https://github.com/MirageNet/Mirage/compare/v52.1.1...v53.0.0) (2020-10-23)


* Remove FallbackTransport (#432) ([261bf24](https://github.com/MirageNet/Mirage/commit/261bf24bfcf5a201e7e96ba1a4dc16f9429f8121)), closes [#432](https://github.com/MirageNet/Mirage/issues/432)
* Remove FallbackTransport ([2c04202](https://github.com/MirageNet/Mirage/commit/2c042021b965322e7fc08d9f82177bf9cf5268ff))


### BREAKING CHANGES

* FallbackTransport removed

* fix docs
* FallbackTransport removed

## [52.1.1](https://github.com/MirageNet/Mirage/compare/v52.1.0...v52.1.1) (2020-10-23)


### Performance Improvements

* simplify NetworkWriter/Reader dispose ([#431](https://github.com/MirageNet/Mirage/issues/431)) ([bf62345](https://github.com/MirageNet/Mirage/commit/bf62345f22137dd69f476b5ae1b2da63029e68a9))

# [52.1.0](https://github.com/MirageNet/Mirage/compare/v52.0.1...v52.1.0) (2020-10-23)


### Features

* SyncDictionary raise event when initially synchronized ([23349af](https://github.com/MirageNet/Mirage/commit/23349af382ca911e4b16d30b811a3acb4f5ab7b9))
* SyncList raise event when initially synchronized ([9f679c5](https://github.com/MirageNet/Mirage/commit/9f679c5706df22ea18345d5f0b40833089171110))
* SyncSet raise event when initially synchronized ([03f2075](https://github.com/MirageNet/Mirage/commit/03f20751a871e63c8e34382b2c1d09ce0275860c))

# [52.1.0](https://github.com/MirageNet/Mirage/compare/v52.0.1...v52.1.0) (2020-10-23)


### Features

* SyncDictionary raise event when initially synchronized ([23349af](https://github.com/MirageNet/Mirage/commit/23349af382ca911e4b16d30b811a3acb4f5ab7b9))
* SyncList raise event when initially synchronized ([9f679c5](https://github.com/MirageNet/Mirage/commit/9f679c5706df22ea18345d5f0b40833089171110))
* SyncSet raise event when initially synchronized ([03f2075](https://github.com/MirageNet/Mirage/commit/03f20751a871e63c8e34382b2c1d09ce0275860c))

## [52.0.1](https://github.com/MirageNet/Mirage/compare/v52.0.0...v52.0.1) (2020-10-23)


### Bug Fixes

* use the spawn calls that already happen via scene loading ([#426](https://github.com/MirageNet/Mirage/issues/426)) ([cc19f3b](https://github.com/MirageNet/Mirage/commit/cc19f3b1b1e69e7f6b46d800fba75adfd468cca3))

# [52.0.0](https://github.com/MirageNet/Mirage/compare/v51.1.5...v52.0.0) (2020-10-22)


* Remove TcpTransport (#425) ([076c05a](https://github.com/MirageNet/Mirage/commit/076c05a8473d08ecb3d886d59b9a32d7b12c3eff)), closes [#425](https://github.com/MirageNet/Mirage/issues/425)


### Bug Fixes

* names and refs clear that PATH should be sent in scene msg ([#423](https://github.com/MirageNet/Mirage/issues/423)) ([c68189c](https://github.com/MirageNet/Mirage/commit/c68189cc62d59cade5f27245d566129a6fb77b72))


### BREAKING CHANGES

* TCPTransport removed. Use KCPTransport instead

## [51.1.5](https://github.com/MirageNet/Mirage/compare/v51.1.4...v51.1.5) (2020-10-22)


### Bug Fixes

* TCPTransport is obsolete, use KCPTransport instead ([c031ae9](https://github.com/MirageNet/Mirage/commit/c031ae99b1666f76494bed80ca32df438bf705a3))

## [51.1.4](https://github.com/MirageNet/Mirage/compare/v51.1.3...v51.1.4) (2020-10-22)


### Bug Fixes

* expect reserved header to be in the input ([547bdd6](https://github.com/MirageNet/Mirage/commit/547bdd6a83521c753d703765d5592b20dabf4032))

## [51.1.3](https://github.com/MirageNet/Mirage/compare/v51.1.2...v51.1.3) (2020-10-22)


### Performance Improvements

* port kcp code from vis2k ([#422](https://github.com/MirageNet/Mirage/issues/422)) ([aaab0e3](https://github.com/MirageNet/Mirage/commit/aaab0e3ae1f2c39b281a01fb82e8f2c3b54edcda))

## [51.1.2](https://github.com/MirageNet/Mirage/compare/v51.1.1...v51.1.2) (2020-10-21)


### Bug Fixes

* fix multiple scene loading order issues ([#418](https://github.com/MirageNet/Mirage/issues/418)) ([6d8265d](https://github.com/MirageNet/Mirage/commit/6d8265d4f71029365381603abd5f4bb7a28ffeb0))

## [51.1.1](https://github.com/MirageNet/Mirage/compare/v51.1.0...v51.1.1) (2020-10-21)


### Bug Fixes

* InvalidDataException not found problem ([229f73d](https://github.com/MirageNet/Mirage/commit/229f73d88485264ebd517603f848e9ebed133fcf))

# [51.1.0](https://github.com/MirageNet/Mirage/compare/v51.0.0...v51.1.0) (2020-10-21)


### Features

* KCP transport now provides unreliable channel ([#420](https://github.com/MirageNet/Mirage/issues/420)) ([8aac115](https://github.com/MirageNet/Mirage/commit/8aac115f8a7cb7b2c62d35716eeb081d7d81664b))

# [51.0.0](https://github.com/MirageNet/Mirage/compare/v50.2.0...v51.0.0) (2020-10-21)


* Transport api can now send messages in channels (#419) ([9a2690e](https://github.com/MirageNet/Mirage/commit/9a2690e31c6dee354254bd4c659975d900c18423)), closes [#419](https://github.com/MirageNet/Mirage/issues/419)


### BREAKING CHANGES

* Transports now receive and return channels

# [50.2.0](https://github.com/MirageNet/Mirage/compare/v50.1.5...v50.2.0) (2020-10-20)


### Bug Fixes

* method access exception sending rpcs ([531e908](https://github.com/MirageNet/Mirage/commit/531e908c95f8bd8f8ccc622f7314db4536932835))


### Features

* display user-friendly log with an unexpected messages. ([#417](https://github.com/MirageNet/Mirage/issues/417)) ([7b78c29](https://github.com/MirageNet/Mirage/commit/7b78c29309dd5f40c4981879e18facebf8fe7837))

## [50.1.5](https://github.com/MirageNet/Mirage/compare/v50.1.4...v50.1.5) (2020-10-20)


### Bug Fixes

* recursive types with collections ([5cb0058](https://github.com/MirageNet/Mirage/commit/5cb005826b79019de03df8a4b2e85fe27cdbbffc))

## [50.1.4](https://github.com/MirageNet/Mirage/compare/v50.1.3...v50.1.4) (2020-10-20)


### Bug Fixes

* potential NRE generating readers ([324ba60](https://github.com/MirageNet/Mirage/commit/324ba60615944429a30fcdead4f8b0dc6c98aab7))

## [50.1.3](https://github.com/MirageNet/Mirage/compare/v50.1.2...v50.1.3) (2020-10-20)


### Bug Fixes

* Multiplex transport exception ([#415](https://github.com/MirageNet/Mirage/issues/415)) ([6534fbb](https://github.com/MirageNet/Mirage/commit/6534fbb23d5d4e946ff9058cc84115d826e51672)), closes [#414](https://github.com/MirageNet/Mirage/issues/414)

## [50.1.2](https://github.com/MirageNet/Mirage/compare/v50.1.1...v50.1.2) (2020-10-19)


### Bug Fixes

* add WaitUntilWithTimeout to prevent tests from getting stuck ([#412](https://github.com/MirageNet/Mirage/issues/412)) ([df1ccb4](https://github.com/MirageNet/Mirage/commit/df1ccb48359b5a6b0b914133dbdb6f2d702e47a6))

## [50.1.1](https://github.com/MirageNet/Mirage/compare/v50.1.0...v50.1.1) (2020-10-19)


### Performance Improvements

* don't drop acks,  follow original C algorithm ([c0b5a12](https://github.com/MirageNet/Mirage/commit/c0b5a123ea6fd203676b987ab91cb49651d9ad9a))

# [50.1.0](https://github.com/MirageNet/Mirage/compare/v50.0.2...v50.1.0) (2020-10-18)


### Features

* User can set KcpDelayMode via KcpTransport([#403](https://github.com/MirageNet/Mirage/issues/403)) ([bdd0d9c](https://github.com/MirageNet/Mirage/commit/bdd0d9cc207f1b1b878acb93ec8e7fdff0b43b7e))

## [50.0.2](https://github.com/MirageNet/Mirage/compare/v50.0.1...v50.0.2) (2020-10-18)


### Bug Fixes

* recycling segments ([9d12658](https://github.com/MirageNet/Mirage/commit/9d12658616b822f846f88184fde0875bb3154b38))
* use buffer same size as C version ([20e1324](https://github.com/MirageNet/Mirage/commit/20e13245e813f9a06571d826cc2a0d3926d93e4b))

## [50.0.1](https://github.com/MirageNet/Mirage/compare/v50.0.0...v50.0.1) (2020-10-18)


### Bug Fixes

* bug setting the rto,  it should be 100 in normal mode ([091bde6](https://github.com/MirageNet/Mirage/commit/091bde6e2c95efb2ee54a34ad8b7f03fde259c5a))

# [50.0.0](https://github.com/MirageNet/Mirage/compare/v49.4.0...v50.0.0) (2020-10-17)


* Remove out parameter (#404) ([43dc156](https://github.com/MirageNet/Mirage/commit/43dc1562d9b60a1820585e71e06a8d8db88d37c7)), closes [#404](https://github.com/MirageNet/Mirage/issues/404)


### BREAKING CHANGES

* GetPrefab now just returns the prefab or null

# [49.4.0](https://github.com/MirageNet/Mirage/compare/v49.3.3...v49.4.0) (2020-10-17)


### Features

* DoS prevention ([#401](https://github.com/MirageNet/Mirage/issues/401)) ([4016259](https://github.com/MirageNet/Mirage/commit/4016259719931f51c1f93dcd3f7302abe0af98fd))

## [49.3.3](https://github.com/MirageNet/Mirage/compare/v49.3.2...v49.3.3) (2020-10-16)


### Bug Fixes

* reading and writing a network identity before spawning ([#400](https://github.com/MirageNet/Mirage/issues/400)) ([870f49d](https://github.com/MirageNet/Mirage/commit/870f49d9bd03f21e7b85e9b6f98cc55551f433c9)), closes [#399](https://github.com/MirageNet/Mirage/issues/399)

## [49.3.2](https://github.com/MirageNet/Mirage/compare/v49.3.1...v49.3.2) (2020-10-16)


### Performance Improvements

* Make KCP the default transport ([#398](https://github.com/MirageNet/Mirage/issues/398)) ([dc6cc4f](https://github.com/MirageNet/Mirage/commit/dc6cc4f944cef3e0185f9d4ee3765309a965f051))

## [49.3.1](https://github.com/MirageNet/Mirage/compare/v49.3.0...v49.3.1) (2020-10-15)


### Bug Fixes

* use hostname for serverUri ([4b38fbe](https://github.com/MirageNet/Mirage/commit/4b38fbe91c7aebd291bb6b78d08f1e9526b9e598))

# [49.3.0](https://github.com/MirageNet/Mirage/compare/v49.2.0...v49.3.0) (2020-10-15)


### Bug Fixes

* double counting packets on kcp receive ([89cb937](https://github.com/MirageNet/Mirage/commit/89cb937ebcfbbad75f299d7dd95b4cd6746a5e04))
* NRE when disconnecting with no owned objects ([4e7983e](https://github.com/MirageNet/Mirage/commit/4e7983e621dfdaf89ba39b313701e9b8c7a75411))


### Features

* KCP transport now has CRC64 validation ([#397](https://github.com/MirageNet/Mirage/issues/397)) ([21c8649](https://github.com/MirageNet/Mirage/commit/21c8649aa34b6bbdca11dbffb1b81d52e331baee))


### Performance Improvements

* recycle segments ([13a3c3d](https://github.com/MirageNet/Mirage/commit/13a3c3d233b121322be177f414ac00ac373e0f6f))
* reduce allocation sending to observers ([f5f7f6c](https://github.com/MirageNet/Mirage/commit/f5f7f6c9b5b33f1f432d7bf6016b8e63f4699e7d))
* reduce allocations per client ([8d37f8c](https://github.com/MirageNet/Mirage/commit/8d37f8c987addfd99a2078d29e1456ce715239c7))
* Refactore move to receive queue ([173735c](https://github.com/MirageNet/Mirage/commit/173735c7212410fbcbe942a87882b5ee412c8080))
* remove allocation ([9311fbd](https://github.com/MirageNet/Mirage/commit/9311fbdd2ef02ce617377a11286a585e358d7c24))
* reuse segment if it is a repeat ([4879010](https://github.com/MirageNet/Mirage/commit/4879010a2056f972dc4737e9f1e9fae4ef7486d9))
* Use allocation free completion source ([20227a7](https://github.com/MirageNet/Mirage/commit/20227a7a05bd5919507bcfae6d74a84168a56b4f))
* use Unitask ([384d02b](https://github.com/MirageNet/Mirage/commit/384d02bbc298f63cfd08bdd8fb8771d60d5b6ec4))
* Use UniTask instead of task ([70c9cfb](https://github.com/MirageNet/Mirage/commit/70c9cfbd80c489f0c6d6fb78cec486856574c1f5))

## [49.2.1](https://github.com/MirageNet/Mirage/compare/v49.2.0...v49.2.1) (2020-10-15)


### Bug Fixes

* double counting packets on kcp receive ([89cb937](https://github.com/MirageNet/Mirage/commit/89cb937ebcfbbad75f299d7dd95b4cd6746a5e04))
* NRE when disconnecting with no owned objects ([4e7983e](https://github.com/MirageNet/Mirage/commit/4e7983e621dfdaf89ba39b313701e9b8c7a75411))


### Performance Improvements

* recycle segments ([13a3c3d](https://github.com/MirageNet/Mirage/commit/13a3c3d233b121322be177f414ac00ac373e0f6f))
* reduce allocation sending to observers ([f5f7f6c](https://github.com/MirageNet/Mirage/commit/f5f7f6c9b5b33f1f432d7bf6016b8e63f4699e7d))
* reduce allocations per client ([8d37f8c](https://github.com/MirageNet/Mirage/commit/8d37f8c987addfd99a2078d29e1456ce715239c7))
* Refactore move to receive queue ([173735c](https://github.com/MirageNet/Mirage/commit/173735c7212410fbcbe942a87882b5ee412c8080))
* remove allocation ([9311fbd](https://github.com/MirageNet/Mirage/commit/9311fbdd2ef02ce617377a11286a585e358d7c24))
* reuse segment if it is a repeat ([4879010](https://github.com/MirageNet/Mirage/commit/4879010a2056f972dc4737e9f1e9fae4ef7486d9))
* Use allocation free completion source ([20227a7](https://github.com/MirageNet/Mirage/commit/20227a7a05bd5919507bcfae6d74a84168a56b4f))
* use Unitask ([384d02b](https://github.com/MirageNet/Mirage/commit/384d02bbc298f63cfd08bdd8fb8771d60d5b6ec4))
* Use UniTask instead of task ([70c9cfb](https://github.com/MirageNet/Mirage/commit/70c9cfbd80c489f0c6d6fb78cec486856574c1f5))

## [49.2.1](https://github.com/MirageNet/Mirage/compare/v49.2.0...v49.2.1) (2020-10-15)


### Bug Fixes

* double counting packets on kcp receive ([89cb937](https://github.com/MirageNet/Mirage/commit/89cb937ebcfbbad75f299d7dd95b4cd6746a5e04))


### Performance Improvements

* recycle segments ([13a3c3d](https://github.com/MirageNet/Mirage/commit/13a3c3d233b121322be177f414ac00ac373e0f6f))
* reduce allocation sending to observers ([f5f7f6c](https://github.com/MirageNet/Mirage/commit/f5f7f6c9b5b33f1f432d7bf6016b8e63f4699e7d))
* reduce allocations per client ([8d37f8c](https://github.com/MirageNet/Mirage/commit/8d37f8c987addfd99a2078d29e1456ce715239c7))
* Refactore move to receive queue ([173735c](https://github.com/MirageNet/Mirage/commit/173735c7212410fbcbe942a87882b5ee412c8080))
* remove allocation ([9311fbd](https://github.com/MirageNet/Mirage/commit/9311fbdd2ef02ce617377a11286a585e358d7c24))
* reuse segment if it is a repeat ([4879010](https://github.com/MirageNet/Mirage/commit/4879010a2056f972dc4737e9f1e9fae4ef7486d9))
* Use allocation free completion source ([20227a7](https://github.com/MirageNet/Mirage/commit/20227a7a05bd5919507bcfae6d74a84168a56b4f))
* Use UniTask instead of task ([70c9cfb](https://github.com/MirageNet/Mirage/commit/70c9cfbd80c489f0c6d6fb78cec486856574c1f5))

# [49.2.0](https://github.com/MirageNet/Mirage/compare/v49.1.1...v49.2.0) (2020-10-14)


### Bug Fixes

* throw weaver error when trying to use GameObject param ([#395](https://github.com/MirageNet/Mirage/issues/395)) ([2726b9e](https://github.com/MirageNet/Mirage/commit/2726b9efbd4dd44d1a6b55ea53371a74682e84f1))


### Features

* display FPS every second ([ae62e35](https://github.com/MirageNet/Mirage/commit/ae62e35987676727a7e90dc29446aa1c015d0bdf))
* Headless Benchmark ([#394](https://github.com/MirageNet/Mirage/issues/394)) ([0546de9](https://github.com/MirageNet/Mirage/commit/0546de9c285c0280ff6deb307fc99f9522ebf01f))

## [49.1.1](https://github.com/MirageNet/Mirage/compare/v49.1.0...v49.1.1) (2020-10-14)


### Bug Fixes

* bug in kcp that caused invalid smoothing of rtt values ([eacfefe](https://github.com/MirageNet/Mirage/commit/eacfefe19769f2ed46bf7e0c0492a542dff43f42))

# [49.1.0](https://github.com/MirageNet/Mirage/compare/v49.0.0...v49.1.0) (2020-10-14)


### Features

* new KCP transport ([#393](https://github.com/MirageNet/Mirage/issues/393)) ([5de53e1](https://github.com/MirageNet/Mirage/commit/5de53e1fd4d3395f5c489ba248fdf4c2e8a7cafc))

# [49.0.0](https://github.com/MirageNet/Mirage/compare/v48.0.1...v49.0.0) (2020-10-14)


* Reduce allocations by using Unitask (#392) ([a45413a](https://github.com/MirageNet/Mirage/commit/a45413a3512e7fe5f365ddad3ae42795cb688984)), closes [#392](https://github.com/MirageNet/Mirage/issues/392)


### BREAKING CHANGES

* Mirage now uses UniTask

## [48.0.1](https://github.com/MirageNet/Mirage/compare/v48.0.0...v48.0.1) (2020-10-12)


### Bug Fixes

* InvalidOperationException when disconnecting ([b131eb7](https://github.com/MirageNet/Mirage/commit/b131eb7512892896c624f360fcbd104549d63aa9))

# [48.0.0](https://github.com/MirageNet/Mirage/compare/v47.0.3...v48.0.0) (2020-10-11)


* Users must initialize syncobjects (#391) ([c0e2632](https://github.com/MirageNet/Mirage/commit/c0e2632e928d8f303dd8f0324cf108a756809782)), closes [#391](https://github.com/MirageNet/Mirage/issues/391)


### BREAKING CHANGES

* You must initialize all your SyncLists

## [47.0.3](https://github.com/MirageNet/Mirage/compare/v47.0.2...v47.0.3) (2020-10-10)


### Bug Fixes

* NRE in headless server mode ([6dc8406](https://github.com/MirageNet/Mirage/commit/6dc840671ec1a43d0cd8f85f9b4aff7e36366204))

## [47.0.2](https://github.com/MirageNet/Mirage/compare/v47.0.1...v47.0.2) (2020-10-09)


### Bug Fixes

* inconsistent case in meta file ([4c50834](https://github.com/MirageNet/Mirage/commit/4c5083455c3ab3dc3b747818b4b92493b2c586a2))

## [47.0.1](https://github.com/MirageNet/Mirage/compare/v47.0.0...v47.0.1) (2020-10-08)


### Bug Fixes

* warning about tests.meta file ([148b5a9](https://github.com/MirageNet/Mirage/commit/148b5a9f567f5a3f54905ed4d6b3bf1687120707))

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-07)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* access NI on disabled objects ([#383](https://github.com/MirageNet/Mirage/issues/383)) ([0ab4c60](https://github.com/MirageNet/Mirage/commit/0ab4c6065bcd5e584e311a5558e614ffd250a5d1))
* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* Lobby comp should not reference NetworkConnection.IsReady ([#385](https://github.com/MirageNet/Mirage/issues/385)) ([6a1a190](https://github.com/MirageNet/Mirage/commit/6a1a1905bf67ede3a4dcdbb76c05c3908c004f27))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))
* nre modifying syncvars that have not been spawned ([69883c5](https://github.com/MirageNet/Mirage/commit/69883c5df0671da43bb9610b37d9535e61d95675))


### Features

* Add IncludeOwner option to SendToAll ([#387](https://github.com/MirageNet/Mirage/issues/387)) ([6b0a005](https://github.com/MirageNet/Mirage/commit/6b0a005f539b02db3a6d1b030b473d2ea2ab53d0))
* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### Performance Improvements

* Only synchronize dirty objects ([#381](https://github.com/MirageNet/Mirage/issues/381)) ([64fd6ed](https://github.com/MirageNet/Mirage/commit/64fd6ed862a15074c980c4fb81b4c99fe9698cda))
* Remove 2 messages when player connects ([#384](https://github.com/MirageNet/Mirage/issues/384)) ([c40e0fd](https://github.com/MirageNet/Mirage/commit/c40e0fd083ea80685dc6898828767f784ec147a3))


### BREAKING CHANGES

* It is no longer guaranteed that all objects are spawned before we start calling events

* fix object spawning on scene change

* remove unused variable

Co-authored-by: uwee <uweenukr@gmail.com>
* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-06)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* access NI on disabled objects ([#383](https://github.com/MirageNet/Mirage/issues/383)) ([0ab4c60](https://github.com/MirageNet/Mirage/commit/0ab4c6065bcd5e584e311a5558e614ffd250a5d1))
* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* Lobby comp should not reference NetworkConnection.IsReady ([#385](https://github.com/MirageNet/Mirage/issues/385)) ([6a1a190](https://github.com/MirageNet/Mirage/commit/6a1a1905bf67ede3a4dcdbb76c05c3908c004f27))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))
* nre modifying syncvars that have not been spawned ([69883c5](https://github.com/MirageNet/Mirage/commit/69883c5df0671da43bb9610b37d9535e61d95675))


### Features

* Add IncludeOwner option to SendToAll ([#387](https://github.com/MirageNet/Mirage/issues/387)) ([6b0a005](https://github.com/MirageNet/Mirage/commit/6b0a005f539b02db3a6d1b030b473d2ea2ab53d0))
* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### Performance Improvements

* Only synchronize dirty objects ([#381](https://github.com/MirageNet/Mirage/issues/381)) ([64fd6ed](https://github.com/MirageNet/Mirage/commit/64fd6ed862a15074c980c4fb81b4c99fe9698cda))
* Remove 2 messages when player connects ([#384](https://github.com/MirageNet/Mirage/issues/384)) ([c40e0fd](https://github.com/MirageNet/Mirage/commit/c40e0fd083ea80685dc6898828767f784ec147a3))


### BREAKING CHANGES

* It is no longer guaranteed that all objects are spawned before we start calling events

* fix object spawning on scene change

* remove unused variable

Co-authored-by: uwee <uweenukr@gmail.com>
* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-06)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* access NI on disabled objects ([#383](https://github.com/MirageNet/Mirage/issues/383)) ([0ab4c60](https://github.com/MirageNet/Mirage/commit/0ab4c6065bcd5e584e311a5558e614ffd250a5d1))
* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* Lobby comp should not reference NetworkConnection.IsReady ([#385](https://github.com/MirageNet/Mirage/issues/385)) ([6a1a190](https://github.com/MirageNet/Mirage/commit/6a1a1905bf67ede3a4dcdbb76c05c3908c004f27))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))
* nre modifying syncvars that have not been spawned ([69883c5](https://github.com/MirageNet/Mirage/commit/69883c5df0671da43bb9610b37d9535e61d95675))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### Performance Improvements

* Only synchronize dirty objects ([#381](https://github.com/MirageNet/Mirage/issues/381)) ([64fd6ed](https://github.com/MirageNet/Mirage/commit/64fd6ed862a15074c980c4fb81b4c99fe9698cda))
* Remove 2 messages when player connects ([#384](https://github.com/MirageNet/Mirage/issues/384)) ([c40e0fd](https://github.com/MirageNet/Mirage/commit/c40e0fd083ea80685dc6898828767f784ec147a3))


### BREAKING CHANGES

* It is no longer guaranteed that all objects are spawned before we start calling events

* fix object spawning on scene change

* remove unused variable

Co-authored-by: uwee <uweenukr@gmail.com>
* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-06)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* access NI on disabled objects ([#383](https://github.com/MirageNet/Mirage/issues/383)) ([0ab4c60](https://github.com/MirageNet/Mirage/commit/0ab4c6065bcd5e584e311a5558e614ffd250a5d1))
* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))
* nre modifying syncvars that have not been spawned ([69883c5](https://github.com/MirageNet/Mirage/commit/69883c5df0671da43bb9610b37d9535e61d95675))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### Performance Improvements

* Only synchronize dirty objects ([#381](https://github.com/MirageNet/Mirage/issues/381)) ([64fd6ed](https://github.com/MirageNet/Mirage/commit/64fd6ed862a15074c980c4fb81b4c99fe9698cda))
* Remove 2 messages when player connects ([#384](https://github.com/MirageNet/Mirage/issues/384)) ([c40e0fd](https://github.com/MirageNet/Mirage/commit/c40e0fd083ea80685dc6898828767f784ec147a3))


### BREAKING CHANGES

* It is no longer guaranteed that all objects are spawned before we start calling events

* fix object spawning on scene change

* remove unused variable

Co-authored-by: uwee <uweenukr@gmail.com>
* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-06)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* access NI on disabled objects ([#383](https://github.com/MirageNet/Mirage/issues/383)) ([0ab4c60](https://github.com/MirageNet/Mirage/commit/0ab4c6065bcd5e584e311a5558e614ffd250a5d1))
* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))
* nre modifying syncvars that have not been spawned ([69883c5](https://github.com/MirageNet/Mirage/commit/69883c5df0671da43bb9610b37d9535e61d95675))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### Performance Improvements

* Only synchronize dirty objects ([#381](https://github.com/MirageNet/Mirage/issues/381)) ([64fd6ed](https://github.com/MirageNet/Mirage/commit/64fd6ed862a15074c980c4fb81b4c99fe9698cda))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-05)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-05)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* Examples can exit property in client mode ([35faaf3](https://github.com/MirageNet/Mirage/commit/35faaf38eb5816f49e0c4d3d3aebff94e01f9101))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-04)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-04)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-04)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))
* networkmenu should add NSM to playerSpawner too ([c7bd0be](https://github.com/MirageNet/Mirage/commit/c7bd0be18870d4c4e4baa3f751426d159404b01a))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-04)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* adding namespace for sonar bug ([2ed0859](https://github.com/MirageNet/Mirage/commit/2ed08596489f02f3e8ff177a4f6983add4ce7774))
* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-04)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))
* examples no longer use prefabs or common files ([#378](https://github.com/MirageNet/Mirage/issues/378)) ([718ec9e](https://github.com/MirageNet/Mirage/commit/718ec9e6ec8928e19e29e619a2194ef2eb206eff))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-03)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Remove GameObject sync ([#370](https://github.com/MirageNet/Mirage/issues/370)) ([5b223fa](https://github.com/MirageNet/Mirage/commit/5b223fa31985bd07e658eb43122a4f3cd426511d))
* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* bug Client ConnectionState not set properly in Disconnect ([#369](https://github.com/MirageNet/Mirage/issues/369)) ([74298c5](https://github.com/MirageNet/Mirage/commit/74298c50b330216edbd19cf087eece910a05f656))
* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* You can no longer use gameobjects in syncvars
* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Bug Fixes

* Bug with  NI destroy order ([#374](https://github.com/MirageNet/Mirage/issues/374)) ([485f78b](https://github.com/MirageNet/Mirage/commit/485f78b0d011950bb98ebf5ed0bd12673773224b))


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [47.0.0](https://github.com/MirageNet/Mirage/compare/v46.2.0...v47.0.0) (2020-10-02)


### breaking

* Use SyncLists directly (delete overrides) ([#2307](https://github.com/MirageNet/Mirage/issues/2307)) ([fb49d19](https://github.com/MirageNet/Mirage/commit/fb49d197939e41816310694e33325c75a8fa986b)), closes [#2305](https://github.com/MirageNet/Mirage/issues/2305)


### Features

* new generic Read and Write methods for all types ([#2301](https://github.com/MirageNet/Mirage/issues/2301)) ([85252c3](https://github.com/MirageNet/Mirage/commit/85252c3d840353014f003eaa1d565eeb8635c673))
* support Jagged arrays ([0267dbe](https://github.com/MirageNet/Mirage/commit/0267dbe1f22efd9452371e5a29c2b85958ddf7e5))


### BREAKING CHANGES

* Serialize and Deserialize methods in synclists don't do anything anymore

* Remove old comment

* Fix compilatio error

# [46.2.0](https://github.com/MirageNet/Mirage/compare/v46.1.0...v46.2.0) (2020-10-02)


### Features

* Generate rw for synclist items ([518840c](https://github.com/MirageNet/Mirage/commit/518840c1a49fd5f2343122822b3fb22d600859b0))

# [46.1.0](https://github.com/MirageNet/Mirage/compare/v46.0.2...v46.1.0) (2020-09-30)


### Bug Fixes

* reduce prefab count to prevent errors when examples are missing ([a531fa0](https://github.com/MirageNet/Mirage/commit/a531fa0a3572eed5a3792fded4cd7cca65982c26))


### Features

* NetworkBehaviors can be added to child gameobjects ([#371](https://github.com/MirageNet/Mirage/issues/371)) ([266dc8d](https://github.com/MirageNet/Mirage/commit/266dc8d6f2a748483d88fc09991c9309716fc69a))

## [46.0.2](https://github.com/MirageNet/Mirage/compare/v46.0.1...v46.0.2) (2020-09-30)


### Bug Fixes

* invalid reference to UnityEditor in standalone build ([69f5be7](https://github.com/MirageNet/Mirage/commit/69f5be7523087c2ad3e5b44d33558b958f3d844f))

## [46.0.1](https://github.com/MirageNet/Mirage/compare/v46.0.0...v46.0.1) (2020-09-30)


### Bug Fixes

* NetMan cleanup and simplify ([#364](https://github.com/MirageNet/Mirage/issues/364)) ([4bfa3c6](https://github.com/MirageNet/Mirage/commit/4bfa3c6a4bafdbce65e5384c654bc7819c6ebad9))

# [46.0.0](https://github.com/MirageNet/Mirage/compare/v45.0.1...v46.0.0) (2020-09-30)


### Bug Fixes

* potential NRE with weaver errors ([9e0c18c](https://github.com/MirageNet/Mirage/commit/9e0c18c9a0881ab9df9e8e6d3ed6ae38154d4a66))


### Features

* You can use synclists directly ([#366](https://github.com/MirageNet/Mirage/issues/366)) ([ca18d11](https://github.com/MirageNet/Mirage/commit/ca18d11a7129a90166b397f03c718499e957aaf8))


### BREAKING CHANGES

* SyncList and other syncobjects no longer have override methods to serialize and deserialize data

## [45.0.1](https://github.com/MirageNet/Mirage/compare/v45.0.0...v45.0.1) (2020-09-30)


### Bug Fixes

* issue with readers and writers from other assemblies ([18f3eba](https://github.com/MirageNet/Mirage/commit/18f3eba5609e4a2fd9d668369eb7de2716f14372))

# [45.0.0](https://github.com/MirageNet/Mirage/compare/v44.3.1...v45.0.0) (2020-09-29)


* Renamed ReadMessage -> Reader ([1bb89f2](https://github.com/MirageNet/Mirage/commit/1bb89f2408c51ced2229c9d9635b7e2319b59b05))


### BREAKING CHANGES

* NetworkReader.ReadMessage renamed to NetworkReader.Read
* NetworkWriter.WriteMessage renamed to NetworkReader.Write

## [44.3.1](https://github.com/MirageNet/Mirage/compare/v44.3.0...v44.3.1) (2020-09-29)


### Bug Fixes

* generic arguments resolution ([#2300](https://github.com/MirageNet/Mirage/issues/2300)) ([8dbf467](https://github.com/MirageNet/Mirage/commit/8dbf46720e5b8fb9a0bd06af36e8bd445e772332))
* typos ([#2297](https://github.com/MirageNet/Mirage/issues/2297)) ([aba69ca](https://github.com/MirageNet/Mirage/commit/aba69ca4d00fd093a6184cec0cfdd4a688bca3fb))

# [44.3.0](https://github.com/MirageNet/Mirage/compare/v44.2.0...v44.3.0) (2020-09-29)


### Features

* support null when serializing classes ([#2290](https://github.com/MirageNet/Mirage/issues/2290)) ([513a0f9](https://github.com/MirageNet/Mirage/commit/513a0f9945ec4414b0da1f0b79f7668875d3eea1))
* Support recursive data types ([#2288](https://github.com/MirageNet/Mirage/issues/2288)) ([3ccb7d9](https://github.com/MirageNet/Mirage/commit/3ccb7d9da4a2b67124b2adbb3f1bc4d701552aa7))

# [44.2.0](https://github.com/MirageNet/Mirage/compare/v44.1.0...v44.2.0) (2020-09-28)


### Features

* Support reading and writing recursive types ([a82f3fd](https://github.com/MirageNet/Mirage/commit/a82f3fd95f308aac5ffb07f560163b84f2fe49eb))

# [44.1.0](https://github.com/MirageNet/Mirage/compare/v44.0.0...v44.1.0) (2020-09-28)


### Bug Fixes

* fixing new import ([bdd81e3](https://github.com/MirageNet/Mirage/commit/bdd81e33babe2a35b71028fac2130819ff61c4a2))


### Features

* Component based Ready system ([#358](https://github.com/MirageNet/Mirage/issues/358)) ([23b0afc](https://github.com/MirageNet/Mirage/commit/23b0afc505940117a5e9cc780d4315eba0d46cd7))

# [44.0.0](https://github.com/MirageNet/Mirage/compare/v43.9.1...v44.0.0) (2020-09-27)


### Features

* Send any data type as message ([#359](https://github.com/MirageNet/Mirage/issues/359)) ([f04e17c](https://github.com/MirageNet/Mirage/commit/f04e17cfa8421eb2f23d1f82de4945016857487a))


### BREAKING CHANGES

* IMessageBase has been removed,  you don't need to implement anything

## [43.9.1](https://github.com/MirageNet/Mirage/compare/v43.9.0...v43.9.1) (2020-09-25)


### Bug Fixes

* asmdef issues related to runtime tests ([#357](https://github.com/MirageNet/Mirage/issues/357)) ([175b6b7](https://github.com/MirageNet/Mirage/commit/175b6b78c0d0ae428873da99b9a47b994f6cbd87))

# [43.9.0](https://github.com/MirageNet/Mirage/compare/v43.8.0...v43.9.0) (2020-09-24)


### Features

* starting NetworkObjectManager ([#354](https://github.com/MirageNet/Mirage/issues/354)) ([01f3bb3](https://github.com/MirageNet/Mirage/commit/01f3bb3d02eb7462030cf1ee204ab80f3d9c1a2e))

# [43.8.0](https://github.com/MirageNet/Mirage/compare/v43.7.0...v43.8.0) (2020-09-24)


### Features

* Add support for customer INetworkConnections ([#346](https://github.com/MirageNet/Mirage/issues/346)) ([9d0b219](https://github.com/MirageNet/Mirage/commit/9d0b219e585b6c1f5747f594a46b799ab8ecbba0))

# [43.7.0](https://github.com/MirageNet/Mirage/compare/v43.6.0...v43.7.0) (2020-09-21)


### Bug Fixes

* adding error for generated read writer for abstract class ([#2191](https://github.com/MirageNet/Mirage/issues/2191)) ([a9d21ea](https://github.com/MirageNet/Mirage/commit/a9d21ea9ab28c74348437d7df899d49f913aaf30))
* adding error when Server/Client is used on abstract methods ([#1978](https://github.com/MirageNet/Mirage/issues/1978)) ([c1410b0](https://github.com/MirageNet/Mirage/commit/c1410b09248e0c18646a622bca3cc66ebae972b4))
* adding version define from v17 breaking change ([c6fa49c](https://github.com/MirageNet/Mirage/commit/c6fa49c72ada6859debf26088daed6e8f073b655))
* auto fill serialize/deserialize for classes ([#2120](https://github.com/MirageNet/Mirage/issues/2120)) ([890ee6b](https://github.com/MirageNet/Mirage/commit/890ee6b58da85f040a3157b11ea85ca89d796073)), closes [#2117](https://github.com/MirageNet/Mirage/issues/2117) [#2117](https://github.com/MirageNet/Mirage/issues/2117)
* ClientScene.localplayer is now set to null when it is destroyed ([#2227](https://github.com/MirageNet/Mirage/issues/2227)) ([5edba81](https://github.com/MirageNet/Mirage/commit/5edba81dee66eb6140c7a54bb6050779656828e1))
* fixing cloud log not using logger ([#2141](https://github.com/MirageNet/Mirage/issues/2141)) ([a124f3f](https://github.com/MirageNet/Mirage/commit/a124f3f439c914d422c774495b50ef3fbdadbfaf))
* Fixing IndexChanged hook not being called for NetworkRoomPlayer ([#2242](https://github.com/MirageNet/Mirage/issues/2242)) ([94da8ae](https://github.com/MirageNet/Mirage/commit/94da8aec343ea894a85873dc986889b7a09d09de))
* fixing NullReferenceException when loading scene ([#2240](https://github.com/MirageNet/Mirage/issues/2240)) ([5eb5ff6](https://github.com/MirageNet/Mirage/commit/5eb5ff6601102683023ae0f1a71cdc62217c2227))
* fixing unity crash on StackOverflowException ([#2146](https://github.com/MirageNet/Mirage/issues/2146)) ([ec54ee6](https://github.com/MirageNet/Mirage/commit/ec54ee6d8c0e73bbb7f59510bc7bcfa82229c4bb))
* fixing warning message for client attribute ([#2144](https://github.com/MirageNet/Mirage/issues/2144)) ([ddc6139](https://github.com/MirageNet/Mirage/commit/ddc61397257e518a4740dca154c2fe68f45e98da))
* fixing width and layout group on cloud example ([#2201](https://github.com/MirageNet/Mirage/issues/2201)) ([fc53078](https://github.com/MirageNet/Mirage/commit/fc530782ccaa12b2c412b4e0db04ce5843cbe6af))
* isServer false in OnDestroy ([#2101](https://github.com/MirageNet/Mirage/issues/2101)) ([d46469a](https://github.com/MirageNet/Mirage/commit/d46469a79c6651720cd4d4cbd49cd0d0922f16f2))
* making triggers be called right away instead on owner ([#2125](https://github.com/MirageNet/Mirage/issues/2125)) ([7604e65](https://github.com/MirageNet/Mirage/commit/7604e65c2c2cf9f1d85416284cefeb699e65d91e))
* Nested messages ([#2148](https://github.com/MirageNet/Mirage/issues/2148)) ([e4a5ce7](https://github.com/MirageNet/Mirage/commit/e4a5ce795b221b46b2f9cde01b7234f0763978b8))
* Unity Editor 2019/2020 crashes in Play Mode when resizing Editor for Macbooks with on board graphics cards. Using Metal instead of OpenGL fixes it. See also: https://forum.unity.com/threads/unity-editor-crashes-macos.739535/ ([e2fd195](https://github.com/MirageNet/Mirage/commit/e2fd19547cb5ea54e462803fbb1b1dfa802aaf36))
* weaver test for abstract methods ([#2166](https://github.com/MirageNet/Mirage/issues/2166)) ([3a276b4](https://github.com/MirageNet/Mirage/commit/3a276b4e3079c928df0577a9f1aad92080530cb6))


### Features

* Added Layer Weight to NetworkAnimator ([#2134](https://github.com/MirageNet/Mirage/issues/2134)) ([23b9fb0](https://github.com/MirageNet/Mirage/commit/23b9fb0214c2caf0b99e23a8c77e5ac8d393ec7b))
* Adding Custom Data and Custom Address fields to list server ([#2110](https://github.com/MirageNet/Mirage/issues/2110)) ([e136f48](https://github.com/MirageNet/Mirage/commit/e136f481012d093d8a176771ba7b6a846d843b10))
* adding script to help debug list server ([#2202](https://github.com/MirageNet/Mirage/issues/2202)) ([5701369](https://github.com/MirageNet/Mirage/commit/5701369e84698a6110c06e4dfb0b1db452cc7843))
* allowing lists to automatically be sent in Commands, Rpc, and Messages ([#2151](https://github.com/MirageNet/Mirage/issues/2151)) ([381e5a1](https://github.com/MirageNet/Mirage/commit/381e5a115b8944fd06fae6bf3827be206d77efb6))
* making NetworkBehaviour.IsDirty public ([#2208](https://github.com/MirageNet/Mirage/issues/2208)) ([1ade82d](https://github.com/MirageNet/Mirage/commit/1ade82d4357875fb72472fd1339c5d4ee80214d9))
* Use Server Client attribute outside of NetworkBehaviour ([#2150](https://github.com/MirageNet/Mirage/issues/2150)) ([eec49fa](https://github.com/MirageNet/Mirage/commit/eec49fafce9931727fa6dfa9019aeacee654f631))


### Performance Improvements

* **weaver:** No need to manually load mirror and unity assemblies ([#2261](https://github.com/MirageNet/Mirage/issues/2261)) ([e50ea63](https://github.com/MirageNet/Mirage/commit/e50ea6318f60af454a7021b7d8d6009a9116379d))
* adding check for no connections ([#2130](https://github.com/MirageNet/Mirage/issues/2130)) ([150b14a](https://github.com/MirageNet/Mirage/commit/150b14a2ed73bd5e23e94f8c975b0202db0ed8bf))

# [43.6.0](https://github.com/MirageNet/Mirage/compare/v43.5.0...v43.6.0) (2020-09-07)


### Features

* Remove tests from upm package ([#343](https://github.com/MirageNet/Mirage/issues/343)) ([0abbf2a](https://github.com/MirageNet/Mirage/commit/0abbf2a8b5febc3fa153778ae2c4a94a86fb58db))

# [43.5.0](https://github.com/MirageNet/Mirage/compare/v43.4.2...v43.5.0) (2020-08-31)


### Features

* New NetworkManagerHud ([#340](https://github.com/MirageNet/Mirage/issues/340)) ([267f513](https://github.com/MirageNet/Mirage/commit/267f51349d68d9607cb57dd6cbe11d5cfafb5118))

## [43.4.2](https://github.com/MirageNet/Mirage/compare/v43.4.1...v43.4.2) (2020-08-24)


### Bug Fixes

* scene change race condition. wait for server to be ready ([#339](https://github.com/MirageNet/Mirage/issues/339)) ([2ca0340](https://github.com/MirageNet/Mirage/commit/2ca0340ce1e263815b650582e2a630e6ad9f47d2))

# 1.0.0 (2020-08-23)


### breaking

* AsyncFallbackTransport -> FallbackTransport ([f8f643a](https://github.com/MirageNet/Mirage/commit/f8f643a6245777279de31dc8997a7ea84328533e))
* AsyncMultiplexTransport -> MultiplexTransport ([832b7f9](https://github.com/MirageNet/Mirage/commit/832b7f9528595e45769790c4be4fd94e873c96f4))
* remove redundant scene ready value ([#325](https://github.com/MirageNet/Mirage/issues/325)) ([6cc8f62](https://github.com/MirageNet/Mirage/commit/6cc8f6212413ccdf2a95da7ea2ef93b86fc837bf))
* Remove TargetRPC & use ClientRPC option instead ([#293](https://github.com/MirageNet/Mirage/issues/293)) ([4ace144](https://github.com/MirageNet/Mirage/commit/4ace14477d024d0ef763c0860cdb2abfde8022fd))
* Removed websocket transport ([f26159b](https://github.com/MirageNet/Mirage/commit/f26159b7b4d31d643a1dc2a28b1797bd2ad28f68))
* Rename [Command] to [ServerRpc] ([#271](https://github.com/MirageNet/Mirage/issues/271)) ([fff7459](https://github.com/MirageNet/Mirage/commit/fff7459801fc637c641757c516f85b4d685e0ad1))
* rename AsyncWsTransport -> WsTransport ([9c394bc](https://github.com/MirageNet/Mirage/commit/9c394bc96192a50ad273371b66c9289d75402dc6))
* Transports can now provide their Uri ([#1454](https://github.com/MirageNet/Mirage/issues/1454)) ([b916064](https://github.com/MirageNet/Mirage/commit/b916064856cf78f1c257f0de0ffe8c9c1ab28ce7)), closes [#38](https://github.com/MirageNet/Mirage/issues/38)


### Bug Fixes

* (again) Telepathy updated to latest version (Send SocketExceptions now disconnect the player too) ([46eddc0](https://github.com/MirageNet/Mirage/commit/46eddc01ec104f98701e5552a66728ae48d0720f))
* [#1241](https://github.com/MirageNet/Mirage/issues/1241) - Telepathy updated to latest version. All tests are passing again. Thread.Interrupt was replaced by Abort+Join. ([228b32e](https://github.com/MirageNet/Mirage/commit/228b32e1da8e407e1d63044beca0fd179f0835b4))
* [#1278](https://github.com/MirageNet/Mirage/issues/1278) - only call initial state SyncVar hooks on clients if the SyncVar value is different from the default one. ([#1414](https://github.com/MirageNet/Mirage/issues/1414)) ([a3ffd12](https://github.com/MirageNet/Mirage/commit/a3ffd1264c2ed2780e6e86ce83077fa756c01154))
* [#1359](https://github.com/MirageNet/Mirage/issues/1359). Revert "Destroy objects owned by this connection when disconnecting ([#1179](https://github.com/MirageNet/Mirage/issues/1179))" ([4cc4279](https://github.com/MirageNet/Mirage/commit/4cc4279d7ddeaf61fe300b3dc420143e63942f1f))
* [#1380](https://github.com/MirageNet/Mirage/issues/1380) - NetworkConnection.clientOwnedObjects changed from uint HashSet to NetworkIdentity HashSet for ease of use and to fix a bug where DestroyOwnedObjects wouldn't find a netId anymore in some cases. ([a71ecdb](https://github.com/MirageNet/Mirage/commit/a71ecdba4a020f9f4648b8275ec9d17b19aff55f))
* [#1515](https://github.com/MirageNet/Mirage/issues/1515) - StopHost now invokes OnServerDisconnected for the host client too ([#1601](https://github.com/MirageNet/Mirage/issues/1601)) ([678ac68](https://github.com/MirageNet/Mirage/commit/678ac68b58798816658d29be649bdaf18ad70794))
* [#1593](https://github.com/MirageNet/Mirage/issues/1593) - NetworkRoomManager.ServerChangeScene doesn't destroy the world player before replacing the connection. otherwise ReplacePlayerForConnection removes authority form a destroyed object, causing all kidns of errors. The call wasn't actually needed. ([#1594](https://github.com/MirageNet/Mirage/issues/1594)) ([347cb53](https://github.com/MirageNet/Mirage/commit/347cb5374d0cba72762e893645f076d3161aa0c5))
* [#1599](https://github.com/MirageNet/Mirage/issues/1599) - NetworkManager HUD calls StopHost/Server/Client depending on state. It does not call StopHost in all cases. ([#1600](https://github.com/MirageNet/Mirage/issues/1600)) ([8c6ae0f](https://github.com/MirageNet/Mirage/commit/8c6ae0f8b4fdafbc3abd194c081c75ee75fcfe51))
* [#1659](https://github.com/MirageNet/Mirage/issues/1659) Telepathy LateUpdate processes a limited amount of messages per tick to avoid deadlocks ([#1830](https://github.com/MirageNet/Mirage/issues/1830)) ([d3dccd7](https://github.com/MirageNet/Mirage/commit/d3dccd7a25e4b9171ac04e43a05954b56caefd4b))
* [#718](https://github.com/MirageNet/Mirage/issues/718) remove Tests folder from unitypackage ([#827](https://github.com/MirageNet/Mirage/issues/827)) ([7e487af](https://github.com/MirageNet/Mirage/commit/7e487afe512de9dc8a0d699693884cbfc9c7be7e))
* [#840](https://github.com/MirageNet/Mirage/issues/840) by allowing Mirror to respect the forceHidden flag ([#893](https://github.com/MirageNet/Mirage/issues/893)) ([3ec3d02](https://github.com/MirageNet/Mirage/commit/3ec3d023621e121aed302222fdb6e35ed5ca92b2))
* ArraySegment<byte> work in Messages ([#919](https://github.com/MirageNet/Mirage/issues/919)) ([981ba7c](https://github.com/MirageNet/Mirage/commit/981ba7c2b3a64ebd9e1405e5182daa030886d792))
* code generation works with il2cpp again ([#1056](https://github.com/MirageNet/Mirage/issues/1056)) ([8738562](https://github.com/MirageNet/Mirage/commit/87385628f0836109fb009b1f912575c5c8145005))
* do not accumulate changes if there are no observers fixes [#963](https://github.com/MirageNet/Mirage/issues/963) ([#964](https://github.com/MirageNet/Mirage/issues/964)) ([64a0468](https://github.com/MirageNet/Mirage/commit/64a046803ada79f7602f6e6fda21d821909fbc98))
* don't convert null arrays to empty array ([#913](https://github.com/MirageNet/Mirage/issues/913)) ([dd758ca](https://github.com/MirageNet/Mirage/commit/dd758cac0578629e351bf60d25733d788bd0f668))
* Don't increment counter in Awake ([#971](https://github.com/MirageNet/Mirage/issues/971)) ([45b7118](https://github.com/MirageNet/Mirage/commit/45b711804b1159e390910227796f312f74351025))
* don't use obsolete method ([12437ba](https://github.com/MirageNet/Mirage/commit/12437ba9c2ccc72998f2dd895b888d8eaa66e7a6))
* error with missing assemblies ([#1052](https://github.com/MirageNet/Mirage/issues/1052)) ([00eb23a](https://github.com/MirageNet/Mirage/commit/00eb23aa01210860b9c8ab253929563f695714d7)), closes [#1051](https://github.com/MirageNet/Mirage/issues/1051)
* Fix error scene error message in host mode ([838d4f0](https://github.com/MirageNet/Mirage/commit/838d4f019f60e202b3795a01e4297c2d3efe6bca))
* hooks in host mode can call each other ([#1017](https://github.com/MirageNet/Mirage/issues/1017)) ([f27fd0b](https://github.com/MirageNet/Mirage/commit/f27fd0bdc570ec3ceeef433eb4991beb487d2ddb))
* invalid scene id in 2019.1 by ignoring prefabs in NetworkScenePostProcess ([203a823](https://github.com/MirageNet/Mirage/commit/203a823b19b6e31a50cd193a7bd58c33a73960f2))
* ListServer Ping not found in WebGL ([6c4b34b](https://github.com/MirageNet/Mirage/commit/6c4b34ba065429b57ccfed71ac0adc325de19809))
* maintain Unity's copyright notice ([#961](https://github.com/MirageNet/Mirage/issues/961)) ([7718955](https://github.com/MirageNet/Mirage/commit/771895509a358286377ea3d391ca45f8c0a3b48d))
* missed ushort reader/writer ([74faf2a](https://github.com/MirageNet/Mirage/commit/74faf2a95b5a3e551e7ae344d5772e10ee198318))
* Mono.CecilX namespace to work around Unity 2019 Cecil namespace collision ([#832](https://github.com/MirageNet/Mirage/issues/832)) ([5262592](https://github.com/MirageNet/Mirage/commit/52625923b2d408018f61506ef93b15487764d095))
* NetworkManager OnValidate wouldn't properly save the automatically added Transport before because Undo.RecordObject is needed for that now. ([524abfc](https://github.com/MirageNet/Mirage/commit/524abfc5e8c881d2088a7f9f8bbf07c0371785cf))
* pack works if message is boxed ([55c9bb6](https://github.com/MirageNet/Mirage/commit/55c9bb625aa06ab83c2350b483eaca09b463db0a))
* properly stop client and server in OnApplicationQuit so that clients still get a chance to send then 'quit' packet instead of just timing out. Also fixes a bug where OnStopServer/OnStopClient were not called when stopping the Editor. ([#936](https://github.com/MirageNet/Mirage/issues/936)) ([d6389e6](https://github.com/MirageNet/Mirage/commit/d6389e68be3a951f3ddb9aa51c57a0e3c788e5f6))
* Rebuild observers when we switch scenes, fixes [#978](https://github.com/MirageNet/Mirage/issues/978) ([#1016](https://github.com/MirageNet/Mirage/issues/1016)) ([6dd1350](https://github.com/MirageNet/Mirage/commit/6dd135088bd0b3858dabf5d195d14d85879ead6d))
* Respect Player Prefab Position & Rotation ([#825](https://github.com/MirageNet/Mirage/issues/825)) ([8ebda0f](https://github.com/MirageNet/Mirage/commit/8ebda0fa21b430ce1394eba8e7eeafa56d9290f3))
* Revert "NetworkIdentity.observers dictionary is always created, but always empty on clients. Gets rid of all null checks." to fix server-only bug not allowing movement on client, e.g. in uMMORPG ([f56507f](https://github.com/MirageNet/Mirage/commit/f56507f2fc9f36ca9f8e1df9a7a437a97b416d54))
* Revert "refactor:  consolidate prefab and spawn handlers ([#817](https://github.com/MirageNet/Mirage/issues/817))" to fix a bug where if editor=host, build=client, we receive scene object not found when walking out of and back into an observer's range ([1f07af0](https://github.com/MirageNet/Mirage/commit/1f07af0cae7b41cd52df621f1b5cfcefc77efdfa))
* SceneId was not set to 0 for prefab variants ([#976](https://github.com/MirageNet/Mirage/issues/976)) ([#977](https://github.com/MirageNet/Mirage/issues/977)) ([2ca2c48](https://github.com/MirageNet/Mirage/commit/2ca2c488acc3966ef7dc67cb530c5db9eaa8b0ea))
* suppress warning on standalone build [#1053](https://github.com/MirageNet/Mirage/issues/1053) ([4ef680a](https://github.com/MirageNet/Mirage/commit/4ef680a47483890d6576784ca880f2b3536b6b7f))
* Sync full netAnimator for new clients, fix [#980](https://github.com/MirageNet/Mirage/issues/980) ([#1110](https://github.com/MirageNet/Mirage/issues/1110)) ([db8310f](https://github.com/MirageNet/Mirage/commit/db8310f8385ec45c28356e59d1ba4ef8f4c9ab47))
* Telepathy already supports IPv6, but can no also connect to IPv4-only servers again (e.g. Mirror Booster) ([488446a](https://github.com/MirageNet/Mirage/commit/488446ae040246a631f8921a4cd5bdbb6a6e54d1))
* Telepathy fix a bug where calling Disconnect while connecting to a dead end would freeze Unity because .Join would wait forever. Interrupt fixes it. ([3831cbd](https://github.com/MirageNet/Mirage/commit/3831cbddbea7d98fe8a871133a0ba2bf14f22df0))
* **weaver:** fix [#796](https://github.com/MirageNet/Mirage/issues/796), reload assemblies after initial import ([#1106](https://github.com/MirageNet/Mirage/issues/1106)) ([d91b387](https://github.com/MirageNet/Mirage/commit/d91b387bb29bdba06a62a718533db5c0fe52f642))
* [#573](https://github.com/MirageNet/Mirage/issues/573) (part 1) NetworkScenePostProcess handles NetworkIdentities of all scenes except DontDestroyOnLoad. this way it works for additively loaded scenes too. ([c1af84e](https://github.com/MirageNet/Mirage/commit/c1af84e6bf61ff919607c66affc4a1bd0dc3fb26))
* [#573](https://github.com/MirageNet/Mirage/issues/573) (part 2) NetworkManager detects additive scene loads and respawns objects on server/client again ([e521a20](https://github.com/MirageNet/Mirage/commit/e521a200523b25a874a3cbc743d2a9d98a88b238))
* [#573](https://github.com/MirageNet/Mirage/issues/573) NullReferenceException because destroyed NetworkIdentities were never removed from sceneIds dict ([a2d6317](https://github.com/MirageNet/Mirage/commit/a2d6317642a24571a63bbeade5fe8898f56c1c3e))
* [#609](https://github.com/MirageNet/Mirage/issues/609) by spawning observers in NetworkServer.AddPlayerForConnection after setting the controller. There is no point in trying to spawn with a null controller in SetReady, because by definition no one can observer something that is null. ([#623](https://github.com/MirageNet/Mirage/issues/623)) ([5c00577](https://github.com/MirageNet/Mirage/commit/5c00577746f83eadd948383dd478360e96634ea4))
* [#640](https://github.com/MirageNet/Mirage/issues/640) InternalReplacePlayerForConnection calls SpawnObserversForConnection now too ([bdf12c8](https://github.com/MirageNet/Mirage/commit/bdf12c85d01b20f2a0edc0767454401a6c5a1aad))
* [#651](https://github.com/MirageNet/Mirage/issues/651) GetSceneAt assumes default scene ([#654](https://github.com/MirageNet/Mirage/issues/654)) ([65eaba1](https://github.com/MirageNet/Mirage/commit/65eaba1fe059db159b5fdb1427dc8b783f5720e0))
* [#652](https://github.com/MirageNet/Mirage/issues/652) OnPostProcessScene includes disabled NetworkIdentities in scene ([ee2ace8](https://github.com/MirageNet/Mirage/commit/ee2ace8e428d67309dc219109be5853b1a9b67df))
* [#679](https://github.com/MirageNet/Mirage/issues/679) package for unity ([4a6a4df](https://github.com/MirageNet/Mirage/commit/4a6a4df61bc65c2065cb1150cd05e15528db6b66))
* [#679](https://github.com/MirageNet/Mirage/issues/679) unity package ([9895647](https://github.com/MirageNet/Mirage/commit/98956472969ba8ae1c66d255f1094140aeb275f0))
* [#692](https://github.com/MirageNet/Mirage/issues/692) by always adding connectionToClient when rebuilding observers ([ab44ac8](https://github.com/MirageNet/Mirage/commit/ab44ac8f8bad4749e300ba8c2db4593fcff5474f))
* [#723](https://github.com/MirageNet/Mirage/issues/723) - NetworkTransform teleport works properly now ([fd7dc5e](https://github.com/MirageNet/Mirage/commit/fd7dc5e226a76b27250fb503a98f23eb579387f8))
* [#791](https://github.com/MirageNet/Mirage/issues/791) corrected assembly paths passed to weaver ([#803](https://github.com/MirageNet/Mirage/issues/803)) ([3ba546e](https://github.com/MirageNet/Mirage/commit/3ba546e133dae6dd2762d7c4f719f61e90554473))
* [#791](https://github.com/MirageNet/Mirage/issues/791) stack overflow in the weaver ([#792](https://github.com/MirageNet/Mirage/issues/792)) ([7b57830](https://github.com/MirageNet/Mirage/commit/7b57830e6c8e3b9abf470cf1029eb2e4aba914ee))
* add Changelog metadata fix [#31](https://github.com/MirageNet/Mirage/issues/31) ([c67de22](https://github.com/MirageNet/Mirage/commit/c67de2216aa331de10bba2e09ea3f77e6b1caa3c))
* add client only test for FinishLoadScene ([#262](https://github.com/MirageNet/Mirage/issues/262)) ([50e7fa6](https://github.com/MirageNet/Mirage/commit/50e7fa6e287fee09afbe36a51575f41c4bd50736))
* Add missing channelId to NetworkConnectionToClient.Send calls ([#1509](https://github.com/MirageNet/Mirage/issues/1509)) ([b8bcd9a](https://github.com/MirageNet/Mirage/commit/b8bcd9ad25895eee940a3daaf6fe7ed82eaf76ac))
* add NetworkManager.StartClientUri test ([#2095](https://github.com/MirageNet/Mirage/issues/2095)) ([12827f6](https://github.com/MirageNet/Mirage/commit/12827f65a906232da55ca226129423a5bd806d23))
* add NRE short circuit for scene change ([#335](https://github.com/MirageNet/Mirage/issues/335)) ([7afbe57](https://github.com/MirageNet/Mirage/commit/7afbe57ff3779ba33d225ab604f1477a883badd7))
* add tests for NetworkTransform and NetworkRigidbody ([#273](https://github.com/MirageNet/Mirage/issues/273)) ([e9621dd](https://github.com/MirageNet/Mirage/commit/e9621ddebd50637680fad8fe743c7c99afea3f84))
* Add the transport first so NetworkManager doesn't add Telepathy in OnValidate ([bdec276](https://github.com/MirageNet/Mirage/commit/bdec2762821dc657e8450b576422fcf1f0f69cdf))
* Added ClientOnly check ([fb927f8](https://github.com/MirageNet/Mirage/commit/fb927f814110327867821dac8b0d69ca4251d4f6))
* Added LogFilter.Debug check in a few places ([#1575](https://github.com/MirageNet/Mirage/issues/1575)) ([3156504](https://github.com/MirageNet/Mirage/commit/31565042708ec768fcaafe9986968d095a3a1419))
* added new read/write symbol params ([#806](https://github.com/MirageNet/Mirage/issues/806)) ([3a50ca6](https://github.com/MirageNet/Mirage/commit/3a50ca6352761b47464d0bc7721aa6556d664661))
* Added WriteBytesAndSize tests, and fixed the function to be pedantic. ([#773](https://github.com/MirageNet/Mirage/issues/773)) ([72e4e55](https://github.com/MirageNet/Mirage/commit/72e4e55778edc0acc4ef3546f69c984f0f392867))
* Adding warning when adding handler with RegisterSpawnHandler if assetid already exists ([#1819](https://github.com/MirageNet/Mirage/issues/1819)) ([7f26329](https://github.com/MirageNet/Mirage/commit/7f26329e2db9d00da04bed40399af053436218bd))
* Adding warning when adding prefab with RegisterPrefab if assetid already exists ([#1828](https://github.com/MirageNet/Mirage/issues/1828)) ([9f59e0c](https://github.com/MirageNet/Mirage/commit/9f59e0c439707d66409a617b8f209187856eaf5f))
* addingNetwork rigidbody icon and AddComponentMenu attribute ([#2051](https://github.com/MirageNet/Mirage/issues/2051)) ([ab1b92f](https://github.com/MirageNet/Mirage/commit/ab1b92f74b56787feb7c6fde87c0b9838b8d9d0f))
* Additive scene can respawn objects safely ([#1270](https://github.com/MirageNet/Mirage/issues/1270)) ([8899d20](https://github.com/MirageNet/Mirage/commit/8899d207127be86a01cb859d0539c7927ebc2f67))
* additive scene example ([9fa0169](https://github.com/MirageNet/Mirage/commit/9fa016957f487526ab44d443aabfe58fcc69518a))
* Additive Scene Example was missing Player Auth on movement. ([#234](https://github.com/MirageNet/Mirage/issues/234)) ([09bbd68](https://github.com/MirageNet/Mirage/commit/09bbd686e6c294f24412b35785cfa7a5aa47b5f2))
* additive scene player movement is client authoritative ([e683a92](https://github.com/MirageNet/Mirage/commit/e683a92b081c989314c11e48fb21ee0096465797))
* AdditiveSceneExample missing comp and assignments ([#267](https://github.com/MirageNet/Mirage/issues/267)) ([ab394b8](https://github.com/MirageNet/Mirage/commit/ab394b8f7e823b8c3882de35eaa54c05fbd9316e))
* Allow sync objects to be re-used ([#1744](https://github.com/MirageNet/Mirage/issues/1744)) ([58c89a3](https://github.com/MirageNet/Mirage/commit/58c89a3d32daedc9b6670ed0b5eb1f8753c902e2)), closes [#1714](https://github.com/MirageNet/Mirage/issues/1714)
* Allowing overrides for virtual commands to call base method ([#1944](https://github.com/MirageNet/Mirage/issues/1944)) ([b92da91](https://github.com/MirageNet/Mirage/commit/b92da91d7a04f41098615ff2e2a35cf7ff479201))
* assign spawn locations and fix null refs in example ([#242](https://github.com/MirageNet/Mirage/issues/242)) ([3adf343](https://github.com/MirageNet/Mirage/commit/3adf3438578ff304f1216022aae8e043c52cd71d))
* AsyncTcp now exits normally when client disconnects ([#141](https://github.com/MirageNet/Mirage/issues/141)) ([8896c4a](https://github.com/MirageNet/Mirage/commit/8896c4afa2f937839a54dc71fbe578b9c636f393))
* attributes causing a NRE ([#69](https://github.com/MirageNet/Mirage/issues/69)) ([fc99c67](https://github.com/MirageNet/Mirage/commit/fc99c67712564e2d983674b37858591903294f1a))
* auto reference mirrorng assembly ([93f8688](https://github.com/MirageNet/Mirage/commit/93f8688b39822bb30ed595ca36f44a8a556bec85))
* Avoid FindObjectOfType when not needed ([#66](https://github.com/MirageNet/Mirage/issues/66)) ([e2a4afd](https://github.com/MirageNet/Mirage/commit/e2a4afd0b9ca8dea720acb9c558efca210bd8e71))
* benchmark examples ([b221b74](https://github.com/MirageNet/Mirage/commit/b221b74beae2ee56f6fe536963b17d0aff10c5d8))
* better error for Command, ClientRpc and TargetRpc marked as abstract ([#1947](https://github.com/MirageNet/Mirage/issues/1947)) ([62257d8](https://github.com/MirageNet/Mirage/commit/62257d8c4fc307ba3e23fbd01dcc950515c31e79))
* Better errors when trying to replace existing assetid ([#1827](https://github.com/MirageNet/Mirage/issues/1827)) ([822b041](https://github.com/MirageNet/Mirage/commit/822b04155def9859b24900c6e55a4253f85ebb3f))
* build in IL2CPP ([#1524](https://github.com/MirageNet/Mirage/issues/1524)) ([59faa81](https://github.com/MirageNet/Mirage/commit/59faa819262a166024b16d854e410c7e51763e6a)), closes [#1519](https://github.com/MirageNet/Mirage/issues/1519) [#1520](https://github.com/MirageNet/Mirage/issues/1520)
* call callback after update dictionary in host ([#1476](https://github.com/MirageNet/Mirage/issues/1476)) ([1736bb0](https://github.com/MirageNet/Mirage/commit/1736bb0c42c0d2ad341e31a645658722de3bfe07))
* Call hooks when initializing objects OnStartServer on host ([#1249](https://github.com/MirageNet/Mirage/issues/1249)) ([7aa7815](https://github.com/MirageNet/Mirage/commit/7aa7815754bb3be1071884d6583076badc7cae59))
* call Obsoleted OnStartClient ([#681](https://github.com/MirageNet/Mirage/issues/681)) ([8dea50e](https://github.com/MirageNet/Mirage/commit/8dea50ed18ca45e72cc5e5addf1cc28c7ab08746))
* call OnStartClient only once in room ([#1264](https://github.com/MirageNet/Mirage/issues/1264)) ([4d373c5](https://github.com/MirageNet/Mirage/commit/4d373c5071c45201146333f40d3fbc5d1fa8ec26))
* call the virtual OnRoomServerDisconnect before the base ([e6881ef](https://github.com/MirageNet/Mirage/commit/e6881ef007f199efca3c326ead258f0c350ffb47))
* calling base method when the first base class did not have the virtual method ([#2014](https://github.com/MirageNet/Mirage/issues/2014)) ([4af72c3](https://github.com/MirageNet/Mirage/commit/4af72c3a63e72dac6b3bab193dc58bfa3c44a975))
* calling Connect and Authenticate handler twice ([#102](https://github.com/MirageNet/Mirage/issues/102)) ([515f5a1](https://github.com/MirageNet/Mirage/commit/515f5a15cd5be984f8cb4f94d3be0a0ac919eb63))
* calling syncvar hook when not connected yet ([#77](https://github.com/MirageNet/Mirage/issues/77)) ([e64727b](https://github.com/MirageNet/Mirage/commit/e64727b74bcbb1adfcd8f5efbf96066443254dff))
* cap spawned to match client ([#301](https://github.com/MirageNet/Mirage/issues/301)) ([7d1571a](https://github.com/MirageNet/Mirage/commit/7d1571ab5a9eaf31cd64bff2bc47158c0e1e6ff6))
* changing namespace to match folder name ([#2037](https://github.com/MirageNet/Mirage/issues/2037)) ([e36449c](https://github.com/MirageNet/Mirage/commit/e36449cb22d8a2dede0133cf229bc12885c36bdb))
* chat example ([e6e10a7](https://github.com/MirageNet/Mirage/commit/e6e10a7108bc01e3bd0c208734c97c945003ff86))
* chat example works ([0609d50](https://github.com/MirageNet/Mirage/commit/0609d50d9b93afd3b42d97ddcd00d32e8aaa0db5))
* check event prefix ([7417b68](https://github.com/MirageNet/Mirage/commit/7417b6867175f0a54db56efc4387d2d24b0b37dd))
* Check SceneManager GetSceneByName and GetSceneByPath ([#1684](https://github.com/MirageNet/Mirage/issues/1684)) ([e7cfd5a](https://github.com/MirageNet/Mirage/commit/e7cfd5a498c7359636cd109fe586fce1771bada2))
* Clean up roomSlots on clients in NetworkRoomPlayer ([5032ceb](https://github.com/MirageNet/Mirage/commit/5032ceb00035679e0b80f59e91131cdfa8e0b1bb))
* Cleaning up network objects when server stops ([#1864](https://github.com/MirageNet/Mirage/issues/1864)) ([4c25122](https://github.com/MirageNet/Mirage/commit/4c25122958978557173ec37ca400c47b2d8e834f))
* cleanup the server even after error ([#255](https://github.com/MirageNet/Mirage/issues/255)) ([7bd015e](https://github.com/MirageNet/Mirage/commit/7bd015eac1b77f0ad5974abb5c4c87a5d3da7b6d))
* clear all message handlers on Shutdown ([#1829](https://github.com/MirageNet/Mirage/issues/1829)) ([a6ab352](https://github.com/MirageNet/Mirage/commit/a6ab3527acb9af8f6a68f0151e5231e4ee1a98e9))
* client being disconnected twice ([#132](https://github.com/MirageNet/Mirage/issues/132)) ([36bb3a2](https://github.com/MirageNet/Mirage/commit/36bb3a2418bcf41fd63d1fc79e8a5173e4b0bc51))
* client disconnected on server event never raised ([#133](https://github.com/MirageNet/Mirage/issues/133)) ([9d9efa0](https://github.com/MirageNet/Mirage/commit/9d9efa0e31cbea4d90d7408ae6b3742151b49a21))
* ClientRpc methods now work accross assemblies ([#1129](https://github.com/MirageNet/Mirage/issues/1129)) ([13dbcb9](https://github.com/MirageNet/Mirage/commit/13dbcb9f35d64285258e748ca1fd5c4daac97a16)), closes [#1128](https://github.com/MirageNet/Mirage/issues/1128)
* ClientRPC should skip first arg only if set as Connection ([#315](https://github.com/MirageNet/Mirage/issues/315)) ([168e622](https://github.com/MirageNet/Mirage/commit/168e6222e759016b588e994b76d2f134c9224b0b))
* ClientSceneManager should be responsible for its own cleanup ([#298](https://github.com/MirageNet/Mirage/issues/298)) ([92ab3ff](https://github.com/MirageNet/Mirage/commit/92ab3ffe265e72b3c012dc44075f6e9752323984))
* Cmds can be called from child classes in other assemblies ([d8a98d8](https://github.com/MirageNet/Mirage/commit/d8a98d8d996aeded693223b00b90f2eea5084c11)), closes [#1108](https://github.com/MirageNet/Mirage/issues/1108)
* code smell rename Ready ([#256](https://github.com/MirageNet/Mirage/issues/256)) ([6d92d14](https://github.com/MirageNet/Mirage/commit/6d92d1482cdd31fa663f7475f103476c65b7d875))
* Command and Rpc debugging information ([#1551](https://github.com/MirageNet/Mirage/issues/1551)) ([658847b](https://github.com/MirageNet/Mirage/commit/658847b096571eb7cf14e824ea76359576121e63)), closes [#1550](https://github.com/MirageNet/Mirage/issues/1550)
* comment punctuation ([4d827cd](https://github.com/MirageNet/Mirage/commit/4d827cd9f60e4fb7cd6524d78ca26ea1d88a1eff))
* compilation error ([df7baa4](https://github.com/MirageNet/Mirage/commit/df7baa4db0d347ee69c17bad9f9e56ccefb54fab))
* compilation error ([dc74256](https://github.com/MirageNet/Mirage/commit/dc74256fc380974ad6df59b5d1dee3884b879bd7))
* compilation error on standalone build ([bb70bf9](https://github.com/MirageNet/Mirage/commit/bb70bf963459be02a79c2c40cb7dfb8f10d2b92d))
* compilation issue after merge from mirror ([daf07be](https://github.com/MirageNet/Mirage/commit/daf07bea83c9925bd780e23de53dd50604e8a999))
* compilation issues ([22bf925](https://github.com/MirageNet/Mirage/commit/22bf925f1ebf018b9ea33df22294fb9205574fa5))
* comply with MIT license in upm package ([b879bef](https://github.com/MirageNet/Mirage/commit/b879bef4295e48c19d96a1d45536a11ea47380f3))
* Decouple ChatWindow from player ([#1429](https://github.com/MirageNet/Mirage/issues/1429)) ([42a2f9b](https://github.com/MirageNet/Mirage/commit/42a2f9b853667ef9485a1d4a31979fcf1153c0d7))
* Default port is 7777 ([960c39d](https://github.com/MirageNet/Mirage/commit/960c39da90db156cb58d4765695664f0c084b39a))
* destroy owned objects ([#1352](https://github.com/MirageNet/Mirage/issues/1352)) ([d7a58d2](https://github.com/MirageNet/Mirage/commit/d7a58d25d4aa79d31dfc3fadfa4f68a5fdb895e6)), closes [#1346](https://github.com/MirageNet/Mirage/issues/1346) [#1206](https://github.com/MirageNet/Mirage/issues/1206) [#1351](https://github.com/MirageNet/Mirage/issues/1351)
* Destroyed NetMan due to singleton collision must not continue. ([#1636](https://github.com/MirageNet/Mirage/issues/1636)) ([d2a58a4](https://github.com/MirageNet/Mirage/commit/d2a58a4c251c97cdb38c88c9a381496b3078adf8))
* disconnect even if there is an exception ([#152](https://github.com/MirageNet/Mirage/issues/152)) ([2eb9de6](https://github.com/MirageNet/Mirage/commit/2eb9de6b470579b6de75853ba161b3e7a36de698))
* disconnect properly from the server ([c89bb51](https://github.com/MirageNet/Mirage/commit/c89bb513e536f256e55862b723487bb21281572e))
* disconnect transport without domain reload ([20785b7](https://github.com/MirageNet/Mirage/commit/20785b740e21fb22834cd01d7d628e127df6b80d))
* Do not call InternalAddPlayer twice ([#1246](https://github.com/MirageNet/Mirage/issues/1246)) ([7119dd1](https://github.com/MirageNet/Mirage/commit/7119dd15f8e90e6d6bc929a9e4633082615d0023))
* don't call hook in host if no change,  fixes [#1142](https://github.com/MirageNet/Mirage/issues/1142) ([#1143](https://github.com/MirageNet/Mirage/issues/1143)) ([d8ce80f](https://github.com/MirageNet/Mirage/commit/d8ce80fe0edb243a71d35bdae657805b18a8a85e))
* don't call OnStartLocalPlayer twice ([#1263](https://github.com/MirageNet/Mirage/issues/1263)) ([c753089](https://github.com/MirageNet/Mirage/commit/c7530894788bb843b0f424e8f25029efce72d8ca))
* Don't call RegisterClientMessages every scene change ([#1865](https://github.com/MirageNet/Mirage/issues/1865)) ([05c119f](https://github.com/MirageNet/Mirage/commit/05c119f505390094c8f33e11568d40117360c49e))
* Don't call RegisterClientMessages twice ([#1842](https://github.com/MirageNet/Mirage/issues/1842)) ([2a08aac](https://github.com/MirageNet/Mirage/commit/2a08aac7cb8887934eb7eb8c232ce07976defe35))
* don't crash when stopping the client ([f584388](https://github.com/MirageNet/Mirage/commit/f584388a16e746ac5c3000123a02a5c77387765e))
* Don't destroy the player twice ([#1709](https://github.com/MirageNet/Mirage/issues/1709)) ([cbc2a47](https://github.com/MirageNet/Mirage/commit/cbc2a4772921e01db17033075fa9f7d8cb7e6faf))
* Don't disconnect host ([#608](https://github.com/MirageNet/Mirage/issues/608)) ([c1707e5](https://github.com/MirageNet/Mirage/commit/c1707e5917c4058a9641376d028f5feff51128cc))
* Don't give host player authority by default ([#1158](https://github.com/MirageNet/Mirage/issues/1158)) ([1fc1ed2](https://github.com/MirageNet/Mirage/commit/1fc1ed27081413e48a7898fc185cb238ed0074dc))
* don't report error when stopping the server ([c965d4b](https://github.com/MirageNet/Mirage/commit/c965d4b0ff32288ebe4e5c63a43e32559203deb1))
* Don't set asset id for scene objects ([7e40232](https://github.com/MirageNet/Mirage/commit/7e4023246bc2e6a11909a7c3730ae05ee56d1369))
* Don't set framerate in host mode ([4644bc4](https://github.com/MirageNet/Mirage/commit/4644bc4b7730d4aefae833fb59264230026bb4d0))
* Don't throw exception getting address ([7df3ce3](https://github.com/MirageNet/Mirage/commit/7df3ce37d1a23b8137119015276436a741b7cd9d))
* Dont allow null connections ([#323](https://github.com/MirageNet/Mirage/issues/323)) ([44fef7e](https://github.com/MirageNet/Mirage/commit/44fef7ec7bd6ae0772414ff28bb78bf42a6b4c92))
* dont allow set of networkSceneName directly ([#2100](https://github.com/MirageNet/Mirage/issues/2100)) ([df16a7d](https://github.com/MirageNet/Mirage/commit/df16a7d3ccfddcf3aa1a68fe0965757d91363e16))
* dont directly set NetworkSceneName ([#297](https://github.com/MirageNet/Mirage/issues/297)) ([bd043a3](https://github.com/MirageNet/Mirage/commit/bd043a3001775fe32da558e17566b61c5694ee7c))
* dont register client scene handlers while host ([#296](https://github.com/MirageNet/Mirage/issues/296)) ([37c8ddd](https://github.com/MirageNet/Mirage/commit/37c8ddd87595143149af942dc7e5654de3eef424))
* Draw SyncVar label for Unity objects inline  ([#1291](https://github.com/MirageNet/Mirage/issues/1291)) ([a0425e4](https://github.com/MirageNet/Mirage/commit/a0425e4e84cb08c3fd8d7e7fe07a230579d0c0c7))
* Dummy file for SyncListStructProcessor.cs ([#798](https://github.com/MirageNet/Mirage/issues/798)) ([75e4f15](https://github.com/MirageNet/Mirage/commit/75e4f159e516f8f3b04b6f1a2c77898de8c7c7b5))
* Eliminate NetworkAnimator SetTrigger double firing on Host ([#1723](https://github.com/MirageNet/Mirage/issues/1723)) ([e5b728f](https://github.com/MirageNet/Mirage/commit/e5b728fed515ab679ad1e4581035d32f6c187a98))
* empty scene name isn't considered as empty ([ec3a939](https://github.com/MirageNet/Mirage/commit/ec3a93945b5b52a77fd745b39e1e821db721768d))
* error when there are no network behaviors ([#1303](https://github.com/MirageNet/Mirage/issues/1303)) ([dbe0643](https://github.com/MirageNet/Mirage/commit/dbe064393a6573bcb213b628ec53b547d04891cc))
* examples now work with prefabs in NC ([df4149b](https://github.com/MirageNet/Mirage/commit/df4149b8fea9f174742d20f600ef5261058e5300))
* examples run in background ([#233](https://github.com/MirageNet/Mirage/issues/233)) ([4755650](https://github.com/MirageNet/Mirage/commit/47556500eed7c0e2719e41c0e996925ddf1799bb))
* Fallback and Multiplex now disable their transports when they are disabled  ([#2048](https://github.com/MirageNet/Mirage/issues/2048)) ([61d44b2](https://github.com/MirageNet/Mirage/commit/61d44b2d80c9616f784e855131ba6d1ee8a30136))
* FinishLoadSceneHost calls FinishStart host which now calls StartHostClient AFTER server online scene was loaded. Previously there was a race condition where StartHostClient was called immediately in StartHost, before the scene change even finished. This was still from UNET. ([df9c29a](https://github.com/MirageNet/Mirage/commit/df9c29a6b3f9d0c8adbaff5a500e54abddb303b3))
* first connection id = 1 ([#60](https://github.com/MirageNet/Mirage/issues/60)) ([891dab9](https://github.com/MirageNet/Mirage/commit/891dab92d065821ca46b47ef2d3eb27124058d74))
* fix adding and saving Components ([2de7ecd](https://github.com/MirageNet/Mirage/commit/2de7ecd93029bf5fd2fbb04ad4e47936cbb802cc))
* fix release pipeline ([2a3db0b](https://github.com/MirageNet/Mirage/commit/2a3db0b398cd641c3e1ba65a32b34822e9c9169f))
* Fix Room Slots for clients ([#1439](https://github.com/MirageNet/Mirage/issues/1439)) ([268753c](https://github.com/MirageNet/Mirage/commit/268753c3bd0a9c0695d8d4510a129685be364a11))
* Fixed Capitalization ([c45deb8](https://github.com/MirageNet/Mirage/commit/c45deb808e8e01a7b697e703be783aea2799d4d1))
* Fixed ClienRpc typos ([e946c79](https://github.com/MirageNet/Mirage/commit/e946c79194dd9618992a4136daed4b79f60671a2))
* Fixed NetworkRoomManager Template ([1662c5a](https://github.com/MirageNet/Mirage/commit/1662c5a139363dbe61784bb90ae6544ec74478c3))
* Fixed toc link ([3a0c7fb](https://github.com/MirageNet/Mirage/commit/3a0c7fb1ecd9d8715e797a7665ab9a6a7cb19e2a))
* Fixing ClientScene UnregisterPrefab ([#1815](https://github.com/MirageNet/Mirage/issues/1815)) ([9270765](https://github.com/MirageNet/Mirage/commit/9270765bebf45c34a466694473b43c6d802b99d9))
* fixing cloud scripts not pinging api ([#2097](https://github.com/MirageNet/Mirage/issues/2097)) ([8e545ac](https://github.com/MirageNet/Mirage/commit/8e545ac46863e4fbe874c70bf9559c9b12de83d4))
* Fixing SyncVars not serializing when OnSerialize is overridden ([#1671](https://github.com/MirageNet/Mirage/issues/1671)) ([c66c5a6](https://github.com/MirageNet/Mirage/commit/c66c5a6dcc6837c840e6a51435b88fde10322297))
* folders for meta files no longer in the codebase ([#237](https://github.com/MirageNet/Mirage/issues/237)) ([192fd16](https://github.com/MirageNet/Mirage/commit/192fd1645986c515a804a01e0707c78241882676))
* hasAuthority is now visible in all overrides ([#1251](https://github.com/MirageNet/Mirage/issues/1251)) ([2f19c7c](https://github.com/MirageNet/Mirage/commit/2f19c7ca8961e9d99794e6053abfa88263dea89d)), closes [#1250](https://github.com/MirageNet/Mirage/issues/1250)
* headless build ([7864e8d](https://github.com/MirageNet/Mirage/commit/7864e8d6f4a0952ef3114daac11842e4ee0a7a58))
* headless build ([ab47a45](https://github.com/MirageNet/Mirage/commit/ab47a45d08f4e9a82a5cd26f913f4871d46dd484))
* Host Player Ready Race Condition ([#1498](https://github.com/MirageNet/Mirage/issues/1498)) ([4c4a52b](https://github.com/MirageNet/Mirage/commit/4c4a52bff95e7c56f065409b1399955813f3e145))
* If socket is undefined it will return false. See [#1486](https://github.com/MirageNet/Mirage/issues/1486) ([#2017](https://github.com/MirageNet/Mirage/issues/2017)) ([4ffff19](https://github.com/MirageNet/Mirage/commit/4ffff192a69108b993cf963cfdece47b14ffdbf2))
* Improved error checking for ClientScene.RegisterPrefab ([#1823](https://github.com/MirageNet/Mirage/issues/1823)) ([a0aa4f9](https://github.com/MirageNet/Mirage/commit/a0aa4f9c1425d4eca3fe08cd2d87361f092ded6f))
* Improved error checking for ClientScene.RegisterPrefab with handler ([#1841](https://github.com/MirageNet/Mirage/issues/1841)) ([54071da](https://github.com/MirageNet/Mirage/commit/54071da3afb18d6289de5d0e41dc248e21088641))
* Invoke server.Disconnected before identity is removed for its conn ([#165](https://github.com/MirageNet/Mirage/issues/165)) ([b749c4b](https://github.com/MirageNet/Mirage/commit/b749c4ba126266a1799059f7fb407b6bcaa2bbd9))
* isClient now reports true onStartServer in host mode ([#1252](https://github.com/MirageNet/Mirage/issues/1252)) ([d00c95b](https://github.com/MirageNet/Mirage/commit/d00c95bb55eedceb4f0811de54604c960c9352fe))
* isLocalPlayer is true during OnStartClient for Player ([#1255](https://github.com/MirageNet/Mirage/issues/1255)) ([fb26d00](https://github.com/MirageNet/Mirage/commit/fb26d0023f2ecfcec710d365f23a19036e3f87b2)), closes [#1250](https://github.com/MirageNet/Mirage/issues/1250)
* isLocalPlayer works in host mode onStartServer ([#1253](https://github.com/MirageNet/Mirage/issues/1253)) ([9acde20](https://github.com/MirageNet/Mirage/commit/9acde20b0a4237936fc028747551204208ac9677)), closes [#1250](https://github.com/MirageNet/Mirage/issues/1250)
* it is not safe to modify this outside this class ([bc7a961](https://github.com/MirageNet/Mirage/commit/bc7a961e4db0b269e36cd15f1492410932ff7f5b))
* list server logs properly when disconnected ([f02d317](https://github.com/MirageNet/Mirage/commit/f02d3174db39498749a6663984dcb4bea8ac342e))
* Lobby Remove button not showing for P1 when Server Only ([377c47c](https://github.com/MirageNet/Mirage/commit/377c47ce74808dc7d2871eac80c4cd040894821b))
* Make assembly definition 2018.4 compatible ([99ecc9e](https://github.com/MirageNet/Mirage/commit/99ecc9ec770aa89d5087e5b95821ff3e2e444085))
* make build pass ([08df6d0](https://github.com/MirageNet/Mirage/commit/08df6d0694b10475301b21915214cbbfbbec2826))
* Make SendToReady non-ambiguous ([#1578](https://github.com/MirageNet/Mirage/issues/1578)) ([b627779](https://github.com/MirageNet/Mirage/commit/b627779acd68b2acfcaf5eefc0d3864dcce57fc7))
* making weaver include public fields in base classes in auto generated Read/Write ([#1977](https://github.com/MirageNet/Mirage/issues/1977)) ([3db57e5](https://github.com/MirageNet/Mirage/commit/3db57e5f61ac0748d3a6296d8ea44c202830796f))
* Mark weaver as failed if serializing invalid type ([03c767d](https://github.com/MirageNet/Mirage/commit/03c767db6aea583bb00e56b1ac74bf29e8169a91))
* Message base class not being Serialized if processed in the wrong order ([#2023](https://github.com/MirageNet/Mirage/issues/2023)) ([3418fa2](https://github.com/MirageNet/Mirage/commit/3418fa210602cf1a9b9331b198ac47d8a3cabe69))
* Mirage works with 2019.2 ([9f35d6b](https://github.com/MirageNet/Mirage/commit/9f35d6be427843aa7dd140cde32dd843c62147ce))
* Misc code smells ([#2094](https://github.com/MirageNet/Mirage/issues/2094)) ([e4cc85c](https://github.com/MirageNet/Mirage/commit/e4cc85c413eed01950bf9dddf0bdac2afd8ab479))
* Misc Code Smells ([#257](https://github.com/MirageNet/Mirage/issues/257)) ([278a127](https://github.com/MirageNet/Mirage/commit/278a1279dabefe04b0829015841de68b41e60a7b))
* Misc code smells ([#269](https://github.com/MirageNet/Mirage/issues/269)) ([23dcca6](https://github.com/MirageNet/Mirage/commit/23dcca61ff7c41e8b9f61579605fd56ee82c70e0))
* missing meta ([87ace4d](https://github.com/MirageNet/Mirage/commit/87ace4dda09331968cc9d0185ce1de655f5dfb15))
* move listserver classes into package ([2668b17](https://github.com/MirageNet/Mirage/commit/2668b17162e5a9fbdce2cfc776f80044f9f4d0d9))
* move NetworkStreamExtension in a namespace ([12de543](https://github.com/MirageNet/Mirage/commit/12de543aa4da49edf3c14c69388f739ad315c84d))
* moved SpawnObjects call for hostmode to after LocalClient Connected ([#317](https://github.com/MirageNet/Mirage/issues/317)) ([1423a6d](https://github.com/MirageNet/Mirage/commit/1423a6d0160c5d14a0ee6bad84973df73956fc05))
* movement in room demo ([49f7904](https://github.com/MirageNet/Mirage/commit/49f7904b4a83fc5318031d273cb10a4b87af2172))
* MultiplexTransport GetMaxMessageSize NullReferenceException when called on server. And fixes potential exploits / out of sync issues where clients with different transports might see different game states because of different max message sizes. ([#1332](https://github.com/MirageNet/Mirage/issues/1332)) ([b3127be](https://github.com/MirageNet/Mirage/commit/b3127beb89c20447bf8044fd3bae71ae04f553e7))
* Network rigidbody fixes ([#2050](https://github.com/MirageNet/Mirage/issues/2050)) ([0c30d33](https://github.com/MirageNet/Mirage/commit/0c30d3398aaabcbf094a88a9c9c77ab4d5062acf))
* NetworkBehaviour.SyncVarGameObjectEqual made protected again so that Weaver finds it again ([165a1dd](https://github.com/MirageNet/Mirage/commit/165a1dd94cd1a7bebc3365c4f40f968f500043a5))
* NetworkBehaviour.SyncVarNetworkIdentityEqual made protected again so that Weaver finds it again ([20a2d09](https://github.com/MirageNet/Mirage/commit/20a2d09d07ab2c47a204e5d583b538a92f72231e))
* NetworkBehaviourInspector wouldn't show SyncMode if syncvars / syncobjects were only private ([ed572da](https://github.com/MirageNet/Mirage/commit/ed572da6a07791a243715796304c0f7695792225))
* NetworkClient.Shutdown calls ClientScene.Shutdown again to properly clean up client scene. ClientScene only cleans up itself without touching transport or NetworkIdentities (fixes the bug where the player object wouldn't be destroyed after calling StopClient) ([fb716df](https://github.com/MirageNet/Mirage/commit/fb716df12997417ce41a063508937d68a75991bf))
* NetworkConnectionEvent should be serialized in editor ([0e756ce](https://github.com/MirageNet/Mirage/commit/0e756cec06c5fda9eb4b5c8aa9de093c32b0315c))
* NetworkIdentity.OnStartLocalPlayer catches exceptions now too. fixes a potential bug where an exception in PlayerInventory.OnStartLocalPlayer would cause PlayerEquipment.OnStartLocalPlayer to not be called ([5ed5f84](https://github.com/MirageNet/Mirage/commit/5ed5f844090442e16e78f466f7a785881283fbd4))
* NetworkIdentity.RebuildObservers: added missing null check for observers coming from components that implement OnRebuildObservers. Previously this caused a NullReferenceException. ([a5f495a](https://github.com/MirageNet/Mirage/commit/a5f495a77485b972cf39f8a42bae6d7d852be38c))
* NetworkIdentity.SetClientOwner: overwriting the owner was still possible even though it shouldn't be. all caller functions double check and return early if it already has an owner, so we should do the same here. ([548db52](https://github.com/MirageNet/Mirage/commit/548db52fdf224f06ba9d8b2d75460d31181b7066))
* NetworkRoomManager.minPlayers is now protected so it's available for derived classes. ([3179f08](https://github.com/MirageNet/Mirage/commit/3179f08e3dc11340227a57da0104b1c8d1d7b45d))
* NetworkServer.SpawnObjects: return false if server isn't running ([d4d524d](https://github.com/MirageNet/Mirage/commit/d4d524dad2a0a9d89538e6212dceda6bea71d2b7))
* NetworkTransform clientAuthority works again via clientAuthority option that is configurable in inspector. this had to be fixed after we removed local authority. ([d712cd0](https://github.com/MirageNet/Mirage/commit/d712cd03039aea92083b1be97197f6272b2296b5))
* NinjaWS code smells ([#272](https://github.com/MirageNet/Mirage/issues/272)) ([71d9428](https://github.com/MirageNet/Mirage/commit/71d942804c0d404f287dc51b7bcdd1fcc39bcee8))
* no longer requires hook to be the first overload in a class ([#1913](https://github.com/MirageNet/Mirage/issues/1913)) ([0348699](https://github.com/MirageNet/Mirage/commit/03486997fb0abb91d14f233658d375f21afbc3e3))
* non ready connections should not observe objects ([1352334](https://github.com/MirageNet/Mirage/commit/135233474752373b473b6094fe52bcb3ab3c4eae))
* not removing server if id is empty ([#2078](https://github.com/MirageNet/Mirage/issues/2078)) ([f717945](https://github.com/MirageNet/Mirage/commit/f7179455256bb7341ffa9e6921fe0de50498150a))
* NRE on gamemanager in scene ([#268](https://github.com/MirageNet/Mirage/issues/268)) ([58a124a](https://github.com/MirageNet/Mirage/commit/58a124a99c267091142f00adc7f8898160a9dd97))
* NRE when destroying all objects ([#85](https://github.com/MirageNet/Mirage/issues/85)) ([71e78a7](https://github.com/MirageNet/Mirage/commit/71e78a7f5e15f99af89cd15c1ddd8a975e463916))
* NS call SpawnObjects. No NetMan dependency for spawning objects ([#300](https://github.com/MirageNet/Mirage/issues/300)) ([e1bb8de](https://github.com/MirageNet/Mirage/commit/e1bb8deba81713c8998cf47b1ec4b8b870fc55eb))
* null reference exception ([7ce95c5](https://github.com/MirageNet/Mirage/commit/7ce95c5cea58446549d9a282b48c2e8b3f7c8323))
* OnClientEnterRoom should only fire on clients ([d9b7bb7](https://github.com/MirageNet/Mirage/commit/d9b7bb735729e68ae399e1175d6c485a873b379e))
* OnClientReady is called and passed the appropriate ready state value in NetworkLobbyPlayer ([#618](https://github.com/MirageNet/Mirage/issues/618)) ([c9eac57](https://github.com/MirageNet/Mirage/commit/c9eac57ce858d5977a03979e7514f9833a958d3c))
* OnSetHostVisibility can now check if it has authority ([888e46c](https://github.com/MirageNet/Mirage/commit/888e46c6850c9d32c6428f72d0dddf5f7e8a8564))
* Optional Server or Client for PlayerSpawner ([#231](https://github.com/MirageNet/Mirage/issues/231)) ([3fa5f89](https://github.com/MirageNet/Mirage/commit/3fa5f89d8c934b233330efe52b42e59198a920cb))
* overriden hooks are invoked (fixes [#1581](https://github.com/MirageNet/Mirage/issues/1581)) ([#1584](https://github.com/MirageNet/Mirage/issues/1584)) ([cf55333](https://github.com/MirageNet/Mirage/commit/cf55333a072c0ffe36a2972ca0a5122a48d708d0))
* pass the correct connection to TargetRpcs ([#146](https://github.com/MirageNet/Mirage/issues/146)) ([9df2f79](https://github.com/MirageNet/Mirage/commit/9df2f798953f78de113ef6fa9fea111b03a52cd0))
* Pass the name of the invoking class and desired command when an object has no authority. ([#1216](https://github.com/MirageNet/Mirage/issues/1216)) ([701f4f4](https://github.com/MirageNet/Mirage/commit/701f4f41838a01d9268335d16380f871abaf8cf5))
* port network discovery ([d6a1154](https://github.com/MirageNet/Mirage/commit/d6a1154e98c52e7873411ce9d7b87f7b294dc436))
* Potential DOS attack by sending invalid UTF8 byte sequences  ([#727](https://github.com/MirageNet/Mirage/issues/727)) ([3cee3ab](https://github.com/MirageNet/Mirage/commit/3cee3abc48fa58ab2bdb6affc8cbd4ae8b4fa815))
* Potential DOS attack on server by sending packed ulongs when packed uints are expected. ([#730](https://github.com/MirageNet/Mirage/issues/730)) ([015d0d5](https://github.com/MirageNet/Mirage/commit/015d0d508e193a694b254c182dcb0a906fe1f3bc))
* potential exploits / out of sync issues where clients with different transports might see different game states because of different max message sizes when using FallbackTransport. ([#1331](https://github.com/MirageNet/Mirage/issues/1331)) ([5449840](https://github.com/MirageNet/Mirage/commit/54498403a540db62b3ac1994494ff071327330c9))
* potential null reference exception with debug logging ([33493a0](https://github.com/MirageNet/Mirage/commit/33493a0137a899754c433c428b13e6f6c621300b))
* Prevent Compiler Paradox ([#1145](https://github.com/MirageNet/Mirage/issues/1145)) ([fd43c67](https://github.com/MirageNet/Mirage/commit/fd43c67d6866ede681024d3753b17ab5427e16f4))
* Prevent Double Call of NetworkServer.Destroy ([#1554](https://github.com/MirageNet/Mirage/issues/1554)) ([2d1b142](https://github.com/MirageNet/Mirage/commit/2d1b142276193be1e93a3a3f6ce482c87a134a2c))
* Prevent host client redundantly changing to offline scene ([b4511a0](https://github.com/MirageNet/Mirage/commit/b4511a0637958f10f4a482364c654d1d9add5ef2))
* prevent NRE when operating as a separated client and server ([#283](https://github.com/MirageNet/Mirage/issues/283)) ([e10e198](https://github.com/MirageNet/Mirage/commit/e10e198b4865fc8c941244c3e368eebc6cf73179))
* properly detect NT rotation ([#1516](https://github.com/MirageNet/Mirage/issues/1516)) ([f0a993c](https://github.com/MirageNet/Mirage/commit/f0a993c1064384e0b3bd690d4d66be38875ed50e))
* race condition closing tcp connections ([717f1f5](https://github.com/MirageNet/Mirage/commit/717f1f5ad783e13a6d55920e454cb91f380cd621))
* Re-enable transport if aborting additive load/unload ([#1683](https://github.com/MirageNet/Mirage/issues/1683)) ([bc37497](https://github.com/MirageNet/Mirage/commit/bc37497ac963bb0f2820b103591afd05177d078d))
* register prefab error with same guid ([#2092](https://github.com/MirageNet/Mirage/issues/2092)) ([984eb73](https://github.com/MirageNet/Mirage/commit/984eb73ea495cf876446a21775fde5c33119695b))
* release job requires node 10 ([3f50e63](https://github.com/MirageNet/Mirage/commit/3f50e63bc32f4942e1c130c681dabd34ae81b117))
* release unitypackage ([#1355](https://github.com/MirageNet/Mirage/issues/1355)) ([d0cc669](https://github.com/MirageNet/Mirage/commit/d0cc6690178df0af02be7bfd1fa9aacd037c57be))
* remove customHandling as its no longer used ([#265](https://github.com/MirageNet/Mirage/issues/265)) ([dbd9d84](https://github.com/MirageNet/Mirage/commit/dbd9d84ee46ac90a8d78daba0c23fc9be71ca77d))
* Remove leftover AddPlayer methods now that extraData is gone ([#1751](https://github.com/MirageNet/Mirage/issues/1751)) ([2d006fe](https://github.com/MirageNet/Mirage/commit/2d006fe7301eb8a0194af9ce9244988a6877f8f0))
* remove pause network comment and log ([#238](https://github.com/MirageNet/Mirage/issues/238)) ([1a8c09d](https://github.com/MirageNet/Mirage/commit/1a8c09d8a5a8cf70508d4e42e4912e3989478ff1))
* Remove RoomPlayer from roomSlots on Disconnect ([2a2f76c](https://github.com/MirageNet/Mirage/commit/2a2f76c263093c342f307856e400aeabbedc58df))
* remove samples from upm package ([#25](https://github.com/MirageNet/Mirage/issues/25)) ([a71e21f](https://github.com/MirageNet/Mirage/commit/a71e21fe6953f6edf54fed3499801e271e2447f3))
* remove scriptableobject error Tests ([479b78b](https://github.com/MirageNet/Mirage/commit/479b78bf3cabe93938bf61b7f8fd63ba46da0f4a))
* remove tests from npm package ([#32](https://github.com/MirageNet/Mirage/issues/32)) ([5ed9b4f](https://github.com/MirageNet/Mirage/commit/5ed9b4f1235d5d1dc54c3f50bb1aeefd5dbe3038))
* remove Tests from UPM ([#33](https://github.com/MirageNet/Mirage/issues/33)) ([8f42af0](https://github.com/MirageNet/Mirage/commit/8f42af0a3992cfa549eb404ad9f9693101895ce9))
* remove Tests from upm package ([#34](https://github.com/MirageNet/Mirage/issues/34)) ([8d8ea0f](https://github.com/MirageNet/Mirage/commit/8d8ea0f10743044e4a9a3d6c5b9f9973cf48e28b))
* remove unused code ([#308](https://github.com/MirageNet/Mirage/issues/308)) ([554d2c5](https://github.com/MirageNet/Mirage/commit/554d2c5030a9ff1ebcd9ca17ed7d673865709a1c))
* remove unused events ([#334](https://github.com/MirageNet/Mirage/issues/334)) ([c20f6de](https://github.com/MirageNet/Mirage/commit/c20f6de07ff97960a8cf9972fbb4d4d13b507b3b))
* Removed NetworkClient.Update because NetworkManager does it in LateUpdate ([984945e](https://github.com/MirageNet/Mirage/commit/984945e482529bfc03bf735562f3eff297a1bad4))
* Removed NetworkServer.Listen because HostSetup does that ([cf6823a](https://github.com/MirageNet/Mirage/commit/cf6823acb5151d5bc9beca2b0911a99dfbcd4472))
* Removed unnecessary registration of player prefab in NetworkRoomManager ([b2f52d7](https://github.com/MirageNet/Mirage/commit/b2f52d78921ff0136c74bbed0980e3aaf5e2b379))
* Removed unused variable ([ae3dc04](https://github.com/MirageNet/Mirage/commit/ae3dc04fb999c3b7133589ab631c1d23f1a8bdde))
* renaming call/invoke prefix for SyncEvent ([#2089](https://github.com/MirageNet/Mirage/issues/2089)) ([18d6957](https://github.com/MirageNet/Mirage/commit/18d695744f7c253d749792e4f9f8759ef16fcbab)), closes [#2088](https://github.com/MirageNet/Mirage/issues/2088)
* replace player (remove authority by default) ([#1261](https://github.com/MirageNet/Mirage/issues/1261)) ([ad724fe](https://github.com/MirageNet/Mirage/commit/ad724fe23c4776855ee1a2a22b53ae59ddac8992)), closes [#1257](https://github.com/MirageNet/Mirage/issues/1257) [#1257](https://github.com/MirageNet/Mirage/issues/1257) [#1257](https://github.com/MirageNet/Mirage/issues/1257)
* Replaced Icosphere with centered pivot ([1dc0d98](https://github.com/MirageNet/Mirage/commit/1dc0d9837458c0403916476805f58442ff87e364))
* ReplacePlayer now calls OnStartLocalPlayer ([#1280](https://github.com/MirageNet/Mirage/issues/1280)) ([0e1bc81](https://github.com/MirageNet/Mirage/commit/0e1bc8110fb3cc4e162464a2e080eac6c70ab95e)), closes [#962](https://github.com/MirageNet/Mirage/issues/962)
* Replacing ClearDelegates with RemoveDelegates for test ([#1971](https://github.com/MirageNet/Mirage/issues/1971)) ([927c4de](https://github.com/MirageNet/Mirage/commit/927c4dede5930b320537150466e05112ebe70c3e))
* Report correct channel to profiler in SendToObservers ([0b84d4c](https://github.com/MirageNet/Mirage/commit/0b84d4c5e1b8455e32eeb4d4c00b60bbc1301436))
* reset buffer every time ([a8a62a6](https://github.com/MirageNet/Mirage/commit/a8a62a64b6fa67223505505c1225269d3a047a92))
* return & continue on separate line ([#1504](https://github.com/MirageNet/Mirage/issues/1504)) ([61fdd89](https://github.com/MirageNet/Mirage/commit/61fdd892d9e6882e1e51094a2ceddfadc8fd1ebc))
* Revert "NetworkClient.Shutdown: call ClientScene.Shutdown, otherwise it's never called" - caused client's player to not be removed from scene after disconnecting ([13bb748](https://github.com/MirageNet/Mirage/commit/13bb7486034b72e899365f1b0aed3707a3ac0cb1))
* Room example to use new override ([e1d1d41](https://github.com/MirageNet/Mirage/commit/e1d1d41ed69f192b5fb91f92dcdeae1ee057d38f))
* rooms demo ([44598e5](https://github.com/MirageNet/Mirage/commit/44598e58325c877bd6b53ee5a77dd95e421ec404))
* Round Robin Spawning by Hierarchy Order ([#790](https://github.com/MirageNet/Mirage/issues/790)) ([531e202](https://github.com/MirageNet/Mirage/commit/531e202bbec43d855b0ba24e445588fda2f08102)), closes [#724](https://github.com/MirageNet/Mirage/issues/724)
* SceneManager Exceptions and Tests ([#287](https://github.com/MirageNet/Mirage/issues/287)) ([388d218](https://github.com/MirageNet/Mirage/commit/388d21872bb8b4c7f9d3742ecfa9b857ee734dfa))
* SendToAll sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4b90aaf](https://github.com/MirageNet/Mirage/commit/4b90aafe12970e00949ee43b07b9edd5213745da))
* SendToObservers missing result variable ([9c09c26](https://github.com/MirageNet/Mirage/commit/9c09c26a5cd28cadae4049fea71cddc38c00cf79))
* SendToObservers sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4267983](https://github.com/MirageNet/Mirage/commit/426798313920d23548048aa1c678167fd9b45cbd))
* SendToReady sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4596b19](https://github.com/MirageNet/Mirage/commit/4596b19dd959722d5dc659552206fe90c015fb01))
* set authority when replacing the player ([2195fee](https://github.com/MirageNet/Mirage/commit/2195fee81c455ac6c2ea7cca28290fbda6f30260))
* Set syncvar variables after calling the hook ([#659](https://github.com/MirageNet/Mirage/issues/659)) ([2d63ee1](https://github.com/MirageNet/Mirage/commit/2d63ee13180a54d06ce68b641f35ee2a7702cffa))
* set version number ([#1338](https://github.com/MirageNet/Mirage/issues/1338)) ([0d1d7b5](https://github.com/MirageNet/Mirage/commit/0d1d7b5a1c0e6d94c5749a94aa7b75f2c9a2ca0d))
* show private serializable fields in network behavior inspector ([#1557](https://github.com/MirageNet/Mirage/issues/1557)) ([b8c87d9](https://github.com/MirageNet/Mirage/commit/b8c87d9053e7fd7c3b69bcf1d4179e6e4c9bc4a6))
* smell cleanup left if bug. repaired with parenthesis. ([#275](https://github.com/MirageNet/Mirage/issues/275)) ([dd52be3](https://github.com/MirageNet/Mirage/commit/dd52be3bb9406de7b2527c72fce90c9ed6c9d5bf))
* Spawn Handler Order ([#223](https://github.com/MirageNet/Mirage/issues/223)) ([8674274](https://github.com/MirageNet/Mirage/commit/86742740ef2707f420d5cb6aeeb257bf07511f0b)), closes [#222](https://github.com/MirageNet/Mirage/issues/222)
* spawnwithauthority works again in host mode ([5b04836](https://github.com/MirageNet/Mirage/commit/5b04836bb220b8fc0a8c3d0a3636966af3c538f2))
* stack overflow getting logger ([55e075c](https://github.com/MirageNet/Mirage/commit/55e075c872a076f524ec62f44d81df17819e81ba))
* Telepathy forgot to set socket options for accepted clients on the server ([22931fc](https://github.com/MirageNet/Mirage/commit/22931fcd84e402a60b74f5261313c830913754fc))
* Telepathy updated to latest version (IPv6 fix again) ([535b4d4](https://github.com/MirageNet/Mirage/commit/535b4d40fa50cec9abfac37c61aaf207ecbb43b9))
* Telepathy updated to latest version (Send SocketExceptions now disconnect the player too) ([98d3fb0](https://github.com/MirageNet/Mirage/commit/98d3fb0c31b7ac8bd27ec176ebdf7fb19908d472))
* Telepathy updated to latest version: Correctly support IPv4 and IPv6 sockets ([2761ff2](https://github.com/MirageNet/Mirage/commit/2761ff23f459b5647a5700c9b9b29abdcff13f97))
* Telepathy updated to latest version. connectionId counter is properly reset after stopping server. ([abf06df](https://github.com/MirageNet/Mirage/commit/abf06df25d932d3cfb016e2da0bb5e4ee72d259d))
* TelepathyTransport.ToString UWP exception ([8a190bf](https://github.com/MirageNet/Mirage/commit/8a190bfd176f043322097e64bd041e80e38cc6d2))
* update NetworkIdentityEditor FindProperty to renamed variables ([a2cc14b](https://github.com/MirageNet/Mirage/commit/a2cc14bd202311aa36e61804183c983c6df8c7b4))
* Updated Telepathy to latest version to fix IPAddress.Parse error for "localhost" ([cc6e4f6](https://github.com/MirageNet/Mirage/commit/cc6e4f696dbc462c3880a2c9fc73163d88351b5a))
* workaround for [#791](https://github.com/MirageNet/Mirage/issues/791) ([5c850aa](https://github.com/MirageNet/Mirage/commit/5c850aa9ca5b480449c453aa14191aeb9998e6cb))
* **serialization:** Added NetworkWriter tests, found and fixed a bug in Write(Ray). ([#769](https://github.com/MirageNet/Mirage/issues/769)) ([99c8f5c](https://github.com/MirageNet/Mirage/commit/99c8f5c356d2e3bd298fbd3508a3ed2abcb04351))
* **tests:** Added missing SyncListByteValid test file ([#634](https://github.com/MirageNet/Mirage/issues/634)) ([b0af876](https://github.com/MirageNet/Mirage/commit/b0af87622159ceca9aebf4d939a3b7ad733fbe4f))
* **weaver:** [#696](https://github.com/MirageNet/Mirage/issues/696) detect .mystruct = new MyStruct() changes with syncvars ([#702](https://github.com/MirageNet/Mirage/issues/702)) ([053949b](https://github.com/MirageNet/Mirage/commit/053949b7d2e1e3178025a75cddb6e47b83cdbd48))
* **weaver:** fix [#706](https://github.com/MirageNet/Mirage/issues/706) find system dlls ([#729](https://github.com/MirageNet/Mirage/issues/729)) ([53be9b6](https://github.com/MirageNet/Mirage/commit/53be9b6d9949645d5334690961ff31f90065a93a))
* **websocket:** [#829](https://github.com/MirageNet/Mirage/issues/829) fix InvalidOperationException with wss:// ([#830](https://github.com/MirageNet/Mirage/issues/830)) ([2d682b5](https://github.com/MirageNet/Mirage/commit/2d682b5fad2811d3838e8d61ccaea381cc218bb2))
* **websocket:** Remove send queues (they never worked) and SSL (temporarily) ([#879](https://github.com/MirageNet/Mirage/issues/879)) ([3c60b08](https://github.com/MirageNet/Mirage/commit/3c60b087627175833c616619941722927aa9cd5d))
* **websocket:** Use a buffer for most WS messages in WebGL client resulting in 0 alloc for most messages ([#848](https://github.com/MirageNet/Mirage/issues/848)) ([8967a20](https://github.com/MirageNet/Mirage/commit/8967a20244a2e16e3861d60c1d13c9e808248607))
* Stop calling ClientDisconnect on host ([#597](https://github.com/MirageNet/Mirage/issues/597)) ([b67b3e4](https://github.com/MirageNet/Mirage/commit/b67b3e43049405808fe123276b3637c625b0ca9b))
* StopHost with offline scene calls scene change twice ([#1409](https://github.com/MirageNet/Mirage/issues/1409)) ([a0c96f8](https://github.com/MirageNet/Mirage/commit/a0c96f85189bfc9b5a936a8a33ebda34b460f17f))
* Suppress warning ([fffd462](https://github.com/MirageNet/Mirage/commit/fffd462df8cc1c0265890cdfa4ceb3e24606b1c1))
* Suspend server transport while changing scenes ([#1169](https://github.com/MirageNet/Mirage/issues/1169)) ([e8fac8a](https://github.com/MirageNet/Mirage/commit/e8fac8aba5c570edfb3346c1f68ad9e5fd3b1176))
* sync events can not have the same name if they are in different classes ([#2054](https://github.com/MirageNet/Mirage/issues/2054)) ([c91308f](https://github.com/MirageNet/Mirage/commit/c91308fb0461e54292940ce6fa42bb6cd9800d89))
* syncvars in commands work again ([#1131](https://github.com/MirageNet/Mirage/issues/1131)) ([c24a73f](https://github.com/MirageNet/Mirage/commit/c24a73f6c9bbe25e98a6708f05b89a63dfc54b74))
* syntax error in release job ([2eeaea4](https://github.com/MirageNet/Mirage/commit/2eeaea41bc81cfe0c191b39da912adc565e11ec7))
* TargetRpc now works accross assemblies ([#1130](https://github.com/MirageNet/Mirage/issues/1130)) ([5ecd646](https://github.com/MirageNet/Mirage/commit/5ecd646134791c80d8b53759de0d8aafddc31aeb)), closes [#1128](https://github.com/MirageNet/Mirage/issues/1128)
* tcp server Tests ([3b95477](https://github.com/MirageNet/Mirage/commit/3b954777f16a41469d953e3744c5d68554e3d200))
* Telepathy reverted to older version to fix freezes when calling Client.Disconnect on some platforms like Windows 10 ([d0d77b6](https://github.com/MirageNet/Mirage/commit/d0d77b661cd07e25887f0e2f4c2d72b4f65240a2))
* Telepathy updated to latest version: protect against allocation attacks via MaxMessageSize. Can be configured in the TelepathyTransport component now. ([67d715f](https://github.com/MirageNet/Mirage/commit/67d715fe7416e790bcddcd4e23bb2cb8fbbc54e8))
* Telepathy updated to latest version. Threads are closed properly now. ([4007423](https://github.com/MirageNet/Mirage/commit/4007423db28f7044f6aa97b108a6bfbe3f2d46a9))
* Telepathy works on .net core again ([cb3d9f0](https://github.com/MirageNet/Mirage/commit/cb3d9f0d08a961b345ce533d1ce64602f7041e1c))
* the Room scene references other scenes ([9b60871](https://github.com/MirageNet/Mirage/commit/9b60871e2ea1a2912c0af3d95796660fc04dc569))
* there is no lobby example ([b1e05ef](https://github.com/MirageNet/Mirage/commit/b1e05efb19984ce615d20a16a6c4b09a8897da6a))
* ui bug where additive button is not reset ([#311](https://github.com/MirageNet/Mirage/issues/311)) ([5effce9](https://github.com/MirageNet/Mirage/commit/5effce9abcea0274412cb97100e1f06e4ae01028))
* update interfaces for recent changes that were missed ([#309](https://github.com/MirageNet/Mirage/issues/309)) ([a17e760](https://github.com/MirageNet/Mirage/commit/a17e760e36d581ba964120af11678b66a1248ecc))
* Updated NetworkRoomPlayer inspector and doc and image ([a4ffcbe](https://github.com/MirageNet/Mirage/commit/a4ffcbe280e2e2318d7cd2988379af74f0d32021))
* Use big endian for packet size ([1ddcbec](https://github.com/MirageNet/Mirage/commit/1ddcbec93509e14169bddbb2a38a7cf0d53776e4))
* Use path instead of name in Room Example ([5d4bc47](https://github.com/MirageNet/Mirage/commit/5d4bc47d46098f920f9e3468d0f276e336488e42))
* Use ReplaceHandler instead of RegisterHandler in NetworkManager ([ffc276c](https://github.com/MirageNet/Mirage/commit/ffc276cb79c4202964275642097451b78a817c8a))
* version file ([#1337](https://github.com/MirageNet/Mirage/issues/1337)) ([ed7e509](https://github.com/MirageNet/Mirage/commit/ed7e509ed6f77f2e694966a1c21130e3488f443d))
* weaver Cmd/Rpc/Target prefix check is no longer trash ([#707](https://github.com/MirageNet/Mirage/issues/707)) ([699a261](https://github.com/MirageNet/Mirage/commit/699a261e91078b65f3fb1a51a5838b05be2c87d6))
* weaver now processes multiple SyncEvent per class ([#2055](https://github.com/MirageNet/Mirage/issues/2055)) ([b316b35](https://github.com/MirageNet/Mirage/commit/b316b35d46868a7e11c7b2005570efeec843efe1))
* weaver support array of custom types ([#1470](https://github.com/MirageNet/Mirage/issues/1470)) ([d0b0bc9](https://github.com/MirageNet/Mirage/commit/d0b0bc92bc33ff34491102a2f9e1855f2a5ed476))
* weaver syncLists now checks for SerializeItem in base class ([#1768](https://github.com/MirageNet/Mirage/issues/1768)) ([1af5b4e](https://github.com/MirageNet/Mirage/commit/1af5b4ed2f81b4450881fb11fa9b4b7e198274cb))
* webgl build fix [#1136](https://github.com/MirageNet/Mirage/issues/1136) ([#1137](https://github.com/MirageNet/Mirage/issues/1137)) ([c85d0df](https://github.com/MirageNet/Mirage/commit/c85d0df5332c63c0e0107e6c99cea5de37c087fc))
* Websockets Transport now handles being disabled for scene changes ([#1994](https://github.com/MirageNet/Mirage/issues/1994)) ([5480a58](https://github.com/MirageNet/Mirage/commit/5480a583e13b9183a3670450af639f4e766cc358))
* WebSockets: Force KeepAliveInterval to Zero ([9a42fe3](https://github.com/MirageNet/Mirage/commit/9a42fe334251852ab12e721db72cb12e98de82e8))
* when modifying a prefab, Unity calls OnValidate for all scene objects based on that prefab, which caused Mirror to reset the sceneId because we only checked if a prefab is currently edited, not if THIS prefab is currently edited ([db99dd7](https://github.com/MirageNet/Mirage/commit/db99dd7b3d4c93969c02c5fa7b3e3a1a2948cd7e))
* Wrong method names in ClientSceneTests ([ab3f353](https://github.com/MirageNet/Mirage/commit/ab3f353b33b3068a6ac1649613a28b0977a72685))


### Code Refactoring

*  Client and server keep their own spawned list ([#71](https://github.com/MirageNet/Mirage/issues/71)) ([c2599e2](https://github.com/MirageNet/Mirage/commit/c2599e2c6157dd7539b560cd4645c10713a39276))
* observers is now a set of connections ([#74](https://github.com/MirageNet/Mirage/issues/74)) ([4848920](https://github.com/MirageNet/Mirage/commit/484892058b448012538754c4a1f2eac30a42ceaa))
* Remove networkAddress from NetworkManager ([#67](https://github.com/MirageNet/Mirage/issues/67)) ([e89c32d](https://github.com/MirageNet/Mirage/commit/e89c32dc16b3d9b9cdcb38f0d7d170da94dbf874))
* Remove offline/online scenes ([#120](https://github.com/MirageNet/Mirage/issues/120)) ([a4c881a](https://github.com/MirageNet/Mirage/commit/a4c881a36e26b20fc72166741e20c84ce030ad8f))


### Features

* **installation:** Simplify packaging ([#336](https://github.com/MirageNet/Mirage/issues/336)) ([58a0f68](https://github.com/MirageNet/Mirage/commit/58a0f68560d1d113bb4536a4c264937cdee7f3df))
* [#869](https://github.com/MirageNet/Mirage/issues/869) support structs in other assemblies ([#1022](https://github.com/MirageNet/Mirage/issues/1022)) ([62d1887](https://github.com/MirageNet/Mirage/commit/62d1887e62c7262e9e88c0c72190b82324d644e4))
* Add excludeOwner option to ClientRpc ([#1954](https://github.com/MirageNet/Mirage/issues/1954)) ([864fdd5](https://github.com/MirageNet/Mirage/commit/864fdd5fdce7a35ee4bf553176ed7a4ec3dc0653)), closes [#1963](https://github.com/MirageNet/Mirage/issues/1963) [#1962](https://github.com/MirageNet/Mirage/issues/1962) [#1961](https://github.com/MirageNet/Mirage/issues/1961) [#1960](https://github.com/MirageNet/Mirage/issues/1960) [#1959](https://github.com/MirageNet/Mirage/issues/1959) [#1958](https://github.com/MirageNet/Mirage/issues/1958) [#1957](https://github.com/MirageNet/Mirage/issues/1957) [#1956](https://github.com/MirageNet/Mirage/issues/1956)
* Add fallback transport ([1b02796](https://github.com/MirageNet/Mirage/commit/1b02796c1468c1e1650eab0f278cd9a11cc597c7))
* add IChannelConnection interface for transports with channels ([#332](https://github.com/MirageNet/Mirage/issues/332)) ([887118e](https://github.com/MirageNet/Mirage/commit/887118e2d20009c97d0732f6176c72484780b5bb))
* Add Network Menu  ([#253](https://github.com/MirageNet/Mirage/issues/253)) ([d81f444](https://github.com/MirageNet/Mirage/commit/d81f444c42475439d24bf5b4abd2bbf15fd34e92))
* Add NetworkServer.RemovePlayerForConnection ([#1772](https://github.com/MirageNet/Mirage/issues/1772)) ([e3790c5](https://github.com/MirageNet/Mirage/commit/e3790c51eb9b79bebc48522fb832ae39f11d31e2))
* Add roomPlayer parameter to OnRoomServerCreateGamePlayer ([#1317](https://github.com/MirageNet/Mirage/issues/1317)) ([abf5cdc](https://github.com/MirageNet/Mirage/commit/abf5cdcf36574a00995f5c229ebcbc41d0286546))
* Add Sensitivity to NetworkTransform ([#1425](https://github.com/MirageNet/Mirage/issues/1425)) ([f69f174](https://github.com/MirageNet/Mirage/commit/f69f1743c54aa7810c5a218e2059c115760c54a3))
* Add SyncHashSet and SyncSortedSet ([#685](https://github.com/MirageNet/Mirage/issues/685)) ([695979e](https://github.com/MirageNet/Mirage/commit/695979e914882dd9ea80058474f147cd031d408f))
* add SyncList.RemoveAll ([#1881](https://github.com/MirageNet/Mirage/issues/1881)) ([eb7c87d](https://github.com/MirageNet/Mirage/commit/eb7c87d15aa2fe0a5b0c08fc9cde0adbeba0b416))
* Add UPM configuration ([#11](https://github.com/MirageNet/Mirage/issues/11)) ([9280af1](https://github.com/MirageNet/Mirage/commit/9280af158317597a53f6ddf5da4191b607e0c0f1))
* Add version to package file ([#1361](https://github.com/MirageNet/Mirage/issues/1361)) ([e97ab93](https://github.com/MirageNet/Mirage/commit/e97ab9379f798063e50a433ea6c2759f73d199ac))
* Add weaver support for Vector2Int and Vector3Int ([#646](https://github.com/MirageNet/Mirage/issues/646)) ([e2a6ce9](https://github.com/MirageNet/Mirage/commit/e2a6ce98114adda39bd28ec1fe31f337fc6bafc4))
* Added NetworkConnection to OnRoomServerSceneLoadedForPlayer ([b5dfcf4](https://github.com/MirageNet/Mirage/commit/b5dfcf45bc9838e89dc37b00cf3653688083bdd8))
* Added Read<T> Method to NetworkReader ([#1480](https://github.com/MirageNet/Mirage/issues/1480)) ([58df3fd](https://github.com/MirageNet/Mirage/commit/58df3fd6d6f53336668536081bc33e2ca22fd38d))
* Added SyncList.Find and SyncList.FindAll ([#1716](https://github.com/MirageNet/Mirage/issues/1716)) ([0fe6328](https://github.com/MirageNet/Mirage/commit/0fe6328800daeef8680a19a394260295b7241442)), closes [#1710](https://github.com/MirageNet/Mirage/issues/1710)
* Added Virtual OnRoomStopServer to NetworkRoomManager and Script Template ([d034ef6](https://github.com/MirageNet/Mirage/commit/d034ef616f3d479729064d652f74a905ea05b495))
* Added virtual SyncVar hook for index in NetworkRoomPlayer ([0c3e079](https://github.com/MirageNet/Mirage/commit/0c3e079d04a034f4d4ca8a34c88188013f36c377))
* adding demo for mirror cloud services ([#2026](https://github.com/MirageNet/Mirage/issues/2026)) ([f1fdc95](https://github.com/MirageNet/Mirage/commit/f1fdc959dcd62e7228ecfd656bc87cbabca8c1bc))
* Adding ignoreAuthority Option to Command ([#1918](https://github.com/MirageNet/Mirage/issues/1918)) ([3ace2c6](https://github.com/MirageNet/Mirage/commit/3ace2c6eb68ad94d78c57df6f63107cca466effa))
* adding log handler that sets console color ([#2001](https://github.com/MirageNet/Mirage/issues/2001)) ([4623978](https://github.com/MirageNet/Mirage/commit/46239783f313159ac47e192499aa8e7fcc5df0ec))
* Adding onLocalPlayerChanged to ClientScene for when localPlayer is changed ([#1920](https://github.com/MirageNet/Mirage/issues/1920)) ([b4acf7d](https://github.com/MirageNet/Mirage/commit/b4acf7d9a20c05eadba8d433ebfd476a30e914dd))
* adding OnRoomServerPlayersNotReady to NetworkRoomManager that is called when player ready changes and atleast 1 player is not ready ([#1921](https://github.com/MirageNet/Mirage/issues/1921)) ([9ae7fa2](https://github.com/MirageNet/Mirage/commit/9ae7fa2a8c683f5d2a7ebe6c243a2d95acad9683))
* Adding ReplaceHandler functions to NetworkServer and NetworkClient ([#1775](https://github.com/MirageNet/Mirage/issues/1775)) ([877f4e9](https://github.com/MirageNet/Mirage/commit/877f4e9c729e5242d4f8c9653868a3cb27c933db))
* adding script that displays ping ([#1975](https://github.com/MirageNet/Mirage/issues/1975)) ([7e93030](https://github.com/MirageNet/Mirage/commit/7e93030849c98f0bc8d95fa310d208fef3028b29))
* additive scene msging added to server ([#285](https://github.com/MirageNet/Mirage/issues/285)) ([bd7a17a](https://github.com/MirageNet/Mirage/commit/bd7a17a65fbc9aed64aaef6c65641697e8d89a74))
* allow more than one NetworkManager ([#135](https://github.com/MirageNet/Mirage/issues/135)) ([92968e4](https://github.com/MirageNet/Mirage/commit/92968e4e45a33fa5ab77ce2bfc9e8f826a888711))
* Allow Multiple Network Animator ([#1778](https://github.com/MirageNet/Mirage/issues/1778)) ([34a76a2](https://github.com/MirageNet/Mirage/commit/34a76a2834cbeebb4c623f6650c1d67345b386ac))
* allow Play mode options ([f9afb64](https://github.com/MirageNet/Mirage/commit/f9afb6407b015c239975c5a1fba6609e12ab5c8f))
* allow users to detect mirror version 3 ([ee9c737](https://github.com/MirageNet/Mirage/commit/ee9c737bdaf47ff48fb72c017731fb61e63043b1))
* Allowing extra base types to be used for SyncLists and other SyncObjects ([#1729](https://github.com/MirageNet/Mirage/issues/1729)) ([9bf816a](https://github.com/MirageNet/Mirage/commit/9bf816a014fd393617422ee6efa52bdf730cc3c9))
* Allowing Multiple Concurrent Additive Scenes ([#1697](https://github.com/MirageNet/Mirage/issues/1697)) ([e32a9b6](https://github.com/MirageNet/Mirage/commit/e32a9b6f0b0744b6bd0eeeb0d9fca0b4dc33cbdf))
* An authenticator that times out other authenticators ([#1211](https://github.com/MirageNet/Mirage/issues/1211)) ([09f6892](https://github.com/MirageNet/Mirage/commit/09f6892c55f74d3d480621b7d334154a979d3b6a))
* async multiplex transport ([#145](https://github.com/MirageNet/Mirage/issues/145)) ([c0e7e92](https://github.com/MirageNet/Mirage/commit/c0e7e9280931a5996f595e41aa516bef20208b6f))
* asynchronous transport ([#134](https://github.com/MirageNet/Mirage/issues/134)) ([0e84f45](https://github.com/MirageNet/Mirage/commit/0e84f451e822fe7c1ca1cd04e052546ed273cfce))
* Authentication Framework ([#1057](https://github.com/MirageNet/Mirage/issues/1057)) ([56bcb02](https://github.com/MirageNet/Mirage/commit/56bcb02c158050001e1910852df5994c1995424c))
* Authenticators can now provide AuthenticationData ([310ce81](https://github.com/MirageNet/Mirage/commit/310ce81c8378707e044108b94faac958e35cbca6))
* awaitable connect ([#55](https://github.com/MirageNet/Mirage/issues/55)) ([952e8a4](https://github.com/MirageNet/Mirage/commit/952e8a43e2b2e4443e24865c60af1ee47467e4cf))
* Block Play Mode and Builds for Weaver Errors ([#1479](https://github.com/MirageNet/Mirage/issues/1479)) ([0e80e19](https://github.com/MirageNet/Mirage/commit/0e80e1996fb2673364169782c330e69cd598a21d))
* Button to register all prefabs in NetworkClient ([#179](https://github.com/MirageNet/Mirage/issues/179)) ([9f5f0b2](https://github.com/MirageNet/Mirage/commit/9f5f0b27f8857bf55bf4f5ffbd436247f99cf390))
* Chat example ([#1305](https://github.com/MirageNet/Mirage/issues/1305)) ([9926472](https://github.com/MirageNet/Mirage/commit/9926472d98730d8fc77231c5ea261158bd09d46b))
* Check for client authority in CmdClientToServerSync ([#1500](https://github.com/MirageNet/Mirage/issues/1500)) ([8b359ff](https://github.com/MirageNet/Mirage/commit/8b359ff6d07352a751f200768dcde6febd8e9303))
* Check for client authority in NetworkAnimator Cmd's ([#1501](https://github.com/MirageNet/Mirage/issues/1501)) ([ecc0659](https://github.com/MirageNet/Mirage/commit/ecc0659b87f3d910dc2370f4861ec70244a25622))
* Client attribute now throws error ([#274](https://github.com/MirageNet/Mirage/issues/274)) ([f1b52f3](https://github.com/MirageNet/Mirage/commit/f1b52f3d23e9aa50b5fab8509f3c769e97eac2e7))
* ClientRpc no longer need Rpc prefix ([#2086](https://github.com/MirageNet/Mirage/issues/2086)) ([eb93c34](https://github.com/MirageNet/Mirage/commit/eb93c34b330189c79727b0332bb66f3675cfd641))
* ClientScene uses log window ([b3656a9](https://github.com/MirageNet/Mirage/commit/b3656a9edc5ff00329ce00847671ade7b8f2add2))
* Commands no longer need Cmd prefix ([#2084](https://github.com/MirageNet/Mirage/issues/2084)) ([b6d1d09](https://github.com/MirageNet/Mirage/commit/b6d1d09f846f7cf0310db0db9d931a9cfbbb36b2))
* Commands no longer need to start with Cmd ([#263](https://github.com/MirageNet/Mirage/issues/263)) ([9578e19](https://github.com/MirageNet/Mirage/commit/9578e19ff70bf3a09a9fe31926c8ac337f945ba9))
* Component based NetworkSceneManager ([#244](https://github.com/MirageNet/Mirage/issues/244)) ([7579d71](https://github.com/MirageNet/Mirage/commit/7579d712ad97db98cd729c51568631e4c3257b58))
* component based SyncToOwner, fixes [#39](https://github.com/MirageNet/Mirage/issues/39) ([#1023](https://github.com/MirageNet/Mirage/issues/1023)) ([c6d86b3](https://github.com/MirageNet/Mirage/commit/c6d86b301ba19ad8bdaadff12e347f77c621cdc2))
* connections can retrieve end point ([#114](https://github.com/MirageNet/Mirage/issues/114)) ([d239718](https://github.com/MirageNet/Mirage/commit/d239718498c5750edf0b5d11cc762136f45500fd))
* Cosmetic Enhancement of Network Manager ([#1512](https://github.com/MirageNet/Mirage/issues/1512)) ([f53b12b](https://github.com/MirageNet/Mirage/commit/f53b12b2f7523574d7ceffa2a833dbd7fac755c9))
* Creating method to replace all log handlers ([#1880](https://github.com/MirageNet/Mirage/issues/1880)) ([d8aaf76](https://github.com/MirageNet/Mirage/commit/d8aaf76fb972dd153f6002edb96cd2b9350e572c))
* custom reader/writer via extension methods ([#1047](https://github.com/MirageNet/Mirage/issues/1047)) ([b45afad](https://github.com/MirageNet/Mirage/commit/b45afad641b1dd9cca3eb3046f6b727d7063d4ef))
* default log level option ([#1728](https://github.com/MirageNet/Mirage/issues/1728)) ([5c56adc](https://github.com/MirageNet/Mirage/commit/5c56adc1dc47ef91f7ee1d766cd70fa1681cb9df))
* Disconnect Dead Clients ([#1724](https://github.com/MirageNet/Mirage/issues/1724)) ([a2eb666](https://github.com/MirageNet/Mirage/commit/a2eb666f158d72851d6c62997ed4b24dc3c473bc)), closes [#1753](https://github.com/MirageNet/Mirage/issues/1753)
* Disposable PooledNetworkReader / PooledNetworkWriter ([#1490](https://github.com/MirageNet/Mirage/issues/1490)) ([bb55baa](https://github.com/MirageNet/Mirage/commit/bb55baa679ae780e127ed5817ef10d7f12cd08c8))
* Example with 10k monster that change unfrequently ([2b2e71c](https://github.com/MirageNet/Mirage/commit/2b2e71cc007dba2c1d90b565c4983814c1e0b7d1))
* Exclude fields from weaver's automatic Read/Write using System.NonSerialized attribute  ([#1727](https://github.com/MirageNet/Mirage/issues/1727)) ([7f8733c](https://github.com/MirageNet/Mirage/commit/7f8733ce6a8f712c195ab7a5baea166a16b52d09))
* Experimental Network Transform ([#1990](https://github.com/MirageNet/Mirage/issues/1990)) ([7e2b733](https://github.com/MirageNet/Mirage/commit/7e2b7338a18855f156e52b663ac24eef153b43a7))
* Experimental NetworkRigidbody  ([#1822](https://github.com/MirageNet/Mirage/issues/1822)) ([25285b1](https://github.com/MirageNet/Mirage/commit/25285b1574c4e025373e86735ec3eb9734272fd2))
* fallback transport now supports uri ([#1296](https://github.com/MirageNet/Mirage/issues/1296)) ([e4a701e](https://github.com/MirageNet/Mirage/commit/e4a701ed4e22f1ad89fc3805fa63fe9fef61a8e0))
* generate serializers for IMessageBase structs ([#1353](https://github.com/MirageNet/Mirage/issues/1353)) ([3c0bc28](https://github.com/MirageNet/Mirage/commit/3c0bc2822847410213ee137f6e848c7be296d65a))
* get a convenient property to get network time ([1e8c2fe](https://github.com/MirageNet/Mirage/commit/1e8c2fe0522d7843a6a2fae7287287c7afa4e417))
* HasAuthority attribute now throws error ([#276](https://github.com/MirageNet/Mirage/issues/276)) ([da2355b](https://github.com/MirageNet/Mirage/commit/da2355b4c1a51dbcbf6ceb405b6fc7b5bb14fa14))
* HeadlessAutoStart and HeadlessFrameLimiter ([#318](https://github.com/MirageNet/Mirage/issues/318)) ([ce6ef50](https://github.com/MirageNet/Mirage/commit/ce6ef50c37690623a5dcafc96cc949966ed6363b))
* Implement IReadOnlyList<T> in SyncLists ([#903](https://github.com/MirageNet/Mirage/issues/903)) ([3eaaa77](https://github.com/MirageNet/Mirage/commit/3eaaa773b3c126897ed0c84c2e776045793556f7))
* Implemented NetworkReaderPool ([#1464](https://github.com/MirageNet/Mirage/issues/1464)) ([9257112](https://github.com/MirageNet/Mirage/commit/9257112c65c92324ad0bd51e4a017aa1b4c9c1fc))
* Improve weaver error messages ([#1779](https://github.com/MirageNet/Mirage/issues/1779)) ([bcd76c5](https://github.com/MirageNet/Mirage/commit/bcd76c5bdc88af6d95a96e35d47b1b167d375652))
* Improved Log Settings Window Appearance ([#1885](https://github.com/MirageNet/Mirage/issues/1885)) ([69b8451](https://github.com/MirageNet/Mirage/commit/69b845183c099744455e34c6f12e75acecb9098a))
* Improved RoomPayer template ([042b4e1](https://github.com/MirageNet/Mirage/commit/042b4e1965580a4cdbd5ea50b11a1377fe3bf3cc))
* include generated changelog ([#27](https://github.com/MirageNet/Mirage/issues/27)) ([a60f1ac](https://github.com/MirageNet/Mirage/commit/a60f1acd3a544639a5e58a8946e75fd6c9012327))
* Include version in release ([#1336](https://github.com/MirageNet/Mirage/issues/1336)) ([e4f89cf](https://github.com/MirageNet/Mirage/commit/e4f89cf12f6dca42bbb5c25e50f03ed7fcde3f82))
* individual events for SyncDictionary ([#112](https://github.com/MirageNet/Mirage/issues/112)) ([b3c1b16](https://github.com/MirageNet/Mirage/commit/b3c1b16100c440131d6d933627a9f6479aed11ad))
* individual events for SyncSet ([#111](https://github.com/MirageNet/Mirage/issues/111)) ([261f5d6](https://github.com/MirageNet/Mirage/commit/261f5d6a1481634dc524fb57b5866e378a1d909d))
* LAN Network discovery ([#1453](https://github.com/MirageNet/Mirage/issues/1453)) ([e75b45f](https://github.com/MirageNet/Mirage/commit/e75b45f8889478456573ea395694b4efc560ace0)), closes [#38](https://github.com/MirageNet/Mirage/issues/38)
* LLAPI transport can receive port from uri ([#1294](https://github.com/MirageNet/Mirage/issues/1294)) ([7865a84](https://github.com/MirageNet/Mirage/commit/7865a840b66db74acfdf48989adec2c72222020c))
* LocalPlayer attribute now throws error ([#277](https://github.com/MirageNet/Mirage/issues/277)) ([15aa537](https://github.com/MirageNet/Mirage/commit/15aa537947cd14e4d71853f1786c387519d8828b))
* logger factory works for static classes by passing the type ([f9328c7](https://github.com/MirageNet/Mirage/commit/f9328c771cfb0974ce4765dc0d5af01440d838c0))
* logging api ([#1611](https://github.com/MirageNet/Mirage/issues/1611)) ([f2ccb59](https://github.com/MirageNet/Mirage/commit/f2ccb59ae6db90bc84f8a36802bfe174b4493127))
* LogSettings that can be saved and included in a build ([#1863](https://github.com/MirageNet/Mirage/issues/1863)) ([fd4357c](https://github.com/MirageNet/Mirage/commit/fd4357cd264b257aa648a26f9392726b2921b870))
* Make AsyncQueue public for transports ([5df0d98](https://github.com/MirageNet/Mirage/commit/5df0d98307eff409dd16e67fddedb25710b68b6d))
* Mirror Icon for all components ([#1452](https://github.com/MirageNet/Mirage/issues/1452)) ([a7efb13](https://github.com/MirageNet/Mirage/commit/a7efb13e29e0bc9ed695a86070e3eb57b7506b4c))
* Mirror now supports message inheritance ([#1286](https://github.com/MirageNet/Mirage/issues/1286)) ([f9d34d5](https://github.com/MirageNet/Mirage/commit/f9d34d586368df2917a0ee834f823a4dd140cb31))
* More examples for Mirror Cloud Service ([#2029](https://github.com/MirageNet/Mirage/issues/2029)) ([7d0e907](https://github.com/MirageNet/Mirage/commit/7d0e907b73530c9a625eaf663837b7eeb36fcee6))
* Multiple Concurrent Additive Physics Scenes Example ([#1686](https://github.com/MirageNet/Mirage/issues/1686)) ([87c6ebc](https://github.com/MirageNet/Mirage/commit/87c6ebc5ddf71b3fc358bb1a90bd9ee2470e333c))
* Multiplex based on url ([#1295](https://github.com/MirageNet/Mirage/issues/1295)) ([c206f9a](https://github.com/MirageNet/Mirage/commit/c206f9ad974249c5514fad6ef21b27387d1b7ace))
* Network Animator can reset triggers ([#1420](https://github.com/MirageNet/Mirage/issues/1420)) ([dffdf02](https://github.com/MirageNet/Mirage/commit/dffdf02be596db3d35bdd8d19ba6ada7d796a137))
* Network Scene Checker Component ([#1271](https://github.com/MirageNet/Mirage/issues/1271)) ([71c0d3b](https://github.com/MirageNet/Mirage/commit/71c0d3b2ee1bbdb29d1c39ee6eca3ef9635d70bf))
* network writer and reader now support uri ([0c2556a](https://github.com/MirageNet/Mirage/commit/0c2556ac64bd93b9e52dae34cf8d84db114b4107))
* network writer pool to avoid expensive allocations ([3659acb](https://github.com/MirageNet/Mirage/commit/3659acbbdd43321e22269591bfd08189b40e6b44))
* network writer pool to avoid expensive allocations ([#928](https://github.com/MirageNet/Mirage/issues/928)) ([f5e9318](https://github.com/MirageNet/Mirage/commit/f5e93180a1161e62ef74eb5c5ad81308e91d5687))
* NetworkAnimator warns if you use it incorrectly ([#1424](https://github.com/MirageNet/Mirage/issues/1424)) ([c30e4a9](https://github.com/MirageNet/Mirage/commit/c30e4a9f83921416f936ef5fd1bb0e2b3a410807))
* NetworkClient raises event after authentication ([#96](https://github.com/MirageNet/Mirage/issues/96)) ([c332271](https://github.com/MirageNet/Mirage/commit/c332271d918f782d4b1a84b3f0fd660239f95743))
* NetworkConnection is optional for handlers ([#1202](https://github.com/MirageNet/Mirage/issues/1202)) ([bf9eb61](https://github.com/MirageNet/Mirage/commit/bf9eb610dced2434f4a045f5b01bc758b9f72327))
* NetworkConnection manages messages handlers ([#93](https://github.com/MirageNet/Mirage/issues/93)) ([5c79f0d](https://github.com/MirageNet/Mirage/commit/5c79f0db64e46905081e6c0b5502376c5acf0d99))
* NetworkConnection to client and server use logger framework ([72154f1](https://github.com/MirageNet/Mirage/commit/72154f1daddaa141fb3b8fe02bcfdf098ef1d44a))
* NetworkConnection uses logging framework ([ec319a1](https://github.com/MirageNet/Mirage/commit/ec319a165dc8445b00b096d09061adda2c7b8b9d))
* NetworkIdentity lifecycle events ([#88](https://github.com/MirageNet/Mirage/issues/88)) ([9a7c572](https://github.com/MirageNet/Mirage/commit/9a7c5726eb3d333b85c3d0e44b884c11e34be45d))
* NetworkIdentity use logger framework ([2e39e13](https://github.com/MirageNet/Mirage/commit/2e39e13c012aa79d50a54fc5d07b85da3e52391b))
* NetworkMatchChecker Component ([#1688](https://github.com/MirageNet/Mirage/issues/1688)) ([21acf66](https://github.com/MirageNet/Mirage/commit/21acf661905ebc35f31a52eb527a50c6eff68a44)), closes [#1685](https://github.com/MirageNet/Mirage/issues/1685) [#1681](https://github.com/MirageNet/Mirage/issues/1681) [#1689](https://github.com/MirageNet/Mirage/issues/1689)
* NetworkSceneChecker use Scene instead of string name ([#1496](https://github.com/MirageNet/Mirage/issues/1496)) ([7bb80e3](https://github.com/MirageNet/Mirage/commit/7bb80e3b796f2c69d0958519cf1b4a9f4373268b))
* NetworkServer uses new logging framework ([8b4f105](https://github.com/MirageNet/Mirage/commit/8b4f1051f27f1d5b845e6bd0a090368082ab1603))
* NetworkServer.SendToReady ([#1773](https://github.com/MirageNet/Mirage/issues/1773)) ([f6545d4](https://github.com/MirageNet/Mirage/commit/f6545d4871bf6881b3150a3231af195e7f9eb8cd))
* new virtual OnStopServer called when object is unspawned ([#1743](https://github.com/MirageNet/Mirage/issues/1743)) ([d1695dd](https://github.com/MirageNet/Mirage/commit/d1695dd16f477fc9edaaedb90032c188bcbba6e2))
* new way to connect using uri ([#1279](https://github.com/MirageNet/Mirage/issues/1279)) ([7c3622c](https://github.com/MirageNet/Mirage/commit/7c3622cfaed0c062f51342294264c8b389b2846d))
* new websocket transport ([#156](https://github.com/MirageNet/Mirage/issues/156)) ([23c7b0d](https://github.com/MirageNet/Mirage/commit/23c7b0d1b32684d4f959495fe96b2d16a68bd356))
* next gen async transport ([#106](https://github.com/MirageNet/Mirage/issues/106)) ([4a8dc67](https://github.com/MirageNet/Mirage/commit/4a8dc676b96840493d178718049b9e20c8eb6510))
* now you can assign scenes even if not in Editor ([#1576](https://github.com/MirageNet/Mirage/issues/1576)) ([c8a1a5e](https://github.com/MirageNet/Mirage/commit/c8a1a5e56f7561487e3180f26e28484f714f36c1))
* Now you can pass NetworkIdentity and GameObjects ([#83](https://github.com/MirageNet/Mirage/issues/83)) ([dca2d40](https://github.com/MirageNet/Mirage/commit/dca2d4056fe613793480b378d25509284a1fd46a))
* onstopserver event in NetworkIdentity ([#186](https://github.com/MirageNet/Mirage/issues/186)) ([eb81190](https://github.com/MirageNet/Mirage/commit/eb8119007b19faca767969700b0209ade135650c))
* Pass all information to spawn handler ([#1215](https://github.com/MirageNet/Mirage/issues/1215)) ([d741bae](https://github.com/MirageNet/Mirage/commit/d741baed789366ed7ce93f289eac3552dfe54fdc))
* Piped connection ([#138](https://github.com/MirageNet/Mirage/issues/138)) ([471a881](https://github.com/MirageNet/Mirage/commit/471a881cdae1c6e526b5aa2d552cc91dc27f793a))
* PlayerSpawner component ([#123](https://github.com/MirageNet/Mirage/issues/123)) ([e8b933d](https://github.com/MirageNet/Mirage/commit/e8b933ddff9a47b64be371edb63af130bd3958b4))
* Prettify Log Names ([c7d8c09](https://github.com/MirageNet/Mirage/commit/c7d8c0933d37abc919305b660cbf3a57828eaace))
* profiler info is available in production builds ([5649cc6](https://github.com/MirageNet/Mirage/commit/5649cc69777a4a49f11cbce92e6f149d92b6e747))
* Remove Command shortcut for host mode ([#1168](https://github.com/MirageNet/Mirage/issues/1168)) ([94eda38](https://github.com/MirageNet/Mirage/commit/94eda38803c141f279a5f42317c4d07c16b0223d))
* Rigidbody example ([#2076](https://github.com/MirageNet/Mirage/issues/2076)) ([ef47ee7](https://github.com/MirageNet/Mirage/commit/ef47ee7a57bddcdc669aef32fbeffcd4446f98a8))
* safer and consistent writers names ([#979](https://github.com/MirageNet/Mirage/issues/979)) ([b4077c1](https://github.com/MirageNet/Mirage/commit/b4077c1112a529ae7494709c1da0b6351d48c4b5))
* scene transition uses routine instead of asyncoperation ([#305](https://github.com/MirageNet/Mirage/issues/305)) ([a16eb00](https://github.com/MirageNet/Mirage/commit/a16eb005e31e576346b4b4cffbe266c25b8709ca))
* Script Templates ([#1217](https://github.com/MirageNet/Mirage/issues/1217)) ([8cf6a07](https://github.com/MirageNet/Mirage/commit/8cf6a0707e0ada3d27b14ec55c4c5a082f0e214b))
* Secure messages that require authentication, fixes [#720](https://github.com/MirageNet/Mirage/issues/720) ([#1089](https://github.com/MirageNet/Mirage/issues/1089)) ([7ac43cd](https://github.com/MirageNet/Mirage/commit/7ac43cd56af6dd1b37f6696e97d7b671d6c21865))
* Semantic release for UPM ([#24](https://github.com/MirageNet/Mirage/issues/24)) ([8cbc005](https://github.com/MirageNet/Mirage/commit/8cbc005543a8b919ec022b2e9d1b5b8a6c85ef14))
* Server and Client share the same scene loading method ([#286](https://github.com/MirageNet/Mirage/issues/286)) ([acb6dd1](https://github.com/MirageNet/Mirage/commit/acb6dd192244adcfab15d013a96c7402151d226b))
* Server attribute now throws error ([#270](https://github.com/MirageNet/Mirage/issues/270)) ([f3b5dc4](https://github.com/MirageNet/Mirage/commit/f3b5dc4fef5fba05e585d274d9df05c3954ff6c7))
* Server raises an event when it starts ([#126](https://github.com/MirageNet/Mirage/issues/126)) ([d5b0a6f](https://github.com/MirageNet/Mirage/commit/d5b0a6f0dd65f9dbb6c4848bce5e81f93772a235))
* ship as a unitypackage ([11edc14](https://github.com/MirageNet/Mirage/commit/11edc142cfddfb7abecc11d8a12d6d32522ceb14))
* Show compile time error if overriding unused OnServerAddPlayer ([#682](https://github.com/MirageNet/Mirage/issues/682)) ([a8599c1](https://github.com/MirageNet/Mirage/commit/a8599c1af2b3b2abe377a580760cb13bbb3c9c7d))
* Spawn objects in clients in same order as server ([#247](https://github.com/MirageNet/Mirage/issues/247)) ([b786646](https://github.com/MirageNet/Mirage/commit/b786646f1859bb0e1836460c6319a507e1cc31aa))
* spawning invalid object now gives exception ([e2fc829](https://github.com/MirageNet/Mirage/commit/e2fc8292400aae8b3b8b972ff5824b8d9cdd6b88))
* support sending and receiving ArraySegment<byte> ([#898](https://github.com/MirageNet/Mirage/issues/898)) ([e5eecbf](https://github.com/MirageNet/Mirage/commit/e5eecbff729f426e0de387f86fed70dc1c28b35a))
* support writing and reading array segments ([#918](https://github.com/MirageNet/Mirage/issues/918)) ([f9ff443](https://github.com/MirageNet/Mirage/commit/f9ff44399ba42c3c7dbc4d4f2615ee4837aa6133))
* supports scriptable objects ([4b8f819](https://github.com/MirageNet/Mirage/commit/4b8f8192123fe0b79ea71f2255a4bbac404c88b1))
* supports scriptable objects ([#1471](https://github.com/MirageNet/Mirage/issues/1471)) ([0f10c72](https://github.com/MirageNet/Mirage/commit/0f10c72744864ac55d2e1aa96ba8d7713c77d9e7))
* Sync Events no longer need Event prefix ([#2087](https://github.com/MirageNet/Mirage/issues/2087)) ([ed40c2d](https://github.com/MirageNet/Mirage/commit/ed40c2d210f174f1ed50b1e929e4fb161414f228))
* SyncDictionary can now be used for any IDictionary ([#703](https://github.com/MirageNet/Mirage/issues/703)) ([2683572](https://github.com/MirageNet/Mirage/commit/2683572fb43cbe22c58d9994007ffebaf001fb4a))
* SyncList now supports any IList implementation ([#704](https://github.com/MirageNet/Mirage/issues/704)) ([040bcb4](https://github.com/MirageNet/Mirage/commit/040bcb45adbb4d7d2ad47c5c2e0275e7c05a7971))
* SyncList.FindIndex added ([#823](https://github.com/MirageNet/Mirage/issues/823)) ([b5ff43a](https://github.com/MirageNet/Mirage/commit/b5ff43ada3fa1ec39f88dd689117761bbefcdd0a))
* synclists has individual meaningful events ([#109](https://github.com/MirageNet/Mirage/issues/109)) ([e326064](https://github.com/MirageNet/Mirage/commit/e326064b51e8372726b30d19973df6293c74c376)), closes [#103](https://github.com/MirageNet/Mirage/issues/103)
* SyncSet and SyncDictionary now show in inspector ([#1561](https://github.com/MirageNet/Mirage/issues/1561)) ([5510711](https://github.com/MirageNet/Mirage/commit/55107115c66ea38b75edf4a912b5cc48351128f7))
* SyncSet custom Equality Comparer support ([#1147](https://github.com/MirageNet/Mirage/issues/1147)) ([0f95185](https://github.com/MirageNet/Mirage/commit/0f951858c553abd34be8544bf717744fae1d35c5))
* SyncToOwner now works with authority ([#1204](https://github.com/MirageNet/Mirage/issues/1204)) ([92d0df7](https://github.com/MirageNet/Mirage/commit/92d0df7b399027ccd8f5983fc4bc4fea4530badc))
* TargetRpc no longer need Target prefix ([#2085](https://github.com/MirageNet/Mirage/issues/2085)) ([d89ac9f](https://github.com/MirageNet/Mirage/commit/d89ac9fb052c17c2edfdf381aff35f70d23f4f0a))
* telepathy can now receive port from uri ([#1284](https://github.com/MirageNet/Mirage/issues/1284)) ([06946cf](https://github.com/MirageNet/Mirage/commit/06946cf37fc2ed8660c93394d30632de3edc35db))
* throw exception if assigning incorrect asset id ([#250](https://github.com/MirageNet/Mirage/issues/250)) ([7741fb1](https://github.com/MirageNet/Mirage/commit/7741fb1f11abc8eb2aec8c1a94ac53380ac5a562))
* Time sync is now done per NetworkClient ([b24542f](https://github.com/MirageNet/Mirage/commit/b24542f62c6a2d0c43588af005f360ed74c619ca))
* transport can provide their preferred scheme ([774a07e](https://github.com/MirageNet/Mirage/commit/774a07e1bf26cce964cf14d502b71b43ce4f5cd0))
* Transport can send to multiple connections at once ([#1120](https://github.com/MirageNet/Mirage/issues/1120)) ([bc7e116](https://github.com/MirageNet/Mirage/commit/bc7e116a6e1e3f1a7dd326109631c8c8d12b2622))
* transports can give server uri ([#113](https://github.com/MirageNet/Mirage/issues/113)) ([dc700ec](https://github.com/MirageNet/Mirage/commit/dc700ec721cf4ecf6ddd082d88b933c9afffbc67))
* Transports can have multiple uri ([#292](https://github.com/MirageNet/Mirage/issues/292)) ([155a29c](https://github.com/MirageNet/Mirage/commit/155a29c053421f870241a75427db748fbef08910))
* Transports can tell if they are supported ([#282](https://github.com/MirageNet/Mirage/issues/282)) ([890c6b8](https://github.com/MirageNet/Mirage/commit/890c6b8808ccbf4f4ffffae8c00a9d897ccac7e4))
* Transports may support any number of schemes ([#291](https://github.com/MirageNet/Mirage/issues/291)) ([2af7b9d](https://github.com/MirageNet/Mirage/commit/2af7b9d19cef3878147eee412adf2b9b32c91147))
* update upm package if tests pass ([#21](https://github.com/MirageNet/Mirage/issues/21)) ([7447776](https://github.com/MirageNet/Mirage/commit/7447776a3bb47aa1e8f262671d62b48d52591247))
* Use logger framework for NetworkClient ([#1685](https://github.com/MirageNet/Mirage/issues/1685)) ([6e92bf5](https://github.com/MirageNet/Mirage/commit/6e92bf5616d0d2486ce86497128094c4e33b5a3f))
* Use SortedDictionary for LogSettings ([#1914](https://github.com/MirageNet/Mirage/issues/1914)) ([7d4c0a9](https://github.com/MirageNet/Mirage/commit/7d4c0a9cb6f24fa3c2834b9bf351e30dde88665f))
* user friendly weaver error ([#896](https://github.com/MirageNet/Mirage/issues/896)) ([954a3d5](https://github.com/MirageNet/Mirage/commit/954a3d594d53adba4fbea25193170760ed810ee8))
* Users may provide custom serializers for any type ([#1153](https://github.com/MirageNet/Mirage/issues/1153)) ([9cb309e](https://github.com/MirageNet/Mirage/commit/9cb309e5bcb01ff3de4781e49d3a4f0a1227891b))
* Weaver can now automatically create Reader/Writer for types in a different assembly ([#1708](https://github.com/MirageNet/Mirage/issues/1708)) ([b1644ae](https://github.com/MirageNet/Mirage/commit/b1644ae481497d4347f404543c8200d2754617b9)), closes [#1570](https://github.com/MirageNet/Mirage/issues/1570)
* websocket can receive port in url ([#1287](https://github.com/MirageNet/Mirage/issues/1287)) ([c8ad118](https://github.com/MirageNet/Mirage/commit/c8ad118d5065f2570c45914d8c1d6daeac2de7ef))
* Websockets now give client address, fix [#1121](https://github.com/MirageNet/Mirage/issues/1121) ([#1125](https://github.com/MirageNet/Mirage/issues/1125)) ([c9f317d](https://github.com/MirageNet/Mirage/commit/c9f317ddee092a59d2de8ad5988bea09a1ca152f))
* **scene:** Add support for scene loading params ([#644](https://github.com/MirageNet/Mirage/issues/644)) ([d48a375](https://github.com/MirageNet/Mirage/commit/d48a3757dabe072002f93293fe9c7bcb13b1354d))
* **syncvar:** Add SyncDictionary ([#602](https://github.com/MirageNet/Mirage/issues/602)) ([7d21bde](https://github.com/MirageNet/Mirage/commit/7d21bded9a521e53acc212b11a756d41e1b4218c))
* **telepathy:** Split MaxMessageSize to allow setting a different value for client and server ([#749](https://github.com/MirageNet/Mirage/issues/749)) ([f0a8b5d](https://github.com/MirageNet/Mirage/commit/f0a8b5dea817cf59d961643f409d2347349a1261))
* **websocket:** Re-enable native SSL ([#965](https://github.com/MirageNet/Mirage/issues/965)) ([7ed4a9a](https://github.com/MirageNet/Mirage/commit/7ed4a9a1e0727e067795ef7a9a24c6203f8ceb34))


### Performance Improvements

* Adding buffer for local connection ([#1621](https://github.com/MirageNet/Mirage/issues/1621)) ([4d5cee8](https://github.com/MirageNet/Mirage/commit/4d5cee893d0104c0070a0b1814c8c84f11f24f18))
* Adding dirty check before update sync var ([#1702](https://github.com/MirageNet/Mirage/issues/1702)) ([58219c8](https://github.com/MirageNet/Mirage/commit/58219c8f726cd65f8987c9edd747987057967ea4))
* AddPlayerMessage is now a value type ([246a551](https://github.com/MirageNet/Mirage/commit/246a551151ea33892aa3bc04faca68c0b755a653))
* allocation free enumerator for syncsets,  fixes [#1171](https://github.com/MirageNet/Mirage/issues/1171) ([#1173](https://github.com/MirageNet/Mirage/issues/1173)) ([035e630](https://github.com/MirageNet/Mirage/commit/035e6307f98e3296f5d0f5c37eea5d4ce9c26fd0))
* allocation free syncdict foreach, fix [#1172](https://github.com/MirageNet/Mirage/issues/1172) ([#1174](https://github.com/MirageNet/Mirage/issues/1174)) ([1ec8910](https://github.com/MirageNet/Mirage/commit/1ec89105758cb6a76c438aa990c1dcfbf0a78af6))
* avoid allocation for error messages ([c669ff1](https://github.com/MirageNet/Mirage/commit/c669ff155df16a007ee69d703a1f72f2e0e0b919))
* Avoid allocation when reading message payload ([#912](https://github.com/MirageNet/Mirage/issues/912)) ([11750a9](https://github.com/MirageNet/Mirage/commit/11750a9e7a3f330e508642d1fca51173c3a4d5a8))
* avoid allocation with message structs ([#939](https://github.com/MirageNet/Mirage/issues/939)) ([7c7c910](https://github.com/MirageNet/Mirage/commit/7c7c910a5e5ce15dc81b1008e4797222abe7fd9a))
* avoid boxing for getting message id ([#1144](https://github.com/MirageNet/Mirage/issues/1144)) ([9513842](https://github.com/MirageNet/Mirage/commit/95138427f3c6765ba25bccc98968f477c1f8bcda))
* avoid boxing if there is no profiler ([a351879](https://github.com/MirageNet/Mirage/commit/a351879f910be15492d498b3cc38e2ea8861d231))
* ClientAuthorityMessage is now a value type ([#991](https://github.com/MirageNet/Mirage/issues/991)) ([d071438](https://github.com/MirageNet/Mirage/commit/d071438d01080b56065e800c3ddf492c87231ed6))
* Custom IEnumerator for SyncLists to avoid allocations on foreach ([#904](https://github.com/MirageNet/Mirage/issues/904)) ([4ffd5a2](https://github.com/MirageNet/Mirage/commit/4ffd5a2e06849fc812106f67d5d2b9c3d40a99b9))
* don't varint bytes and shorts ([4867415](https://github.com/MirageNet/Mirage/commit/48674151f01c040979e4a9aebbf3f6037f7b2226))
* eliminate boxing with lists ([#901](https://github.com/MirageNet/Mirage/issues/901)) ([8f6d4cb](https://github.com/MirageNet/Mirage/commit/8f6d4cb22e0417bb0de1cb744e307250298e20f4))
* eliminate small allocation on remote calls ([#907](https://github.com/MirageNet/Mirage/issues/907)) ([1c18743](https://github.com/MirageNet/Mirage/commit/1c18743788be9d051fa617345e463cf0df6e38d8))
* eliminate string concat during remote method calls ([#908](https://github.com/MirageNet/Mirage/issues/908)) ([70a532b](https://github.com/MirageNet/Mirage/commit/70a532b5db7cd0c20797b1168d84c6368480450c))
* empty messages are value types now ([145edaa](https://github.com/MirageNet/Mirage/commit/145edaa50bd225db9f1442aa7c86bae13daa6388))
* faster NetworkReader pooling ([#1623](https://github.com/MirageNet/Mirage/issues/1623)) ([1ae0381](https://github.com/MirageNet/Mirage/commit/1ae038172ac7f5e18e0e09b0081f7f42fa0eff7a))
* faster NetworkWriter pooling ([#1620](https://github.com/MirageNet/Mirage/issues/1620)) ([4fa43a9](https://github.com/MirageNet/Mirage/commit/4fa43a947132f89e5348c63e393dd3b80e1fe7e1))
* Increasing Network Writer performance ([#1674](https://github.com/MirageNet/Mirage/issues/1674)) ([f057983](https://github.com/MirageNet/Mirage/commit/f0579835ca52270de424e81691f12c02022c3909))
* messages should be value types ([#987](https://github.com/MirageNet/Mirage/issues/987)) ([633fb19](https://github.com/MirageNet/Mirage/commit/633fb19f8d0f29eef2fd96a97c4da32203cb3408))
* MultiplexTransport: avoid Linq allocations that would happen on every packet send because Send calls .ServerActive() each time ([7fe8888](https://github.com/MirageNet/Mirage/commit/7fe8888df5a74667914c66c336625309279ff28a))
* NetworkProximityChecker checks Server.connections instead of doing 10k sphere casts for 10k monsters. 2k NetworkTransforms demo is significantly faster. Stable 80fps instead of 500ms freezes in between. ([#1852](https://github.com/MirageNet/Mirage/issues/1852)) ([2d89f05](https://github.com/MirageNet/Mirage/commit/2d89f059afd9175dd7e6d81a0e2e38c0a28915c8))
* Networkreader nonalloc ([#910](https://github.com/MirageNet/Mirage/issues/910)) ([18f035d](https://github.com/MirageNet/Mirage/commit/18f035d268d5c84fb6b34d2836b188692cd5a96c))
* objdestroy message is now a value type ([#993](https://github.com/MirageNet/Mirage/issues/993)) ([a32c5a9](https://github.com/MirageNet/Mirage/commit/a32c5a945689285241cadb809b24c2883ac6078c))
* ObjHideMessage is now a value type ([#992](https://github.com/MirageNet/Mirage/issues/992)) ([a49d938](https://github.com/MirageNet/Mirage/commit/a49d938fbe3c27f7237705dae3b2a2d21114de81))
* OnDeserializeSafely without GC ([#804](https://github.com/MirageNet/Mirage/issues/804)) ([27b7e25](https://github.com/MirageNet/Mirage/commit/27b7e250a0451ae6a04222a9d035a5b0efdbeb99))
* Optimize interest management ([f1ceb0c](https://github.com/MirageNet/Mirage/commit/f1ceb0c7a0438d3b7febbccc1ab8fde0a7e2580b))
* Optimize interest management ([#899](https://github.com/MirageNet/Mirage/issues/899)) ([ff1a234](https://github.com/MirageNet/Mirage/commit/ff1a2346b4b28acef7054f5e460e4b863dec6372))
* Pack small 32 bit negatives efficiently ([480af1a](https://github.com/MirageNet/Mirage/commit/480af1aa6c8aca96b67f1532994c5d7d2d8902c5))
* Pack small 64 bit negatives efficiently ([5f1ef4a](https://github.com/MirageNet/Mirage/commit/5f1ef4ab1f5a895e4537dcb5b928b557487c5e60))
* Recycle argument writer to avoid allocations ([#945](https://github.com/MirageNet/Mirage/issues/945)) ([9743216](https://github.com/MirageNet/Mirage/commit/97432169ead1e212bff4496ac2f1afe1c7ad2898))
* Reduce enum bandwidth ([#794](https://github.com/MirageNet/Mirage/issues/794)) ([97e9ac2](https://github.com/MirageNet/Mirage/commit/97e9ac24830b1e0e0aec28c8608ad630ed024f5c))
* remove allocations during syncvar sync ([#946](https://github.com/MirageNet/Mirage/issues/946)) ([d2381ce](https://github.com/MirageNet/Mirage/commit/d2381ce892968a91405afd52f00a357144817539))
* remove BinaryWriter,  it allocates like crazy ([#929](https://github.com/MirageNet/Mirage/issues/929)) ([7b3e82a](https://github.com/MirageNet/Mirage/commit/7b3e82a1fc4339698583633b00b9ed052780f6ed))
* remove network transform allocation ([9e3ecc1](https://github.com/MirageNet/Mirage/commit/9e3ecc1cedc5239d30e91bcdcff9841b94e3dec8))
* Remove redundant mask ([#1604](https://github.com/MirageNet/Mirage/issues/1604)) ([5d76afb](https://github.com/MirageNet/Mirage/commit/5d76afbe29f456a657c9e1cb7c97435242031091))
* remove syncvar boxing ([#927](https://github.com/MirageNet/Mirage/issues/927)) ([b2ba589](https://github.com/MirageNet/Mirage/commit/b2ba5896fa6b58fa524e6cde1b763ef6f3cba4b3))
* replace isValueType with faster alternative ([#1617](https://github.com/MirageNet/Mirage/issues/1617)) ([61163ca](https://github.com/MirageNet/Mirage/commit/61163cacb4cb2652aa8632f84be89212674436ff)), closes [/github.com/vis2k/Mirror/issues/1614#issuecomment-605443808](https://github.com//github.com/vis2k/Mirror/issues/1614/issues/issuecomment-605443808)
* return the contents of the writer as an array segment ([#916](https://github.com/MirageNet/Mirage/issues/916)) ([ced3690](https://github.com/MirageNet/Mirage/commit/ced36906bcb6dca2a1edb439da6b00d0b5d0d09d))
* reuse the network writer used for rpc parameters ([5dafc4d](https://github.com/MirageNet/Mirage/commit/5dafc4d932584710c4fec62b9bfb52cedd7f02fb))
* rpc messages are now value types ([#997](https://github.com/MirageNet/Mirage/issues/997)) ([b5b2f3e](https://github.com/MirageNet/Mirage/commit/b5b2f3e1eb8c64d54cd322e095c42a224dad0f1a))
* SceneMessage is now a value type ([#989](https://github.com/MirageNet/Mirage/issues/989)) ([407b36a](https://github.com/MirageNet/Mirage/commit/407b36acb724cbf90737465e9faf4918d0cee345))
* simplify and speed up getting methods in weaver ([c1cfc42](https://github.com/MirageNet/Mirage/commit/c1cfc421811e4c12e84cb28677ac68c82575958d))
* spawn with client authority only takes 1 message ([#1206](https://github.com/MirageNet/Mirage/issues/1206)) ([3b9414f](https://github.com/MirageNet/Mirage/commit/3b9414f131450e5f96c621f57d9e061dbda62661))
* SpawnPrefabMessage is now a value type ([a44efd1](https://github.com/MirageNet/Mirage/commit/a44efd1f92b66cbf7325830463e7e310dabe3fd8))
* SpawnSceneObjectMessage is now a value type ([40c7d97](https://github.com/MirageNet/Mirage/commit/40c7d97ed99fe7a478b74d1530b70fbc2ae3cfa9))
* Transports now give ArraySegment<byte> instead of byte[] (based on [#569](https://github.com/MirageNet/Mirage/issues/569) and [#846](https://github.com/MirageNet/Mirage/issues/846)) ([77bee45](https://github.com/MirageNet/Mirage/commit/77bee450b91661e9f3902a30e0749cf6c786020c))
* update vars is now a value type ([#990](https://github.com/MirageNet/Mirage/issues/990)) ([f99e71e](https://github.com/MirageNet/Mirage/commit/f99e71ebdfe2fdf50116270618a5bf0f2b97b748))
* Use 0 for null byte arrays ([#925](https://github.com/MirageNet/Mirage/issues/925)) ([21ca49d](https://github.com/MirageNet/Mirage/commit/21ca49d1be9a15445baf1f7417d7fc6ec1df1053))
* use 0 for null strings ([#926](https://github.com/MirageNet/Mirage/issues/926)) ([7181cd9](https://github.com/MirageNet/Mirage/commit/7181cd9ca190a5f3eb45ec7878492479db55d9f3))
* use bitshift operations instead of division in varint ([fff765c](https://github.com/MirageNet/Mirage/commit/fff765c96be0c61d77ade8f5c12997e709ab3d89))
* use byte[] directly instead of MemoryStream ([#1618](https://github.com/MirageNet/Mirage/issues/1618)) ([166b8c9](https://github.com/MirageNet/Mirage/commit/166b8c946736447a76c1886c4d1fb036f6e56e20))
* Use continuewith to queue up ssl messages ([#1640](https://github.com/MirageNet/Mirage/issues/1640)) ([84b2c8c](https://github.com/MirageNet/Mirage/commit/84b2c8cf2671728baecf734487ddaa7fab9943a0))
* Use invokeRepeating instead of Update ([#2066](https://github.com/MirageNet/Mirage/issues/2066)) ([264f9b8](https://github.com/MirageNet/Mirage/commit/264f9b8f945f0294a8420202abcd0c80e27e6ee6))
* Use NetworkWriterPool in NetworkAnimator ([#1421](https://github.com/MirageNet/Mirage/issues/1421)) ([7d472f2](https://github.com/MirageNet/Mirage/commit/7d472f21f9a807357df244a3f0ac259dd431661f))
* Use NetworkWriterPool in NetworkTransform ([#1422](https://github.com/MirageNet/Mirage/issues/1422)) ([a457845](https://github.com/MirageNet/Mirage/commit/a4578458a15e3d2840a49dd029b4c404cadf85a4))
* Use RemoveAt to remove elements from lists ([22b45f7](https://github.com/MirageNet/Mirage/commit/22b45f7a11be6f3c09e49a302506b540c1c5adc6))
* use value types for empty messages ([#988](https://github.com/MirageNet/Mirage/issues/988)) ([81d915e](https://github.com/MirageNet/Mirage/commit/81d915eb7350878f69db2f579355ad1224359194))
* Use WritePackedUInt32 in SyncList ([#688](https://github.com/MirageNet/Mirage/issues/688)) ([2db7576](https://github.com/MirageNet/Mirage/commit/2db7576bbc163cf53e1b28384972361e3ca4a720))


* remove NetworkConnection.isAuthenticated (#167) ([8a0e0b3](https://github.com/MirageNet/Mirage/commit/8a0e0b3af37e8b0c74a8b97f12ec29cf202715ea)), closes [#167](https://github.com/MirageNet/Mirage/issues/167)
* Simplify RegisterHandler (#160) ([f4f5167](https://github.com/MirageNet/Mirage/commit/f4f516791b8390f0babf8a7aefa19254427d4145)), closes [#160](https://github.com/MirageNet/Mirage/issues/160)
* Remove NetworkConnectionToClient (#155) ([bd95cea](https://github.com/MirageNet/Mirage/commit/bd95cea4d639753335b21c48781603acd758a9e7)), closes [#155](https://github.com/MirageNet/Mirage/issues/155)
* remove room feature for now (#122) ([87dd495](https://github.com/MirageNet/Mirage/commit/87dd495a6fca6c85349afd42ba6449d98de1f567)), closes [#122](https://github.com/MirageNet/Mirage/issues/122)
* Server Disconnect is now an event not a message (#121) ([82ebd71](https://github.com/MirageNet/Mirage/commit/82ebd71456cbd2e819540d961a93814c57735784)), closes [#121](https://github.com/MirageNet/Mirage/issues/121)
* remove OnClientStart virtual ([eb5242d](https://github.com/MirageNet/Mirage/commit/eb5242d63fa011381e7692470713fd144476454a))
* Move on client stop (#118) ([678e386](https://github.com/MirageNet/Mirage/commit/678e3867a9f232e52d2a6cdbfae8140b0e82bd11)), closes [#118](https://github.com/MirageNet/Mirage/issues/118)
* merge clientscene and networkclient (#84) ([dee1046](https://github.com/MirageNet/Mirage/commit/dee10460325119337401dc4d237dec8bfb9ddb29)), closes [#84](https://github.com/MirageNet/Mirage/issues/84)
* removed obsoletes (#1542) ([4faec29](https://github.com/MirageNet/Mirage/commit/4faec295593b81a709a57aaf374bb5b080a04538)), closes [#1542](https://github.com/MirageNet/Mirage/issues/1542)
* Assign/Remove client authority now throws exception ([7679d3b](https://github.com/MirageNet/Mirage/commit/7679d3bef369de5245fd301b33e85dbdd74e84cd))
* Removed LLAPI ([b0c936c](https://github.com/MirageNet/Mirage/commit/b0c936cb7d1a803b7096806a905a4c121e45bcdf))
* Simplify unpacking messages (#65) ([c369da8](https://github.com/MirageNet/Mirage/commit/c369da84dc34dbbde68a7b30758a6a14bc2573b1)), closes [#65](https://github.com/MirageNet/Mirage/issues/65)
* Remove all compiler defines ([a394345](https://github.com/MirageNet/Mirage/commit/a3943459598d30a325fb1e1315b84c0dedf1741c))
* Simplify AddPlayerForConnection (#62) ([fb26755](https://github.com/MirageNet/Mirage/commit/fb267557af292e048df248d4f85fff3569ac2963)), closes [#62](https://github.com/MirageNet/Mirage/issues/62)
* Renamed localEulerAnglesSensitivity (#1474) ([eee9692](https://github.com/MirageNet/Mirage/commit/eee969201d69df1e1ee1f1623b55a78f6003fbb1)), closes [#1474](https://github.com/MirageNet/Mirage/issues/1474)
* Rename NetworkServer.localClientActive ([7cd0894](https://github.com/MirageNet/Mirage/commit/7cd0894853b97fb804ae15b8a75b75c9d7bc04a7))
* Simplify spawning ([c87a38a](https://github.com/MirageNet/Mirage/commit/c87a38a4ff0c350901138b90db7fa8e61b1ab7db))
* Merge pull request #650 from vis2k/networkclient_static_2 ([fac0542](https://github.com/MirageNet/Mirage/commit/fac05428cc7f49f53d2322a010d61b61349241ef)), closes [#650](https://github.com/MirageNet/Mirage/issues/650)


### Reverts

* Revert "Revert "Explain why 10"" ([d727e4f](https://github.com/MirageNet/Mirage/commit/d727e4fd4b9e811025c7309efeba090e3ac14ccd))
* Revert "Revert "perf: faster NetworkWriter pooling (#1616)"" ([20e9e5d](https://github.com/MirageNet/Mirage/commit/20e9e5dab0dfb8a67d11d84152b0580ea5370551)), closes [#1616](https://github.com/MirageNet/Mirage/issues/1616)
* Revert "fix: replacing the player does not mean giving up authority (#1254)" ([b8618d3](https://github.com/MirageNet/Mirage/commit/b8618d356f0eeb7aa7bde5ea41c56d7a2cdb3373)), closes [#1254](https://github.com/MirageNet/Mirage/issues/1254)
* Revert "feat: Add Timeout to NetworkAuthenticator (#1091)" ([12c5a8f](https://github.com/MirageNet/Mirage/commit/12c5a8fdc30280b9ad113c3a8116b2d046d3b31f)), closes [#1091](https://github.com/MirageNet/Mirage/issues/1091)
* Revert "Remove add component from deprecated components (#1087)" ([9f09c21](https://github.com/MirageNet/Mirage/commit/9f09c216807271e5fc89aa441fa95af0b0eaf80c)), closes [#1087](https://github.com/MirageNet/Mirage/issues/1087)
* Revert "Inserted blank lines where appropriate" ([913d503](https://github.com/MirageNet/Mirage/commit/913d503fdade774e668ceb43d9397b86870d99cd))
* Revert "feat: Custom readers and writers" ([07ef8c9](https://github.com/MirageNet/Mirage/commit/07ef8c91c0931628adb589b67893a80145134c15))
* Revert "New Basic Example (#985)" ([35b9919](https://github.com/MirageNet/Mirage/commit/35b9919d91a9b942b133426eed0d45733f48cd6b)), closes [#985](https://github.com/MirageNet/Mirage/issues/985)
* Revert "fix: reduce allocations at the transport" ([bb128fe](https://github.com/MirageNet/Mirage/commit/bb128fe3b5f05becaf7ea0546c9198707d2c76ba))
* Revert "doc: messages can be struct now" ([79f7c81](https://github.com/MirageNet/Mirage/commit/79f7c815207e18e35fddf9c2e528178eb88decd9))
* Revert "ClientScene.OnSpawnPrefab and NetworkManager.OnServerAddPlayerInternal: spawn objects with prefab names to avoid unnecessary "(Clone)" suffix from Unity. otherwise we need a name sync component in all games that show the names, e.g. MMOs for all monsters. This way we only need name sync components for objects that actually do change names, e.g. players." because of issue #426 ([82d4cf0](https://github.com/MirageNet/Mirage/commit/82d4cf08ff187c002d5ddf04b7b88d72709b3a44)), closes [#426](https://github.com/MirageNet/Mirage/issues/426)
* Revert "Source based weaver (#319)" ([9b232b0](https://github.com/MirageNet/Mirage/commit/9b232b05517bc215e005a333aafcb92cdb832d60)), closes [#319](https://github.com/MirageNet/Mirage/issues/319)
* Revert "Code style and comment typo fix." (#386) ([dc3b767](https://github.com/MirageNet/Mirage/commit/dc3b767743309c414b5e88ba3443f82b93dab0e2)), closes [#386](https://github.com/MirageNet/Mirage/issues/386)
* Revert "Onserialize improvements (#302)" ([00a3610](https://github.com/MirageNet/Mirage/commit/00a36109a0a0ebad860a0f37245a22de3cd05f70)), closes [#302](https://github.com/MirageNet/Mirage/issues/302)
* Revert "Documented the attributes." ([9e3dcc7](https://github.com/MirageNet/Mirage/commit/9e3dcc7acdded4980b5ccef3a3b3104e9c27d80a))
* Revert "Documented NetworkBehaviour and NetworkIdentity." ([a5cfc81](https://github.com/MirageNet/Mirage/commit/a5cfc810dbddb7aabada07c0200d0d52f743a2d6))
* Revert "Documented NetworkManager." ([5bc44a9](https://github.com/MirageNet/Mirage/commit/5bc44a97398a476139ef4aebcdf024921b8d1f18))
* Revert "Don't generate OnSerialize/OnDeserialize if not needed (#199)" (#217) ([40a3ecc](https://github.com/MirageNet/Mirage/commit/40a3ecce083e021adaeb56daf2653fb89a0e08b0)), closes [#199](https://github.com/MirageNet/Mirage/issues/199) [#217](https://github.com/MirageNet/Mirage/issues/217)
* Revert "Don't require Cmd, Rpc and Target prefixes (#127)" ([96992c3](https://github.com/MirageNet/Mirage/commit/96992c35bce04c07c4cbdd29c7ea534dc096fdc7)), closes [#127](https://github.com/MirageNet/Mirage/issues/127)


### BREAKING CHANGES

* Remove redundant scene ready value
* Removed [TargetRpc],  use [ClientR(target=Client.Owner)] instead
* Removed websocket transport
* rename AsyncMultiplexTransport -> MultiplexTransport
* rename AsyncFallbackTransport -> FallbackTransport
* rename AsyncWsTransport -> WsTransport
* [LocalPlayerCallback] is now [LocalPlayer(error = false)]

* Local Player guard

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>
* [ClientCallback] is now [Client(error = false)]
* [HasAuthorityCallback] is now [HasAuthority(error = false)]

* fix test

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>
* [ServerCallback] is now [Server(error = false)]

* fixed weaver test

* Remove unused code

* fix comment

* document replacement of ServerCallback

* No need to be serializable

* Exception should be serializable?

* Fix code smell

* No need to implement interface,  parent does

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>
* Renamed [Command] to [ServerRpc]
* NetworkManager no longer handles scene changes
* Remove isAuthenticated

* Fix typo

* Fix smells

* Remove smells
* NetworkConneciton.RegisterHandler only needs message class
* NetworkConnectionToClient and networkConnectionToServer are gone
* connecition Id is gone
* websocket transport does not work,  needs to be replaced.
* NetworkManager no longer has OnServerStart virtual
* NetworkManager no longer spawns the player.  You need to add PlayerSpawner component if you want that behavior
* You will need to reassign your scenes after upgrade

* Automatically fix properties that were using name

If you open a NetworkManager or other gameobject that uses a scene name
it now gets converted to scene path by the SceneDrawer

* Use get scene by name

* Scene can never be null

* Update Assets/Mirror/Examples/AdditiveScenes/Scenes/MainScene.unity

* Issue warning if we drop the scene

* Issue error if scene is lost

* Add suggestion on how to fix the error

* Keep backwards compatibility, check for scene name

* cache the active scene

* Update Assets/Mirror/Editor/SceneDrawer.cs

Co-Authored-By: James Frowen <jamesfrowendev@gmail.com>

* GetSceneByName only works if scene is loaded

* Remove unused using

Co-authored-by: James Frowen <jamesfrowendev@gmail.com>
* Room feature and example are gone
* offline/online scenes are gone
* OnServerDisconnect is gone
* Removed OnStartClient virtual,  use event from NetworkClient instead
* OnStopClient virtual is replaced by event in Client
* SyncDictionary callback has been replaced
* callback signature changed in SyncSet
* Sync lists no longer have a Callback event with an operation enum
* NetworkBehavior no longer has virtuals for lifecycle events
* ClientScene is gone
* removed obsoletes
* NetworkTime.Time is no longer static
* observers is now a set of connections, not a dictionary
* cannot pass GameObjects and NetworkIdentities anymore.
Will be restored in the future.
* StartClient now receives the server ip
* NetworkManager no longer has NetworkAddress
* Assign/Remove client authority throws exception instead of returning boolean
* Removed LLAPITransport
* MessagePacker.UnpackMessage replaced by UnpackId
* removed compilation defines,  use upm version defines instead
* AddPlayerForConnection no longer receives the client

* fix compilatio errors

* fix build errors
* ClientConnect replaced with ClientConnectAsync
that can be awaited

* fix: TCP transport for async compliance

* use async connect

* Ignore telepathy tests until they are fixed

* It is ok to interrupt sockets

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings
* localEulerAnglesSensitivity renamed to localRotationSensitivity
* Make the server uri method mandatory in transports

Co-authored-by: MrGadget <chris@clevertech.net>
* rename localClientActive to LocalClientActive
* Spawn no longer receives NetworkClient
* Remove obsolete OnServerAddPlayer

Any person that overrides this method has a broken game.  These methods are never called anywhere.

The user gets a warning because they are overriding an obsolete method,  which might get ignored if they have lots of other warnings.   They would run their game and their game would not work at all.  No errors.

By removing these methods, users that override these methods will get a compile time error instead.  So they cannot ignore this error and they will fix it.

* Method is no longer available in NetworkLobbyManager
* Make NetworkClient Great Again!

## [43.3.2](https://github.com/MirageNet/Mirage/compare/43.3.1-master...43.3.2-master) (2020-08-10)


### Bug Fixes

* add NRE short circuit for scene change ([#335](https://github.com/MirageNet/Mirage/issues/335)) ([7afbe57](https://github.com/MirageNet/Mirage/commit/7afbe57ff3779ba33d225ab604f1477a883badd7))

## [43.3.1](https://github.com/MirageNet/Mirage/compare/43.3.0-master...43.3.1-master) (2020-08-07)


### Bug Fixes

* remove unused events ([#334](https://github.com/MirageNet/Mirage/issues/334)) ([c20f6de](https://github.com/MirageNet/Mirage/commit/c20f6de07ff97960a8cf9972fbb4d4d13b507b3b))

# [43.3.0](https://github.com/MirageNet/Mirage/compare/43.2.1-master...43.3.0-master) (2020-08-04)


### Features

* add IChannelConnection interface for transports with channels ([#332](https://github.com/MirageNet/Mirage/issues/332)) ([887118e](https://github.com/MirageNet/Mirage/commit/887118e2d20009c97d0732f6176c72484780b5bb))

## [43.2.1](https://github.com/MirageNet/Mirage/compare/43.2.0-master...43.2.1-master) (2020-08-02)


### Bug Fixes

* Dont allow null connections ([#323](https://github.com/MirageNet/Mirage/issues/323)) ([44fef7e](https://github.com/MirageNet/Mirage/commit/44fef7ec7bd6ae0772414ff28bb78bf42a6b4c92))

# [43.2.0](https://github.com/MirageNet/Mirage/compare/43.1.4-master...43.2.0-master) (2020-08-01)


### Features

* HeadlessAutoStart and HeadlessFrameLimiter ([#318](https://github.com/MirageNet/Mirage/issues/318)) ([ce6ef50](https://github.com/MirageNet/Mirage/commit/ce6ef50c37690623a5dcafc96cc949966ed6363b))

## [43.1.4](https://github.com/MirageNet/Mirage/compare/43.1.3-master...43.1.4-master) (2020-08-01)


### Bug Fixes

* moved SpawnObjects call for hostmode to after LocalClient Connected ([#317](https://github.com/MirageNet/Mirage/issues/317)) ([1423a6d](https://github.com/MirageNet/Mirage/commit/1423a6d0160c5d14a0ee6bad84973df73956fc05))

## [43.1.3](https://github.com/MirageNet/Mirage/compare/43.1.2-master...43.1.3-master) (2020-07-31)


### Bug Fixes

* ClientRPC should skip first arg only if set as Connection ([#315](https://github.com/MirageNet/Mirage/issues/315)) ([168e622](https://github.com/MirageNet/Mirage/commit/168e6222e759016b588e994b76d2f134c9224b0b))

## [43.1.2](https://github.com/MirageNet/Mirage/compare/43.1.1-master...43.1.2-master) (2020-07-28)


### Bug Fixes

* ui bug where additive button is not reset ([#311](https://github.com/MirageNet/Mirage/issues/311)) ([5effce9](https://github.com/MirageNet/Mirage/commit/5effce9abcea0274412cb97100e1f06e4ae01028))

## [43.1.1](https://github.com/MirageNet/Mirage/compare/43.1.0-master...43.1.1-master) (2020-07-22)


### Bug Fixes

* remove unused code ([#308](https://github.com/MirageNet/Mirage/issues/308)) ([554d2c5](https://github.com/MirageNet/Mirage/commit/554d2c5030a9ff1ebcd9ca17ed7d673865709a1c))
* update interfaces for recent changes that were missed ([#309](https://github.com/MirageNet/Mirage/issues/309)) ([a17e760](https://github.com/MirageNet/Mirage/commit/a17e760e36d581ba964120af11678b66a1248ecc))

# [43.1.0](https://github.com/MirageNet/Mirage/compare/43.0.1-master...43.1.0-master) (2020-07-22)


### Bug Fixes

* add NetworkManager.StartClientUri test ([#2095](https://github.com/MirageNet/Mirage/issues/2095)) ([12827f6](https://github.com/MirageNet/Mirage/commit/12827f65a906232da55ca226129423a5bd806d23))
* dont allow set of networkSceneName directly ([#2100](https://github.com/MirageNet/Mirage/issues/2100)) ([df16a7d](https://github.com/MirageNet/Mirage/commit/df16a7d3ccfddcf3aa1a68fe0965757d91363e16))
* fixing cloud scripts not pinging api ([#2097](https://github.com/MirageNet/Mirage/issues/2097)) ([8e545ac](https://github.com/MirageNet/Mirage/commit/8e545ac46863e4fbe874c70bf9559c9b12de83d4))
* Misc code smells ([#2094](https://github.com/MirageNet/Mirage/issues/2094)) ([e4cc85c](https://github.com/MirageNet/Mirage/commit/e4cc85c413eed01950bf9dddf0bdac2afd8ab479))
* register prefab error with same guid ([#2092](https://github.com/MirageNet/Mirage/issues/2092)) ([984eb73](https://github.com/MirageNet/Mirage/commit/984eb73ea495cf876446a21775fde5c33119695b))
* renaming call/invoke prefix for SyncEvent ([#2089](https://github.com/MirageNet/Mirage/issues/2089)) ([18d6957](https://github.com/MirageNet/Mirage/commit/18d695744f7c253d749792e4f9f8759ef16fcbab)), closes [#2088](https://github.com/MirageNet/Mirage/issues/2088)


### Features

* Rigidbody example ([#2076](https://github.com/MirageNet/Mirage/issues/2076)) ([ef47ee7](https://github.com/MirageNet/Mirage/commit/ef47ee7a57bddcdc669aef32fbeffcd4446f98a8))

## [43.0.1](https://github.com/MirageNet/Mirage/compare/43.0.0-master...43.0.1-master) (2020-07-20)


### Bug Fixes

* benchmark examples ([b221b74](https://github.com/MirageNet/Mirage/commit/b221b74beae2ee56f6fe536963b17d0aff10c5d8))

# [43.0.0](https://github.com/MirageNet/Mirage/compare/42.2.0-master...43.0.0-master) (2020-07-20)


### breaking

* Remove TargetRPC & use ClientRPC option instead ([#293](https://github.com/MirageNet/Mirage/issues/293)) ([4ace144](https://github.com/MirageNet/Mirage/commit/4ace14477d024d0ef763c0860cdb2abfde8022fd))


### BREAKING CHANGES

* Removed [TargetRpc],  use [ClientR(target=Client.Owner)] instead

# [42.2.0](https://github.com/MirageNet/Mirage/compare/42.1.5-master...42.2.0-master) (2020-07-19)


### Features

* scene transition uses routine instead of asyncoperation ([#305](https://github.com/MirageNet/Mirage/issues/305)) ([a16eb00](https://github.com/MirageNet/Mirage/commit/a16eb005e31e576346b4b4cffbe266c25b8709ca))

## [42.1.5](https://github.com/MirageNet/Mirage/compare/42.1.4-master...42.1.5-master) (2020-07-18)


### Bug Fixes

* NS call SpawnObjects. No NetMan dependency for spawning objects ([#300](https://github.com/MirageNet/Mirage/issues/300)) ([e1bb8de](https://github.com/MirageNet/Mirage/commit/e1bb8deba81713c8998cf47b1ec4b8b870fc55eb))

## [42.1.4](https://github.com/MirageNet/Mirage/compare/42.1.3-master...42.1.4-master) (2020-07-18)


### Bug Fixes

* cap spawned to match client ([#301](https://github.com/MirageNet/Mirage/issues/301)) ([7d1571a](https://github.com/MirageNet/Mirage/commit/7d1571ab5a9eaf31cd64bff2bc47158c0e1e6ff6))

## [42.1.3](https://github.com/MirageNet/Mirage/compare/42.1.2-master...42.1.3-master) (2020-07-17)


### Bug Fixes

* dont directly set NetworkSceneName ([#297](https://github.com/MirageNet/Mirage/issues/297)) ([bd043a3](https://github.com/MirageNet/Mirage/commit/bd043a3001775fe32da558e17566b61c5694ee7c))

## [42.1.2](https://github.com/MirageNet/Mirage/compare/42.1.1-master...42.1.2-master) (2020-07-17)


### Bug Fixes

* ClientSceneManager should be responsible for its own cleanup ([#298](https://github.com/MirageNet/Mirage/issues/298)) ([92ab3ff](https://github.com/MirageNet/Mirage/commit/92ab3ffe265e72b3c012dc44075f6e9752323984))

## [42.1.1](https://github.com/MirageNet/Mirage/compare/42.1.0-master...42.1.1-master) (2020-07-16)


### Bug Fixes

* dont register client scene handlers while host ([#296](https://github.com/MirageNet/Mirage/issues/296)) ([37c8ddd](https://github.com/MirageNet/Mirage/commit/37c8ddd87595143149af942dc7e5654de3eef424))

# [42.1.0](https://github.com/MirageNet/Mirage/compare/42.0.0-master...42.1.0-master) (2020-07-15)


### Features

* Make AsyncQueue public for transports ([5df0d98](https://github.com/MirageNet/Mirage/commit/5df0d98307eff409dd16e67fddedb25710b68b6d))

# [42.0.0](https://github.com/MirageNet/Mirage/compare/41.1.0-master...42.0.0-master) (2020-07-15)


### breaking

* Removed websocket transport ([f26159b](https://github.com/MirageNet/Mirage/commit/f26159b7b4d31d643a1dc2a28b1797bd2ad28f68))


### BREAKING CHANGES

* Removed websocket transport

# [41.1.0](https://github.com/MirageNet/Mirage/compare/41.0.0-master...41.1.0-master) (2020-07-15)


### Features

* Transports can have multiple uri ([#292](https://github.com/MirageNet/Mirage/issues/292)) ([155a29c](https://github.com/MirageNet/Mirage/commit/155a29c053421f870241a75427db748fbef08910))

# [41.0.0](https://github.com/MirageNet/Mirage/compare/40.3.0-master...41.0.0-master) (2020-07-15)


### breaking

* AsyncFallbackTransport -> FallbackTransport ([f8f643a](https://github.com/MirageNet/Mirage/commit/f8f643a6245777279de31dc8997a7ea84328533e))
* AsyncMultiplexTransport -> MultiplexTransport ([832b7f9](https://github.com/MirageNet/Mirage/commit/832b7f9528595e45769790c4be4fd94e873c96f4))
* rename AsyncWsTransport -> WsTransport ([9c394bc](https://github.com/MirageNet/Mirage/commit/9c394bc96192a50ad273371b66c9289d75402dc6))


### Features

* Transports may support any number of schemes ([#291](https://github.com/MirageNet/Mirage/issues/291)) ([2af7b9d](https://github.com/MirageNet/Mirage/commit/2af7b9d19cef3878147eee412adf2b9b32c91147))


### BREAKING CHANGES

* rename AsyncMultiplexTransport -> MultiplexTransport
* rename AsyncFallbackTransport -> FallbackTransport
* rename AsyncWsTransport -> WsTransport

# [40.3.0](https://github.com/MirageNet/Mirage/compare/40.2.0-master...40.3.0-master) (2020-07-14)


### Bug Fixes

* SceneManager Exceptions and Tests ([#287](https://github.com/MirageNet/Mirage/issues/287)) ([388d218](https://github.com/MirageNet/Mirage/commit/388d21872bb8b4c7f9d3742ecfa9b857ee734dfa))


### Features

* Server and Client share the same scene loading method ([#286](https://github.com/MirageNet/Mirage/issues/286)) ([acb6dd1](https://github.com/MirageNet/Mirage/commit/acb6dd192244adcfab15d013a96c7402151d226b))

# [40.2.0](https://github.com/MirageNet/Mirage/compare/40.1.1-master...40.2.0-master) (2020-07-14)


### Features

* additive scene msging added to server ([#285](https://github.com/MirageNet/Mirage/issues/285)) ([bd7a17a](https://github.com/MirageNet/Mirage/commit/bd7a17a65fbc9aed64aaef6c65641697e8d89a74))

## [40.1.1](https://github.com/MirageNet/Mirage/compare/40.1.0-master...40.1.1-master) (2020-07-14)


### Bug Fixes

* prevent NRE when operating as a separated client and server ([#283](https://github.com/MirageNet/Mirage/issues/283)) ([e10e198](https://github.com/MirageNet/Mirage/commit/e10e198b4865fc8c941244c3e368eebc6cf73179))

# [40.1.0](https://github.com/MirageNet/Mirage/compare/40.0.0-master...40.1.0-master) (2020-07-14)


### Features

* Transports can tell if they are supported ([#282](https://github.com/MirageNet/Mirage/issues/282)) ([890c6b8](https://github.com/MirageNet/Mirage/commit/890c6b8808ccbf4f4ffffae8c00a9d897ccac7e4))

# [40.0.0](https://github.com/MirageNet/Mirage/compare/39.0.0-master...40.0.0-master) (2020-07-14)


### Features

* LocalPlayer attribute now throws error ([#277](https://github.com/MirageNet/Mirage/issues/277)) ([15aa537](https://github.com/MirageNet/Mirage/commit/15aa537947cd14e4d71853f1786c387519d8828b))


### BREAKING CHANGES

* [LocalPlayerCallback] is now [LocalPlayer(error = false)]

* Local Player guard

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>

# [39.0.0](https://github.com/MirageNet/Mirage/compare/38.0.0-master...39.0.0-master) (2020-07-14)


### Features

* Client attribute now throws error ([#274](https://github.com/MirageNet/Mirage/issues/274)) ([f1b52f3](https://github.com/MirageNet/Mirage/commit/f1b52f3d23e9aa50b5fab8509f3c769e97eac2e7))


### BREAKING CHANGES

* [ClientCallback] is now [Client(error = false)]

# [38.0.0](https://github.com/MirageNet/Mirage/compare/37.0.1-master...38.0.0-master) (2020-07-14)


### Features

* HasAuthority attribute now throws error ([#276](https://github.com/MirageNet/Mirage/issues/276)) ([da2355b](https://github.com/MirageNet/Mirage/commit/da2355b4c1a51dbcbf6ceb405b6fc7b5bb14fa14))


### BREAKING CHANGES

* [HasAuthorityCallback] is now [HasAuthority(error = false)]

* fix test

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>

## [37.0.1](https://github.com/MirageNet/Mirage/compare/37.0.0-master...37.0.1-master) (2020-07-14)


### Bug Fixes

* smell cleanup left if bug. repaired with parenthesis. ([#275](https://github.com/MirageNet/Mirage/issues/275)) ([dd52be3](https://github.com/MirageNet/Mirage/commit/dd52be3bb9406de7b2527c72fce90c9ed6c9d5bf))

# [37.0.0](https://github.com/MirageNet/Mirage/compare/36.0.0-master...37.0.0-master) (2020-07-13)


### Features

* Server attribute now throws error ([#270](https://github.com/MirageNet/Mirage/issues/270)) ([f3b5dc4](https://github.com/MirageNet/Mirage/commit/f3b5dc4fef5fba05e585d274d9df05c3954ff6c7))


### BREAKING CHANGES

* [ServerCallback] is now [Server(error = false)]

* fixed weaver test

* Remove unused code

* fix comment

* document replacement of ServerCallback

* No need to be serializable

* Exception should be serializable?

* Fix code smell

* No need to implement interface,  parent does

Co-authored-by: Paul Pacheco <paul.pacheco@aa.com>

# [36.0.0](https://github.com/MirageNet/Mirage/compare/35.3.4-master...36.0.0-master) (2020-07-13)


### breaking

* Rename [Command] to [ServerRpc] ([#271](https://github.com/MirageNet/Mirage/issues/271)) ([fff7459](https://github.com/MirageNet/Mirage/commit/fff7459801fc637c641757c516f85b4d685e0ad1))


### BREAKING CHANGES

* Renamed [Command] to [ServerRpc]

## [35.3.4](https://github.com/MirageNet/Mirage/compare/35.3.3-master...35.3.4-master) (2020-07-13)


### Bug Fixes

* add tests for NetworkTransform and NetworkRigidbody ([#273](https://github.com/MirageNet/Mirage/issues/273)) ([e9621dd](https://github.com/MirageNet/Mirage/commit/e9621ddebd50637680fad8fe743c7c99afea3f84))
* NinjaWS code smells ([#272](https://github.com/MirageNet/Mirage/issues/272)) ([71d9428](https://github.com/MirageNet/Mirage/commit/71d942804c0d404f287dc51b7bcdd1fcc39bcee8))

## [35.3.3](https://github.com/MirageNet/Mirage/compare/35.3.2-master...35.3.3-master) (2020-07-13)


### Bug Fixes

* Misc code smells ([#269](https://github.com/MirageNet/Mirage/issues/269)) ([23dcca6](https://github.com/MirageNet/Mirage/commit/23dcca61ff7c41e8b9f61579605fd56ee82c70e0))

## [35.3.2](https://github.com/MirageNet/Mirage/compare/35.3.1-master...35.3.2-master) (2020-07-13)


### Bug Fixes

* remove customHandling as its no longer used ([#265](https://github.com/MirageNet/Mirage/issues/265)) ([dbd9d84](https://github.com/MirageNet/Mirage/commit/dbd9d84ee46ac90a8d78daba0c23fc9be71ca77d))

## [35.3.1](https://github.com/MirageNet/Mirage/compare/35.3.0-master...35.3.1-master) (2020-07-13)


### Bug Fixes

* AdditiveSceneExample missing comp and assignments ([#267](https://github.com/MirageNet/Mirage/issues/267)) ([ab394b8](https://github.com/MirageNet/Mirage/commit/ab394b8f7e823b8c3882de35eaa54c05fbd9316e))
* NRE on gamemanager in scene ([#268](https://github.com/MirageNet/Mirage/issues/268)) ([58a124a](https://github.com/MirageNet/Mirage/commit/58a124a99c267091142f00adc7f8898160a9dd97))

# [35.3.0](https://github.com/MirageNet/Mirage/compare/35.2.0-master...35.3.0-master) (2020-07-13)


### Bug Fixes

* Message base class not being Serialized if processed in the wrong order ([#2023](https://github.com/MirageNet/Mirage/issues/2023)) ([3418fa2](https://github.com/MirageNet/Mirage/commit/3418fa210602cf1a9b9331b198ac47d8a3cabe69))
* not removing server if id is empty ([#2078](https://github.com/MirageNet/Mirage/issues/2078)) ([f717945](https://github.com/MirageNet/Mirage/commit/f7179455256bb7341ffa9e6921fe0de50498150a))


### Features

* ClientRpc no longer need Rpc prefix ([#2086](https://github.com/MirageNet/Mirage/issues/2086)) ([eb93c34](https://github.com/MirageNet/Mirage/commit/eb93c34b330189c79727b0332bb66f3675cfd641))
* Commands no longer need Cmd prefix ([#2084](https://github.com/MirageNet/Mirage/issues/2084)) ([b6d1d09](https://github.com/MirageNet/Mirage/commit/b6d1d09f846f7cf0310db0db9d931a9cfbbb36b2))
* Sync Events no longer need Event prefix ([#2087](https://github.com/MirageNet/Mirage/issues/2087)) ([ed40c2d](https://github.com/MirageNet/Mirage/commit/ed40c2d210f174f1ed50b1e929e4fb161414f228))
* TargetRpc no longer need Target prefix ([#2085](https://github.com/MirageNet/Mirage/issues/2085)) ([d89ac9f](https://github.com/MirageNet/Mirage/commit/d89ac9fb052c17c2edfdf381aff35f70d23f4f0a))


### Performance Improvements

* Use invokeRepeating instead of Update ([#2066](https://github.com/MirageNet/Mirage/issues/2066)) ([264f9b8](https://github.com/MirageNet/Mirage/commit/264f9b8f945f0294a8420202abcd0c80e27e6ee6))

# [35.2.0](https://github.com/MirageNet/Mirage/compare/35.1.0-master...35.2.0-master) (2020-07-12)


### Bug Fixes

* add client only test for FinishLoadScene ([#262](https://github.com/MirageNet/Mirage/issues/262)) ([50e7fa6](https://github.com/MirageNet/Mirage/commit/50e7fa6e287fee09afbe36a51575f41c4bd50736))


### Features

* Commands no longer need to start with Cmd ([#263](https://github.com/MirageNet/Mirage/issues/263)) ([9578e19](https://github.com/MirageNet/Mirage/commit/9578e19ff70bf3a09a9fe31926c8ac337f945ba9))
* throw exception if assigning incorrect asset id ([#250](https://github.com/MirageNet/Mirage/issues/250)) ([7741fb1](https://github.com/MirageNet/Mirage/commit/7741fb1f11abc8eb2aec8c1a94ac53380ac5a562))

# [35.1.0](https://github.com/MirageNet/Mirage/compare/35.0.3-master...35.1.0-master) (2020-07-12)


### Features

* Add Network Menu  ([#253](https://github.com/MirageNet/Mirage/issues/253)) ([d81f444](https://github.com/MirageNet/Mirage/commit/d81f444c42475439d24bf5b4abd2bbf15fd34e92))

## [35.0.3](https://github.com/MirageNet/Mirage/compare/35.0.2-master...35.0.3-master) (2020-07-11)


### Bug Fixes

* code smell rename Ready ([#256](https://github.com/MirageNet/Mirage/issues/256)) ([6d92d14](https://github.com/MirageNet/Mirage/commit/6d92d1482cdd31fa663f7475f103476c65b7d875))
* Misc Code Smells ([#257](https://github.com/MirageNet/Mirage/issues/257)) ([278a127](https://github.com/MirageNet/Mirage/commit/278a1279dabefe04b0829015841de68b41e60a7b))

## [35.0.2](https://github.com/MirageNet/Mirage/compare/35.0.1-master...35.0.2-master) (2020-07-11)


### Bug Fixes

* cleanup the server even after error ([#255](https://github.com/MirageNet/Mirage/issues/255)) ([7bd015e](https://github.com/MirageNet/Mirage/commit/7bd015eac1b77f0ad5974abb5c4c87a5d3da7b6d))

## [35.0.1](https://github.com/MirageNet/Mirage/compare/35.0.0-master...35.0.1-master) (2020-07-11)


### Bug Fixes

* fix adding and saving Components ([2de7ecd](https://github.com/MirageNet/Mirage/commit/2de7ecd93029bf5fd2fbb04ad4e47936cbb802cc))

# [35.0.0](https://github.com/MirageNet/Mirage/compare/34.13.0-master...35.0.0-master) (2020-07-10)


### Features

* Component based NetworkSceneManager ([#244](https://github.com/MirageNet/Mirage/issues/244)) ([7579d71](https://github.com/MirageNet/Mirage/commit/7579d712ad97db98cd729c51568631e4c3257b58))


### BREAKING CHANGES

* NetworkManager no longer handles scene changes

# [34.13.0](https://github.com/MirageNet/Mirage/compare/34.12.0-master...34.13.0-master) (2020-07-05)


### Features

* Spawn objects in clients in same order as server ([#247](https://github.com/MirageNet/Mirage/issues/247)) ([b786646](https://github.com/MirageNet/Mirage/commit/b786646f1859bb0e1836460c6319a507e1cc31aa))

# [34.12.0](https://github.com/MirageNet/Mirage/compare/34.11.0-master...34.12.0-master) (2020-07-04)


### Features

* Example with 10k monster that change unfrequently ([2b2e71c](https://github.com/MirageNet/Mirage/commit/2b2e71cc007dba2c1d90b565c4983814c1e0b7d1))

# [34.11.0](https://github.com/MirageNet/Mirage/compare/34.10.1-master...34.11.0-master) (2020-07-04)


### Bug Fixes

* addingNetwork rigidbody icon and AddComponentMenu attribute ([#2051](https://github.com/MirageNet/Mirage/issues/2051)) ([ab1b92f](https://github.com/MirageNet/Mirage/commit/ab1b92f74b56787feb7c6fde87c0b9838b8d9d0f))
* calling base method when the first base class did not have the virtual method ([#2014](https://github.com/MirageNet/Mirage/issues/2014)) ([4af72c3](https://github.com/MirageNet/Mirage/commit/4af72c3a63e72dac6b3bab193dc58bfa3c44a975))
* changing namespace to match folder name ([#2037](https://github.com/MirageNet/Mirage/issues/2037)) ([e36449c](https://github.com/MirageNet/Mirage/commit/e36449cb22d8a2dede0133cf229bc12885c36bdb))
* Clean up roomSlots on clients in NetworkRoomPlayer ([5032ceb](https://github.com/MirageNet/Mirage/commit/5032ceb00035679e0b80f59e91131cdfa8e0b1bb))
* Fallback and Multiplex now disable their transports when they are disabled  ([#2048](https://github.com/MirageNet/Mirage/issues/2048)) ([61d44b2](https://github.com/MirageNet/Mirage/commit/61d44b2d80c9616f784e855131ba6d1ee8a30136))
* If socket is undefined it will return false. See [#1486](https://github.com/MirageNet/Mirage/issues/1486) ([#2017](https://github.com/MirageNet/Mirage/issues/2017)) ([4ffff19](https://github.com/MirageNet/Mirage/commit/4ffff192a69108b993cf963cfdece47b14ffdbf2))
* Network rigidbody fixes ([#2050](https://github.com/MirageNet/Mirage/issues/2050)) ([0c30d33](https://github.com/MirageNet/Mirage/commit/0c30d3398aaabcbf094a88a9c9c77ab4d5062acf))
* sync events can not have the same name if they are in different classes ([#2054](https://github.com/MirageNet/Mirage/issues/2054)) ([c91308f](https://github.com/MirageNet/Mirage/commit/c91308fb0461e54292940ce6fa42bb6cd9800d89))
* weaver now processes multiple SyncEvent per class ([#2055](https://github.com/MirageNet/Mirage/issues/2055)) ([b316b35](https://github.com/MirageNet/Mirage/commit/b316b35d46868a7e11c7b2005570efeec843efe1))


### Features

* adding demo for mirror cloud services ([#2026](https://github.com/MirageNet/Mirage/issues/2026)) ([f1fdc95](https://github.com/MirageNet/Mirage/commit/f1fdc959dcd62e7228ecfd656bc87cbabca8c1bc))
* adding log handler that sets console color ([#2001](https://github.com/MirageNet/Mirage/issues/2001)) ([4623978](https://github.com/MirageNet/Mirage/commit/46239783f313159ac47e192499aa8e7fcc5df0ec))
* Experimental NetworkRigidbody  ([#1822](https://github.com/MirageNet/Mirage/issues/1822)) ([25285b1](https://github.com/MirageNet/Mirage/commit/25285b1574c4e025373e86735ec3eb9734272fd2))
* More examples for Mirror Cloud Service ([#2029](https://github.com/MirageNet/Mirage/issues/2029)) ([7d0e907](https://github.com/MirageNet/Mirage/commit/7d0e907b73530c9a625eaf663837b7eeb36fcee6))

## [34.10.1](https://github.com/MirageNet/Mirage/compare/34.10.0-master...34.10.1-master) (2020-07-04)


### Bug Fixes

* assign spawn locations and fix null refs in example ([#242](https://github.com/MirageNet/Mirage/issues/242)) ([3adf343](https://github.com/MirageNet/Mirage/commit/3adf3438578ff304f1216022aae8e043c52cd71d))
* folders for meta files no longer in the codebase ([#237](https://github.com/MirageNet/Mirage/issues/237)) ([192fd16](https://github.com/MirageNet/Mirage/commit/192fd1645986c515a804a01e0707c78241882676))
* remove pause network comment and log ([#238](https://github.com/MirageNet/Mirage/issues/238)) ([1a8c09d](https://github.com/MirageNet/Mirage/commit/1a8c09d8a5a8cf70508d4e42e4912e3989478ff1))

# [34.10.0](https://github.com/MirageNet/Mirage/compare/34.9.4-master...34.10.0-master) (2020-07-04)


### Bug Fixes

* [#1659](https://github.com/MirageNet/Mirage/issues/1659) Telepathy LateUpdate processes a limited amount of messages per tick to avoid deadlocks ([#1830](https://github.com/MirageNet/Mirage/issues/1830)) ([d3dccd7](https://github.com/MirageNet/Mirage/commit/d3dccd7a25e4b9171ac04e43a05954b56caefd4b))
* Added ClientOnly check ([fb927f8](https://github.com/MirageNet/Mirage/commit/fb927f814110327867821dac8b0d69ca4251d4f6))
* Adding warning when adding handler with RegisterSpawnHandler if assetid already exists ([#1819](https://github.com/MirageNet/Mirage/issues/1819)) ([7f26329](https://github.com/MirageNet/Mirage/commit/7f26329e2db9d00da04bed40399af053436218bd))
* Adding warning when adding prefab with RegisterPrefab if assetid already exists ([#1828](https://github.com/MirageNet/Mirage/issues/1828)) ([9f59e0c](https://github.com/MirageNet/Mirage/commit/9f59e0c439707d66409a617b8f209187856eaf5f))
* Allowing overrides for virtual commands to call base method ([#1944](https://github.com/MirageNet/Mirage/issues/1944)) ([b92da91](https://github.com/MirageNet/Mirage/commit/b92da91d7a04f41098615ff2e2a35cf7ff479201))
* better error for Command, ClientRpc and TargetRpc marked as abstract ([#1947](https://github.com/MirageNet/Mirage/issues/1947)) ([62257d8](https://github.com/MirageNet/Mirage/commit/62257d8c4fc307ba3e23fbd01dcc950515c31e79))
* Better errors when trying to replace existing assetid ([#1827](https://github.com/MirageNet/Mirage/issues/1827)) ([822b041](https://github.com/MirageNet/Mirage/commit/822b04155def9859b24900c6e55a4253f85ebb3f))
* Cleaning up network objects when server stops ([#1864](https://github.com/MirageNet/Mirage/issues/1864)) ([4c25122](https://github.com/MirageNet/Mirage/commit/4c25122958978557173ec37ca400c47b2d8e834f))
* clear all message handlers on Shutdown ([#1829](https://github.com/MirageNet/Mirage/issues/1829)) ([a6ab352](https://github.com/MirageNet/Mirage/commit/a6ab3527acb9af8f6a68f0151e5231e4ee1a98e9))
* Don't call RegisterClientMessages every scene change ([#1865](https://github.com/MirageNet/Mirage/issues/1865)) ([05c119f](https://github.com/MirageNet/Mirage/commit/05c119f505390094c8f33e11568d40117360c49e))
* Don't call RegisterClientMessages twice ([#1842](https://github.com/MirageNet/Mirage/issues/1842)) ([2a08aac](https://github.com/MirageNet/Mirage/commit/2a08aac7cb8887934eb7eb8c232ce07976defe35))
* Fixed Capitalization ([c45deb8](https://github.com/MirageNet/Mirage/commit/c45deb808e8e01a7b697e703be783aea2799d4d1))
* Fixing ClientScene UnregisterPrefab ([#1815](https://github.com/MirageNet/Mirage/issues/1815)) ([9270765](https://github.com/MirageNet/Mirage/commit/9270765bebf45c34a466694473b43c6d802b99d9))
* Improved error checking for ClientScene.RegisterPrefab ([#1823](https://github.com/MirageNet/Mirage/issues/1823)) ([a0aa4f9](https://github.com/MirageNet/Mirage/commit/a0aa4f9c1425d4eca3fe08cd2d87361f092ded6f))
* Improved error checking for ClientScene.RegisterPrefab with handler ([#1841](https://github.com/MirageNet/Mirage/issues/1841)) ([54071da](https://github.com/MirageNet/Mirage/commit/54071da3afb18d6289de5d0e41dc248e21088641))
* making weaver include public fields in base classes in auto generated Read/Write ([#1977](https://github.com/MirageNet/Mirage/issues/1977)) ([3db57e5](https://github.com/MirageNet/Mirage/commit/3db57e5f61ac0748d3a6296d8ea44c202830796f))
* NetworkRoomManager.minPlayers is now protected so it's available for derived classes. ([3179f08](https://github.com/MirageNet/Mirage/commit/3179f08e3dc11340227a57da0104b1c8d1d7b45d))
* no longer requires hook to be the first overload in a class ([#1913](https://github.com/MirageNet/Mirage/issues/1913)) ([0348699](https://github.com/MirageNet/Mirage/commit/03486997fb0abb91d14f233658d375f21afbc3e3))
* OnClientEnterRoom should only fire on clients ([d9b7bb7](https://github.com/MirageNet/Mirage/commit/d9b7bb735729e68ae399e1175d6c485a873b379e))
* Prevent host client redundantly changing to offline scene ([b4511a0](https://github.com/MirageNet/Mirage/commit/b4511a0637958f10f4a482364c654d1d9add5ef2))
* Removed unnecessary registration of player prefab in NetworkRoomManager ([b2f52d7](https://github.com/MirageNet/Mirage/commit/b2f52d78921ff0136c74bbed0980e3aaf5e2b379))
* Removed unused variable ([ae3dc04](https://github.com/MirageNet/Mirage/commit/ae3dc04fb999c3b7133589ab631c1d23f1a8bdde))
* Replaced Icosphere with centered pivot ([1dc0d98](https://github.com/MirageNet/Mirage/commit/1dc0d9837458c0403916476805f58442ff87e364))
* Replacing ClearDelegates with RemoveDelegates for test ([#1971](https://github.com/MirageNet/Mirage/issues/1971)) ([927c4de](https://github.com/MirageNet/Mirage/commit/927c4dede5930b320537150466e05112ebe70c3e))
* Suppress warning ([fffd462](https://github.com/MirageNet/Mirage/commit/fffd462df8cc1c0265890cdfa4ceb3e24606b1c1))
* Use ReplaceHandler instead of RegisterHandler in NetworkManager ([ffc276c](https://github.com/MirageNet/Mirage/commit/ffc276cb79c4202964275642097451b78a817c8a))
* Websockets Transport now handles being disabled for scene changes ([#1994](https://github.com/MirageNet/Mirage/issues/1994)) ([5480a58](https://github.com/MirageNet/Mirage/commit/5480a583e13b9183a3670450af639f4e766cc358))
* WebSockets: Force KeepAliveInterval to Zero ([9a42fe3](https://github.com/MirageNet/Mirage/commit/9a42fe334251852ab12e721db72cb12e98de82e8))
* Wrong method names in ClientSceneTests ([ab3f353](https://github.com/MirageNet/Mirage/commit/ab3f353b33b3068a6ac1649613a28b0977a72685))


### Features

* Add excludeOwner option to ClientRpc ([#1954](https://github.com/MirageNet/Mirage/issues/1954)) ([864fdd5](https://github.com/MirageNet/Mirage/commit/864fdd5fdce7a35ee4bf553176ed7a4ec3dc0653)), closes [#1963](https://github.com/MirageNet/Mirage/issues/1963) [#1962](https://github.com/MirageNet/Mirage/issues/1962) [#1961](https://github.com/MirageNet/Mirage/issues/1961) [#1960](https://github.com/MirageNet/Mirage/issues/1960) [#1959](https://github.com/MirageNet/Mirage/issues/1959) [#1958](https://github.com/MirageNet/Mirage/issues/1958) [#1957](https://github.com/MirageNet/Mirage/issues/1957) [#1956](https://github.com/MirageNet/Mirage/issues/1956)
* Add NetworkServer.RemovePlayerForConnection ([#1772](https://github.com/MirageNet/Mirage/issues/1772)) ([e3790c5](https://github.com/MirageNet/Mirage/commit/e3790c51eb9b79bebc48522fb832ae39f11d31e2))
* add SyncList.RemoveAll ([#1881](https://github.com/MirageNet/Mirage/issues/1881)) ([eb7c87d](https://github.com/MirageNet/Mirage/commit/eb7c87d15aa2fe0a5b0c08fc9cde0adbeba0b416))
* Added virtual SyncVar hook for index in NetworkRoomPlayer ([0c3e079](https://github.com/MirageNet/Mirage/commit/0c3e079d04a034f4d4ca8a34c88188013f36c377))
* Adding ignoreAuthority Option to Command ([#1918](https://github.com/MirageNet/Mirage/issues/1918)) ([3ace2c6](https://github.com/MirageNet/Mirage/commit/3ace2c6eb68ad94d78c57df6f63107cca466effa))
* Adding onLocalPlayerChanged to ClientScene for when localPlayer is changed ([#1920](https://github.com/MirageNet/Mirage/issues/1920)) ([b4acf7d](https://github.com/MirageNet/Mirage/commit/b4acf7d9a20c05eadba8d433ebfd476a30e914dd))
* adding OnRoomServerPlayersNotReady to NetworkRoomManager that is called when player ready changes and atleast 1 player is not ready ([#1921](https://github.com/MirageNet/Mirage/issues/1921)) ([9ae7fa2](https://github.com/MirageNet/Mirage/commit/9ae7fa2a8c683f5d2a7ebe6c243a2d95acad9683))
* Adding ReplaceHandler functions to NetworkServer and NetworkClient ([#1775](https://github.com/MirageNet/Mirage/issues/1775)) ([877f4e9](https://github.com/MirageNet/Mirage/commit/877f4e9c729e5242d4f8c9653868a3cb27c933db))
* adding script that displays ping ([#1975](https://github.com/MirageNet/Mirage/issues/1975)) ([7e93030](https://github.com/MirageNet/Mirage/commit/7e93030849c98f0bc8d95fa310d208fef3028b29))
* Allowing Multiple Concurrent Additive Scenes ([#1697](https://github.com/MirageNet/Mirage/issues/1697)) ([e32a9b6](https://github.com/MirageNet/Mirage/commit/e32a9b6f0b0744b6bd0eeeb0d9fca0b4dc33cbdf))
* ClientScene uses log window ([b3656a9](https://github.com/MirageNet/Mirage/commit/b3656a9edc5ff00329ce00847671ade7b8f2add2))
* Creating method to replace all log handlers ([#1880](https://github.com/MirageNet/Mirage/issues/1880)) ([d8aaf76](https://github.com/MirageNet/Mirage/commit/d8aaf76fb972dd153f6002edb96cd2b9350e572c))
* Experimental Network Transform ([#1990](https://github.com/MirageNet/Mirage/issues/1990)) ([7e2b733](https://github.com/MirageNet/Mirage/commit/7e2b7338a18855f156e52b663ac24eef153b43a7))
* Improved Log Settings Window Appearance ([#1885](https://github.com/MirageNet/Mirage/issues/1885)) ([69b8451](https://github.com/MirageNet/Mirage/commit/69b845183c099744455e34c6f12e75acecb9098a))
* Improved RoomPayer template ([042b4e1](https://github.com/MirageNet/Mirage/commit/042b4e1965580a4cdbd5ea50b11a1377fe3bf3cc))
* LogSettings that can be saved and included in a build ([#1863](https://github.com/MirageNet/Mirage/issues/1863)) ([fd4357c](https://github.com/MirageNet/Mirage/commit/fd4357cd264b257aa648a26f9392726b2921b870))
* Multiple Concurrent Additive Physics Scenes Example ([#1686](https://github.com/MirageNet/Mirage/issues/1686)) ([87c6ebc](https://github.com/MirageNet/Mirage/commit/87c6ebc5ddf71b3fc358bb1a90bd9ee2470e333c))
* NetworkConnection to client and server use logger framework ([72154f1](https://github.com/MirageNet/Mirage/commit/72154f1daddaa141fb3b8fe02bcfdf098ef1d44a))
* NetworkConnection uses logging framework ([ec319a1](https://github.com/MirageNet/Mirage/commit/ec319a165dc8445b00b096d09061adda2c7b8b9d))
* NetworkIdentity use logger framework ([2e39e13](https://github.com/MirageNet/Mirage/commit/2e39e13c012aa79d50a54fc5d07b85da3e52391b))
* NetworkServer uses new logging framework ([8b4f105](https://github.com/MirageNet/Mirage/commit/8b4f1051f27f1d5b845e6bd0a090368082ab1603))
* Prettify Log Names ([c7d8c09](https://github.com/MirageNet/Mirage/commit/c7d8c0933d37abc919305b660cbf3a57828eaace))
* Use SortedDictionary for LogSettings ([#1914](https://github.com/MirageNet/Mirage/issues/1914)) ([7d4c0a9](https://github.com/MirageNet/Mirage/commit/7d4c0a9cb6f24fa3c2834b9bf351e30dde88665f))


### Performance Improvements

* NetworkProximityChecker checks Server.connections instead of doing 10k sphere casts for 10k monsters. 2k NetworkTransforms demo is significantly faster. Stable 80fps instead of 500ms freezes in between. ([#1852](https://github.com/MirageNet/Mirage/issues/1852)) ([2d89f05](https://github.com/MirageNet/Mirage/commit/2d89f059afd9175dd7e6d81a0e2e38c0a28915c8))

## [34.9.4](https://github.com/MirageNet/Mirage/compare/34.9.3-master...34.9.4-master) (2020-06-27)


### Bug Fixes

* Additive Scene Example was missing Player Auth on movement. ([#234](https://github.com/MirageNet/Mirage/issues/234)) ([09bbd68](https://github.com/MirageNet/Mirage/commit/09bbd686e6c294f24412b35785cfa7a5aa47b5f2))
* examples run in background ([#233](https://github.com/MirageNet/Mirage/issues/233)) ([4755650](https://github.com/MirageNet/Mirage/commit/47556500eed7c0e2719e41c0e996925ddf1799bb))

## [34.9.3](https://github.com/MirageNet/Mirage/compare/34.9.2-master...34.9.3-master) (2020-06-25)


### Bug Fixes

* Optional Server or Client for PlayerSpawner ([#231](https://github.com/MirageNet/Mirage/issues/231)) ([3fa5f89](https://github.com/MirageNet/Mirage/commit/3fa5f89d8c934b233330efe52b42e59198a920cb))

## [34.9.2](https://github.com/MirageNet/Mirage/compare/34.9.1-master...34.9.2-master) (2020-06-14)


### Bug Fixes

* Spawn Handler Order ([#223](https://github.com/MirageNet/Mirage/issues/223)) ([8674274](https://github.com/MirageNet/Mirage/commit/86742740ef2707f420d5cb6aeeb257bf07511f0b)), closes [#222](https://github.com/MirageNet/Mirage/issues/222)

## [34.9.1](https://github.com/MirageNet/Mirage/compare/34.9.0-master...34.9.1-master) (2020-05-24)


### Bug Fixes

* disconnect transport without domain reload ([20785b7](https://github.com/MirageNet/Mirage/commit/20785b740e21fb22834cd01d7d628e127df6b80d))

# [34.9.0](https://github.com/MirageNet/Mirage/compare/34.8.1-master...34.9.0-master) (2020-04-26)


### Bug Fixes

* Add the transport first so NetworkManager doesn't add Telepathy in OnValidate ([bdec276](https://github.com/MirageNet/Mirage/commit/bdec2762821dc657e8450b576422fcf1f0f69cdf))
* call the virtual OnRoomServerDisconnect before the base ([e6881ef](https://github.com/MirageNet/Mirage/commit/e6881ef007f199efca3c326ead258f0c350ffb47))
* compilation error on standalone build ([bb70bf9](https://github.com/MirageNet/Mirage/commit/bb70bf963459be02a79c2c40cb7dfb8f10d2b92d))
* Removed NetworkClient.Update because NetworkManager does it in LateUpdate ([984945e](https://github.com/MirageNet/Mirage/commit/984945e482529bfc03bf735562f3eff297a1bad4))
* Removed NetworkServer.Listen because HostSetup does that ([cf6823a](https://github.com/MirageNet/Mirage/commit/cf6823acb5151d5bc9beca2b0911a99dfbcd4472))
* weaver syncLists now checks for SerializeItem in base class ([#1768](https://github.com/MirageNet/Mirage/issues/1768)) ([1af5b4e](https://github.com/MirageNet/Mirage/commit/1af5b4ed2f81b4450881fb11fa9b4b7e198274cb))


### Features

* Allow Multiple Network Animator ([#1778](https://github.com/MirageNet/Mirage/issues/1778)) ([34a76a2](https://github.com/MirageNet/Mirage/commit/34a76a2834cbeebb4c623f6650c1d67345b386ac))
* Allowing extra base types to be used for SyncLists and other SyncObjects ([#1729](https://github.com/MirageNet/Mirage/issues/1729)) ([9bf816a](https://github.com/MirageNet/Mirage/commit/9bf816a014fd393617422ee6efa52bdf730cc3c9))
* Disconnect Dead Clients ([#1724](https://github.com/MirageNet/Mirage/issues/1724)) ([a2eb666](https://github.com/MirageNet/Mirage/commit/a2eb666f158d72851d6c62997ed4b24dc3c473bc)), closes [#1753](https://github.com/MirageNet/Mirage/issues/1753)
* Exclude fields from weaver's automatic Read/Write using System.NonSerialized attribute  ([#1727](https://github.com/MirageNet/Mirage/issues/1727)) ([7f8733c](https://github.com/MirageNet/Mirage/commit/7f8733ce6a8f712c195ab7a5baea166a16b52d09))
* Improve weaver error messages ([#1779](https://github.com/MirageNet/Mirage/issues/1779)) ([bcd76c5](https://github.com/MirageNet/Mirage/commit/bcd76c5bdc88af6d95a96e35d47b1b167d375652))
* NetworkServer.SendToReady ([#1773](https://github.com/MirageNet/Mirage/issues/1773)) ([f6545d4](https://github.com/MirageNet/Mirage/commit/f6545d4871bf6881b3150a3231af195e7f9eb8cd))

## [34.8.1](https://github.com/MirageNet/Mirage/compare/34.8.0-master...34.8.1-master) (2020-04-21)


### Bug Fixes

* Allow sync objects to be re-used ([#1744](https://github.com/MirageNet/Mirage/issues/1744)) ([58c89a3](https://github.com/MirageNet/Mirage/commit/58c89a3d32daedc9b6670ed0b5eb1f8753c902e2)), closes [#1714](https://github.com/MirageNet/Mirage/issues/1714)
* Remove leftover AddPlayer methods now that extraData is gone ([#1751](https://github.com/MirageNet/Mirage/issues/1751)) ([2d006fe](https://github.com/MirageNet/Mirage/commit/2d006fe7301eb8a0194af9ce9244988a6877f8f0))
* Remove RoomPlayer from roomSlots on Disconnect ([2a2f76c](https://github.com/MirageNet/Mirage/commit/2a2f76c263093c342f307856e400aeabbedc58df))
* Use path instead of name in Room Example ([5d4bc47](https://github.com/MirageNet/Mirage/commit/5d4bc47d46098f920f9e3468d0f276e336488e42))

# [34.8.0](https://github.com/MirageNet/Mirage/compare/34.7.0-master...34.8.0-master) (2020-04-21)


### Bug Fixes

* Don't destroy the player twice ([#1709](https://github.com/MirageNet/Mirage/issues/1709)) ([cbc2a47](https://github.com/MirageNet/Mirage/commit/cbc2a4772921e01db17033075fa9f7d8cb7e6faf))
* Eliminate NetworkAnimator SetTrigger double firing on Host ([#1723](https://github.com/MirageNet/Mirage/issues/1723)) ([e5b728f](https://github.com/MirageNet/Mirage/commit/e5b728fed515ab679ad1e4581035d32f6c187a98))


### Features

* default log level option ([#1728](https://github.com/MirageNet/Mirage/issues/1728)) ([5c56adc](https://github.com/MirageNet/Mirage/commit/5c56adc1dc47ef91f7ee1d766cd70fa1681cb9df))
* NetworkMatchChecker Component ([#1688](https://github.com/MirageNet/Mirage/issues/1688)) ([21acf66](https://github.com/MirageNet/Mirage/commit/21acf661905ebc35f31a52eb527a50c6eff68a44)), closes [#1685](https://github.com/MirageNet/Mirage/issues/1685) [#1681](https://github.com/MirageNet/Mirage/issues/1681) [#1689](https://github.com/MirageNet/Mirage/issues/1689)
* new virtual OnStopServer called when object is unspawned ([#1743](https://github.com/MirageNet/Mirage/issues/1743)) ([d1695dd](https://github.com/MirageNet/Mirage/commit/d1695dd16f477fc9edaaedb90032c188bcbba6e2))

# [34.7.0](https://github.com/MirageNet/Mirage/compare/34.6.0-master...34.7.0-master) (2020-04-19)


### Features

* transport can provide their preferred scheme ([774a07e](https://github.com/MirageNet/Mirage/commit/774a07e1bf26cce964cf14d502b71b43ce4f5cd0))

# [34.6.0](https://github.com/MirageNet/Mirage/compare/34.5.0-master...34.6.0-master) (2020-04-19)


### Features

* onstopserver event in NetworkIdentity ([#186](https://github.com/MirageNet/Mirage/issues/186)) ([eb81190](https://github.com/MirageNet/Mirage/commit/eb8119007b19faca767969700b0209ade135650c))

# [34.5.0](https://github.com/MirageNet/Mirage/compare/34.4.1-master...34.5.0-master) (2020-04-17)


### Features

* Added SyncList.Find and SyncList.FindAll ([#1716](https://github.com/MirageNet/Mirage/issues/1716)) ([0fe6328](https://github.com/MirageNet/Mirage/commit/0fe6328800daeef8680a19a394260295b7241442)), closes [#1710](https://github.com/MirageNet/Mirage/issues/1710)
* Weaver can now automatically create Reader/Writer for types in a different assembly ([#1708](https://github.com/MirageNet/Mirage/issues/1708)) ([b1644ae](https://github.com/MirageNet/Mirage/commit/b1644ae481497d4347f404543c8200d2754617b9)), closes [#1570](https://github.com/MirageNet/Mirage/issues/1570)


### Performance Improvements

* Adding dirty check before update sync var ([#1702](https://github.com/MirageNet/Mirage/issues/1702)) ([58219c8](https://github.com/MirageNet/Mirage/commit/58219c8f726cd65f8987c9edd747987057967ea4))

## [34.4.1](https://github.com/MirageNet/Mirage/compare/34.4.0-master...34.4.1-master) (2020-04-15)


### Bug Fixes

* Fixing SyncVars not serializing when OnSerialize is overridden ([#1671](https://github.com/MirageNet/Mirage/issues/1671)) ([c66c5a6](https://github.com/MirageNet/Mirage/commit/c66c5a6dcc6837c840e6a51435b88fde10322297))

# [34.4.0](https://github.com/MirageNet/Mirage/compare/34.3.0-master...34.4.0-master) (2020-04-14)


### Features

* Button to register all prefabs in NetworkClient ([#179](https://github.com/MirageNet/Mirage/issues/179)) ([9f5f0b2](https://github.com/MirageNet/Mirage/commit/9f5f0b27f8857bf55bf4f5ffbd436247f99cf390))

# [34.3.0](https://github.com/MirageNet/Mirage/compare/34.2.0-master...34.3.0-master) (2020-04-13)


### Features

* Authenticators can now provide AuthenticationData ([310ce81](https://github.com/MirageNet/Mirage/commit/310ce81c8378707e044108b94faac958e35cbca6))

# [34.2.0](https://github.com/MirageNet/Mirage/compare/34.1.0-master...34.2.0-master) (2020-04-11)


### Features

* Use logger framework for NetworkClient ([#1685](https://github.com/MirageNet/Mirage/issues/1685)) ([6e92bf5](https://github.com/MirageNet/Mirage/commit/6e92bf5616d0d2486ce86497128094c4e33b5a3f))

# [34.1.0](https://github.com/MirageNet/Mirage/compare/34.0.0-master...34.1.0-master) (2020-04-10)


### Bug Fixes

* Check SceneManager GetSceneByName and GetSceneByPath ([#1684](https://github.com/MirageNet/Mirage/issues/1684)) ([e7cfd5a](https://github.com/MirageNet/Mirage/commit/e7cfd5a498c7359636cd109fe586fce1771bada2))
* Re-enable transport if aborting additive load/unload ([#1683](https://github.com/MirageNet/Mirage/issues/1683)) ([bc37497](https://github.com/MirageNet/Mirage/commit/bc37497ac963bb0f2820b103591afd05177d078d))
* stack overflow getting logger ([55e075c](https://github.com/MirageNet/Mirage/commit/55e075c872a076f524ec62f44d81df17819e81ba))


### Features

* logger factory works for static classes by passing the type ([f9328c7](https://github.com/MirageNet/Mirage/commit/f9328c771cfb0974ce4765dc0d5af01440d838c0))


### Performance Improvements

* Increasing Network Writer performance ([#1674](https://github.com/MirageNet/Mirage/issues/1674)) ([f057983](https://github.com/MirageNet/Mirage/commit/f0579835ca52270de424e81691f12c02022c3909))

# [34.0.0](https://github.com/MirageNet/Mirage/compare/33.1.1-master...34.0.0-master) (2020-04-10)


* remove NetworkConnection.isAuthenticated (#167) ([8a0e0b3](https://github.com/MirageNet/Mirage/commit/8a0e0b3af37e8b0c74a8b97f12ec29cf202715ea)), closes [#167](https://github.com/MirageNet/Mirage/issues/167)


### BREAKING CHANGES

* Remove isAuthenticated

* Fix typo

* Fix smells

* Remove smells

## [33.1.1](https://github.com/MirageNet/Mirage/compare/33.1.0-master...33.1.1-master) (2020-04-09)


### Bug Fixes

* Invoke server.Disconnected before identity is removed for its conn ([#165](https://github.com/MirageNet/Mirage/issues/165)) ([b749c4b](https://github.com/MirageNet/Mirage/commit/b749c4ba126266a1799059f7fb407b6bcaa2bbd9))

# [33.1.0](https://github.com/MirageNet/Mirage/compare/33.0.0-master...33.1.0-master) (2020-04-08)


### Features

* new websocket transport ([#156](https://github.com/MirageNet/Mirage/issues/156)) ([23c7b0d](https://github.com/MirageNet/Mirage/commit/23c7b0d1b32684d4f959495fe96b2d16a68bd356))

# [33.0.0](https://github.com/MirageNet/Mirage/compare/32.0.1-master...33.0.0-master) (2020-04-08)


* Simplify RegisterHandler (#160) ([f4f5167](https://github.com/MirageNet/Mirage/commit/f4f516791b8390f0babf8a7aefa19254427d4145)), closes [#160](https://github.com/MirageNet/Mirage/issues/160)


### BREAKING CHANGES

* NetworkConneciton.RegisterHandler only needs message class

## [32.0.1](https://github.com/MirageNet/Mirage/compare/32.0.0-master...32.0.1-master) (2020-04-08)


### Performance Improvements

* Use continuewith to queue up ssl messages ([#1640](https://github.com/MirageNet/Mirage/issues/1640)) ([84b2c8c](https://github.com/MirageNet/Mirage/commit/84b2c8cf2671728baecf734487ddaa7fab9943a0))

# [32.0.0](https://github.com/MirageNet/Mirage/compare/31.4.0-master...32.0.0-master) (2020-04-07)


* Remove NetworkConnectionToClient (#155) ([bd95cea](https://github.com/MirageNet/Mirage/commit/bd95cea4d639753335b21c48781603acd758a9e7)), closes [#155](https://github.com/MirageNet/Mirage/issues/155)


### BREAKING CHANGES

* NetworkConnectionToClient and networkConnectionToServer are gone

# [31.4.0](https://github.com/MirageNet/Mirage/compare/31.3.1-master...31.4.0-master) (2020-04-04)


### Bug Fixes

* disconnect even if there is an exception ([#152](https://github.com/MirageNet/Mirage/issues/152)) ([2eb9de6](https://github.com/MirageNet/Mirage/commit/2eb9de6b470579b6de75853ba161b3e7a36de698))


### Features

* spawning invalid object now gives exception ([e2fc829](https://github.com/MirageNet/Mirage/commit/e2fc8292400aae8b3b8b972ff5824b8d9cdd6b88))

## [31.3.1](https://github.com/MirageNet/Mirage/compare/31.3.0-master...31.3.1-master) (2020-04-03)


### Performance Improvements

* Adding buffer for local connection ([#1621](https://github.com/MirageNet/Mirage/issues/1621)) ([4d5cee8](https://github.com/MirageNet/Mirage/commit/4d5cee893d0104c0070a0b1814c8c84f11f24f18))

# [31.3.0](https://github.com/MirageNet/Mirage/compare/31.2.1-master...31.3.0-master) (2020-04-01)


### Bug Fixes

* Destroyed NetMan due to singleton collision must not continue. ([#1636](https://github.com/MirageNet/Mirage/issues/1636)) ([d2a58a4](https://github.com/MirageNet/Mirage/commit/d2a58a4c251c97cdb38c88c9a381496b3078adf8))


### Features

* logging api ([#1611](https://github.com/MirageNet/Mirage/issues/1611)) ([f2ccb59](https://github.com/MirageNet/Mirage/commit/f2ccb59ae6db90bc84f8a36802bfe174b4493127))


### Performance Improvements

* faster NetworkReader pooling ([#1623](https://github.com/MirageNet/Mirage/issues/1623)) ([1ae0381](https://github.com/MirageNet/Mirage/commit/1ae038172ac7f5e18e0e09b0081f7f42fa0eff7a))

## [31.2.1](https://github.com/MirageNet/Mirage/compare/31.2.0-master...31.2.1-master) (2020-04-01)


### Bug Fixes

* pass the correct connection to TargetRpcs ([#146](https://github.com/MirageNet/Mirage/issues/146)) ([9df2f79](https://github.com/MirageNet/Mirage/commit/9df2f798953f78de113ef6fa9fea111b03a52cd0))

# [31.2.0](https://github.com/MirageNet/Mirage/compare/31.1.0-master...31.2.0-master) (2020-04-01)


### Features

* Add fallback transport ([1b02796](https://github.com/MirageNet/Mirage/commit/1b02796c1468c1e1650eab0f278cd9a11cc597c7))

# [31.1.0](https://github.com/MirageNet/Mirage/compare/31.0.0-master...31.1.0-master) (2020-04-01)


### Features

* async multiplex transport ([#145](https://github.com/MirageNet/Mirage/issues/145)) ([c0e7e92](https://github.com/MirageNet/Mirage/commit/c0e7e9280931a5996f595e41aa516bef20208b6f))

# [31.0.0](https://github.com/MirageNet/Mirage/compare/30.3.3-master...31.0.0-master) (2020-04-01)


### Bug Fixes

* chat example ([e6e10a7](https://github.com/MirageNet/Mirage/commit/e6e10a7108bc01e3bd0c208734c97c945003ff86))
* missing meta ([87ace4d](https://github.com/MirageNet/Mirage/commit/87ace4dda09331968cc9d0185ce1de655f5dfb15))


### Features

* asynchronous transport ([#134](https://github.com/MirageNet/Mirage/issues/134)) ([0e84f45](https://github.com/MirageNet/Mirage/commit/0e84f451e822fe7c1ca1cd04e052546ed273cfce))


### BREAKING CHANGES

* connecition Id is gone
* websocket transport does not work,  needs to be replaced.

## [30.3.3](https://github.com/MirageNet/Mirage/compare/30.3.2-master...30.3.3-master) (2020-03-31)


### Bug Fixes

* headless build ([7864e8d](https://github.com/MirageNet/Mirage/commit/7864e8d6f4a0952ef3114daac11842e4ee0a7a58))
* headless build ([ab47a45](https://github.com/MirageNet/Mirage/commit/ab47a45d08f4e9a82a5cd26f913f4871d46dd484))

## [30.3.2](https://github.com/MirageNet/Mirage/compare/30.3.1-master...30.3.2-master) (2020-03-31)


### Bug Fixes

* AsyncTcp now exits normally when client disconnects ([#141](https://github.com/MirageNet/Mirage/issues/141)) ([8896c4a](https://github.com/MirageNet/Mirage/commit/8896c4afa2f937839a54dc71fbe578b9c636f393))

## [30.3.1](https://github.com/MirageNet/Mirage/compare/30.3.0-master...30.3.1-master) (2020-03-30)


### Bug Fixes

* reset buffer every time ([a8a62a6](https://github.com/MirageNet/Mirage/commit/a8a62a64b6fa67223505505c1225269d3a047a92))

# [30.3.0](https://github.com/MirageNet/Mirage/compare/30.2.0-master...30.3.0-master) (2020-03-30)


### Features

* Piped connection ([#138](https://github.com/MirageNet/Mirage/issues/138)) ([471a881](https://github.com/MirageNet/Mirage/commit/471a881cdae1c6e526b5aa2d552cc91dc27f793a))

# [30.2.0](https://github.com/MirageNet/Mirage/compare/30.1.2-master...30.2.0-master) (2020-03-30)


### Features

* allow more than one NetworkManager ([#135](https://github.com/MirageNet/Mirage/issues/135)) ([92968e4](https://github.com/MirageNet/Mirage/commit/92968e4e45a33fa5ab77ce2bfc9e8f826a888711))

## [30.1.2](https://github.com/MirageNet/Mirage/compare/30.1.1-master...30.1.2-master) (2020-03-29)


### Bug Fixes

* client being disconnected twice ([#132](https://github.com/MirageNet/Mirage/issues/132)) ([36bb3a2](https://github.com/MirageNet/Mirage/commit/36bb3a2418bcf41fd63d1fc79e8a5173e4b0bc51))
* client disconnected on server event never raised ([#133](https://github.com/MirageNet/Mirage/issues/133)) ([9d9efa0](https://github.com/MirageNet/Mirage/commit/9d9efa0e31cbea4d90d7408ae6b3742151b49a21))

## [30.1.1](https://github.com/MirageNet/Mirage/compare/30.1.0-master...30.1.1-master) (2020-03-29)


### Performance Improvements

* faster NetworkWriter pooling ([#1620](https://github.com/MirageNet/Mirage/issues/1620)) ([4fa43a9](https://github.com/MirageNet/Mirage/commit/4fa43a947132f89e5348c63e393dd3b80e1fe7e1))

# [30.1.0](https://github.com/MirageNet/Mirage/compare/30.0.0-master...30.1.0-master) (2020-03-29)


### Features

* allow Play mode options ([f9afb64](https://github.com/MirageNet/Mirage/commit/f9afb6407b015c239975c5a1fba6609e12ab5c8f))

# [30.0.0](https://github.com/MirageNet/Mirage/compare/29.1.1-master...30.0.0-master) (2020-03-29)


### Features

* Server raises an event when it starts ([#126](https://github.com/MirageNet/Mirage/issues/126)) ([d5b0a6f](https://github.com/MirageNet/Mirage/commit/d5b0a6f0dd65f9dbb6c4848bce5e81f93772a235))


### BREAKING CHANGES

* NetworkManager no longer has OnServerStart virtual

## [29.1.1](https://github.com/MirageNet/Mirage/compare/29.1.0-master...29.1.1-master) (2020-03-29)


### Reverts

* Revert "Revert "Explain why 10"" ([d727e4f](https://github.com/MirageNet/Mirage/commit/d727e4fd4b9e811025c7309efeba090e3ac14ccd))

# [29.1.0](https://github.com/MirageNet/Mirage/compare/29.0.3-master...29.1.0-master) (2020-03-28)


### Features

* get a convenient property to get network time ([1e8c2fe](https://github.com/MirageNet/Mirage/commit/1e8c2fe0522d7843a6a2fae7287287c7afa4e417))

## [29.0.3](https://github.com/MirageNet/Mirage/compare/29.0.2-master...29.0.3-master) (2020-03-28)


### Performance Improvements

* faster NetworkWriter pooling ([#1616](https://github.com/MirageNet/Mirage/issues/1616)) ([5128b12](https://github.com/MirageNet/Mirage/commit/5128b122fe205f250d44ba5c7a88a50de2f3e4cd)), closes [#1614](https://github.com/MirageNet/Mirage/issues/1614)
* replace isValueType with faster alternative ([#1617](https://github.com/MirageNet/Mirage/issues/1617)) ([61163ca](https://github.com/MirageNet/Mirage/commit/61163cacb4cb2652aa8632f84be89212674436ff)), closes [/github.com/vis2k/Mirror/issues/1614#issuecomment-605443808](https://github.com//github.com/vis2k/Mirror/issues/1614/issues/issuecomment-605443808)
* use byte[] directly instead of MemoryStream ([#1618](https://github.com/MirageNet/Mirage/issues/1618)) ([166b8c9](https://github.com/MirageNet/Mirage/commit/166b8c946736447a76c1886c4d1fb036f6e56e20))

## [29.0.2](https://github.com/MirageNet/Mirage/compare/29.0.1-master...29.0.2-master) (2020-03-27)


### Performance Improvements

* Remove redundant mask ([#1604](https://github.com/MirageNet/Mirage/issues/1604)) ([5d76afb](https://github.com/MirageNet/Mirage/commit/5d76afbe29f456a657c9e1cb7c97435242031091))

## [29.0.1](https://github.com/MirageNet/Mirage/compare/29.0.0-master...29.0.1-master) (2020-03-27)


### Bug Fixes

* [#1515](https://github.com/MirageNet/Mirage/issues/1515) - StopHost now invokes OnServerDisconnected for the host client too ([#1601](https://github.com/MirageNet/Mirage/issues/1601)) ([678ac68](https://github.com/MirageNet/Mirage/commit/678ac68b58798816658d29be649bdaf18ad70794))


### Performance Improvements

* simplify and speed up getting methods in weaver ([c1cfc42](https://github.com/MirageNet/Mirage/commit/c1cfc421811e4c12e84cb28677ac68c82575958d))

# [29.0.0](https://github.com/MirageNet/Mirage/compare/28.0.0-master...29.0.0-master) (2020-03-26)


### Features

* PlayerSpawner component ([#123](https://github.com/MirageNet/Mirage/issues/123)) ([e8b933d](https://github.com/MirageNet/Mirage/commit/e8b933ddff9a47b64be371edb63af130bd3958b4))


### BREAKING CHANGES

* NetworkManager no longer spawns the player.  You need to add PlayerSpawner component if you want that behavior

# [28.0.0](https://github.com/MirageNet/Mirage/compare/27.0.1-master...28.0.0-master) (2020-03-26)


### Bug Fixes

* [#1599](https://github.com/MirageNet/Mirage/issues/1599) - NetworkManager HUD calls StopHost/Server/Client depending on state. It does not call StopHost in all cases. ([#1600](https://github.com/MirageNet/Mirage/issues/1600)) ([8c6ae0f](https://github.com/MirageNet/Mirage/commit/8c6ae0f8b4fdafbc3abd194c081c75ee75fcfe51))


### Features

* now you can assign scenes even if not in Editor ([#1576](https://github.com/MirageNet/Mirage/issues/1576)) ([c8a1a5e](https://github.com/MirageNet/Mirage/commit/c8a1a5e56f7561487e3180f26e28484f714f36c1))


### BREAKING CHANGES

* You will need to reassign your scenes after upgrade

* Automatically fix properties that were using name

If you open a NetworkManager or other gameobject that uses a scene name
it now gets converted to scene path by the SceneDrawer

* Use get scene by name

* Scene can never be null

* Update Assets/Mirror/Examples/AdditiveScenes/Scenes/MainScene.unity

* Issue warning if we drop the scene

* Issue error if scene is lost

* Add suggestion on how to fix the error

* Keep backwards compatibility, check for scene name

* cache the active scene

* Update Assets/Mirror/Editor/SceneDrawer.cs

Co-Authored-By: James Frowen <jamesfrowendev@gmail.com>

* GetSceneByName only works if scene is loaded

* Remove unused using

Co-authored-by: James Frowen <jamesfrowendev@gmail.com>

## [27.0.1](https://github.com/MirageNet/Mirage/compare/27.0.0-master...27.0.1-master) (2020-03-26)


### Bug Fixes

* empty scene name isn't considered as empty ([ec3a939](https://github.com/MirageNet/Mirage/commit/ec3a93945b5b52a77fd745b39e1e821db721768d))

# [27.0.0](https://github.com/MirageNet/Mirage/compare/26.0.0-master...27.0.0-master) (2020-03-26)


* remove room feature for now (#122) ([87dd495](https://github.com/MirageNet/Mirage/commit/87dd495a6fca6c85349afd42ba6449d98de1f567)), closes [#122](https://github.com/MirageNet/Mirage/issues/122)
* Server Disconnect is now an event not a message (#121) ([82ebd71](https://github.com/MirageNet/Mirage/commit/82ebd71456cbd2e819540d961a93814c57735784)), closes [#121](https://github.com/MirageNet/Mirage/issues/121)


### Code Refactoring

* Remove offline/online scenes ([#120](https://github.com/MirageNet/Mirage/issues/120)) ([a4c881a](https://github.com/MirageNet/Mirage/commit/a4c881a36e26b20fc72166741e20c84ce030ad8f))


### BREAKING CHANGES

* Room feature and example are gone
* offline/online scenes are gone
* OnServerDisconnect is gone

# [26.0.0](https://github.com/MirageNet/Mirage/compare/25.0.0-master...26.0.0-master) (2020-03-25)


* remove OnClientStart virtual ([eb5242d](https://github.com/MirageNet/Mirage/commit/eb5242d63fa011381e7692470713fd144476454a))


### BREAKING CHANGES

* Removed OnStartClient virtual,  use event from NetworkClient instead

# [25.0.0](https://github.com/MirageNet/Mirage/compare/24.1.1-master...25.0.0-master) (2020-03-25)


* Move on client stop (#118) ([678e386](https://github.com/MirageNet/Mirage/commit/678e3867a9f232e52d2a6cdbfae8140b0e82bd11)), closes [#118](https://github.com/MirageNet/Mirage/issues/118)


### Features

* Added Virtual OnRoomStopServer to NetworkRoomManager and Script Template ([d034ef6](https://github.com/MirageNet/Mirage/commit/d034ef616f3d479729064d652f74a905ea05b495))


### BREAKING CHANGES

* OnStopClient virtual is replaced by event in Client

## [24.1.1](https://github.com/MirageNet/Mirage/compare/24.1.0-master...24.1.1-master) (2020-03-25)


### Bug Fixes

* [#1593](https://github.com/MirageNet/Mirage/issues/1593) - NetworkRoomManager.ServerChangeScene doesn't destroy the world player before replacing the connection. otherwise ReplacePlayerForConnection removes authority form a destroyed object, causing all kidns of errors. The call wasn't actually needed. ([#1594](https://github.com/MirageNet/Mirage/issues/1594)) ([347cb53](https://github.com/MirageNet/Mirage/commit/347cb5374d0cba72762e893645f076d3161aa0c5))

# [24.1.0](https://github.com/MirageNet/Mirage/compare/24.0.1-master...24.1.0-master) (2020-03-24)


### Features

* connections can retrieve end point ([#114](https://github.com/MirageNet/Mirage/issues/114)) ([d239718](https://github.com/MirageNet/Mirage/commit/d239718498c5750edf0b5d11cc762136f45500fd))
* transports can give server uri ([#113](https://github.com/MirageNet/Mirage/issues/113)) ([dc700ec](https://github.com/MirageNet/Mirage/commit/dc700ec721cf4ecf6ddd082d88b933c9afffbc67))

## [24.0.1](https://github.com/MirageNet/Mirage/compare/24.0.0-master...24.0.1-master) (2020-03-23)


### Bug Fixes

* Default port is 7777 ([960c39d](https://github.com/MirageNet/Mirage/commit/960c39da90db156cb58d4765695664f0c084b39a))

# [24.0.0](https://github.com/MirageNet/Mirage/compare/23.0.0-master...24.0.0-master) (2020-03-23)


### Features

* individual events for SyncDictionary ([#112](https://github.com/MirageNet/Mirage/issues/112)) ([b3c1b16](https://github.com/MirageNet/Mirage/commit/b3c1b16100c440131d6d933627a9f6479aed11ad))


### BREAKING CHANGES

* SyncDictionary callback has been replaced

# [23.0.0](https://github.com/MirageNet/Mirage/compare/22.0.0-master...23.0.0-master) (2020-03-23)


### Features

* individual events for SyncSet ([#111](https://github.com/MirageNet/Mirage/issues/111)) ([261f5d6](https://github.com/MirageNet/Mirage/commit/261f5d6a1481634dc524fb57b5866e378a1d909d))


### BREAKING CHANGES

* callback signature changed in SyncSet

# [22.0.0](https://github.com/MirageNet/Mirage/compare/21.2.1-master...22.0.0-master) (2020-03-23)


### Features

* synclists has individual meaningful events ([#109](https://github.com/MirageNet/Mirage/issues/109)) ([e326064](https://github.com/MirageNet/Mirage/commit/e326064b51e8372726b30d19973df6293c74c376)), closes [#103](https://github.com/MirageNet/Mirage/issues/103)


### BREAKING CHANGES

* Sync lists no longer have a Callback event with an operation enum

## [21.2.1](https://github.com/MirageNet/Mirage/compare/21.2.0-master...21.2.1-master) (2020-03-23)


### Bug Fixes

* overriden hooks are invoked (fixes [#1581](https://github.com/MirageNet/Mirage/issues/1581)) ([#1584](https://github.com/MirageNet/Mirage/issues/1584)) ([cf55333](https://github.com/MirageNet/Mirage/commit/cf55333a072c0ffe36a2972ca0a5122a48d708d0))

# [21.2.0](https://github.com/MirageNet/Mirage/compare/21.1.0-master...21.2.0-master) (2020-03-23)


### Features

* next gen async transport ([#106](https://github.com/MirageNet/Mirage/issues/106)) ([4a8dc67](https://github.com/MirageNet/Mirage/commit/4a8dc676b96840493d178718049b9e20c8eb6510))

# [21.1.0](https://github.com/MirageNet/Mirage/compare/21.0.1-master...21.1.0-master) (2020-03-22)


### Features

* NetworkConnection manages messages handlers ([#93](https://github.com/MirageNet/Mirage/issues/93)) ([5c79f0d](https://github.com/MirageNet/Mirage/commit/5c79f0db64e46905081e6c0b5502376c5acf0d99))

## [21.0.1](https://github.com/MirageNet/Mirage/compare/21.0.0-master...21.0.1-master) (2020-03-22)


### Bug Fixes

* calling Connect and Authenticate handler twice ([#102](https://github.com/MirageNet/Mirage/issues/102)) ([515f5a1](https://github.com/MirageNet/Mirage/commit/515f5a15cd5be984f8cb4f94d3be0a0ac919eb63))

# [21.0.0](https://github.com/MirageNet/Mirage/compare/20.1.0-master...21.0.0-master) (2020-03-22)


### Features

* NetworkIdentity lifecycle events ([#88](https://github.com/MirageNet/Mirage/issues/88)) ([9a7c572](https://github.com/MirageNet/Mirage/commit/9a7c5726eb3d333b85c3d0e44b884c11e34be45d))


### BREAKING CHANGES

* NetworkBehavior no longer has virtuals for lifecycle events

# [20.1.0](https://github.com/MirageNet/Mirage/compare/20.0.6-master...20.1.0-master) (2020-03-22)


### Bug Fixes

* tcp server Tests ([3b95477](https://github.com/MirageNet/Mirage/commit/3b954777f16a41469d953e3744c5d68554e3d200))


### Features

* NetworkClient raises event after authentication ([#96](https://github.com/MirageNet/Mirage/issues/96)) ([c332271](https://github.com/MirageNet/Mirage/commit/c332271d918f782d4b1a84b3f0fd660239f95743))

## [20.0.6](https://github.com/MirageNet/Mirage/compare/20.0.5-master...20.0.6-master) (2020-03-22)


### Bug Fixes

* NetworkConnectionEvent should be serialized in editor ([0e756ce](https://github.com/MirageNet/Mirage/commit/0e756cec06c5fda9eb4b5c8aa9de093c32b0315c))

## [20.0.5](https://github.com/MirageNet/Mirage/compare/20.0.4-master...20.0.5-master) (2020-03-21)


### Bug Fixes

* Added LogFilter.Debug check in a few places ([#1575](https://github.com/MirageNet/Mirage/issues/1575)) ([3156504](https://github.com/MirageNet/Mirage/commit/31565042708ec768fcaafe9986968d095a3a1419))
* comment punctuation ([4d827cd](https://github.com/MirageNet/Mirage/commit/4d827cd9f60e4fb7cd6524d78ca26ea1d88a1eff))
* Make SendToReady non-ambiguous ([#1578](https://github.com/MirageNet/Mirage/issues/1578)) ([b627779](https://github.com/MirageNet/Mirage/commit/b627779acd68b2acfcaf5eefc0d3864dcce57fc7))

## [20.0.4](https://github.com/MirageNet/Mirage/compare/20.0.3-master...20.0.4-master) (2020-03-21)


### Bug Fixes

* movement in room demo ([49f7904](https://github.com/MirageNet/Mirage/commit/49f7904b4a83fc5318031d273cb10a4b87af2172))

## [20.0.3](https://github.com/MirageNet/Mirage/compare/20.0.2-master...20.0.3-master) (2020-03-21)


### Bug Fixes

* additive scene player movement is client authoritative ([e683a92](https://github.com/MirageNet/Mirage/commit/e683a92b081c989314c11e48fb21ee0096465797))
* the Room scene references other scenes ([9b60871](https://github.com/MirageNet/Mirage/commit/9b60871e2ea1a2912c0af3d95796660fc04dc569))

## [20.0.2](https://github.com/MirageNet/Mirage/compare/20.0.1-master...20.0.2-master) (2020-03-21)


### Bug Fixes

* additive scene example ([9fa0169](https://github.com/MirageNet/Mirage/commit/9fa016957f487526ab44d443aabfe58fcc69518a))

## [20.0.1](https://github.com/MirageNet/Mirage/compare/20.0.0-master...20.0.1-master) (2020-03-20)


### Bug Fixes

* NRE when destroying all objects ([#85](https://github.com/MirageNet/Mirage/issues/85)) ([71e78a7](https://github.com/MirageNet/Mirage/commit/71e78a7f5e15f99af89cd15c1ddd8a975e463916))

# [20.0.0](https://github.com/MirageNet/Mirage/compare/19.1.2-master...20.0.0-master) (2020-03-20)


### Bug Fixes

* compilation issue after merge from mirror ([daf07be](https://github.com/MirageNet/Mirage/commit/daf07bea83c9925bd780e23de53dd50604e8a999))


* merge clientscene and networkclient (#84) ([dee1046](https://github.com/MirageNet/Mirage/commit/dee10460325119337401dc4d237dec8bfb9ddb29)), closes [#84](https://github.com/MirageNet/Mirage/issues/84)


### Features

* SyncSet and SyncDictionary now show in inspector ([#1561](https://github.com/MirageNet/Mirage/issues/1561)) ([5510711](https://github.com/MirageNet/Mirage/commit/55107115c66ea38b75edf4a912b5cc48351128f7))


### BREAKING CHANGES

* ClientScene is gone

## [19.1.2](https://github.com/MirageNet/Mirage/compare/19.1.1-master...19.1.2-master) (2020-03-20)


### Bug Fixes

* examples now work with prefabs in NC ([df4149b](https://github.com/MirageNet/Mirage/commit/df4149b8fea9f174742d20f600ef5261058e5300))

## [19.1.1](https://github.com/MirageNet/Mirage/compare/19.1.0-master...19.1.1-master) (2020-03-20)


### Bug Fixes

* Fixed ClienRpc typos ([e946c79](https://github.com/MirageNet/Mirage/commit/e946c79194dd9618992a4136daed4b79f60671a2))
* Prevent Double Call of NetworkServer.Destroy ([#1554](https://github.com/MirageNet/Mirage/issues/1554)) ([2d1b142](https://github.com/MirageNet/Mirage/commit/2d1b142276193be1e93a3a3f6ce482c87a134a2c))
* show private serializable fields in network behavior inspector ([#1557](https://github.com/MirageNet/Mirage/issues/1557)) ([b8c87d9](https://github.com/MirageNet/Mirage/commit/b8c87d9053e7fd7c3b69bcf1d4179e6e4c9bc4a6))
* Updated NetworkRoomPlayer inspector and doc and image ([a4ffcbe](https://github.com/MirageNet/Mirage/commit/a4ffcbe280e2e2318d7cd2988379af74f0d32021))

# [19.1.0](https://github.com/MirageNet/Mirage/compare/19.0.1-master...19.1.0-master) (2020-03-19)


### Features

* Now you can pass NetworkIdentity and GameObjects ([#83](https://github.com/MirageNet/Mirage/issues/83)) ([dca2d40](https://github.com/MirageNet/Mirage/commit/dca2d4056fe613793480b378d25509284a1fd46a))

## [19.0.1](https://github.com/MirageNet/Mirage/compare/19.0.0-master...19.0.1-master) (2020-03-17)


### Bug Fixes

* calling syncvar hook when not connected yet ([#77](https://github.com/MirageNet/Mirage/issues/77)) ([e64727b](https://github.com/MirageNet/Mirage/commit/e64727b74bcbb1adfcd8f5efbf96066443254dff))

# [19.0.0](https://github.com/MirageNet/Mirage/compare/18.0.0-master...19.0.0-master) (2020-03-17)


* removed obsoletes (#1542) ([4faec29](https://github.com/MirageNet/Mirage/commit/4faec295593b81a709a57aaf374bb5b080a04538)), closes [#1542](https://github.com/MirageNet/Mirage/issues/1542)


### BREAKING CHANGES

* removed obsoletes

# [18.0.0](https://github.com/MirageNet/Mirage/compare/17.0.2-master...18.0.0-master) (2020-03-17)


### Features

* Time sync is now done per NetworkClient ([b24542f](https://github.com/MirageNet/Mirage/commit/b24542f62c6a2d0c43588af005f360ed74c619ca))


### BREAKING CHANGES

* NetworkTime.Time is no longer static

## [17.0.2](https://github.com/MirageNet/Mirage/compare/17.0.1-master...17.0.2-master) (2020-03-17)


### Bug Fixes

* Command and Rpc debugging information ([#1551](https://github.com/MirageNet/Mirage/issues/1551)) ([658847b](https://github.com/MirageNet/Mirage/commit/658847b096571eb7cf14e824ea76359576121e63)), closes [#1550](https://github.com/MirageNet/Mirage/issues/1550)

## [17.0.1](https://github.com/MirageNet/Mirage/compare/17.0.0-master...17.0.1-master) (2020-03-15)


### Bug Fixes

* Report correct channel to profiler in SendToObservers ([0b84d4c](https://github.com/MirageNet/Mirage/commit/0b84d4c5e1b8455e32eeb4d4c00b60bbc1301436))

# [17.0.0](https://github.com/MirageNet/Mirage/compare/16.0.0-master...17.0.0-master) (2020-03-15)


### Code Refactoring

* observers is now a set of connections ([#74](https://github.com/MirageNet/Mirage/issues/74)) ([4848920](https://github.com/MirageNet/Mirage/commit/484892058b448012538754c4a1f2eac30a42ceaa))


### BREAKING CHANGES

* observers is now a set of connections, not a dictionary

# [16.0.0](https://github.com/MirageNet/Mirage/compare/15.0.7-master...16.0.0-master) (2020-03-10)


### Code Refactoring

*  Client and server keep their own spawned list ([#71](https://github.com/MirageNet/Mirage/issues/71)) ([c2599e2](https://github.com/MirageNet/Mirage/commit/c2599e2c6157dd7539b560cd4645c10713a39276))


### BREAKING CHANGES

* cannot pass GameObjects and NetworkIdentities anymore.
Will be restored in the future.

## [15.0.7](https://github.com/MirageNet/Mirage/compare/15.0.6-master...15.0.7-master) (2020-03-10)


### Bug Fixes

* Use big endian for packet size ([1ddcbec](https://github.com/MirageNet/Mirage/commit/1ddcbec93509e14169bddbb2a38a7cf0d53776e4))

## [15.0.6](https://github.com/MirageNet/Mirage/compare/15.0.5-master...15.0.6-master) (2020-03-09)


### Bug Fixes

* compilation issues ([22bf925](https://github.com/MirageNet/Mirage/commit/22bf925f1ebf018b9ea33df22294fb9205574fa5))
* NetworkBehaviour.SyncVarGameObjectEqual made protected again so that Weaver finds it again ([165a1dd](https://github.com/MirageNet/Mirage/commit/165a1dd94cd1a7bebc3365c4f40f968f500043a5))
* NetworkBehaviour.SyncVarNetworkIdentityEqual made protected again so that Weaver finds it again ([20a2d09](https://github.com/MirageNet/Mirage/commit/20a2d09d07ab2c47a204e5d583b538a92f72231e))

## [15.0.5](https://github.com/MirageNet/Mirage/compare/15.0.4-master...15.0.5-master) (2020-03-08)


### Bug Fixes

* don't crash when stopping the client ([f584388](https://github.com/MirageNet/Mirage/commit/f584388a16e746ac5c3000123a02a5c77387765e))
* race condition closing tcp connections ([717f1f5](https://github.com/MirageNet/Mirage/commit/717f1f5ad783e13a6d55920e454cb91f380cd621))

## [15.0.4](https://github.com/MirageNet/Mirage/compare/15.0.3-master...15.0.4-master) (2020-03-08)


### Bug Fixes

* attributes causing a NRE ([#69](https://github.com/MirageNet/Mirage/issues/69)) ([fc99c67](https://github.com/MirageNet/Mirage/commit/fc99c67712564e2d983674b37858591903294f1a))

## [15.0.3](https://github.com/MirageNet/Mirage/compare/15.0.2-master...15.0.3-master) (2020-03-08)


### Bug Fixes

* NetworkIdentity.RebuildObservers: added missing null check for observers coming from components that implement OnRebuildObservers. Previously this caused a NullReferenceException. ([a5f495a](https://github.com/MirageNet/Mirage/commit/a5f495a77485b972cf39f8a42bae6d7d852be38c))
* SendToObservers missing result variable ([9c09c26](https://github.com/MirageNet/Mirage/commit/9c09c26a5cd28cadae4049fea71cddc38c00cf79))

## [15.0.2](https://github.com/MirageNet/Mirage/compare/15.0.1-master...15.0.2-master) (2020-03-06)


### Bug Fixes

* rooms demo ([44598e5](https://github.com/MirageNet/Mirage/commit/44598e58325c877bd6b53ee5a77dd95e421ec404))

## [15.0.1](https://github.com/MirageNet/Mirage/compare/15.0.0-master...15.0.1-master) (2020-03-06)


### Bug Fixes

* chat example works ([0609d50](https://github.com/MirageNet/Mirage/commit/0609d50d9b93afd3b42d97ddcd00d32e8aaa0db5))
* there is no lobby example ([b1e05ef](https://github.com/MirageNet/Mirage/commit/b1e05efb19984ce615d20a16a6c4b09a8897da6a))

# [15.0.0](https://github.com/MirageNet/Mirage/compare/14.0.1-master...15.0.0-master) (2020-03-05)


### Code Refactoring

* Remove networkAddress from NetworkManager ([#67](https://github.com/MirageNet/Mirage/issues/67)) ([e89c32d](https://github.com/MirageNet/Mirage/commit/e89c32dc16b3d9b9cdcb38f0d7d170da94dbf874))


### BREAKING CHANGES

* StartClient now receives the server ip
* NetworkManager no longer has NetworkAddress

## [14.0.1](https://github.com/MirageNet/Mirage/compare/14.0.0-master...14.0.1-master) (2020-03-04)


### Bug Fixes

* Avoid FindObjectOfType when not needed ([#66](https://github.com/MirageNet/Mirage/issues/66)) ([e2a4afd](https://github.com/MirageNet/Mirage/commit/e2a4afd0b9ca8dea720acb9c558efca210bd8e71))

# [14.0.0](https://github.com/MirageNet/Mirage/compare/13.0.0-master...14.0.0-master) (2020-03-03)


* Assign/Remove client authority now throws exception ([7679d3b](https://github.com/MirageNet/Mirage/commit/7679d3bef369de5245fd301b33e85dbdd74e84cd))


### BREAKING CHANGES

* Assign/Remove client authority throws exception instead of returning boolean

# [13.0.0](https://github.com/MirageNet/Mirage/compare/12.0.2-master...13.0.0-master) (2020-03-02)


* Removed LLAPI ([b0c936c](https://github.com/MirageNet/Mirage/commit/b0c936cb7d1a803b7096806a905a4c121e45bcdf))


### BREAKING CHANGES

* Removed LLAPITransport

## [12.0.2](https://github.com/MirageNet/Mirage/compare/12.0.1-master...12.0.2-master) (2020-02-29)


### Bug Fixes

* NetworkIdentity.OnStartLocalPlayer catches exceptions now too. fixes a potential bug where an exception in PlayerInventory.OnStartLocalPlayer would cause PlayerEquipment.OnStartLocalPlayer to not be called ([5ed5f84](https://github.com/MirageNet/Mirage/commit/5ed5f844090442e16e78f466f7a785881283fbd4))

## [12.0.1](https://github.com/MirageNet/Mirage/compare/12.0.0-master...12.0.1-master) (2020-02-29)


### Bug Fixes

* disconnect properly from the server ([c89bb51](https://github.com/MirageNet/Mirage/commit/c89bb513e536f256e55862b723487bb21281572e))

# [12.0.0](https://github.com/MirageNet/Mirage/compare/11.1.0-master...12.0.0-master) (2020-02-28)


* Simplify unpacking messages (#65) ([c369da8](https://github.com/MirageNet/Mirage/commit/c369da84dc34dbbde68a7b30758a6a14bc2573b1)), closes [#65](https://github.com/MirageNet/Mirage/issues/65)


### BREAKING CHANGES

* MessagePacker.UnpackMessage replaced by UnpackId

# [11.1.0](https://github.com/MirageNet/Mirage/compare/11.0.0-master...11.1.0-master) (2020-02-27)


### Bug Fixes

* Add missing channelId to NetworkConnectionToClient.Send calls ([#1509](https://github.com/MirageNet/Mirage/issues/1509)) ([b8bcd9a](https://github.com/MirageNet/Mirage/commit/b8bcd9ad25895eee940a3daaf6fe7ed82eaf76ac))
* build in IL2CPP ([#1524](https://github.com/MirageNet/Mirage/issues/1524)) ([59faa81](https://github.com/MirageNet/Mirage/commit/59faa819262a166024b16d854e410c7e51763e6a)), closes [#1519](https://github.com/MirageNet/Mirage/issues/1519) [#1520](https://github.com/MirageNet/Mirage/issues/1520)
* Fixed NetworkRoomManager Template ([1662c5a](https://github.com/MirageNet/Mirage/commit/1662c5a139363dbe61784bb90ae6544ec74478c3))
* Fixed toc link ([3a0c7fb](https://github.com/MirageNet/Mirage/commit/3a0c7fb1ecd9d8715e797a7665ab9a6a7cb19e2a))
* Host Player Ready Race Condition ([#1498](https://github.com/MirageNet/Mirage/issues/1498)) ([4c4a52b](https://github.com/MirageNet/Mirage/commit/4c4a52bff95e7c56f065409b1399955813f3e145))
* NetworkIdentity.SetClientOwner: overwriting the owner was still possible even though it shouldn't be. all caller functions double check and return early if it already has an owner, so we should do the same here. ([548db52](https://github.com/MirageNet/Mirage/commit/548db52fdf224f06ba9d8b2d75460d31181b7066))
* NetworkServer.SpawnObjects: return false if server isn't running ([d4d524d](https://github.com/MirageNet/Mirage/commit/d4d524dad2a0a9d89538e6212dceda6bea71d2b7))
* properly detect NT rotation ([#1516](https://github.com/MirageNet/Mirage/issues/1516)) ([f0a993c](https://github.com/MirageNet/Mirage/commit/f0a993c1064384e0b3bd690d4d66be38875ed50e))
* return & continue on separate line ([#1504](https://github.com/MirageNet/Mirage/issues/1504)) ([61fdd89](https://github.com/MirageNet/Mirage/commit/61fdd892d9e6882e1e51094a2ceddfadc8fd1ebc))
* Room example to use new override ([e1d1d41](https://github.com/MirageNet/Mirage/commit/e1d1d41ed69f192b5fb91f92dcdeae1ee057d38f))
* SendToAll sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4b90aaf](https://github.com/MirageNet/Mirage/commit/4b90aafe12970e00949ee43b07b9edd5213745da))
* SendToObservers sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4267983](https://github.com/MirageNet/Mirage/commit/426798313920d23548048aa1c678167fd9b45cbd))
* SendToReady sends to that exact connection if it is detected as local connection, instead of falling back to the .localConnection field which might be something completely different. ([4596b19](https://github.com/MirageNet/Mirage/commit/4596b19dd959722d5dc659552206fe90c015fb01))


### Features

* Added NetworkConnection to OnRoomServerSceneLoadedForPlayer ([b5dfcf4](https://github.com/MirageNet/Mirage/commit/b5dfcf45bc9838e89dc37b00cf3653688083bdd8))
* Check for client authority in CmdClientToServerSync ([#1500](https://github.com/MirageNet/Mirage/issues/1500)) ([8b359ff](https://github.com/MirageNet/Mirage/commit/8b359ff6d07352a751f200768dcde6febd8e9303))
* Check for client authority in NetworkAnimator Cmd's ([#1501](https://github.com/MirageNet/Mirage/issues/1501)) ([ecc0659](https://github.com/MirageNet/Mirage/commit/ecc0659b87f3d910dc2370f4861ec70244a25622))
* Cosmetic Enhancement of Network Manager ([#1512](https://github.com/MirageNet/Mirage/issues/1512)) ([f53b12b](https://github.com/MirageNet/Mirage/commit/f53b12b2f7523574d7ceffa2a833dbd7fac755c9))
* NetworkSceneChecker use Scene instead of string name ([#1496](https://github.com/MirageNet/Mirage/issues/1496)) ([7bb80e3](https://github.com/MirageNet/Mirage/commit/7bb80e3b796f2c69d0958519cf1b4a9f4373268b))

# [11.0.0](https://github.com/MirageNet/Mirage/compare/10.0.0-master...11.0.0-master) (2020-02-13)


* Remove all compiler defines ([a394345](https://github.com/MirageNet/Mirage/commit/a3943459598d30a325fb1e1315b84c0dedf1741c))


### Features

* Block Play Mode and Builds for Weaver Errors ([#1479](https://github.com/MirageNet/Mirage/issues/1479)) ([0e80e19](https://github.com/MirageNet/Mirage/commit/0e80e1996fb2673364169782c330e69cd598a21d))
* Disposable PooledNetworkReader / PooledNetworkWriter ([#1490](https://github.com/MirageNet/Mirage/issues/1490)) ([bb55baa](https://github.com/MirageNet/Mirage/commit/bb55baa679ae780e127ed5817ef10d7f12cd08c8))


### BREAKING CHANGES

* removed compilation defines,  use upm version defines instead

# [10.0.0](https://github.com/MirageNet/Mirage/compare/9.1.0-master...10.0.0-master) (2020-02-12)


* Simplify AddPlayerForConnection (#62) ([fb26755](https://github.com/MirageNet/Mirage/commit/fb267557af292e048df248d4f85fff3569ac2963)), closes [#62](https://github.com/MirageNet/Mirage/issues/62)


### BREAKING CHANGES

* AddPlayerForConnection no longer receives the client

* fix compilatio errors

* fix build errors

# [9.1.0](https://github.com/MirageNet/Mirage/compare/9.0.0-master...9.1.0-master) (2020-02-12)


### Bug Fixes

* weaver support array of custom types ([#1470](https://github.com/MirageNet/Mirage/issues/1470)) ([d0b0bc9](https://github.com/MirageNet/Mirage/commit/d0b0bc92bc33ff34491102a2f9e1855f2a5ed476))


### Features

* Added Read<T> Method to NetworkReader ([#1480](https://github.com/MirageNet/Mirage/issues/1480)) ([58df3fd](https://github.com/MirageNet/Mirage/commit/58df3fd6d6f53336668536081bc33e2ca22fd38d))
* supports scriptable objects ([#1471](https://github.com/MirageNet/Mirage/issues/1471)) ([0f10c72](https://github.com/MirageNet/Mirage/commit/0f10c72744864ac55d2e1aa96ba8d7713c77d9e7))

# [9.0.0](https://github.com/MirageNet/Mirage/compare/8.0.1-master...9.0.0-master) (2020-02-08)


### Bug Fixes

* don't report error when stopping the server ([c965d4b](https://github.com/MirageNet/Mirage/commit/c965d4b0ff32288ebe4e5c63a43e32559203deb1))


### Features

* awaitable connect ([#55](https://github.com/MirageNet/Mirage/issues/55)) ([952e8a4](https://github.com/MirageNet/Mirage/commit/952e8a43e2b2e4443e24865c60af1ee47467e4cf))


### BREAKING CHANGES

* ClientConnect replaced with ClientConnectAsync
that can be awaited

* fix: TCP transport for async compliance

* use async connect

* Ignore telepathy tests until they are fixed

* It is ok to interrupt sockets

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

* Remove some warnings

## [8.0.1](https://github.com/MirageNet/Mirage/compare/8.0.0-master...8.0.1-master) (2020-02-06)


### Bug Fixes

* first connection id = 1 ([#60](https://github.com/MirageNet/Mirage/issues/60)) ([891dab9](https://github.com/MirageNet/Mirage/commit/891dab92d065821ca46b47ef2d3eb27124058d74))

# [8.0.0](https://github.com/MirageNet/Mirage/compare/7.0.0-master...8.0.0-master) (2020-02-06)


### Bug Fixes

* call callback after update dictionary in host ([#1476](https://github.com/MirageNet/Mirage/issues/1476)) ([1736bb0](https://github.com/MirageNet/Mirage/commit/1736bb0c42c0d2ad341e31a645658722de3bfe07))
* port network discovery ([d6a1154](https://github.com/MirageNet/Mirage/commit/d6a1154e98c52e7873411ce9d7b87f7b294dc436))
* remove scriptableobject error Tests ([479b78b](https://github.com/MirageNet/Mirage/commit/479b78bf3cabe93938bf61b7f8fd63ba46da0f4a))
* Telepathy reverted to older version to fix freezes when calling Client.Disconnect on some platforms like Windows 10 ([d0d77b6](https://github.com/MirageNet/Mirage/commit/d0d77b661cd07e25887f0e2f4c2d72b4f65240a2))
* Telepathy updated to latest version. Threads are closed properly now. ([4007423](https://github.com/MirageNet/Mirage/commit/4007423db28f7044f6aa97b108a6bfbe3f2d46a9))


* Renamed localEulerAnglesSensitivity (#1474) ([eee9692](https://github.com/MirageNet/Mirage/commit/eee969201d69df1e1ee1f1623b55a78f6003fbb1)), closes [#1474](https://github.com/MirageNet/Mirage/issues/1474)


### breaking

* Transports can now provide their Uri ([#1454](https://github.com/MirageNet/Mirage/issues/1454)) ([b916064](https://github.com/MirageNet/Mirage/commit/b916064856cf78f1c257f0de0ffe8c9c1ab28ce7)), closes [#38](https://github.com/MirageNet/Mirage/issues/38)


### Features

* Implemented NetworkReaderPool ([#1464](https://github.com/MirageNet/Mirage/issues/1464)) ([9257112](https://github.com/MirageNet/Mirage/commit/9257112c65c92324ad0bd51e4a017aa1b4c9c1fc))
* LAN Network discovery ([#1453](https://github.com/MirageNet/Mirage/issues/1453)) ([e75b45f](https://github.com/MirageNet/Mirage/commit/e75b45f8889478456573ea395694b4efc560ace0)), closes [#38](https://github.com/MirageNet/Mirage/issues/38)
* Mirror Icon for all components ([#1452](https://github.com/MirageNet/Mirage/issues/1452)) ([a7efb13](https://github.com/MirageNet/Mirage/commit/a7efb13e29e0bc9ed695a86070e3eb57b7506b4c))
* supports scriptable objects ([4b8f819](https://github.com/MirageNet/Mirage/commit/4b8f8192123fe0b79ea71f2255a4bbac404c88b1))


### BREAKING CHANGES

* localEulerAnglesSensitivity renamed to localRotationSensitivity
* Make the server uri method mandatory in transports

Co-authored-by: MrGadget <chris@clevertech.net>

# [7.0.0](https://github.com/MirageNet/Mirage/compare/6.0.0-master...7.0.0-master) (2020-01-27)


### Features

* Network Scene Checker Component ([#1271](https://github.com/MirageNet/Mirage/issues/1271)) ([71c0d3b](https://github.com/MirageNet/Mirage/commit/71c0d3b2ee1bbdb29d1c39ee6eca3ef9635d70bf))
* network writer and reader now support uri ([0c2556a](https://github.com/MirageNet/Mirage/commit/0c2556ac64bd93b9e52dae34cf8d84db114b4107))


* Rename NetworkServer.localClientActive ([7cd0894](https://github.com/MirageNet/Mirage/commit/7cd0894853b97fb804ae15b8a75b75c9d7bc04a7))
* Simplify spawning ([c87a38a](https://github.com/MirageNet/Mirage/commit/c87a38a4ff0c350901138b90db7fa8e61b1ab7db))


### BREAKING CHANGES

* rename localClientActive to LocalClientActive
* Spawn no longer receives NetworkClient

# [6.0.0](https://github.com/MirageNet/Mirage/compare/5.0.0-master...6.0.0-master) (2020-01-22)


### Bug Fixes

* compilation error ([df7baa4](https://github.com/MirageNet/Mirage/commit/df7baa4db0d347ee69c17bad9f9e56ccefb54fab))
* compilation error ([dc74256](https://github.com/MirageNet/Mirage/commit/dc74256fc380974ad6df59b5d1dee3884b879bd7))
* Fix Room Slots for clients ([#1439](https://github.com/MirageNet/Mirage/issues/1439)) ([268753c](https://github.com/MirageNet/Mirage/commit/268753c3bd0a9c0695d8d4510a129685be364a11))

# [5.0.0](https://github.com/MirageNet/Mirage/compare/4.0.0-master...5.0.0-master) (2020-01-19)

# [4.0.0](https://github.com/MirageNet/Mirage/compare/3.1.0-master...4.0.0-master) (2020-01-18)

# [3.1.0](https://github.com/MirageNet/Mirage/compare/3.0.4-master...3.1.0-master) (2020-01-16)


### Bug Fixes

* Decouple ChatWindow from player ([#1429](https://github.com/MirageNet/Mirage/issues/1429)) ([42a2f9b](https://github.com/MirageNet/Mirage/commit/42a2f9b853667ef9485a1d4a31979fcf1153c0d7))
* StopHost with offline scene calls scene change twice ([#1409](https://github.com/MirageNet/Mirage/issues/1409)) ([a0c96f8](https://github.com/MirageNet/Mirage/commit/a0c96f85189bfc9b5a936a8a33ebda34b460f17f))
* Telepathy works on .net core again ([cb3d9f0](https://github.com/MirageNet/Mirage/commit/cb3d9f0d08a961b345ce533d1ce64602f7041e1c))


### Features

* Add Sensitivity to NetworkTransform ([#1425](https://github.com/MirageNet/Mirage/issues/1425)) ([f69f174](https://github.com/MirageNet/Mirage/commit/f69f1743c54aa7810c5a218e2059c115760c54a3))

## [3.0.4](https://github.com/MirageNet/Mirage/compare/3.0.3-master...3.0.4-master) (2020-01-12)


### Bug Fixes

* comply with MIT license in upm package ([b879bef](https://github.com/MirageNet/Mirage/commit/b879bef4295e48c19d96a1d45536a11ea47380f3))

## [3.0.3](https://github.com/MirageNet/Mirage/compare/3.0.2-master...3.0.3-master) (2020-01-12)


### Bug Fixes

* auto reference mirrorng assembly ([93f8688](https://github.com/MirageNet/Mirage/commit/93f8688b39822bb30ed595ca36f44a8a556bec85))
* Mirage works with 2019.2 ([9f35d6b](https://github.com/MirageNet/Mirage/commit/9f35d6be427843aa7dd140cde32dd843c62147ce))

## [3.0.2](https://github.com/MirageNet/Mirage/compare/3.0.1-master...3.0.2-master) (2020-01-12)


### Bug Fixes

* remove Tests from upm package ([#34](https://github.com/MirageNet/Mirage/issues/34)) ([8d8ea0f](https://github.com/MirageNet/Mirage/commit/8d8ea0f10743044e4a9a3d6c5b9f9973cf48e28b))

## [3.0.1](https://github.com/MirageNet/Mirage/compare/3.0.0-master...3.0.1-master) (2020-01-11)


### Bug Fixes

* remove Tests from UPM ([#33](https://github.com/MirageNet/Mirage/issues/33)) ([8f42af0](https://github.com/MirageNet/Mirage/commit/8f42af0a3992cfa549eb404ad9f9693101895ce9))

# [3.0.0](https://github.com/MirageNet/Mirage/compare/2.0.0-master...3.0.0-master) (2020-01-11)


### Bug Fixes

* [#723](https://github.com/MirageNet/Mirage/issues/723) - NetworkTransform teleport works properly now ([fd7dc5e](https://github.com/MirageNet/Mirage/commit/fd7dc5e226a76b27250fb503a98f23eb579387f8))
* fix release pipeline ([2a3db0b](https://github.com/MirageNet/Mirage/commit/2a3db0b398cd641c3e1ba65a32b34822e9c9169f))
* release job requires node 10 ([3f50e63](https://github.com/MirageNet/Mirage/commit/3f50e63bc32f4942e1c130c681dabd34ae81b117))
* remove tests from npm package ([#32](https://github.com/MirageNet/Mirage/issues/32)) ([5ed9b4f](https://github.com/MirageNet/Mirage/commit/5ed9b4f1235d5d1dc54c3f50bb1aeefd5dbe3038))
* syntax error in release job ([2eeaea4](https://github.com/MirageNet/Mirage/commit/2eeaea41bc81cfe0c191b39da912adc565e11ec7))


### Features

* Network Animator can reset triggers ([#1420](https://github.com/MirageNet/Mirage/issues/1420)) ([dffdf02](https://github.com/MirageNet/Mirage/commit/dffdf02be596db3d35bdd8d19ba6ada7d796a137))
* NetworkAnimator warns if you use it incorrectly ([#1424](https://github.com/MirageNet/Mirage/issues/1424)) ([c30e4a9](https://github.com/MirageNet/Mirage/commit/c30e4a9f83921416f936ef5fd1bb0e2b3a410807))


### Performance Improvements

* Use NetworkWriterPool in NetworkAnimator ([#1421](https://github.com/MirageNet/Mirage/issues/1421)) ([7d472f2](https://github.com/MirageNet/Mirage/commit/7d472f21f9a807357df244a3f0ac259dd431661f))
* Use NetworkWriterPool in NetworkTransform ([#1422](https://github.com/MirageNet/Mirage/issues/1422)) ([a457845](https://github.com/MirageNet/Mirage/commit/a4578458a15e3d2840a49dd029b4c404cadf85a4))

# [2.0.0](https://github.com/MirageNet/Mirage/compare/1.1.2-master...2.0.0-master) (2020-01-09)

## [1.1.2](https://github.com/MirageNet/Mirage/compare/1.1.1-master...1.1.2-master) (2020-01-09)


### Bug Fixes

* [#1241](https://github.com/MirageNet/Mirage/issues/1241) - Telepathy updated to latest version. All tests are passing again. Thread.Interrupt was replaced by Abort+Join. ([228b32e](https://github.com/MirageNet/Mirage/commit/228b32e1da8e407e1d63044beca0fd179f0835b4))
* [#1278](https://github.com/MirageNet/Mirage/issues/1278) - only call initial state SyncVar hooks on clients if the SyncVar value is different from the default one. ([#1414](https://github.com/MirageNet/Mirage/issues/1414)) ([a3ffd12](https://github.com/MirageNet/Mirage/commit/a3ffd1264c2ed2780e6e86ce83077fa756c01154))
* [#1380](https://github.com/MirageNet/Mirage/issues/1380) - NetworkConnection.clientOwnedObjects changed from uint HashSet to NetworkIdentity HashSet for ease of use and to fix a bug where DestroyOwnedObjects wouldn't find a netId anymore in some cases. ([a71ecdb](https://github.com/MirageNet/Mirage/commit/a71ecdba4a020f9f4648b8275ec9d17b19aff55f))
* FinishLoadSceneHost calls FinishStart host which now calls StartHostClient AFTER server online scene was loaded. Previously there was a race condition where StartHostClient was called immediately in StartHost, before the scene change even finished. This was still from UNET. ([df9c29a](https://github.com/MirageNet/Mirage/commit/df9c29a6b3f9d0c8adbaff5a500e54abddb303b3))

## [1.1.1](https://github.com/MirageNet/Mirage/compare/1.1.0-master...1.1.1-master) (2020-01-05)


### Bug Fixes

* add Changelog metadata fix [#31](https://github.com/MirageNet/Mirage/issues/31) ([c67de22](https://github.com/MirageNet/Mirage/commit/c67de2216aa331de10bba2e09ea3f77e6b1caa3c))

# [1.1.0](https://github.com/MirageNet/Mirage/compare/1.0.0-master...1.1.0-master) (2020-01-04)


### Features

* include generated changelog ([#27](https://github.com/MirageNet/Mirage/issues/27)) ([a60f1ac](https://github.com/MirageNet/Mirage/commit/a60f1acd3a544639a5e58a8946e75fd6c9012327))
