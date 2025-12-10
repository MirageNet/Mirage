using System;
using System.Collections.Generic;
using Mirage.Logging;
using NUnit.Framework;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    public abstract class TestBase
    {
        protected static readonly ILogger logger = LogFactory.GetLogger("Mirage.Tests.Test");

        [OneTimeSetUp]
        public void AddTestLogger()
        {
            ReplaceLogHandler(true, MirageLogHandler.TimePrefix.DateTimeMilliSeconds);
            Console.WriteLine($"[[AddTestLogger]] {GetType().FullName}");
        }


        protected List<Object> toDestroy = new List<Object>();

        /// <summary>
        /// Call this from child class teardown
        /// </summary>
        protected void TearDownTestObjects()
        {
            foreach (var obj in toDestroy)
            {
                if (obj == null)
                    continue;

                // obj could be any Unity object: GO, SO, comp, etc.
                // if it is comp we want to destroy its gameobject instead of just the comp, this lets us add NetworkIdentities to list
                if (obj is Component comp)
                {
                    Object.DestroyImmediate(comp.gameObject);
                }
                else
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
            var identity = go.AddComponent<NetworkIdentity>();
            identity.PrefabHash = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            return identity;
        }

        /// <summary>
        /// Creates a new NetworkIdentity and Behaviour that can be used by tests, then destroyed in teardown
        /// </summary>
        /// <returns></returns>
        protected T CreateBehaviour<T>(bool disable = false) where T : NetworkBehaviour
        {
            var go = CreateGameObject($"A NetworkBehaviour {typeof(T).Name} {toDestroy.Count}", disable);
            var identity = go.AddComponent<NetworkIdentity>();
            identity.PrefabHash = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
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

        /// <summary>
        /// Replaces the default log handler with one that prepends the frame count 
        /// </summary>
        public void ReplaceLogHandler(bool addLabel, MirageLogHandler.TimePrefix timePrefix)
        {
            var settings = new MirageLogHandler.Settings(timePrefix, coloredLabel: !Application.isBatchMode, addLabel);

            if (addLabel)
            {
                LogFactory.ReplaceLogHandler((fullName) => new MirageLogHandler(settings, fullName));
            }
            else
            {
                LogFactory.ReplaceLogHandler(new MirageLogHandler(settings));
            }
        }
    }
}
