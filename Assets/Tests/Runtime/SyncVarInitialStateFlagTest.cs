using System;
using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime
{
    public class SyncVarInitialStateFlagBehaviour : NetworkBehaviour
    {
        public event Action<int> Hook;

        [SyncVar(hook = nameof(Hook))]
        public int number;
    }
    public class SyncVarInitialStateFlagTest : TestBase
    {
        private readonly NetworkWriter ownerWriter = new NetworkWriter(1300);
        private readonly NetworkWriter observersWriter = new NetworkWriter(1300);
        private readonly NetworkReader reader = new NetworkReader();

        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();

            ownerWriter.Reset();
            observersWriter.Reset();
            reader.Dispose();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void InitialStateHasCorrectValue(bool input)
        {
            var server = CreateBehaviour<SyncVarInitialStateFlagBehaviour>();
            var client = CreateBehaviour<SyncVarInitialStateFlagBehaviour>();

            server.number = 10;

            var invoked = 0;
            var result = false;
            client.Hook += (_) =>
            {
                invoked++;
                result = client.Identity.InitialState;
            };

            server.Identity.OnSerializeAll(input, ownerWriter, observersWriter);
            reader.Reset(observersWriter.ToArraySegment());
            client.Identity.OnDeserializeAll(reader, input);

            Assert.That(invoked, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(input));
        }
    }
}
