using Mirage.Events;
using Mirage.Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    /// <summary>
    /// Manages visibility state of GameObject's across additive scenes for the host
    /// </summary>
    public class AdditiveVisibilityManager : MonoBehaviour
    {

        private static readonly ILogger logger = LogFactory.GetLogger<AdditiveVisibilityManager>();

        public NetworkServer Server;

        /// <summary>
        /// Ignored by visibilty manager. Useful for shared scenes or when you want additive scenes that are still visible by player
        /// </summary>
        public List<string> ForceVisibleSceneNames = new List<string>();

        private Scene playerSceneCache;

        protected virtual void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected virtual void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void Update()
        {
            if (!TryGetLocalPlayerIfCharacterExists(out INetworkPlayer localPlayer)) return;

            //Check if local players scene has changed
            //This is an every frame scene check, so there may be a better way

            Scene playerScene = localPlayer.Identity.gameObject.scene;

            if (playerScene != playerSceneCache)
            {
                //Player has changed scenes
                OnLocalPlayerChangedScenes(localPlayer, playerSceneCache, playerScene);

                //Make sure to update our cache
                playerSceneCache = playerScene;
            }
        }

        private void OnLocalPlayerChangedScenes(INetworkPlayer localPlayer, Scene oldScene, Scene newScene)
        {
            if (logger.LogEnabled()) logger.Log($"[AdditiveVisibilityManager] - Local player changed from scene {oldScene.name} to {newScene.name}");

            //Because our player is in a new scene, we want to change visibility states.
            //We want to hide the old scene objects and reveal the new scene objects

            if (oldScene.IsValid() && oldScene.isLoaded)
                SetSceneVisibility(oldScene, false);

            SetSceneVisibility(newScene, true);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            //Check if we should maintain visibility of objects in this scene

            if (logger.LogEnabled()) logger.Log("[AdditiveVisibilityManager] - Scene Loaded");
            
            //Attempt to get the local player
            //If the player does not exist, we do not want to continue with our visibility check
            if (!TryGetLocalPlayerIfCharacterExists(out INetworkPlayer localPlayer)) return;

            if (localPlayer.Identity.gameObject.scene != scene)
            {
                //Local player is not in the same scene as the loaded scene
                //Disable visibility for this scene

                if (logger.LogEnabled()) logger.Log($"[AdditiveVisibilityManager] - Disabling visibility for scene {scene.name}");

                SetSceneVisibility(scene, false);
            }
        }

        /// <summary>
        /// Sets the visibility of objects within a scene.
        /// By default, this will set all renderer's enable to the state.
        /// It is recommended to override in the event you want to set the visibility for other objects or components such as decals
        /// </summary>
        /// <param name="state">The state of visibility</param>
        protected virtual void SetSceneVisibility(Scene scene, bool state)
        {
            //Check if this scene is supposed to be part of visibility.
            //In the event you have a shared scene, or an additive scene for graphics
            //you may not want this scene being disabled just because your player is not in the scene.
            //This will prevent that.
            if (ForceVisibleSceneNames.Contains(scene.name))
            {
                if (logger.LogEnabled()) logger.Log($"[AdditiveVisibilityManager] - Ignoring SetSceneVisibility request because scene ({scene.name}) was foudn in ForceVisibleSceneNames collection");

                return;
            }

            if (logger.LogEnabled()) logger.Log($"[AdditiveVisibilityManager] - Setting scene visibility of scene {scene.name} to {state}");

            //Disable all renderers.
            Renderer[] renderers = GetObjectsOfTypeInScene<Renderer>(scene);

            foreach (Renderer renderer in renderers)
            {
                //Set each visibility of the renderer to the state

                renderer.enabled = state;
            }
        }

        /// <summary>
        /// Scans for all objects of a type in a scene
        /// </summary>
        /// <typeparam name="T">The type to scan for</typeparam>
        /// <param name="scene">The scene to scan in</param>
        /// <returns>A collection of the type found in the scene</returns>
        protected T[] GetObjectsOfTypeInScene<T>(Scene scene) where T : Object
        {
            //Get all of the scenes root objects.
            //This is the only way to get objects in a scene, unless you use FindObjectsOfType<T> and filter the scene
            GameObject[] rootObjects = scene.GetRootGameObjects();

            List<T> targets = new List<T>();

            foreach (GameObject rootObject in rootObjects)
            {
                //This also gets the root component, so no need to explicitly check
                targets.AddRange(rootObject.GetComponentsInChildren<T>(true));
            }

            return targets.ToArray();
        }

        /// <summary>
        /// Attempts to get the local player given that the identity and character exists, providing logs along the way
        /// </summary>
        /// <param name="localPlayer">The local player output</param>
        /// <returns>If the operation was successful</returns>
        private bool TryGetLocalPlayerIfCharacterExists(out INetworkPlayer localPlayer)
        {
            //Get host local player
            localPlayer = Server.LocalPlayer;

            //Check if local player is set
            if (localPlayer == null)
            {
                if (logger.LogEnabled()) logger.Log("[AdditiveVisibilityManager] - Local player is null. Ignoring scene load");
                return false;
            }

            //Check if character exists
            if (localPlayer.Identity == null)
            {
                if (logger.LogEnabled()) logger.Log("[AdditiveVisibilityManager] - Local player identity is null. Ignoring scene load.");
                return false;
            }

            return true;
        }
    }
}