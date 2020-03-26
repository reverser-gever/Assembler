using System.Linq;
using Assembler.Base;
using Assembler.Base.MessageEnrichers;
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
            _frameMock = new Mock<BaseFrame>(AssemblingPosition.Initial);
            _message = new RawMessageInAssembly();
            _enricher = new RawFrameMessageEnricher();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Enrich_NonEmptyListOfFramesInMessage_MessageBringEnriched(int numberOfFrames)
        {
            // Arrange
            for (int i = 0; i < numberOfFrames; i++)
            {
                _message.AssembledFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Final).Object);
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