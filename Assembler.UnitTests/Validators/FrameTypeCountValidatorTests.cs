using Assembler.Base.Validators;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.Validators
{
    [TestFixture]
    public class FrameTypeCountValidatorTests
    {
        // Testing Initial
        [TestCase(1, 0, 0, 1, 10, 10, true)]
        [TestCase(2, 0, 0, 1, 10, 10, false)]
        [TestCase(2, 0, 0, 10, 0, 0, true)]
        [TestCase(20, 50, 50, 1, 0, 0, false)]
        // Testing Middle
        [TestCase(0, 1, 0, 10, 1, 10, true)]
        [TestCase(0, 2, 0, 0, 1, 0, false)]
        [TestCase(0, 2, 0, 0, 10, 0, true)]
        [TestCase(50, 20, 50, 0, 1, 0, false)]
        [TestCase(0, 0, 1, 10, 10, 1, true)]
        // Testing Final
        [TestCase(0, 0, 2, 0, 0, 1, false)]
        [TestCase(0, 0, 2, 0, 0, 10, true)]
        [TestCase(50, 50, 20, 0, 0, 1, false)]
        [TestCase(50, 50, 20, 0, 0, 1, false)]
        // Testing All
        [TestCase(1, 1, 1, 20, 20, 0, false)]
        [TestCase(1, 1, 1, 20, 20, 1, true)]
        public void IsValid_VariousMessages_ReturnsValueMatchingTheInput(int minimumNumberOfInitialFrames,
            int minimumNumberOfMiddleFrames, int minimumNumberOfFinalFrames, int actualNumberOfInitialFrames,
            int actualNumberOfMiddleFrames, int actualNumberOfFinalFrames, bool expectedResult)
        {
            // Arrange
            FrameTypeCountValidator validator = new FrameTypeCountValidator(minimumNumberOfInitialFrames,
                minimumNumberOfMiddleFrames, minimumNumberOfFinalFrames);

            var message = new RawMessageInAssembly();

            for (int i = 0; i < actualNumberOfInitialFrames; i++)
            {
                message.InitialFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Initial).Object);
            }

            for (int i = 0; i < actualNumberOfMiddleFrames; i++)
            {
                message.MiddleFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Middle).Object);
            }

            for (int i = 0; i < actualNumberOfFinalFrames; i++)
            {
                message.FinalFrames.Add(new Mock<BaseFrame>(AssemblingPosition.Final).Object);
            }

            // Act
            var result = validator.IsValid(message);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}