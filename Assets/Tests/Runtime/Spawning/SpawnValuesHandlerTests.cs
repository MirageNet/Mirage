using System.Collections;
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
                var pos = values.Position.HasValue ? values.Position.Value - ClientOffset : prefab.transform.position;
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
                Assert.That(Quaternion.Angle(rot1, Quaternion.Euler(0, 180, 0)), Is.LessThan(0.1f));

                var emptyValues = new SpawnValues();
                var (pos2, rot2) = DefaultSpawnValuesHandler.Instance.GetPrefabPosition(identity, emptyValues);
                Assert.That(pos2, Is.EqualTo(new Vector3(5, 5, 5)));
                Assert.That(Quaternion.Angle(rot2, Quaternion.Euler(0, 90, 0)), Is.LessThan(0.1f));
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

        private class FullOverrideSpawnValuesHandler : ISpawnValuesHandler
        {
            public SpawnValues ValuesToCreate;
            public SpawnValues ValuesToApply;
            public bool UseCustomApply;

            public SpawnValues CreateSpawnValues(NetworkIdentity identity)
            {
                return ValuesToCreate;
            }

            public (Vector3 pos, Quaternion rot) GetPrefabPosition(NetworkIdentity prefab, SpawnValues values)
            {
                var effectiveValues = UseCustomApply ? ValuesToApply : values;
                var pos = effectiveValues.Position ?? prefab.transform.position;
                var rot = effectiveValues.Rotation ?? prefab.transform.rotation;
                return (pos, rot);
            }

            public void ApplySpawnValues(NetworkIdentity identity, SpawnValues values)
            {
                var effectiveValues = UseCustomApply ? ValuesToApply : values;
                DefaultSpawnValuesHandler.Instance.ApplySpawnValues(identity, effectiveValues);
            }
        }

        [UnityTest]
        public IEnumerator CustomServerHandlerFullyOverridesAllValues()
        {
            var expectedValues = new SpawnValues
            {
                Position = new Vector3(123, 456, 789),
                Rotation = Quaternion.Euler(30, 60, 90),
                Scale = new Vector3(3.5f, 4.5f, 5.5f),
                Name = "FullyOverriddenEntity",
                SelfActive = true
            };

            serverObjectManager.SpawnValuesHandler = new FullOverrideSpawnValuesHandler
            {
                ValuesToCreate = expectedValues
            };

            var clone = InstantiateForTest(_characterPrefab);
            clone.transform.position = Vector3.zero;
            clone.transform.rotation = Quaternion.identity;
            clone.transform.localScale = Vector3.one;
            clone.name = "OriginalServerName";
            clone.gameObject.SetActive(false);

            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(123, 456, 789)));
            // 0.3f tolerance accounts for Mirage's 9-bit Quaternion smallest-three network compression
            Assert.That(Quaternion.Angle(clientIdentity.transform.rotation, Quaternion.Euler(30, 60, 90)), Is.LessThan(0.3f));
            Assert.That(clientIdentity.transform.localScale, Is.EqualTo(new Vector3(3.5f, 4.5f, 5.5f)));
            Assert.That(clientIdentity.name, Is.EqualTo("FullyOverriddenEntity"));
            Assert.That(clientIdentity.gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomServerHandlerFullyOverridesActiveFalse()
        {
            var expectedValues = new SpawnValues
            {
                Position = new Vector3(10, 20, 30),
                Rotation = Quaternion.Euler(0, 45, 0),
                Scale = new Vector3(2, 2, 2),
                Name = "InactiveOverriddenEntity",
                SelfActive = false
            };

            serverObjectManager.SpawnValuesHandler = new FullOverrideSpawnValuesHandler
            {
                ValuesToCreate = expectedValues
            };

            var clone = InstantiateForTest(_characterPrefab);
            clone.gameObject.SetActive(true);

            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.gameObject.activeSelf, Is.False);
            Assert.That(clientIdentity.name, Is.EqualTo("InactiveOverriddenEntity"));
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(10, 20, 30)));
            Assert.That(clientIdentity.transform.localScale, Is.EqualTo(new Vector3(2, 2, 2)));
        }

        [UnityTest]
        public IEnumerator CustomClientHandlerFullyOverridesAllValues()
        {
            var clientOverrideValues = new SpawnValues
            {
                Position = new Vector3(777, 888, 999),
                Rotation = Quaternion.Euler(15, 30, 45),
                Scale = new Vector3(0.5f, 0.5f, 0.5f),
                Name = "ClientOverriddenName",
                SelfActive = true
            };

            clientObjectManager.SpawnValuesHandler = new FullOverrideSpawnValuesHandler
            {
                UseCustomApply = true,
                ValuesToApply = clientOverrideValues
            };

            var clone = InstantiateForTest(_characterPrefab);
            clone.transform.position = new Vector3(1, 2, 3);
            clone.name = "ServerDefaultName";

            serverObjectManager.Spawn(clone);

            yield return null;
            yield return null;

            var clientIdentity = _remoteClients[0].Get(clone);
            Assert.That(clientIdentity.transform.position, Is.EqualTo(new Vector3(777, 888, 999)));
            Assert.That(Quaternion.Angle(clientIdentity.transform.rotation, Quaternion.Euler(15, 30, 45)), Is.LessThan(0.5f));
            Assert.That(clientIdentity.transform.localScale, Is.EqualTo(new Vector3(0.5f, 0.5f, 0.5f)));
            Assert.That(clientIdentity.name, Is.EqualTo("ClientOverriddenName"));
            Assert.That(clientIdentity.gameObject.activeSelf, Is.True);
        }
    }

    [TestFixture]
    public class SpawnValuesHandlerHostTests : Mirage.Tests.Runtime.Host.HostSetup
    {
        [Test]
        public void HostModeSpawningPreservesServerObjectState()
        {
            var identity = CreateNetworkIdentity();
            identity.PrefabHash = 12345;
            identity.transform.position = new Vector3(42, 84, 126);
            identity.transform.rotation = Quaternion.Euler(0, 45, 0);
            identity.transform.localScale = new Vector3(2, 2, 2);

            serverObjectManager.Spawn(identity);

            Assert.That(identity.IsSpawned, Is.True);
            Assert.That(identity.IsHost, Is.True);
            Assert.That(identity.IsClient, Is.True);
            Assert.That(identity.IsServer, Is.True);
            Assert.That(identity.transform.position, Is.EqualTo(new Vector3(42, 84, 126)));
            Assert.That(Quaternion.Angle(identity.transform.rotation, Quaternion.Euler(0, 45, 0)), Is.LessThan(0.1f));
            Assert.That(identity.transform.localScale, Is.EqualTo(new Vector3(2, 2, 2)));
        }
    }
}
