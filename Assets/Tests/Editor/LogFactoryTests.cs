using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Mirage
{
    public class LogFactoryTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void SameClassSameLogger()
        {
            ILogger logger1 = LogFactory.GetLogger<LogFactoryTests>();
            ILogger logger2 = LogFactory.GetLogger<LogFactoryTests>();
            Assert.That(logger1, Is.SameAs(logger2));
        }

        [Test]
        public void DifferentClassDifferentLogger()
        {
            ILogger logger1 = LogFactory.GetLogger<LogFactoryTests>();
            ILogger logger2 = LogFactory.GetLogger<NetworkManager>();
            Assert.That(logger1, Is.Not.SameAs(logger2));
        }

        [Test]
        public void LogDebugIgnore()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Warning;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.Log("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(LogType.Log, null, null);
        }

        [Test]
        public void LogDebugFull()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Log;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.Log(msg);
            mockHandler.Received().LogFormat(LogType.Log, null, "{0}", msg);
        }

        [Test]
        public void LogWarningIgnore()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Error;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.LogWarning("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(default, null, null);
        }

        [Test]
        public void LogWarningFull()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Warning;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.LogWarning(msg);
            mockHandler.Received().LogFormat(LogType.Warning, null, "{0}", msg);
        }

        [Test]
        public void LogErrorIgnore()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Exception;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            logger.LogError("This message should not be logged");
            mockHandler.DidNotReceiveWithAnyArgs().LogFormat(default, null, null);
        }

        [Test]
        public void LogErrorFull()
        {
            ILogger logger = LogFactory.GetLogger<LogFactoryTests>();
            logger.filterLogType = LogType.Error;

            ILogHandler mockHandler = Substitute.For<ILogHandler>();
            logger.logHandler = mockHandler;
            const string msg = "This message be logged";
            logger.LogError(msg);
            mockHandler.Received().LogFormat(LogType.Error, null, "{0}", msg);
        }
    }
}
