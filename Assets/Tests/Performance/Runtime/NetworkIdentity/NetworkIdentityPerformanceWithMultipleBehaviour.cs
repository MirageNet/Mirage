using NSubstitute;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Mirage.Tests.Performance
{
    [Category("Performance")]
    [Category("Benchmark")]
    public class NetworkIdentityPerformanceWithMultipleBehaviour
    {
        private const int healthCount = 32;
        private GameObject gameObject;
        private NetworkIdentity identity;
        private Health[] health;


        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.SetOwner(Substitute.For<INetworkPlayer>());
            identity.observers.Add(identity.Owner);
            health = new Health[healthCount];
            for (var i = 0; i < healthCount; i++)
            {
                health[i] = gameObject.AddComponent<Health>();
                health[i].SyncSettings.From = SyncFrom.Server;
                health[i].SyncSettings.To = SyncTo.Owner;
                health[i].SyncSettings.Timing = SyncTiming.NoInterval;
            }
        }
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        [Performance]
        public void ServerUpdateIsDirty()
        {
            Measure.Method(RunServerUpdateIsDirty)
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }

        private void RunServerUpdateIsDirty()
        {
            for (var j = 0; j < 1000; j++)
            {
                for (var i = 0; i < healthCount; i++)
                {
                    health[i].SetDirtyBit(1UL);
                }
                SyncVarSender.SendUpdateVarsMessage(identity, 0);
            }
        }

        [Test]
        [Performance]
        public void ServerUpdateNotDirty()
        {
            Measure.Method(RunServerUpdateNotDirty)
                .WarmupCount(10)
                .MeasurementCount(100)
                .Run();
        }

        private void RunServerUpdateNotDirty()
        {
            for (var j = 0; j < 1000; j++)
            {
                SyncVarSender.SendUpdateVarsMessage(identity, 0);
            }
        }
    }
}

