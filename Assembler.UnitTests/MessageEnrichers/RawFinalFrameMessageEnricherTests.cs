using System.Linq;
using Assembler.Base.MessageEnrichers;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.MessageEnrichers
{
    [TestFixture]
    public class RawFinalFrameMessageEnricherTests
    {
        private RawFinalFrameMessageEnricher _enricher;
        private Mock<BaseFrame> _frameMock;
        private RawMessageInAssembly _message;

        [SetUp]
        public void Setup()
        {
            _frameMock = new Mock<BaseFrame>(AssemblingPosition.Final);
            _message = new RawMessageInAssembly();
            _enricher = new RawFinalFrameMessageEnricher();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Enrich_NonEmptyListOfFramesInMessage_MessageBringEnriched(int numberOfFrames)
        {
            // Arrange
            for (int i = 0; i < numberOfFrames; i++)
            {
                _message.FinalFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Final).Object);
            }

            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(numberOfFrames + 1, _message.FinalFrames.Count);
            Assert.AreEqual(_frameMock.Object, _message.FinalFrames.Last());
        }

        [Test]
        public void Enrich_EmptyListOfFramesInMessage_MessageBringEnriched()
        {
            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(1, _message.FinalFrames.Count);
            Assert.AreEqual(_frameMock.Object, _message.FinalFrames.First());
        }
    }
}