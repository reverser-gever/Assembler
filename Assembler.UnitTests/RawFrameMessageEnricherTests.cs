using System.Linq;
using Assembler.Base;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class RawFrameMessageEnricherTests
    {
        private RawFrameMessageEnricher _enricher;
        private Mock<BaseFrame> _frameMock;
        private RawMessageInAssembly _message;

        [SetUp]
        public void Setup()
        {
            _frameMock = new Mock<BaseFrame>(FrameType.Initial);
            _message = new RawMessageInAssembly();
            _enricher = new RawFrameMessageEnricher();
        }

        [Test]
        public void Enrich_MessageIsNull_DoesNotThrow()
        {
            // Act + Assert
            Assert.DoesNotThrow(() => _enricher.Enrich(_frameMock.Object, null));
        }

        [Test]
        public void Enrich_MessageIsNotRawMessageInAssembly_DoesNotThrow()
        {
            // Act + Assert
            Assert.DoesNotThrow(() => _enricher.Enrich(_frameMock.Object, new Mock<RawMessageInAssembly>().Object));
        }

        [Test]
        public void Enrich_NonEmptyListOfFramesInMessage_MessageBringEnriched()
        {
            // Arrange
            var numberOfFrames = 10;
            for (int i = 0; i < numberOfFrames; i++)
            {
                _message.AssembledFrames.Add(new Mock<BaseFrame>(FrameType.End).Object);
            }

            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(numberOfFrames + 1, _message.AssembledFrames.Count);
            Assert.AreEqual(_frameMock.Object, _message.AssembledFrames.Last());
        }

        [Test]
        public void Enrich_EmptyListOfFramesInMessage_MessageBringEnriched()
        {
            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(1, _message.AssembledFrames.Count);
            Assert.AreEqual(_frameMock.Object, _message.AssembledFrames.First());
        }
    }
}