using System.Collections.Generic;
using System.Linq;
using Assembler.Base.Releasing;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.Releasing
{
    [TestFixture]
    public class ConditionalMessageInAssemblyReleaserTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;
        private Mock<IValidator<BaseMessageInAssembly>> _validatorMock;

        private ConditionalMessageInAssemblyReleaser<BaseMessageInAssembly> _releaser;

        private BaseMessageInAssembly _message;
        private List<BaseMessageInAssembly> _releasedMessages;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _validatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
            _message = TestUtilities.GenerateBaseMessageInAssembly();
            _releasedMessages = new List<BaseMessageInAssembly>();

            _releaser = new ConditionalMessageInAssemblyReleaser<BaseMessageInAssembly>(_validatorMock.Object, _cacheMock.Object,
                TestUtilities.GetLoggerFactory());

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
            var releaseReason = ReleaseReason.AnotherMessageInitialized;
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(true);

            // Act
            _releaser.Release(_message, releaseReason);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(_message), Times.Once);

            Assert.AreEqual(_message, _releasedMessages.Single());
            Assert.AreEqual(releaseReason, _message.ReleaseReason);
        }

        [Test]
        public void Release_ValidatorReturnsFalse_MessageNotBeingReleased()
        {
            // Arrange
            var releaseReason = ReleaseReason.AnotherMessageInitialized;

            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(false);

            // Act
            _releaser.Release(_message, releaseReason);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(_message), Times.Once);

            Assert.Zero(_releasedMessages.Count);
            Assert.AreNotEqual(releaseReason, _message.ReleaseReason);
        }

        [Test]
        public void Release_CacheRaisesEventAndValidatorReturnsTrue_MessageBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(true);

            // Act
            _releaser.Start();
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(_message), Times.Once);

            Assert.AreEqual(_message, _releasedMessages.Single());
            Assert.AreEqual(ReleaseReason.TimeoutReached, _message.ReleaseReason);
        }

        [Test]
        public void Release_CacheRaisesEventAndValidatorReturnsFalse_MessageNotBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(false);

            // Act
            _releaser.Start();
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(_message), Times.Once);

            Assert.Zero(_releasedMessages.Count);
            Assert.AreNotEqual(ReleaseReason.TimeoutReached, _message.ReleaseReason);
        }

        [Test]
        public void Release_StartNotBeingCalledAndCacheRaisesEvent_MessageNotBeingReleased()
        {
            // Arrange
            _validatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>())).Returns(false);

            // Act
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            _validatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Never);

            Assert.Zero(_releasedMessages.Count);
            Assert.AreNotEqual(ReleaseReason.TimeoutReached, _message.ReleaseReason);
        }

        [Test]
        public void Dispose_StartAndDisposeBeingCalledAndCacheRaisesEvent_MessageNotBeingReleased()
        {
            // Act
            _releaser.Start();
            _releaser.Dispose();
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            Assert.Zero(_releasedMessages.Count);
        }
    }
}