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
    public class ConditionalMessageReleaserTests
    {
        private ConditionalMessageReleaser<BaseMessageInAssembly> _releaser;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IValidator<BaseMessageInAssembly>> _validatorMock;

        private Mock<BaseMessageInAssembly> _messageMock;
        private List<BaseMessageInAssembly> _releasedMessages;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _validatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
            _messageMock = new Mock<BaseMessageInAssembly>();
            _releasedMessages = new List<BaseMessageInAssembly>();

            _releaser = new ConditionalMessageReleaser<BaseMessageInAssembly>(_validatorMock.Object, _cacheMock.Object,
                Utilities.GetLoggerFactory());

            _releaser.MessageReleased += _releasedMessages.Add;
        }

        [TearDown]
        public void Teardown()
        {
            _cacheMock.VerifyNoOtherCalls();
            _validatorMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Release_ValidatorReturnsTrue_MessageBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(true);

            // Act
            _releaser.Release(_messageMock.Object, ReleaseReason.AnotherMessageStarted);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _validatorMock.Verify(validator => validator.IsValid(_messageMock.Object), Times.Once);

            Assert.AreEqual(1, _releasedMessages.Count);
            Assert.AreEqual(_messageMock.Object, _releasedMessages.First());
        }

        [Test]
        public void Release_ValidatorReturnsTrue_MessageNotBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(false);

            // Act
            _releaser.Release(_messageMock.Object, ReleaseReason.AnotherMessageStarted);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _validatorMock.Verify(validator => validator.IsValid(_messageMock.Object), Times.Once);

            Assert.Zero(_releasedMessages.Count);
        }

        [Test]
        public void Release_CacheRaisesEventAndValidatorReturnsTrue_MessageBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(true);

            // Act
            _cacheMock.Raise(cache => cache.ItemExpired += null, _messageMock.Object);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _validatorMock.Verify(validator => validator.IsValid(_messageMock.Object), Times.Once);

            Assert.AreEqual(1, _releasedMessages.Count);
            Assert.AreEqual(_messageMock.Object, _releasedMessages.First());
        }

        [Test]
        public void Release_CacheRaisesEventAndValidatorReturnsFalse_MessageNotBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(false);

            // Act
            _cacheMock.Raise(cache => cache.ItemExpired += null, _messageMock.Object);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _validatorMock.Verify(validator => validator.IsValid(_messageMock.Object), Times.Once);

            Assert.Zero(_releasedMessages.Count);
        }
    }
}