using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Mirage.Logging;
#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
#endif

namespace Mirage
{
    internal static class NetworkIdentityIdGenerator
    {
        static readonly ILogger logger = LogFactory.GetLogger(nameof(NetworkIdentityIdGenerator));

        /// <summary>
        /// Keep track of all sceneIds to detect scene duplicates
        /// </summary>
        public static readonly Dictionary<ulong, NetworkIdentity> sceneIds = new Dictionary<ulong, NetworkIdentity>();

        // copy scene path hash into sceneId for scene objects.
        // this is the only way for scene file duplication to not contain
        // duplicate sceneIds as it seems.
        // -> sceneId before: 0x00000000AABBCCDD
        // -> then we clear the left 4 bytes, so that our 'OR' uses 0x00000000
        // -> then we OR the hash into the 0x00000000 part
        // -> buildIndex is not enough, because Editor and Build have different
        //    build indices if there are disabled scenes in build settings, and
        //    if no scene is in build settings then Editor and Build have
        //    different indices too (Editor=0, Build=-1)
        // => ONLY USE THIS FROM POSTPROCESSSCENE!
        public static void SetSceneIdSceneHashPartInternal(NetworkIdentity identity)
        {
            // get deterministic scene hash
            uint pathHash = (uint)identity.gameObject.scene.path.GetStableHashCode();

            // shift hash from 0x000000FFFFFFFF to 0xFFFFFFFF00000000
            ulong shiftedHash = (ulong)pathHash << 32;

            // OR into scene id
            identity.sceneId = (identity.sceneId & 0xFFFFFFFF) | shiftedHash;

            // log it. this is incredibly useful to debug sceneId issues.
            if (logger.LogEnabled()) logger.Log($"{identity.name} in scene={identity.gameObject.scene.name} scene index hash({pathHash:X}) copied into sceneId: {identity.sceneId:X}");
        }

        public static void SetupIDs(NetworkIdentity identity)
        {
            if (ThisIsAPrefab(identity))
            {
                // force 0 for prefabs
                identity.sceneId = 0;
                AssignAssetID(identity);
            }
            // are we currently in prefab editing mode? aka prefab stage
            // => check prefabstage BEFORE SceneObjectWithPrefabParent
            //    (fixes https://github.com/vis2k/Mirror/issues/976)
            // => if we don't check GetCurrentPrefabStage and only check
            //    GetPrefabStage(gameObject), then the 'else' case where we
            //    assign a sceneId and clear the assetId would still be
            //    triggered for prefabs. in other words: if we are in prefab
            //    stage, do not bother with anything else ever!
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                // when modifying a prefab in prefab stage, Unity calls
                // OnValidate for that prefab and for all scene objects based on
                // that prefab.
                //
                // is this GameObject the prefab that we modify, and not just a
                // scene object based on the prefab?
                //   * GetCurrentPrefabStage = 'are we editing ANY prefab?'
                //   * GetPrefabStage(go) = 'are we editing THIS prefab?'
                if (PrefabStageUtility.GetPrefabStage(identity.gameObject) != null)
                {
                    // force 0 for prefabs
                    identity.sceneId = 0;
                    // NOTE: might make sense to use GetPrefabStage for asset
                    //       path, but let's not touch it while it works.
#if UNITY_2020_1_OR_NEWER
                    string path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
#else
                    string path = PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
#endif

                    AssignAssetID(identity, path);
                }
            }
            else if (ThisIsASceneObjectWithPrefabParent(identity, out GameObject parent))
            {
                AssignSceneID(identity);
                AssignAssetID(identity, parent);
            }
            else
            {
                AssignSceneID(identity);
                identity._prefabHash = 0;
            }
        }

        static void AssignAssetID(NetworkIdentity identity, GameObject parent)
        {
            string path = AssetDatabase.GetAssetPath(parent);
            AssignAssetID(identity, path);
        }

        static void AssignAssetID(NetworkIdentity identity)
        {
            string path = AssetDatabase.GetAssetPath(identity.gameObject);
            AssignAssetID(identity, path);
        }

        static void AssignAssetID(NetworkIdentity identity, string path)
        {
            identity._prefabHash = path.GetStableHashCode();
        }

        static bool ThisIsAPrefab(NetworkIdentity identity) => PrefabUtility.IsPartOfPrefabAsset(identity.gameObject);

