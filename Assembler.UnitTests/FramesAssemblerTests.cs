﻿using System;
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
    public class FramesAssemblerTests
    {
        private Mock<IResolver<AssemblingPosition, IFrameHandler<BaseFrame>>> _resolverMock;
        private Mock<IFrameHandler<BaseFrame>> _handlerMock;
        private FramesAssembler<BaseFrame> _assembler;

        [SetUp]
        public void Setup()
        {
            _resolverMock = new Mock<IResolver<AssemblingPosition, IFrameHandler<BaseFrame>>>();
            _handlerMock = new Mock<IFrameHandler<BaseFrame>>();

            _assembler = new FramesAssembler<BaseFrame>(_resolverMock.Object, Utilities.GetLoggerFactory());
        }

        [TearDown]
        public void Teardown()
        {
            _resolverMock.VerifyNoOtherCalls();
            _handlerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Assemble_ValidInput_CallsResolverAndThenHandler()
        {
            // Arrange
            var frameType = AssemblingPosition.Initial;
            var message = new Mock<BaseFrame>(frameType);

            _resolverMock.Setup(resolver => resolver.Resolve(It.IsAny<AssemblingPosition>()))
                .Returns(_handlerMock.Object);

            // Act
            _assembler.Assemble(message.Object);

            // Assert
            _resolverMock.Verify(resolver => resolver.Resolve(It.IsAny<AssemblingPosition>()), Times.Once);
            _resolverMock.Verify(resolver => resolver.Resolve(frameType), Times.Once);

            _handlerMock.Verify(handler => handler.Handle(It.IsAny<BaseFrame>()), Times.Once);
            _handlerMock.Verify(handler => handler.Handle(message.Object), Times.Once);
        }

        [Test]
        public void Assemble_ResolverThrowsKeyNotFoundException_DoesNotThrow()
        {
            // Arrange
            var frameType = AssemblingPosition.Initial;
            var message = new Mock<BaseFrame>(frameType);

            _resolverMock.Setup(resolver => resolver.Resolve(It.IsAny<AssemblingPosition>()))
                .Throws<KeyNotFoundException>();

            // Act
            Assert.DoesNotThrow(() => _assembler.Assemble(message.Object));

            // Assert
            _resolverMock.Verify(resolver => resolver.Resolve(It.IsAny<AssemblingPosition>()), Times.Once);
            _resolverMock.Verify(resolver => resolver.Resolve(frameType), Times.Once);

            _handlerMock.Verify(handler => handler.Handle(It.IsAny<BaseFrame>()), Times.Never);
        }
    }
}