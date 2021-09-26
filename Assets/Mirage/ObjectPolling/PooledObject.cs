using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Runtime
{
    public struct PooledObject<T>
    {
        public T Object;
        public NetworkIdentity NetId;
    }

    internal struct ObjectPooling<T> where T : Object
    {
        #region Fields

        private readonly Queue<PooledObject<T>> _inactiveGameObjects;
        private readonly T _objectPrefabToPool;
        private readonly NetworkIdentity _networkObjectPrefabToPool;
        internal readonly HashSet<int> PooledObjectIDs;
        private readonly GameObject _parentBackObject;

        #endregion

        /// <summary>
        ///     Constructor for this object pool.
        /// </summary>
        /// <param name="poolObject">The object we want to have pooled.</param>
        /// <param name="initialPool">How many objects do we want to start out with in the pool.</param>
        /// <param name="networkParentObject">When sending back to pool, where should we parent the object back to.</param>
        public ObjectPooling(T poolObject, int initialPool, GameObject networkParentObject)
        {
            _objectPrefabToPool = poolObject;
            _inactiveGameObjects = new Queue<PooledObject<T>>(initialPool);
            PooledObjectIDs = new HashSet<int>();
            _parentBackObject = networkParentObject;
            _networkObjectPrefabToPool = null;
        }

        /// <summary>
        ///     Constructor for this object pool.
        /// </summary>
        /// <param name="poolObject">The network identity we want to have pooled.</param>
        /// <param name="initialPool">How many objects do we want to start out with in the pool.</param>
        /// <param name="networkParentObject">When sending back to pool, where should we parent the object back to.</param>
        public ObjectPooling(NetworkIdentity poolObject, int initialPool, GameObject networkParentObject)
        {
            _networkObjectPrefabToPool = poolObject;
            _inactiveGameObjects = new Queue<PooledObject<T>>(initialPool);
            PooledObjectIDs = new HashSet<int>();
            _parentBackObject = networkParentObject;
            _objectPrefabToPool = null;
        }

        #region Network Pool Spawning

        /// <summary>
        ///     Method to spawn network object from our inactive pool.
        /// </summary>
        /// <param name="position">The position we want to spawn the object at.</param>
        /// <param name="rotation">The rotation we want to have the object spawn with.</param>
        /// <returns></returns>
        public NetworkIdentity NetworkSpawn(Vector3 position, Quaternion rotation)
        {
            while (true)
            {
                NetworkIdentity spawnedObject;

                switch (_inactiveGameObjects.Count)
                {
                    case 0:
                        spawnedObject = Object.Instantiate(_networkObjectPrefabToPool, position, rotation);

                        PooledObjectIDs.Add(spawnedObject.gameObject.GetInstanceID());

                        return spawnedObject;
                    default:
                        spawnedObject = _inactiveGameObjects.Dequeue().NetId;

                        if (spawnedObject is null)
                            continue;

                        break;
                }

                spawnedObject.transform.SetParent(null);
                spawnedObject.transform.position = position;
                spawnedObject.transform.rotation = rotation;

                return spawnedObject;
            }
        }

        /// <summary>
        ///     Method to spawn network object from our inactive pool.
        /// </summary>
        /// <param name="parentTransform">The transform to parent object to.</param>
        /// <param name="worldStayPosition">Unity specific <see cref="Instantiate" /></param>
        /// <returns></returns>
        public NetworkIdentity NetworkSpawn(Transform parentTransform, bool worldStayPosition)
        {
            while (true)
            {
                NetworkIdentity spawnedObject;

                if (_inactiveGameObjects.Count > 0)
                {
                    spawnedObject = _inactiveGameObjects.Dequeue().NetId;

                    if (spawnedObject is null)
                        continue;

                    spawnedObject.transform.SetParent(parentTransform, worldStayPosition);

                    return spawnedObject;
                }

                spawnedObject = Object.Instantiate(_networkObjectPrefabToPool, parentTransform, worldStayPosition);

                PooledObjectIDs.Add(spawnedObject.gameObject.GetInstanceID());

                return spawnedObject;
            }
        }

        /// <summary>
        ///     Method to return object back to the pool for re-usage.
        /// </summary>
        /// <param name="obj">The object we want to return back to the pool.</param>
        internal void Despawn(NetworkIdentity obj)
        {
            if (!obj.gameObject.activeInHierarchy) return;

            obj.gameObject.SetActive(false);

            obj.transform.SetParent(_parentBackObject.transform);

            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;

            var recycle = new PooledObject<T>();

            _inactiveGameObjects.Enqueue(recycle);
        }

        #endregion

        #region Normal Pool Spawning.

        /// <summary>
        ///     Method to spawn an object from our inactive pool.
        /// </summary>
        /// <param name="position">The position we want to spawn the object at.</param>
        /// <param name="rotation">The rotation we want to have the object spawn with.</param>
        /// <returns></returns>
        public object Spawn(Vector3 position, Quaternion rotation)
        {
            while (true)
            {
                T spawnedObject;

                switch (_inactiveGameObjects.Count)
                {
                    case 0:
                        spawnedObject = Object.Instantiate(_objectPrefabToPool, position, rotation);

                        PooledObjectIDs.Add(spawnedObject.GetInstanceID());

                        return spawnedObject;
                    default:
                        spawnedObject = _inactiveGameObjects.Dequeue().Object;

                        if (spawnedObject is null)
                            continue;

                        break;
                }

                (spawnedObject as GameObject)?.transform.SetParent(null);
                (spawnedObject as GameObject).transform.position = position;
                (spawnedObject as GameObject).transform.rotation = rotation;

                return spawnedObject;
            }
        }

        /// <summary>
        ///     Method to spawn an object from our inactive pool.
        /// </summary>
        /// <param name="parentTransform">The transform to parent object to.</param>
        /// <param name="worldStayPosition">Unity specific <see cref="Instantiate" /></param>
        /// <returns></returns>
        public T Spawn(Transform parentTransform, bool worldStayPosition)
        {
            while (true)
            {
                T spawnedObject;

                if (_inactiveGameObjects.Count > 0)
                {
                    spawnedObject = _inactiveGameObjects.Dequeue().Object;

                    if (spawnedObject is null)
                        continue;

                    (spawnedObject as MonoBehaviour)?.transform.SetParent(parentTransform, worldStayPosition);

                    return spawnedObject;
                }

                spawnedObject = Object.Instantiate(_objectPrefabToPool, parentTransform, worldStayPosition);

                PooledObjectIDs.Add(spawnedObject.GetInstanceID());

                return spawnedObject;
            }
        }

        /// <summary>
        ///     Method to return object back to the pool for re-usage.
        /// </summary>
        /// <param name="obj">The object we want to return back to the pool.</param>
        internal void Despawn(T obj)
        {
            var castObjMono = obj as MonoBehaviour;

            if (!(castObjMono is null) && castObjMono.gameObject.activeInHierarchy)
            {
                castObjMono.gameObject.SetActive(false);

                castObjMono.transform.SetParent(_parentBackObject.transform);

                castObjMono.transform.position = Vector3.zero;
                castObjMono.transform.rotation = Quaternion.identity;
            }

            if (castObjMono is null)
            {
                if (!(obj is GameObject castObjGameObject))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        "Incorrect spawning of local objects. Please check and spawn objects as a monobeahviour type or game object type only.");
#endif
                    return;
                }

                castObjGameObject.gameObject.SetActive(false);

                castObjGameObject.transform.SetParent(_parentBackObject.transform);

                castObjGameObject.transform.position = Vector3.zero;
                castObjGameObject.transform.rotation = Quaternion.identity;
            }

            var recycle = new PooledObject<T>();

            _inactiveGameObjects.Enqueue(recycle);
        }

        #endregion
    }
}
