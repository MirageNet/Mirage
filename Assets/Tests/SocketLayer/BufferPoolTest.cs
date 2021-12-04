using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

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
            Pool<TestBuffer>.CreateNewItem create = Substitute.For<Pool<TestBuffer>.CreateNewItem>();

            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(create, bufferSize, startCount, 100);

            create.Received(startCount).Invoke(bufferSize, pool);
        }

        [Test]
        public void TakeDoesntCreateTillEmpty()
        {
            Pool<TestBuffer>.CreateNewItem create = Substitute.For<Pool<TestBuffer>.CreateNewItem>();
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
            Pool<TestBuffer>.CreateNewItem create = Substitute.For<Pool<TestBuffer>.CreateNewItem>();
            create.Invoke(default, default).Returns((args) => TestBuffer.Create((int)args[0], (Pool<TestBuffer>)args[1]));

            const int startCount = 5;
            const int bufferSize = 100;
            var pool = new Pool<TestBuffer>(create, bufferSize, startCount, 100);

            const int takeCount = 8;

            var temp = new List<TestBuffer>();
            var temp2 = new List<TestBuffer>();
            for (int i = 0; i < takeCount; i++)
            {
                temp.Add(pool.Take());
            }

            create.ClearReceivedCalls();

            for (int i = 0; i < takeCount; i++)
            {
                pool.Put(temp[i]);
            }


            for (int i = 0; i < takeCount; i++)
            {
                temp2.Add(pool.Take());
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

            var temp = new List<TestBuffer>();

            for (int i = 0; i < takeCount; i++)
            {
                temp.Add(pool.Take());
            }

            // check instances are the different
            Assert.That(temp, Is.Unique);
        }

        [Test]
        public void TakingMoreBuffersThanMaxLogsWarning()
        {
            const int maxCount = 5;
            const int bufferSize = 100;
            ILogger logger = Substitute.For<ILogger>();
            logger.IsLogTypeAllowed(LogType.Warning).Returns(true);
            var pool = new Pool<TestBuffer>(TestBuffer.Create, bufferSize, 0, maxCount, logger);

            var temp = new List<TestBuffer>();
            for (int i = 0; i < maxCount; i++)
            {
                temp.Add(pool.Take());
            }

            temp.Add(pool.Take());
            logger.Received(1).Log(LogType.Warning, $"Pool Max Size reached, type:{typeof(TestBuffer).Name} created:{maxCount + 1} max:{maxCount}");

            Assert.That(temp, Is.Unique);
        }
        [Test]
        public void DoesNotSpamLogsIfTakingManyOverMax()
        {
            const int maxCount = 5;
            const int bufferSize = 100;
            ILogger logger = Substitute.For<ILogger>();
            logger.IsLogTypeAllowed(LogType.Warning).Returns(true);
            var pool = new Pool<TestBuffer>(TestBuffer.Create, bufferSize, 0, maxCount, logger);

            var temp = new List<TestBuffer>();
            for (int i = 0; i < maxCount; i++)
            {
                temp.Add(pool.Take());
            }

            temp.Add(pool.Take());
            // should only get 1 log
            logger.Received(1).Log(LogType.Warning, $"Pool Max Size reached, type:{typeof(TestBuffer).Name} created:{maxCount + 1} max:{maxCount}");
            logger.ClearReceivedCalls();
            temp.Add(pool.Take());
            temp.Add(pool.Take());
            logger.DidNotReceive().Log(LogType.Warning, Arg.Any<string>());

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
