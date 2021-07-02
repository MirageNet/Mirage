using System;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Mirage.SocketLayer.Tests
{
    [Category("SocketLayer")]
    public class PoolTest
    {
        [Test]
        public void ThrowsIfStartIsLessThanMax()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Pool<TestBuffer>(TestBuffer.Create, 0, 10, 5));
            Assert.That(exception, Has.Message.EqualTo(new ArgumentException("Start size must be less than max size", "startPoolSize").Message));
        }

        [Test]
        public void ThrowsIfCreateIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Pool<TestBuffer>(null, 0, 10, 100));
            Assert.That(exception, Has.Message.EqualTo(new ArgumentNullException("createNew").Message));
        }

        [Test]
        public void CallsCreateStartCountTimesWithCorrectArgs()
        {
            Func<int, Pool<TestBuffer>, TestBuffer> create = Substitute.For<Func<int, Pool<TestBuffer>, TestBuffer>>();

            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(create, bufferSize, startCount, 100);

            create.Received(startCount).Invoke(bufferSize, pool);
        }

        [Test]
        public void TakeDoesntCreateTillEmpty()
        {
            Func<int, Pool<TestBuffer>, TestBuffer> create = Substitute.For<Func<int, Pool<TestBuffer>, TestBuffer>>();
            create.Invoke(default, default).Returns((args) => TestBuffer.Create((int)args[0], (Pool<TestBuffer>)args[0]));

            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(create, bufferSize, startCount, 100);

            for (int i = 0; i < startCount; i++)
            {
                _ = pool.Take();
            }

            create.ClearReceivedCalls();

            const int extraCreates = 2;
            for (int i = 0; i < extraCreates; i++)
            {
                _ = pool.Take();
            }
            create.Received(extraCreates).Invoke(bufferSize, pool);
        }

        [Test]
        public void CanTakeMoreAfterPuttingSomeBackWithoutCreatingNew()
        {
            Func<int, Pool<TestBuffer>, TestBuffer> create = Substitute.For<Func<int, Pool<TestBuffer>, TestBuffer>>();
            create.Invoke(default, default).Returns((args) => TestBuffer.Create((int)args[0], (Pool<TestBuffer>)args[1]));

            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(create, bufferSize, startCount, 100);

            const int takeCount = 8;

            var temp = new TestBuffer[takeCount];
            var temp2 = new TestBuffer[takeCount];
            for (int i = 0; i < takeCount; i++)
            {
                temp[i] = pool.Take();
            }

            create.ClearReceivedCalls();

            for (int i = 0; i < takeCount; i++)
            {
                pool.Put(temp[i]);
            }


            for (int i = 0; i < takeCount; i++)
            {
                temp2[i] = pool.Take();
            }

            create.DidNotReceiveWithAnyArgs().Invoke(default, default);

            // check instances are the same
            Assert.That(temp, Is.EquivalentTo(temp2));
        }

        [Test]
        public void EachTakeIsANewInstance()
        {
            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(TestBuffer.Create, bufferSize, startCount, 100);

            const int takeCount = 8;

            var temp = new TestBuffer[takeCount];
            for (int i = 0; i < takeCount; i++)
            {
                temp[i] = pool.Take();
            }

            // check instances are the different
            Assert.That(temp, Is.Unique);
        }

        [Test]
        public void TakingMoreBuffersThanMaxLogsWarning()
        {
            const int maxCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(TestBuffer.Create, bufferSize, 0, maxCount);

            var temp = new TestBuffer[maxCount + 1];
            for (int i = 0; i < maxCount; i++)
            {
                temp[i] = pool.Take();
            }

            LogAssert.NoUnexpectedReceived();

            LogAssert.Expect(UnityEngine.LogType.Warning, $"Buffer Max Size reached, created:{maxCount + 1} max:{maxCount}");
            temp[maxCount] = pool.Take();
            LogAssert.NoUnexpectedReceived();

            Assert.That(temp, Is.Unique);

        }
        [Test]
        public void PutingMoreBuffersThanMaxLogsWarning()
        {
            const int maxCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(TestBuffer.Create, bufferSize, 0, maxCount);

            var temp = new TestBuffer[maxCount + 1];
            for (int i = 0; i < maxCount + 1; i++)
            {
                temp[i] = pool.Take();
            }

            // have to expect this so that noUnexpected doesn't fail below
            LogAssert.Expect(UnityEngine.LogType.Warning, $"Buffer Max Size reached, created:{maxCount + 1} max:{maxCount}");

            for (int i = 0; i < maxCount; i++)
            {
                pool.Put(temp[i]);
            }

            LogAssert.NoUnexpectedReceived();

            LogAssert.Expect(UnityEngine.LogType.Warning, $"Cant Put buffer into full pool, leaving for GC");
            pool.Put(temp[maxCount]);

            LogAssert.NoUnexpectedReceived();

            Assert.That(temp, Is.Unique);
        }


        public class TestBuffer
        {
            public readonly int Size;
            public readonly Pool<TestBuffer> Pool;

            private TestBuffer(int size, Pool<TestBuffer> pool)
            {
                Size = size;
                Pool = pool;
            }

            public static TestBuffer Create(int size, Pool<TestBuffer> pool)
            {
                return new TestBuffer(size, pool);
            }
        }
    }
}
