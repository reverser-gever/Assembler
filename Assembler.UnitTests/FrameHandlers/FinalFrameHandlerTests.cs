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
    public class FinalFrameHandlerTests
    {
        private FinalFrameHandler<BaseFrame, BaseMessageInAssembly> _handler;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IIdentifierGenerator<BaseFrame>> _identifierGeneratorMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<IMessageInAssemblyCreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;
        private Mock<IMessageInAssemblyReleaser<BaseMessageInAssembly>> _messageInAssemblyReleaserMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;

        private string _identifierString;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _enricherMock = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _messageInAssemblyCreatorMock = new Mock<IMessageInAssemblyCreator<BaseMessageInAssembly>>();
            _messageInAssemblyReleaserMock = new Mock<IMessageInAssemblyReleaser<BaseMessageInAssembly>>();

            _identifierString = Utilities.GetIdentifierString();
            _identifierGeneratorMock = Utilities.GetIdentifierGeneratorMock();
            _dateTimeProviderMock = Utilities.GetDateTimeProviderMock();

            _handler = new FinalFrameHandler<BaseFrame, BaseMessageInAssembly>(_cacheMock.Object,
                _identifierGeneratorMock.Object, _messageInAssemblyCreatorMock.Object,
                _enricherMock.Object, _messageInAssemblyReleaserMock.Object, _dateTimeProviderMock.Object,
                Utilities.GetLoggerFactory());
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
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);

            _identifierGeneratorMock.Setup(identifierGenerator => identifierGenerator.Generate(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            Assert.DoesNotThrow(() => _handler.Handle(frame.Object));

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(It.IsAny<BaseFrame>()), Times.Once);
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame.Object), Times.Once);
        }

        [Test]
        public void Handle_IdentifierInCache_MessageBeingRemovedEnrichedAndReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var message = Utilities.GenerateBaseMessageInAssembly();

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(true);
            _cacheMock.Setup(cache => cache.Get(It.IsAny<string>())).Returns(message);

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(It.IsAny<BaseFrame>()), Times.Once);
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Get(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Get(_identifierString), Times.Once);

            _cacheMock.Verify(cache => cache.Remove(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Remove(_identifierString), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.LastFrameReceived);

            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(It.IsAny<BaseMessageInAssembly>(), It.IsAny<ReleaseReason>()), Times.Once);
            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(message, ReleaseReason.FinalFrameReceived), Times.Once);
        }

        [Test]
        public void Handle_IdentifierNotInCache_MessageBeingCreatedEnrichedAndReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var message = Utilities.GenerateBaseMessageInAssembly();

            _cacheMock.Setup(cache => cache.Exists(It.IsAny<string>())).Returns(false);
            _messageInAssemblyCreatorMock.Setup(creator => creator.Create()).Returns(message);

            // Act
            _handler.Handle(frame.Object);

            // Assert
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(It.IsAny<BaseFrame>()), Times.Once);
            _identifierGeneratorMock.Verify(identifier => identifier.Generate(frame.Object), Times.Once);

            _cacheMock.Verify(cache => cache.Exists(It.IsAny<string>()), Times.Once);
            _cacheMock.Verify(cache => cache.Exists(_identifierString), Times.Once);

            _messageInAssemblyCreatorMock.Verify(creator => creator.Create(), Times.Once);

            _enricherMock.Verify(enricher => enricher.Enrich(It.IsAny<BaseFrame>(), It.IsAny<BaseMessageInAssembly>()),
                Times.Once);
            _enricherMock.Verify(enricher => enricher.Enrich(frame.Object, message), Times.Once);

            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            Assert.AreEqual(DateTime.MinValue, message.LastFrameReceived);

            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(It.IsAny<BaseMessageInAssembly>(), It.IsAny<ReleaseReason>()), Times.Once);
            _messageInAssemblyReleaserMock.Verify(
                releaser => releaser.Release(message, ReleaseReason.FinalFrameReceived), Times.Once);
        }
    }
}