using System;
using Assembler.Base.FrameHandlers;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.Releasing;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.FrameHandlers
{
    [TestFixture]
    public class InitialFrameHandlerTests
    {
        private InitialFrameHandler<BaseFrame, BaseMessageInAssembly> _handler;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IIdentifierGenerator<BaseFrame>> _identifierGeneratorMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<IMessageInAssemblyCreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;
        private Mock<IMessageInAssemblyReleaser<BaseMessageInAssembly>> _messageInAssemblyReleaserMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;

        private string _identifierString;
        private TimeSpan _maxDifference;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _enricherMock = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _messageInAssemblyCreatorMock = new Mock<IMessageInAssemblyCreator<BaseMessageInAssembly>>();
            _messageInAssemblyReleaserMock = new Mock<IMessageInAssemblyReleaser<BaseMessageInAssembly>>();

            _identifierString = TestUtilities.GetIdentifierString();
            _identifierGeneratorMock = TestUtilities.GetIdentifierGeneratorMock();
            _dateTimeProviderMock = TestUtilities.GetDateTimeProviderMock();
            _maxDifference = TimeSpan.Zero;

            _handler = new InitialFrameHandler<BaseFrame, BaseMessageInAssembly>(_maxDifference, _cacheMock.Object,
                _identifierGeneratorMock.Object, _messageInAssemblyCreatorMock.Object, _enricherMock.Object,
                _messageInAssemblyReleaserMock.Object, _dateTimeProviderMock.Object, TestUtilities.GetLoggerFactory());
        }

        [TearDown]
        public void Teardown()
        {
            _identifierGeneratorMock.VerifyNoOtherCalls();
            _enricherMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _messageInAssemblyCreatorMock.VerifyNoOtherCalls();
            _messageInAssemblyReleaserMock.VerifyNoOtherCalls();
            _dateTimeProviderMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_MessageInCacheAndMiddleReceived_ReleasesTheOldOneWithoutEnrichmentAndCreatesANewOne()
        {
            // Arrange
            var frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial);
            var message = TestUtilities.GenerateBaseMessageInAssembly(DateTime.MaxValue, DateTime.MaxValue);
            var newMessage = TestUtilities.GenerateBaseMessageInAssembly();
            message.MiddleReceived = true;

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get(It.IsAny<string>())).Returns(message);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(newMessage);

            // Act
            _handler.Handle(frame);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Remove(_identifierString), Times.Once);

            // Not being enriched
            Assert.AreNotEqual(DateTime.MinValue, message.LastFrameReceived);

            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(message, ReleaseReason.AnotherMessageInitialized), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(frame, newMessage), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, newMessage.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(_identifierString, newMessage), Times.Once);
        }

        [Test]
        public void
            Handle_MessageInCacheMiddleNotReceivedAndTimeMoreThanMaxDifference_ReleasesTheOldOneWithoutEnrichmentAndCreatesANewOne()
        {
            // Arrange
            var now = DateTime.Now;
            var frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial, DateTime.MaxValue);
            var message = TestUtilities.GenerateBaseMessageInAssembly(now, now);
            var newMessage = TestUtilities.GenerateBaseMessageInAssembly();
            message.MiddleReceived = false;

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get(It.IsAny<string>())).Returns(message);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(newMessage);

            // Act
            _handler.Handle(frame);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Remove(_identifierString), Times.Once);

            // Not being enriched
            Assert.AreNotEqual(DateTime.MinValue, message.LastFrameReceived);

            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(message, ReleaseReason.AnotherMessageInitialized), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(frame, newMessage), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, newMessage.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(_identifierString, newMessage), Times.Once);
        }

        [Test]
        public void Handle_MessageInCacheMiddleNotReceivedAndTimeLessThanMaxDifference_EnrichesTheExistingMessage()
        {
            // Arrange
            var frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial);
            var message = TestUtilities.GenerateBaseMessageInAssembly();
            message.MiddleReceived = false;

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get(It.IsAny<string>())).Returns(message);

            // Act
            _handler.Handle(frame);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get(_identifierString), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(frame, message), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(_identifierString, message), Times.Once);
        }

        [Test]
        public void Handle_MessageNotInCache_CreatesANewMessageAndEnrichesIt()
        {
            // Arrange
            var frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial);
            var message = TestUtilities.GenerateBaseMessageInAssembly();
            message.MiddleReceived = false;

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(false);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(message);

            // Act
            _handler.Handle(frame);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(frame, message), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.LastFrameReceived);

            _cacheMock.Verify(cache => cache.Put(_identifierString, message), Times.Once);
        }

        [Test]
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed()
        {
            // Arrange
            var frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial);

            _identifierGeneratorMock.Setup(identifier => identifier.Generate(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            Assert.DoesNotThrow(() => _handler.Handle(frame));

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame), Times.Once);
        }
    }
}