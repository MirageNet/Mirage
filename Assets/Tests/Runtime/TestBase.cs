using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests.Runtime
{
    public abstract class TestBase
    {
        List<GameObject> toDestroy = new List<GameObject>();

        /// <summary>
        /// Call this from child class teardown
        /// </summary>
        protected void TearDownTestObjects()
        {
            foreach (GameObject obj in toDestroy)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            toDestroy.Clear();
        }

        /// <summary>
        /// Instantiate object that will be destroyed in teardown
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected GameObject InstantiateForTest(GameObject prefab)
        {
            GameObject obj = Object.Instantiate(prefab);
            toDestroy.Add(obj);
            return obj;
        }
        /// <summary>
        /// Instantiate object that will be destroyed in teardown
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected TObj InstantiateForTest<TObj>(TObj prefab) where TObj : Component
        {
            TObj obj = Object.Instantiate(prefab);
            toDestroy.Add(obj.gameObject);
            return obj;
        }

        /// <summary>
        /// Creates a new NetworkIdentity that can be used by tests, then destroyed in teardown 
        /// </summary>
        /// <returns></returns>
        protected NetworkIdentity CreateNetworkIdentity()
        {
            var go = new GameObject($"A NetworkIdentity {toDestroy.Count}", typeof(NetworkIdentity));
            toDestroy.Add(go);
            return go.GetComponent<NetworkIdentity>();
        }

        /// <summary>
        /// Creates a new NetworkIdentity and Behaviour that can be used by tests, then destroyed in teardown 
        /// </summary>
        /// <returns></returns>
        protected T CreateBehaviour<T>() where T : NetworkBehaviour
        {
            var go = new GameObject($"A NetworkBehaviour {typeof(T).Name} {toDestroy.Count}", typeof(NetworkIdentity), typeof(T));
            toDestroy.Add(go);
            return go.GetComponent<T>();
        }

        /// <summary>
        /// Creates a new MonoBehaviour that can be used by tests, then destroyed in teardown 
        /// </summary>
        /// <returns></returns>
        protected T CreateMonoBehaviour<T>() where T : MonoBehaviour
        {
            var go = new GameObject($"A MonoBehaviour {typeof(T).Name} {toDestroy.Count}", typeof(T));
            toDestroy.Add(go);
            return go.GetComponent<T>();
        }

        /// <summary>
        /// Creates a new NetworkIdentity that can be used by tests, then destroyed in teardown 
        /// </summary>
        /// <returns></returns>
        protected GameObject CreateGameObject()
        {
            var go = new GameObject($"A GameObject {toDestroy.Count}");
            toDestroy.Add(go);
            return go;
        }
    }
}
