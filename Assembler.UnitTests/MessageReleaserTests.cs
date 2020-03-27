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
    public class MessageReleaserTests
    {
        private MessageReleaser<BaseMessageInAssembly> _releaser;
        private Mock<ITimeBasedCache<BaseMessageInAssembly>> _cacheMock;

        private Mock<BaseMessageInAssembly> _messageMock;
        private List<BaseMessageInAssembly> _releasedMessages;

        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<ITimeBasedCache<BaseMessageInAssembly>>();
            _messageMock = new Mock<BaseMessageInAssembly>();
            _releasedMessages = new List<BaseMessageInAssembly>();

            _releaser = new MessageReleaser<BaseMessageInAssembly>(_cacheMock.Object,
                Utilities.GetLoggerFactory());

            _releaser.MessageReleased += _releasedMessages.Add;
        }

        [TearDown]
        public void Teardown()
        {
            _cacheMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Release_ValidMessage_EventRaised()
        {
            // Act
            _releaser.Release(_messageMock.Object, ReleaseReason.AnotherMessageStarted);

            // Assert
            Assert.AreEqual(1, _releasedMessages.Count);
            Assert.AreEqual(_messageMock.Object, _releasedMessages.First());
        }

        [Test]
        public void ReleaseExpiredMessage_ValidMessage_EventBeingRaised()
        {
            // Act
            _cacheMock.Raise(cache => cache.ItemExpired += null, _messageMock.Object);

            // Assert
            Assert.AreEqual(1, _releasedMessages.Count);
            Assert.AreEqual(_messageMock.Object, _releasedMessages.First());
        }
    }
}