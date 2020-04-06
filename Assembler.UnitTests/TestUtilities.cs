using System;
using Assembler.Core;
using Assembler.Core.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Assembler.UnitTests
{
    public static class TestUtilities
    {
        public static ILoggerFactory GetLoggerFactory()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();

            loggerFactory.Setup(factory => factory.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            return loggerFactory.Object;
        }

        public static Mock<IIdentifierGenerator<BaseFrame>> GetIdentifierGeneratorMock()
        {
            var identifierGeneratorMock = new Mock<IIdentifierGenerator<BaseFrame>>();

            identifierGeneratorMock.Setup(generator => generator.Generate(It.IsAny<BaseFrame>()))
                .Returns(GetIdentifierString());

            return identifierGeneratorMock;
        }

        public static string GetIdentifierString() => "yes very";

        public static Mock<IDateTimeProvider> GetDateTimeProviderMock()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            dateTimeProviderMock.Setup(provider => provider.Now).Returns(DateTime.MinValue);

            return dateTimeProviderMock;
        }

        public static BaseMessageInAssembly GenerateBaseMessageInAssembly(DateTime assemblingStartTime = default,
            DateTime lastFrameReceived = default) =>
            new Mock<BaseMessageInAssembly>(assemblingStartTime, lastFrameReceived, false).Object;
    }
}