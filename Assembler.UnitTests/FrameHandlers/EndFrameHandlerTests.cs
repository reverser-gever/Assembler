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
    public class EndFrameHandlerTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IFactory<BaseFrame, string>> _identifierFactoryMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<ICreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;
        private Mock<IMessageReleaser<BaseMessageInAssembly>> _messageReleaserMock;

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
        }

        [TearDown]
        public void Teardown()
        {
            _identifierFactoryMock.VerifyNoOtherCalls();
            _enricherMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _messageInAssemblyCreatorMock.VerifyNoOtherCalls();
            _messageReleaserMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed(bool shouldReleaseMessageWithOnlyEndFrame)
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);

            var handler = GenerateHandler(shouldReleaseMessageWithOnlyEndFrame);

            _identifierFactoryMock.Setup(identifierFactory => identifierFactory.Create(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            Assert.DoesNotThrow(() => handler.Handle(frame.Object));

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_IdentifierInCache_MessageBeingRemovedEnrichedAndReleased(bool shouldReleaseMessageWithOnlyEndFrame)
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var message = new Mock<BaseMessageInAssembly>();
            var handler = GenerateHandler(shouldReleaseMessageWithOnlyEndFrame);

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get<BaseMessageInAssembly>(It.IsAny<string>())).Returns(message.Object);

            // Act
            handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Get<It.IsAnyType>(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Remove(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Remove(_identifierString), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message.Object), Times.Once);

            _messageReleaserMock.Verify(
                releaser => releaser.Release(It.IsAny<BaseMessageInAssembly>(), It.IsAny<ReleaseReason>()), Times.Once);
            _messageReleaserMock.Verify(
                releaser => releaser.Release(message.Object, ReleaseReason.EndReceived), Times.Once);
        }

        [Test]
        public void Handle_IdentifierNotInCacheAndShouldDispatchSingleFrame_MessageBeingCreatedEnrichedAndReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var message = new Mock<BaseMessageInAssembly>();
            var handler = GenerateHandler(true);

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(false);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(message.Object);

            // Act
            handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message.Object), Times.Once);

            _messageReleaserMock.Verify(
                releaser => releaser.Release(It.IsAny<BaseMessageInAssembly>(), It.IsAny<ReleaseReason>()), Times.Once);
            _messageReleaserMock.Verify(
                releaser => releaser.Release(message.Object, ReleaseReason.EndReceived), Times.Once);
        }

        [Test]
        public void Handle_IdentifierNotInCacheAndShouldntDispatchSingleFrame_NoMessagesBeingReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var handler = GenerateHandler(false);

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(false);

            // Act
            handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);
        }

        private EndFrameHandler<BaseFrame, BaseMessageInAssembly> GenerateHandler(bool isToReleaseSingleEndFrame)
        {
            var handler = new EndFrameHandler<BaseFrame, BaseMessageInAssembly>(_cacheMock.Object, _identifierFactoryMock.Object,
                _enricherMock.Object, _messageInAssemblyCreatorMock.Object, _messageReleaserMock.Object,
                isToReleaseSingleEndFrame, Utilities.GetLoggerFactory());

            return handler;
        }
    }
}