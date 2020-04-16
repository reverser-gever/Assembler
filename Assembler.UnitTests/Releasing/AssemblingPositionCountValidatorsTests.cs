using System;
using Assembler.Base.Releasing.PositionCountValidators;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Assembler.Core.RawAssemblingEntities;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.Releasing
{
    [TestFixture]
    public class AssemblingPositionCountValidatorsTests
    {
        [TestCaseSource(nameof(GetObjects))]
        public void IsValid_VariousInputs_VariousExpectedOutputs(IValidator<RawMessageInAssembly> validator,
             int numberOfInitialFrames, int numberOfMiddleFrames, int numberOfFinalFrames, bool expectedResult)
        {
            // Arrange
            var message = new RawMessageInAssembly(DateTime.MinValue, DateTime.MinValue);

            for (int i = 0; i < numberOfInitialFrames; i++)
            {
                message.InitialFrames.Add(TestUtilities.GenerateBaseFrame(AssemblingPosition.Initial));
            }

            for (int i = 0; i < numberOfMiddleFrames; i++)
            {
                message.MiddleFrames.Add(TestUtilities.GenerateBaseFrame(AssemblingPosition.Middle));
            }

            for (int i = 0; i < numberOfFinalFrames; i++)
            {
                message.FinalFrames.Add(TestUtilities.GenerateBaseFrame(AssemblingPosition.Final));
            }

            // Act
            var result = validator.IsValid(message);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        private static object[] GetObjects()
        {
            var minimumFrames = 2;

            var initialValidator = new InitialAssemblingPositionCountValidator(minimumFrames, TestUtilities.GetLoggerFactory());
            var middleValidator = new MiddleAssemblingPositionCountValidator(minimumFrames, TestUtilities.GetLoggerFactory());
            var finalValidator = new FinalAssemblingPositionCountValidator(minimumFrames, TestUtilities.GetLoggerFactory());

            return new object[]
            {
                new TestCaseData(initialValidator, 0, 0, 0, false).SetName("InitialWith0"),
                new TestCaseData(initialValidator, 1, 0, 0, false).SetName("InitialWith1"),
                new TestCaseData(initialValidator, 2, 0, 0, true).SetName("InitialWith2"),
                new TestCaseData(initialValidator, 100, 0, 0, true).SetName("InitialWith100"),
                new TestCaseData(middleValidator, 0, 0, 0, false).SetName("MiddleWith0"),
                new TestCaseData(middleValidator, 0, 1, 0, false).SetName("MiddleWith1"),
                new TestCaseData(middleValidator, 0, 2, 0, true).SetName("MiddleWith2"),
                new TestCaseData(middleValidator, 0, 100, 0, true).SetName("MiddleWith100"),
                new TestCaseData(finalValidator, 0, 0, 0, false).SetName("FinalWith0"),
                new TestCaseData(finalValidator, 0, 0, 1, false).SetName("FinalWith1"),
                new TestCaseData(finalValidator, 0, 0, 2, true).SetName("FinalWith2"),
                new TestCaseData(finalValidator, 0, 0, 100, true).SetName("FinalWith100"),
            };
        }
    }
}