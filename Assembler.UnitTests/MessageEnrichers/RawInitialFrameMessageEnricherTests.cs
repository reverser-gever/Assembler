using System;
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
    public class RawInitialFrameMessageEnricherTests
    {
        private RawInitialFrameMessageEnricher _enricher;
        private Mock<BaseFrame> _frameMock;
        private RawMessageInAssembly _message;

        [SetUp]
        public void Setup()
        {
            _frameMock = new Mock<BaseFrame>(AssemblingPosition.Initial);
            _message = new RawMessageInAssembly(DateTime.MinValue, DateTime.MinValue);
            _enricher = new RawInitialFrameMessageEnricher();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Enrich_NonEmptyListOfFramesInMessage_MessageBeingEnriched(int numberOfFrames)
        {
            // Arrange
            for (int i = 0; i < numberOfFrames; i++)
            {
                _message.InitialFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Initial).Object);
            }

            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(numberOfFrames + 1, _message.InitialFrames.Count);
            Assert.AreEqual(_frameMock.Object, _message.InitialFrames.Last());
        }

        [Test]
        public void Enrich_EmptyListOfFramesInMessage_MessageBeingEnriched()
        {
            // Act
            _enricher.Enrich(_frameMock.Object, _message);

            // Assert
            Assert.AreEqual(_frameMock.Object, _message.InitialFrames.Single());
        }
    }
}