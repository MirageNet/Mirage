using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Logging.Tests
{
    public class LogFactoryTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void SameClassSameLogger()
        {
            var logger1 = LogFactory.GetLogger<LogFactoryTests>();
            var logger2 = LogFactory.GetLogger<LogFactoryTests>();
            Assert.That(logger1, Is.SameAs(logger2));
        }

        [Test]
        public void DifferentClassDifferentLogger()
        {
            var logger1 = LogFactory.GetLogger<LogFactoryTests>();
            var logger2 = LogFactory.GetLogger<NetworkManager>();
            Assert.That(logger1, Is.Not.SameAs(logger2));
        }

        [Test]
        public void LogDebugIgnore()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Warning;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.Log("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(LogType.Log, null, null);
        }

        [Test]
        public void LogDebugFull()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Log;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.Log(msg);
            mockHandler.Received().LogFormat(LogType.Log, null, "{0}", msg);
        }

        [Test]
        public void LogWarningIgnore()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Error;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.LogWarning("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(default, null, null);
        }

        [Test]
        public void LogWarningFull()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Warning;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.LogWarning(msg);
            mockHandler.Received().LogFormat(LogType.Warning, null, "{0}", msg);
        }

        [Test]
        public void LogErrorIgnore()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Exception;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.LogError("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(default, null, null);
        }

        [Test]
        public void LogErrorFull()
        {
            var logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Error;

            var mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.LogError(msg);
            mockHandler.Received().LogFormat(LogType.Error, null, "{0}", msg);
        }
    }
}
