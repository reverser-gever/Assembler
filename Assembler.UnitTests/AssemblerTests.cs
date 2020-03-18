using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class MessagesAssemblerTests
    {
        private Mock<IResolver<FrameType, IHandler<BaseFrame, BaseMessageInAssembly>>> _resolverMock;
        private Mock<IConverter<BaseMessageInAssembly, BaseAssembledMessage>> _converterMock;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IHandler<BaseFrame, BaseMessageInAssembly>> _firstHandlerMock;
        private Mock<IHandler<BaseFrame, BaseMessageInAssembly>> _secondHandlerMock;
        private Base.MessagesAssembler _assembler;

        private List<BaseAssembledMessage> _assembledMessages;
        private List<Mock<IHandler<BaseFrame, BaseMessageInAssembly>>> _handlersList;

        [SetUp]
        public void Setup()
        {
            _assembledMessages = new List<BaseAssembledMessage>();
            _resolverMock = new Mock<IResolver<FrameType, IHandler<BaseFrame, BaseMessageInAssembly>>>();
            _converterMock = new Mock<IConverter<BaseMessageInAssembly, BaseAssembledMessage>>();
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();

            _firstHandlerMock = new Mock<IHandler<BaseFrame, BaseMessageInAssembly>>();
            _secondHandlerMock = new Mock<IHandler<BaseFrame, BaseMessageInAssembly>>();

            _handlersList = new List<Mock<IHandler<BaseFrame, BaseMessageInAssembly>>>
            {
                _firstHandlerMock,
                _secondHandlerMock
            };
            var handlersList = _handlersList.Select(handlers => handlers.Object);

            _assembler = new Base.MessagesAssembler(_resolverMock.Object, _cacheMock.Object, handlersList,
                _converterMock.Object, Utilities.GetLoggerFactory());

            _assembler.MessageAssembled += _assembledMessages.Add;
        }

        [TearDown]
        public void Teardown()
        {
            _resolverMock.VerifyNoOtherCalls();
            _converterMock.VerifyNoOtherCalls();
            _cacheMock.VerifyNoOtherCalls();
            _firstHandlerMock.VerifyNoOtherCalls();
            _secondHandlerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Constructor_NoHandlers_NoExceptionBeingThrown()
        {
            // Arrange
            var emptyHandlersList = new List<IHandler<BaseFrame, BaseMessageInAssembly>>();

            // Act + Assert
            Assert.DoesNotThrow(() => _assembler = new Base.MessagesAssembler(_resolverMock.Object, _cacheMock.Object, emptyHandlersList,
                _converterMock.Object, Utilities.GetLoggerFactory()));
            Assert.Zero(_assembledMessages.Count);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ReleaseMessage_FirstHandlerRaisesEvent_AssemblerConvertsAndRaises(int handlerIndex)
        {
            // Arrange
            var messageInAssembly = new Mock<BaseMessageInAssembly>();
            var assembledMessage = new Mock<BaseAssembledMessage>();

            _converterMock.Setup(converter => converter.Convert(It.IsAny<BaseMessageInAssembly>()))
                .Returns(assembledMessage.Object);

            // Act
            _handlersList[handlerIndex].Raise(handler => handler.MessageAssemblyFinished += null, messageInAssembly.Object);

            // Assert
            _converterMock.Verify(converter => converter.Convert(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _converterMock.Verify(converter => converter.Convert(messageInAssembly.Object), Times.Once);

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(assembledMessage.Object, _assembledMessages.First());
        }

        [Test]
        public void ReleaseMessage_FirstAndSecondHandlersRaisesEvent_AssemblerConvertsAndRaisesBoth()
        {
            // Arrange
            var firstMessageInAssembly = new Mock<BaseMessageInAssembly>();
            var firstAssembledMessage = new Mock<BaseAssembledMessage>();

            var secondMessageInAssembly = new Mock<BaseMessageInAssembly>();
            var secondAssembledMessage = new Mock<BaseAssembledMessage>();

            var assembledMessages = new List<BaseAssembledMessage> { firstAssembledMessage.Object, secondAssembledMessage.Object };

            _converterMock.Setup(converter => converter.Convert(firstMessageInAssembly.Object))
                .Returns(firstAssembledMessage.Object);
            _converterMock.Setup(converter => converter.Convert(secondMessageInAssembly.Object))
                .Returns(secondAssembledMessage.Object);

            // Act
            _firstHandlerMock.Raise(handler => handler.MessageAssemblyFinished += null, firstMessageInAssembly.Object);
            _secondHandlerMock.Raise(handler => handler.MessageAssemblyFinished += null, secondMessageInAssembly.Object);

            // Assert
            _converterMock.Verify(converter => converter.Convert(It.IsAny<BaseMessageInAssembly>()), Times.Exactly(2));
            _converterMock.Verify(converter => converter.Convert(firstMessageInAssembly.Object), Times.Once);
            _converterMock.Verify(converter => converter.Convert(secondMessageInAssembly.Object), Times.Once);

            Assert.AreEqual(2, _assembledMessages.Count);
            CollectionAssert.AreEqual(assembledMessages, _assembledMessages);
        }

        [Test]
        public void ReleaseExpiredMessages_MessageExpired_ConvertedAndEventRaised()
        {
            // Arrange
            var messageInAssembly = new Mock<BaseMessageInAssembly>();
            var assembledMessage = new Mock<BaseAssembledMessage>();

            _converterMock.Setup(converter => converter.Convert(It.IsAny<BaseMessageInAssembly>()))
                .Returns(assembledMessage.Object);

            // Act
            _cacheMock.Raise(handler => handler.ItemExpired += null, messageInAssembly.Object);

            // Assert
            _converterMock.Verify(converter => converter.Convert(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _converterMock.Verify(converter => converter.Convert(messageInAssembly.Object), Times.Once);

            Assert.AreEqual(ReleaseReason.TimeoutReached, messageInAssembly.Object.ReleaseReason);

            Assert.AreEqual(1, _assembledMessages.Count);
            Assert.AreEqual(assembledMessage.Object, _assembledMessages.First());
        }

        [Test]
        public void Assemble_ValidInput_CallsResolverAndThenHandler()
        {
            // Arrange
            var frameType = FrameType.Initial;
            var message = new Mock<BaseFrame>(frameType);

            _resolverMock.Setup(resolver => resolver.Resolve(It.IsAny<FrameType>())).Returns(_firstHandlerMock.Object);

            // Act
            _assembler.Assemble(message.Object);

            // Assert
            _resolverMock.Verify(resolver => resolver.Resolve(It.IsAny<FrameType>()), Times.Once);
            _resolverMock.Verify(resolver => resolver.Resolve(frameType), Times.Once);

            _firstHandlerMock.Verify(handler => handler.Handle(It.IsAny<BaseFrame>()), Times.Once);
            _firstHandlerMock.Verify(handler => handler.Handle(message.Object), Times.Once);
        }
    }
}