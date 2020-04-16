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
    public class RawFinalFrameMessageEnricherTests
    {
        private RawFinalFrameMessageEnricher _enricher;
        private BaseFrame _frame;
        private RawMessageInAssembly _message;

        [SetUp]
        public void Setup()
        {
            _frame = TestUtilities.GenerateBaseFrame(AssemblingPosition.Final);
            _message = new RawMessageInAssembly(DateTime.MinValue, DateTime.MinValue);
            _enricher = new RawFinalFrameMessageEnricher();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        public void Enrich_NonEmptyListOfFramesInMessage_MessageBeingEnriched(int numberOfFrames)
        {
            // Arrange
            for (int i = 0; i < numberOfFrames; i++)
            {
                _message.FinalFrames.Add(TestUtilities.GenerateBaseFrame(AssemblingPosition.Final));
            }

            // Act
            _enricher.Enrich(_frame, _message);

            // Assert
            Assert.AreEqual(numberOfFrames + 1, _message.FinalFrames.Count);
            Assert.AreEqual(_frame, _message.FinalFrames.Last());
        }

        [Test]
        public void Enrich_EmptyListOfFramesInMessage_MessageBeingEnriched()
        {
            // Act
            _enricher.Enrich(_frame, _message);

            // Assert
            Assert.AreEqual(_frame, _message.FinalFrames.Single());
        }
    }
}