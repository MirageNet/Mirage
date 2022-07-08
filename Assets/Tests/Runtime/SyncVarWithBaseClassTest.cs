using NUnit.Framework;

namespace Mirage.Tests.Runtime.SyncVarWithBaseClass
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
        [TearDown]
        public void TearDown()
        {
            TearDownTestObjects();
        }

        [Test]
        public void SetsCorrectDirtyBit()
        {
            C behaviour = CreateBehaviour<C>();
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
    }
}
