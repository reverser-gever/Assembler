﻿using System.Collections.Generic;
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
    public class EndMessageHandlerTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IFactory<BaseFrame, string>> _identifierFactoryMock;
        private Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>> _enricherMock;
        private Mock<ICreator<BaseMessageInAssembly>> _messageInAssemblyCreatorMock;

        private List<BaseMessageInAssembly> _assembledMessages;
        private string _identifierString;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _identifierFactoryMock = new Mock<IFactory<BaseFrame, string>>();
            _enricherMock = new Mock<IMessageEnricher<BaseFrame, BaseMessageInAssembly>>();
            _messageInAssemblyCreatorMock = new Mock<ICreator<BaseMessageInAssembly>>();

            _assembledMessages = new List<BaseMessageInAssembly>();

            _identifierString = "yes very";

            _identifierFactoryMock.Setup(identifier => identifier.Create(It.IsAny<BaseFrame>()))
                .Returns(_identifierString);
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
        public void Handle_IdentifierInCache_MessageBeingRemovedEnrichedAndRaised(bool isToReleaseSingleEndFrame)
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);
            var message = new Mock<BaseMessageInAssembly>();
            var handler = GenerateHandler(isToReleaseSingleEndFrame);

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

            Assert.AreEqual(ReleaseReason.EndReceived, message.Object.ReleaseReason);

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(message.Object, _assembledMessages.First());
        }

        [Test]
        public void Handle_IdentifierNotInCacheAndFlagSetToYes_MessageBeingCreatedEnrichedAndReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);
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

            Assert.AreEqual(ReleaseReason.EndReceived, message.Object.ReleaseReason);

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(message.Object, _assembledMessages.First());
        }

        [Test]
        public void Handle_IdentifierNotInCacheAndFlagSetToFalse_NoMessagesBeingReleased()
        {
            // Arrange
            var frame = new Mock<BaseFrame>(FrameType.Initial);
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

        private EndMessageHandler<BaseFrame, BaseMessageInAssembly> GenerateHandler(bool isToReleaseSingleEndFrame)
        {
            var handler = new EndMessageHandler<BaseFrame, BaseMessageInAssembly>(_cacheMock.Object,
                _identifierFactoryMock.Object, _enricherMock.Object, _messageInAssemblyCreatorMock.Object,
                isToReleaseSingleEndFrame, Utilities.GetLoggerFactory());

            handler.MessageAssemblyFinished += _assembledMessages.Add;

            return handler;
        }
    }
}