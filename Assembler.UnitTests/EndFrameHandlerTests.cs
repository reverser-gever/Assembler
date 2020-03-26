using System;
using System.Collections.Generic;
using System.Linq;
using Assembler.Base;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class EndFrameHandlerTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IFactory<BaseFrame, string>> _identifierFactoryMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<ICreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;
        private Mock<IMessageReleaser<BaseMessageInAssembly>> _messageReleaserMock;

        private List<Tuple<BaseMessageInAssembly, ReleaseReason>> _assembledMessages;
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

            _assembledMessages = new List<Tuple<BaseMessageInAssembly, ReleaseReason>>();
        }

        [TearDown]
        public void Teardown()
        {
            _identifierFactoryMock.VerifyNoOtherCalls();
            _enricherMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _messageInAssemblyCreatorMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_IdentifierInCache_MessageBeingRemovedEnrichedAndRaised(bool shouldReleaseSingleEndFrame)
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);
            var message = new Mock<BaseMessageInAssembly>();
            var handler = GenerateHandler(shouldReleaseSingleEndFrame);

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

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(message.Object, _assembledMessages.First().Item1);
            Assert.AreEqual(ReleaseReason.EndReceived, _assembledMessages.First().Item2);
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

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(message.Object, _assembledMessages.First().Item1);
            Assert.AreEqual(ReleaseReason.EndReceived, _assembledMessages.First().Item2);
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

            Assert.Zero(_assembledMessages.Count);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Handle_IdentifierThrowsException_FrameNotBeingUsed(bool shouldReleaseSingleEndFrame)
        {
            // Arrange
            var frame = new Mock<BaseFrame>(AssemblingPosition.Final);

            var handler = GenerateHandler(shouldReleaseSingleEndFrame);

            _identifierFactoryMock.Setup(identifierFactory => identifierFactory.Create(It.IsAny<BaseFrame>()))
                .Throws<NullReferenceException>();

            // Act
            handler.Handle(frame.Object);

            // Assert
            _identifierFactoryMock.Verify(identifier => identifier.Create(It.IsAny<BaseFrame>()), Times.Once);
            _identifierFactoryMock.Verify(identifier => identifier.Create(frame.Object), Times.Once);
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