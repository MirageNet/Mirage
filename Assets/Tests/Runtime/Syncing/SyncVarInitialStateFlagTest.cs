using System;
using Mirage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.Syncing
{
    public class SyncVarInitialStateFlagBehaviour : NetworkBehaviour
    {
        public event Action<int> Hook;

        [SyncVar(hook = nameof(Hook))]
        public int number;
    }
    public class SyncVarInitialStateFlagTest : ClientServerSetup<SyncVarInitialStateFlagBehaviour>
    {
        private readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        private readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            ownerWriter.Reset();
            observersWriter.Reset();
            reader.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void InitialStateHasCorrectValue(bool initial)
        {
            serverComponent.number = 10;

            var invoked = 0;
            var result = false;
            clientComponent.Hook += (_) =>
            {
                invoked++;
                result = clientIdentity.InitialState;
            };

            if (initial)
                serverIdentity.OnSerializeInitial(ownerWriter, observersWriter);
            else
                serverIdentity.OnSerializeDelta(Time.unscaledTimeAsDouble, ownerWriter, observersWriter);
            reader.Reset(observersWriter.ToArraySegment());
            clientIdentity.OnDeserializeAll(reader, initial);

            Assert.That(invoked, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(initial));
        }
    }
}
