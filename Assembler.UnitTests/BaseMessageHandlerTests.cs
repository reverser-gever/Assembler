using Assembler.Base;
using Assembler.Core;
using Assembler.Core.Entities;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class BaseMessageHandlerTests
    {
        private Mock<ITimeBasedCache<BaseMessage>> _cache;
        private Mock<IFactory<BaseFrame, string>> _identifierFactory;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _messageEnricher;
        private Mock<BaseMessageHandler<BaseFrame, BaseMessageInAssembly>> _handler;

        [SetUp]
        public void Setup()
        {
            _cache = new Mock<ITimeBasedCache<BaseMessage>>();
            _identifierFactory = new Mock<IFactory<BaseFrame, string>>();
            _messageEnricher = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _handler = new Mock<BaseMessageHandler<BaseFrame, BaseMessageInAssembly>>();
        }

        [Test]
        public void GetIdentifier_ValidFrame_CallsTheIdentifier()
        {
            // Arrange

            // Act
            _handler.Protected().
            // Assert
        }
    }
}