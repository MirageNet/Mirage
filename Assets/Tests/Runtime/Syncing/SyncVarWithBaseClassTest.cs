using Mirage.Serialization;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.Syncing.SyncVarWithBaseClass
{
    public class A : NetworkBehaviour
    {
        [SyncVar] public int i0;
        [SyncVar] public int i1;
        [SyncVar] public int i2;
        [SyncVar] public int i3;
    }
    public class B : A
    {
        [SyncVar] public int i4;
        [SyncVar] public int i5;
        [SyncVar] public int i6;
    }
    public class C : B
    {
        [SyncVar] public int i7;
        [SyncVar] public int i8;
        [SyncVar] public int i9;
    }

    public class SyncVarWithBaseClassTest : TestBase
    {
        protected readonly NetworkWriter _writer = new NetworkWriter(1300);
        protected readonly MirageNetworkReader _reader = new MirageNetworkReader();
        private C behaviour;

        [SetUp]
        public void Setup()
        {
            behaviour = CreateBehaviour<C>();
        }
        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
            _writer.Reset();
            _reader.Dispose();
        }

        [Test]
        public void SetsCorrectDirtyBit()
        {
            Assert.That(behaviour.SyncVarDirtyBits, Is.Zero, "Should start at zero");

            var expected = 0ul;

            behaviour.i0 = 1;
            SetDirtyBit(ref expected, 0);
            Assert.That(behaviour.SyncVarDirtyBits, Is.EqualTo(expected));

            behaviour.i3 = 1;
            SetDirtyBit(ref expected, 3);
            Assert.That(behaviour.SyncVarDirtyBits, Is.EqualTo(expected));

            behaviour.i6 = 1;
            SetDirtyBit(ref expected, 6);
            Assert.That(behaviour.SyncVarDirtyBits, Is.EqualTo(expected));

            behaviour.i8 = 1;
            SetDirtyBit(ref expected, 8);
            Assert.That(behaviour.SyncVarDirtyBits, Is.EqualTo(expected));
        }

        private void SetDirtyBit(ref ulong bits, int index)
        {
            bits |= 1ul << index;
        }

        [Test]
        public void DeserializeMaskIsSet()
        {
            var expected = 0ul;
            // A
            SetDirtyBit(ref expected, 0);
            SetDirtyBit(ref expected, 2);
            // B
            SetDirtyBit(ref expected, 5);
            SetDirtyBit(ref expected, 6);
            // C
            SetDirtyBit(ref expected, 7);
            SetDirtyBit(ref expected, 9);

            // should be fine to set mask rather than single bit
            behaviour.SetDirtyBit(expected);
            behaviour.OnSerialize(_writer, false);
            behaviour.ClearDirtyBits();

            // we expect 10 bit mask + 6 packed ints
            var expectedLength = 10 + (8 * 6);
            Assert.That(_writer.BitPosition, Is.EqualTo(expectedLength));

            _reader.Reset(_writer.ToArraySegment());

            Assert.That(behaviour._deserializeMask, Is.Zero);
            behaviour.OnDeserialize(_reader, false);

            Assert.That(behaviour._deserializeMask, Is.EqualTo(expected));
        }

        [Test]
        public void DeserializeMaskIsClearedEachTime()
        {
            // set it once using first test
            DeserializeMaskIsSet();
            _writer.Reset();

            // then set it again, but with different mask
            var expected = 0ul;
            // A
            SetDirtyBit(ref expected, 1);
            SetDirtyBit(ref expected, 8);

            // should be fine to set mask rather than single bit
            behaviour.SetDirtyBit(expected);
            behaviour.OnSerialize(_writer, false);
            behaviour.ClearDirtyBits();

            _reader.Reset(_writer.ToArraySegment());

            // wont be cleared until OnDeserialize is called
            Assert.That(behaviour._deserializeMask, Is.Not.Zero.And.Not.EqualTo(expected));
            behaviour.OnDeserialize(_reader, false);

            Assert.That(behaviour._deserializeMask, Is.EqualTo(expected));
        }

        [Test]
        public void DeserializeMaskIsntSetForInitial()
        {
            behaviour.OnSerialize(_writer, true);

            _reader.Reset(_writer.ToArraySegment());

            Assert.That(behaviour._deserializeMask, Is.Zero);
            behaviour.OnDeserialize(_reader, true);

            Assert.That(behaviour._deserializeMask, Is.Zero); // still zero
        }
    }
}
