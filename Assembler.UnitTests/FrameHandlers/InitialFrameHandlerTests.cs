using System;
using Assembler.Base.FrameHandlers;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.FrameHandlers
{
    [TestFixture]
    public class InitialFrameHandlerTests
    {
        private InitialFrameHandler<BaseFrame, BaseMessageInAssembly> _handler;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IFactory<BaseFrame, string>> _identifierFactoryMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<ICreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;
        private Mock<IMessageReleaser<BaseMessageInAssembly>> _messageReleaserMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;

        private string _identifierString;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _enricherMock = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _messageInAssemblyCreatorMock = new Mock<ICreator<BaseMessageInAssembly>>();
            _messageReleaserMock = new Mock<IMessageReleaser<BaseMessageInAssembly>>();

            _identifierString = Utilities.GetIdentifierString();
            _identifierFactoryMock = Utilities.GetIdentifierMock();
            _dateTimeProviderMock = Utilities.GetDateTimeProviderMock();

            _handler = new InitialFrameHandler<BaseFrame, BaseMessageInAssembly>(_cacheMock.Object,
                _identifierFactoryMock.Object, _messageInAssemblyCreatorMock.Object, _enricherMock.Object,
                _messageReleaserMock.Object, _dateTimeProviderMock.Object, Utilities.GetLoggerFactory());
        }

        [TearDown]
        public void Teardown()
        {
            _identifierFactoryMock.VerifyNoOtherCalls();
            _enricherMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _messageInAssemblyCreatorMock.VerifyNoOtherCalls();
            _messageReleaserMock.VerifyNoOtherCalls();
            _dateTimeProviderMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Initial);

            _identifierFactoryMock.Setup(identifier => identifier.Create(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            Assert.DoesNotThrow(() => _handler.Handle(frame.Object));

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);
        }

        [Test]
        public void Handle_MessageInCacheAndMiddleReceived_ReleasesTheOldOneWithoutEnrichementAndCreatesANewOne()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Initial);
            var message = new Mock<BaseMessageInAssembly>();
            var newMessage = new Mock<BaseMessageInAssembly>();
            message.Object.MiddleReceived = true;

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get<BaseMessageInAssembly>(It.IsAny<string>())).Returns(message.Object);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(newMessage.Object);

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Remove(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Remove(_identifierString), Times.Once);

            // Not being enriched
            Assert.AreNotEqual(DateTime.MinValue, message.Object.LastFrameReceived);

            _messageReleaserMock.Verify(
                releaser => releaser.Release(It.IsAny<BaseMessageInAssembly>(), It.IsAny<ReleaseReason>()), Times.Once);
            _messageReleaserMock.Verify(
                releaser => releaser.Release(message.Object, ReleaseReason.AnotherMessageStarted), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, newMessage.Object), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, newMessage.Object.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(It.IsAny<string>(), It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _cacheMock.Verify(cache => cache.Put(_identifierString, newMessage.Object), Times.Once);
        }

        [Test]
        public void Handle_MessageInCacheAndMiddleNotReceived_EnrichesTheExistingMessage()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Initial);
            var message = new Mock<BaseMessageInAssembly>();
            message.Object.MiddleReceived = false;

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

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.Object.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(It.IsAny<string>(), It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _cacheMock.Verify(cache => cache.Put(_identifierString, message.Object), Times.Once);
        }

        [Test]
        public void Handle_MessageNotInCache_CreatesANewMessageAndEnrichesIt()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Initial);
            var message = new Mock<BaseMessageInAssembly>();
            message.Object.MiddleReceived = false;

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

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.Object.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(It.IsAny<string>(), It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _cacheMock.Verify(cache => cache.Put(_identifierString, message.Object), Times.Once);
        }
    }
}