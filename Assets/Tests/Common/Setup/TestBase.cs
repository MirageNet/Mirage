using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    public abstract class TestBase
    {
        protected List<Object> toDestroy = new List<Object>();

        /// <summary>
        /// Call this from child class teardown
        /// </summary>
        protected void TearDownTestObjects()
        {
            foreach (var obj in toDestroy)
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
            var obj = Object.Instantiate(prefab);
            obj.SetActive(true);
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
            var obj = Object.Instantiate(prefab);
            obj.gameObject.SetActive(true);
            toDestroy.Add(obj.gameObject);
            return obj;
        }

        /// <summary>
        /// Creates a new NetworkIdentity that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected NetworkIdentity CreateNetworkIdentity(bool disable = false)
        {
            var go = CreateGameObject($"A GameObject {toDestroy.Count}", disable);
            return go.AddComponent<NetworkIdentity>();
        }

        /// <summary>
        /// Creates a new NetworkIdentity and Behaviour that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected T CreateBehaviour<T>(bool disable = false) where T : NetworkBehaviour
        {
            var go = CreateGameObject($"A NetworkBehaviour {typeof(T).Name} {toDestroy.Count}", disable);
            go.AddComponent<NetworkIdentity>();
            return go.AddComponent<T>();
        }

        /// <summary>
        /// Creates a new MonoBehaviour that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected T CreateMonoBehaviour<T>(bool disable = false) where T : MonoBehaviour
        {
            var go = CreateGameObject($"A MonoBehaviour {typeof(T).Name} {toDestroy.Count}", disable);
            return go.AddComponent<T>();
        }

        /// <summary>
        /// Creates a new ScriptableObject that can be used by tests, then destroyed in teardown
        /// </summary>
        protected T CreateScriptableObject<T>() where T : ScriptableObject
        {
            var obj = ScriptableObject.CreateInstance<T>();
            toDestroy.Add(obj);
            return obj;
        }

        /// <summary>
        /// Creates a new NetworkIdentity that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected GameObject CreateGameObject(bool disable = false)
        {
            return CreateGameObject($"A GameObject {toDestroy.Count}", disable);
        }


        /// <summary>
        /// Creates a new NetworkIdentity that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected GameObject CreateGameObject(string name, bool disable)
        {
            var go = new GameObject(name);
            // it is useful to disable object that will be used as prefabs, so that awake will not call on them
            if (disable)
                go.SetActive(false);
            toDestroy.Add(go);
            return go;
        }
    }
}