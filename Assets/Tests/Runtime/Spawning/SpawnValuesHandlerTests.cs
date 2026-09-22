using System.Collections;
using Mirage.Tests.Runtime.ClientServer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.Spawning
{
    public class SpawnValuesHandlerTests : ClientServerSetup
    {
        private class OffsetSpawnValuesHandler : ISpawnValuesHandler
        {
            public Vector3 ServerOffset;
            public Vector3 ClientOffset;
            public uint ObservedNetIdInApply;

            public SpawnValues CreateSpawnValues(NetworkIdentity identity)
            {
                var values = DefaultSpawnValuesHandler.Instance.CreateSpawnValues(identity);
                if (values.Position.HasValue)
                    values.Position = values.Position.Value + ServerOffset;
                return values;
            }

            public (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
            {
                var pos = (values.Position.HasValue ? values.Position.Value - ClientOffset : prefab.transform.position);
                var rot = values.Rotation ?? prefab.transform.rotation;
                return (pos, rot);
            }

            public void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
            {
                ObservedNetIdInApply = identity.NetId;

                var adjustedValues = values;
                if (adjustedValues.Position.HasValue)
                    adjustedValues.Position = adjustedValues.Position.Value - ClientOffset;

                DefaultSpawnValuesHandler.Instance.ApplySpawnValues(identity, adjustedValues);
            }
        }

        [Test]
        public void DefaultSingletonIsNotNull()
        {
            Assert.That(DefaultSpawnValuesHandler.Instance, Is.Not.Null);
            Assert.That(serverObjectManager.SpawnValuesHandler, Is.SameAs(DefaultSpawnValuesHandler.Instance));
            Assert.That(clientObjectManager.SpawnValuesHandler, Is.SameAs(DefaultSpawnValuesHandler.Instance));
        }

        [Test]
        public void DefaultHandlerGetPrefabPositionUsesValuesOrPrefab()
        {
            var testGo = new GameObject("PrefabTest");
            testGo.transform.position = new Vector3(5, 5, 5);
            testGo.transform.rotation = Quaternion.Euler(0, 90, 0);
            var identity = testGo.AddComponent<NetworkIdentity>();

            try
            {
                var withValues = new SpawnValues
                {
                    Position = new Vector3(10, 20, 30),
                    Rotation = Quaternion.Euler(0, 180, 0)
                };

                var (pos1, rot1) = DefaultSpawnValuesHandler.Instance.GetPrefabPosition(identity, withValues);
                Assert.That(pos1, Is.EqualTo(new Vector3(10, 20, 30)));
                Assert.That(rot1, Is.EqualTo(Quaternion.Euler(0, 180, 0)));

                var emptyValues = new SpawnValues();
                var (pos2, rot2) = DefaultSpawnValuesHandler.Instance.GetPrefabPosition(identity, emptyValues);
                Assert.That(pos2, Is.EqualTo(new Vector3(5, 5, 5)));
                Assert.That(rot2, Is.EqualTo(Quaternion.Euler(0, 90, 0)));
            }
            finally
            {
                Object.DestroyImmediate(testGo);
            }
        }

        [Test]
        public void DefaultHandlerAppliesAllValues()
        {
            var testGo = new GameObject("OriginalName");
            var identity = testGo.AddComponent<NetworkIdentity>();

            try
            {
                var values = new SpawnValues
                {
                    Position = new Vector3(12, 34, 56),
                    Rotation = Quaternion.Euler(10, 20, 30),
                    Scale = new Vector3(2, 3, 4),
                    Name = "AppliedName",
                    SelfActive = false
                };

                DefaultSpawnValuesHandler.Instance.ApplySpawnValues(identity, values);

                Assert.That(identity.transform.localPosition, Is.EqualTo(new Vector3(12, 34, 56)));
                var angle = Quaternion.Angle(identity.transform.localRotation, Quaternion.Euler(10, 20, 30));
                Assert.That(angle, Is.LessThan(0.1f));
                Assert.That(identity.transform.localScale, Is.EqualTo(new Vector3(2, 3, 4)));
                Assert.That(identity.name, Is.EqualTo("AppliedName"));
                Assert.That(identity.gameObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(testGo);
            }
        }

        [UnityTest]
        public IEnumerator CustomServerHandlerModifiesSpawnValues()
        {
            var customHandler = new OffsetSpawnValuesHandler
            {
                ServerOffset = new Vector3(100, 0, 0),
                ClientOffset = Vector3.zero
            };
            serverObjectManager.SpawnValuesHandler = customHandler;

            var clone = InstantiateForTest(_characterPrefab);
            clone.transform.position = new Vector3(10, 20, 30);
            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(110, 20, 30)));
        }

        [UnityTest]
        public IEnumerator CustomClientHandlerTransformsCoordinates()
        {
            var customHandler = new OffsetSpawnValuesHandler
            {
                ServerOffset = Vector3.zero,
                // Simulate client floating origin offset
                ClientOffset = new Vector3(50, 0, 0)
            };
            clientObjectManager.SpawnValuesHandler = customHandler;

            var clone = InstantiateForTest(_characterPrefab);
            clone.transform.position = new Vector3(100, 20, 30);
            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(50, 20, 30)));
            Assert.That(customHandler.ObservedNetIdInApply, Is.EqualTo(clientIdentity.NetId));
            Assert.That(customHandler.ObservedNetIdInApply, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator CustomServerAndClientCoordinateOffset()
        {
            // Simulates server in world space and client in local floating space
            serverObjectManager.SpawnValuesHandler = new OffsetSpawnValuesHandler
            {
                ServerOffset = new Vector3(1000, 0, 0)
            };
            clientObjectManager.SpawnValuesHandler = new OffsetSpawnValuesHandler
            {
                ClientOffset = new Vector3(1000, 0, 0)
            };

            var clone = InstantiateForTest(_characterPrefab);
            clone.transform.position = new Vector3(25, 10, 5);
            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(25, 10, 5)));
        }
    }
}
