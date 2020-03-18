using Assembler.Core;
using Assembler.Core.Entities;
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

        public static Mock<IFactory<BaseFrame, string>> GetIdentifierMock()
        {
            var identifierFactoryMock = new Mock<IFactory<BaseFrame, string>>();

            identifierFactoryMock.Setup(identifier => identifier.Create(It.IsAny<BaseFrame>()))
                .Returns(GetIdentifierString());

            return identifierFactoryMock;
        }

        public static string GetIdentifierString() => "yes very";
    }
}