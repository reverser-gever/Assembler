using System.Collections.Generic;
using Assembler.Base.Releasing;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.Releasing
{
    [TestFixture]
    public class CompositeValidatorTests
    {
        private Mock<IValidator<BaseMessageInAssembly>> _firstValidatorMock;
        private Mock<IValidator<BaseMessageInAssembly>> _secondValidatorMock;

        [SetUp]
        public void Setup()
        {
            _firstValidatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
            _secondValidatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
        }

        [TearDown]
        public void Teardown()
        {
            _firstValidatorMock.VerifyNoOtherCalls();
            _secondValidatorMock.VerifyNoOtherCalls();
        }

        [TestCase(true, true, LogicalOperator.And, true)]
        [TestCase(false, false, LogicalOperator.And, false)]
        [TestCase(true, false, LogicalOperator.And, false)]
        [TestCase(false, true, LogicalOperator.And, false)]
        [TestCase(true, true, LogicalOperator.Or, true)]
        [TestCase(true, false, LogicalOperator.Or, true)]
        [TestCase(false, true, LogicalOperator.Or, true)]
        [TestCase(false, false, LogicalOperator.Or, false)]
        public void IsValid_VariousLogicalOperatorsAndReturns_MatchingResult(bool firstValidatorReturnValue,
                    bool secondValidatorReturnValue, LogicalOperator logicalOperator, bool expectedResult)
        {
            // Arrange
            var message = TestUtilities.GenerateBaseMessageInAssembly();

            _firstValidatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()))
                .Returns(firstValidatorReturnValue);
            _secondValidatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()))
                .Returns(secondValidatorReturnValue);

            var validators = new List<IValidator<BaseMessageInAssembly>>
            {
                _firstValidatorMock.Object,
                _secondValidatorMock.Object,
            };

            var compositeValidator = new CompositeValidator<BaseMessageInAssembly>(validators, logicalOperator);

            // Act
            var result = compositeValidator.IsValid(message);

            // Assert
            Assert.AreEqual(expectedResult, result);

            _firstValidatorMock.Verify(validator => validator.IsValid(message), Times.Once);

            if ((logicalOperator == LogicalOperator.And && firstValidatorReturnValue) ||
                (logicalOperator == LogicalOperator.Or && !firstValidatorReturnValue))
            {
                _secondValidatorMock.Verify(validator => validator.IsValid(message), Times.Once);
            }
        }
    }
}