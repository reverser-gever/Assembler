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
    public class MessageInAssemblyReleaserTests
    {
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;

        private MessageInAssemblyReleaser<BaseMessageInAssembly> _releaser;

        private BaseMessageInAssembly _message;
        private List<BaseMessageInAssembly> _releasedMessages;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _message = Utilities.GenerateBaseMessageInAssembly();
            _releasedMessages = new List<BaseMessageInAssembly>();

            _releaser = new MessageInAssemblyReleaser<BaseMessageInAssembly>(_cacheMock.Object,
                Utilities.GetLoggerFactory());

            _releaser.MessageReleased += _releasedMessages.Add;
        }

        [TearDown]
        public void Teardown()
        {
            _cacheMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Start_CacheRaisesEvent_ReleaserRaisesToo()
        {
            // Act
            _releaser.Start();
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            Assert.AreEqual(_message, _releasedMessages.Single());
        }

        [Test]
        public void Release_ValidMessage_EventRaised()
        {
            // Arrange
            var releaseReason = ReleaseReason.AnotherMessageInitialized;

            // Act
            _releaser.Start();
            _releaser.Release(_message, releaseReason);

            // Assert
            Assert.AreEqual(_message, _releasedMessages.Single());
            Assert.AreEqual(releaseReason, _message.ReleaseReason);
        }

        [Test]
        public void ReleaseExpiredMessage_ValidMessage_EventBeingRaised()
        {
            // Act
            _releaser.Start();
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            Assert.AreEqual(_message, _releasedMessages.Single());
            Assert.AreEqual(ReleaseReason.TimeoutReached, _message.ReleaseReason);
        }

        [Test]
        public void Release_StartNotBeingCalledAndCacheRaisesEvent_MessageNotBeingReleased()
        {
            // Act
            _cacheMock.Raise(cache => cache.ItemExpired += null, _message);

            // Assert
            Assert.Zero(_releasedMessages.Count);
        }

        [Test]
        public void Release_StartAndDisposeBeingCalledAndCacheRaisesEvent_MessageNotBeingReleased()
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