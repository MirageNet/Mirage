#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Mirage.Logging;

using UnityEditor;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#elif UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Mirage
{
    internal static class NetworkIdentityIdGenerator
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkIdentityIdGenerator));

        /// <summary>
        /// Keep track of all sceneIds to detect scene duplicates
        /// <para>we only need to check the id part here. The Scene Hash part is only needed when a scene is duplciated</para>
        /// </summary>
        internal static readonly Dictionary<int, NetworkIdentity> _sceneIds = new Dictionary<int, NetworkIdentity>();

        /// <summary>
        /// Sets the scene hash on the NetworkIdentity
        /// <para>This will stop duplciate ID if the scene is duplicated</para>
        /// <para>NOTE: Only call this from NetworkScenePostProcess</para>
        /// </summary>
        /// <param name="identity"></param>
        // todo: can we call this from OnValidate instead? will that work with scene duplications?
        internal static void SetSceneHash(NetworkIdentity identity)
        {
            var wrapper = new IdentityWrapper(identity);

            // get deterministic scene hash
            var pathHash = GetSceneHash(identity);

            wrapper.SceneHash = pathHash;

            // log it. this is incredibly useful to debug sceneId issues.
            if (logger.LogEnabled()) logger.Log($"{identity.name} in scene={identity.gameObject.scene.name} scene index hash({pathHash:X}) scene id: {wrapper.SceneId:X}");
        }

        private static int GetSceneHash(NetworkIdentity identity)
        {
            return identity.gameObject.scene.path.GetStableHashCode();
        }

        internal static void SetupIDs(NetworkIdentity identity)
        {
            var wrapper = new IdentityWrapper(identity);
            PrefabStage stage;

            if (PrefabUtility.IsPartOfPrefabAsset(identity.gameObject))
            {
                wrapper.ClearSceneId();
                AssignAssetID(identity);
            }
            // Unity calls OnValidate for prefab and other scene objects based on that prefab
            //
            // are we modifying THIS prefab, or just a scene object based on the prefab?
            //   * GetCurrentPrefabStage = 'are we editing ANY prefab?'
            //   * GetPrefabStage(go) = 'are we editing THIS prefab?'
            else if ((stage = PrefabStageUtility.GetCurrentPrefabStage()) != null)
            {
                // nested if, we want to do nothing if we are not the prefab being edited
                if (PrefabStageUtility.GetPrefabStage(identity.gameObject) != null)
                {
                    wrapper.ClearSceneId();
                    AssignAssetID(identity, GetStagePath(stage));
                }
            }
            else if (SceneObjectWithPrefabParent(identity, out var parent))
            {
                AssignSceneID(identity);
                AssignAssetID(identity, parent);
            }
            else
            {
                AssignSceneID(identity);
                wrapper.PrefabHash = 0;
            }
        }

        private static string GetStagePath(PrefabStage stage)
        {
            // NOTE: might make sense to use GetPrefabStage for asset
            //       path, but let's not touch it while it works.
#if UNITY_2020_1_OR_NEWER
            return stage.assetPath;
#else
            return stage.prefabAssetPath;
#endif
        }

        private static bool SceneObjectWithPrefabParent(NetworkIdentity identity, out GameObject parent)
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

        private static void AssignAssetID(NetworkIdentity identity, GameObject parent)
        {
            var path = AssetDatabase.GetAssetPath(parent);
            AssignAssetID(identity, path);
        }

        private static void AssignAssetID(NetworkIdentity identity)
        {
            var path = AssetDatabase.GetAssetPath(identity.gameObject);
            AssignAssetID(identity, path);
        }

        private static void AssignAssetID(NetworkIdentity identity, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                // dont log warning here, sometimes prefab has no asset path
                return;
            }

            var wrapper = new IdentityWrapper(identity);
            wrapper.PrefabHash = path.GetStableHashCode();
        }

        /// <summary>
        /// Ensures that a NetworkIdentity has a Random Unique ID
        /// </summary>
        /// <param name="identity"></param>
        /// <remarks>
        /// We use a random id here instead of index because the order of `FindObjectOfType` might change if something in the scene changes
        /// <para>
        /// Id must be assigned at edit time. This is to make sure they are the the same between builds
        /// </para>
        /// </remarks>
        private static void AssignSceneID(NetworkIdentity identity)
        {
            // Only generate at editor time
            if (Application.isPlaying)
                return;

            var wrapper = new IdentityWrapper(identity);

            if (wrapper.SceneId == 0 || IsDuplicate(identity, wrapper.SceneId))
            {
                // clear in any case, because it might have been a duplicate
                wrapper.ClearSceneId();

                // Dont generate when building
                // this Will Cause a new Random ID for each build
                // we need to generate it as edit time
                if (BuildPipeline.isBuildingPlayer)
                    throw new InvalidOperationException($"Scene {identity.gameObject.scene.path} needs to be opened and resaved before building, because the scene object {identity.name} has no valid sceneId yet.");

                var randomId = GetRandomUInt();

                // only assign if not a duplicate of an existing scene id (small chance, but possible)
                if (!IsDuplicate(identity, randomId))
                {
                    wrapper.SceneId = randomId;
                }
            }

            // Add to dictionary so we can keep track of ID for duplicates
            _sceneIds[wrapper.SceneId] = identity;
        }

        private static bool IsDuplicate(NetworkIdentity identity, int sceneId)
        {
            if (_sceneIds.TryGetValue(sceneId, out var existing))
            {
                // if existing is null we can use this id
                if (existing == null)
                    return false;

                // if not equal, then is 2 objects with duplicate IDs
                return identity != existing;
            }
            else
            {
                // not found, so not duplicate
                return false;
            }
        }

        /// <summary>
        /// Gets random int using secure randon
        /// </summary>
        /// <returns></returns>
        private static int GetRandomUInt()
        {
            // use Crypto RNG to avoid having time based duplicates
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        /// <summary>
        /// Wrapper for NetworkIdentity that will set and save fields
        /// </summary>
        private sealed class IdentityWrapper
        {
            private const ulong ID_MASK = 0x0000_0000_FFFF_FFFFul;
            private const ulong HASH_MASK = 0xFFFF_FFFF_0000_0000ul;
            private readonly NetworkIdentity _identity;

            public IdentityWrapper(NetworkIdentity identity)
            {
                if (identity == null) throw new ArgumentNullException(nameof(identity));

                _identity = identity;
            }

            public int PrefabHash
            {
                get => _identity.Editor_PrefabHash;
                set
                {
                    if (PrefabHash == value)
                        return;

                    Undo.RecordObject(_identity, "Set PrefabHash");
                    _identity.Editor_PrefabHash = value;
                }
            }


            public int SceneId
            {
                get => (int)(_identity.Editor_SceneId & ID_MASK);
                set
                {
                    // just use get here so we get the correct part of id
                    if (SceneId == value)
                        return;

                    Undo.RecordObject(_identity, "Set SceneId");
                    // have to mask incoming number incase it is negative
                    _identity.Editor_SceneId = (_identity.SceneId & HASH_MASK) | ((ulong)value & ID_MASK);
                }
            }

            public int SceneHash
            {
                get => (int)((_identity.Editor_SceneId & HASH_MASK) >> 32);
                set
                {
                    // just use get here so we get the correct part of id
                    if (SceneHash == value)
                        return;

                    Undo.RecordObject(_identity, "Set SceneHash");
                    _identity.Editor_SceneId = (((ulong)value) << 32) | (_identity.SceneId & ID_MASK);
                }
            }

            public void ClearSceneId()
            {
                SceneId = 0;
                SceneHash = 0;
            }
        }
    }
}
#endif