        static bool ThisIsASceneObjectWithPrefabParent(NetworkIdentity identity, out GameObject parent)
        {
            parent = null;

            if (!PrefabUtility.IsPartOfPrefabInstance(identity.gameObject))
            {
                return false;
            }
            parent = PrefabUtility.GetCorrespondingObjectFromSource(identity.gameObject);

            if (parent is null)
            {
                logger.LogError($"Failed to find prefab parent for scene object [name:{identity.gameObject.name}]");
                return false;
            }
            return true;
        }

        static uint GetRandomUInt()
        {
            // use Crypto RNG to avoid having time based duplicates
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        // persistent sceneId assignment
        // (because scene objects have no persistent unique ID in Unity)
        //
        // original UNET used OnPostProcessScene to assign an index based on
        // FindObjectOfType<NetworkIdentity> order.
        // -> this didn't work because FindObjectOfType order isn't deterministic.
        // -> one workaround is to sort them by sibling paths, but it can still
        //    get out of sync when we open scene2 in editor and we have
        //    DontDestroyOnLoad objects that messed with the sibling index.
        //
        // we absolutely need a persistent id. challenges:
        // * it needs to be 0 for prefabs
        //   => we set it to 0 in SetupIDs() if prefab!
        // * it needs to be only assigned in edit time, not at runtime because
        //   only the objects that were in the scene since beginning should have
        //   a scene id.
        //   => Application.isPlaying check solves that
        // * it needs to detect duplicated sceneIds after duplicating scene
        //   objects
        //   => sceneIds dict takes care of that
        // * duplicating the whole scene file shouldn't result in duplicate
        //   scene objects
        //   => buildIndex is shifted into sceneId for that.
        //   => if we have no scenes in build index then it doesn't matter
        //      because by definition a build can't switch to other scenes
        //   => if we do have scenes in build index then it will be != -1
        //   note: the duplicated scene still needs to be opened once for it to
        //          be set properly
        // * scene objects need the correct scene index byte even if the scene's
        //   build index was changed or a duplicated scene wasn't opened yet.
        //   => OnPostProcessScene is the only function that gets called for
        //      each scene before runtime, so this is where we set the scene
        //      byte.
        // * disabled scenes in build settings should result in same scene index
        //   in editor and in build
        //   => .gameObject.scene.buildIndex filters out disabled scenes by
        //      default
        // * generated sceneIds absolutely need to set scene dirty and force the
        //   user to resave.
        //   => Undo.RecordObject does that perfectly.
        // * sceneIds should never be generated temporarily for unopened scenes
        //   when building, otherwise editor and build get out of sync
        //   => BuildPipeline.isBuildingPlayer check solves that
        static void AssignSceneID(NetworkIdentity identity)
        {
            // we only ever assign sceneIds at edit time, never at runtime.
            // by definition, only the original scene objects should get one.
            // -> if we assign at runtime then server and client would generate
            //    different random numbers!
            if (Application.isPlaying)
                return;

            // no valid sceneId yet, or duplicate?
            bool duplicate = sceneIds.TryGetValue(identity.sceneId, out NetworkIdentity existing) && existing != null && existing != identity;
            if (identity.sceneId == 0 || duplicate)
            {
                // clear in any case, because it might have been a duplicate
                identity.sceneId = 0;

                // if a scene was never opened and we are building it, then a
                // sceneId would be assigned to build but not saved in editor,
                // resulting in them getting out of sync.
                // => don't ever assign temporary ids. they always need to be
                //    permanent
                // => throw an exception to cancel the build and let the user
                //    know how to fix it!
                if (BuildPipeline.isBuildingPlayer)
                    throw new InvalidOperationException($"Scene {identity.gameObject.scene.path} needs to be opened and resaved before building, because the scene object {identity.name} has no valid sceneId yet.");

                // if we generate the sceneId then we MUST be sure to set dirty
                // in order to save the scene object properly. otherwise it
                // would be regenerated every time we reopen the scene, and
                // upgrading would be very difficult.
                // -> Undo.RecordObject is the new EditorUtility.SetDirty!
                // -> we need to call it before changing.
                Undo.RecordObject(identity, "Generated SceneId");

                // generate random sceneId part (0x00000000FFFFFFFF)
                uint randomId = GetRandomUInt();

                // only assign if not a duplicate of an existing scene id
                // (small chance, but possible)
                duplicate = sceneIds.TryGetValue(randomId, out existing) && existing != null && existing != identity;
                if (!duplicate)
                {
                    identity.sceneId = randomId;
                }
            }

            // add to sceneIds dict no matter what
            // -> even if we didn't generate anything new, because we still need
            //    existing sceneIds in there to check duplicates
            sceneIds[identity.sceneId] = identity;
        }
    }
}
