using System;
using System.Collections.Generic;
using Assembler.Base;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class MiddleMessageHandlerTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IFactory<BaseFrame, string>> _identifierFactoryMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<ICreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;

        private List<BaseMessageInAssembly> _assembledMessages;
        private string _identifierString;

        private MiddleMessageHandler<BaseFrame, BaseMessageInAssembly> _handler;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _enricherMock = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _messageInAssemblyCreatorMock = new Mock<ICreator<BaseMessageInAssembly>>();
            _identifierString = Utilities.GetIdentifierString();
            _identifierFactoryMock = Utilities.GetIdentifierMock();

            _assembledMessages = new List<BaseMessageInAssembly>();

            _handler = new MiddleMessageHandler<BaseFrame, BaseMessageInAssembly>(_cacheMock.Object,
                _identifierFactoryMock.Object, _messageInAssemblyCreatorMock.Object, _enricherMock.Object,
                Utilities.GetLoggerFactory());

            _handler.MessageAssemblyFinished += _assembledMessages.Add;
        }

        [TearDown]
        public void Teardown()
        {
            _identifierFactoryMock.VerifyNoOtherCalls();
            _enricherMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _messageInAssemblyCreatorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);

            _identifierFactoryMock.Setup(identifier => identifier.Create(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);
        }

        [Test]
        public void Handle_MessageInCache_EnrichesItAndPutsItInTheCache()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);
            var message = new Mock<BaseMessageInAssembly>();

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get<BaseMessageInAssembly>(It.IsAny<string>())).Returns(message.Object);

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(_identifierString), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Put(It.IsAny<string>(), It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _cacheMock.Verify(cache => cache.Put(_identifierString, message.Object), Times.Once);

            Assert.Zero(_assembledMessages.Count);
        }

        [Test]
        public void Handle_MessageNotInCache_CreatesANewMessageAndEnrichesIt()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);
            var message = new Mock<BaseMessageInAssembly>();

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(false);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(message.Object);

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Put(It.IsAny<string>(), It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _cacheMock.Verify(cache => cache.Put(_identifierString, message.Object), Times.Once);

            Assert.Zero(_assembledMessages.Count);
            Assert.True(message.Object.MiddleReceived);
        }
    }
}