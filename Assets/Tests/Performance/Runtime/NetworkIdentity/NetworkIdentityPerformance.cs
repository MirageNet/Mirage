using NSubstitute;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Mirage.Tests.Performance
{
    public class Health : NetworkBehaviour
    {
        [SyncVar] public int health = 10;

        public void Update()
        {
            health = (health + 1) % 10;
        }
    }
    [Category("Performance")]
    [Category("Benchmark")]
    public class NetworkIdentityPerformance
    {
        private GameObject gameObject;
        private NetworkIdentity identity;
        private Health health;


        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();
            identity.SetOwner(Substitute.For<INetworkPlayer>());
            identity.observers.Add(identity.Owner);
            health = gameObject.AddComponent<Health>();
            health.SyncSettings.From = SyncFrom.Server;
            health.SyncSettings.To = SyncTo.Owner;
            health.SyncSettings.Timing = SyncTiming.NoInterval;
        }
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        [Performance]
        public void NetworkIdentityServerUpdateIsDirty()
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
                health.SetDirtyBit(1UL);
                SyncVarSender.SendUpdateVarsMessage(identity);
            }
        }

        [Test]
        [Performance]
        public void NetworkIdentityServerUpdateNotDirty()
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
                SyncVarSender.SendUpdateVarsMessage(identity);
            }
        }
    }
}

