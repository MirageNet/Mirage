﻿using Cysharp.Threading.Tasks;

namespace Mirage.Tests.Runtime.Host
{
    public class HostSetupWithSceneManager<T> : HostSetup<T> where T : NetworkBehaviour
    {
        protected NetworkSceneManager sceneManager;

        public override void ExtraSetup()
        {
            sceneManager = networkManagerGo.AddComponent<NetworkSceneManager>();
            sceneManager.Client = client;
            sceneManager.Server = server;

            serverObjectManager.NetworkSceneManager = sceneManager;
            clientObjectManager.NetworkSceneManager = sceneManager;
        }
        public override UniTask ExtraTearDownAsync()
        {
            return TestScene.UnloadAdditiveScenes();
        }
    }
}
