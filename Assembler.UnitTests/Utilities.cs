using Assembler.Core;
using Moq;

namespace Assembler.UnitTests
{
    public static class Utilities
    {
        public static ILoggerFactory GetLoggerFactory()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();

            loggerFactory.Setup(factory => factory.GetLogger(It.IsAny<object>())).Returns(logger.Object);

            return loggerFactory.Object;
        }
    }
}