using System.Collections.Generic;
using Assembler.Base.Validators;
using Assembler.Core;
using Assembler.Core.Entities;
using Assembler.Core.Enums;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests.Validators
{
    [TestFixture]
    public class CompositeValidatorTests
    {
        private Mock<IValidator<BaseMessageInAssembly>> _firstValidatorMock;
        private Mock<IValidator<BaseMessageInAssembly>> _secondValidatorMock;
        private Mock<IValidator<BaseMessageInAssembly>> _thirdValidatorMock;

        [SetUp]
        public void Setup()
        {
            _firstValidatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
            _secondValidatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
            _thirdValidatorMock = new Mock<IValidator<BaseMessageInAssembly>>();
        }

        [TearDown]
        public void Teardown()
        {
            _firstValidatorMock.VerifyNoOtherCalls();
            _secondValidatorMock.VerifyNoOtherCalls();
            _thirdValidatorMock.VerifyNoOtherCalls();
        }

        [TestCase(true, true, true, Operator.And, true)]
        [TestCase(true, true, true, Operator.Or, true)]
        [TestCase(false, false, false, Operator.Or, false)]
        [TestCase(false, false, false, Operator.And, false)]
        [TestCase(false, false, true, Operator.And, false)]
        [TestCase(false, false, true, Operator.Or, true)]
        [TestCase(false, true, true, Operator.Or, true)]
        [TestCase(false, true, true, Operator.And, false)]
        public void IsValid_VariousOperatorsAndReturns_MatchingResult(bool firstValidatorReturnValue,
                    bool secondValidatorReturnValue, bool thirdValidatorReturnValue, Operator @operator, bool expectedResult)
        {
            // Arrange
            var message = new Mock<BaseMessageInAssembly>();

            _firstValidatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()))
                .Returns(firstValidatorReturnValue);
            _secondValidatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()))
                .Returns(secondValidatorReturnValue);
            _thirdValidatorMock.Setup(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()))
                .Returns(thirdValidatorReturnValue);

            var validators = new List<IValidator<BaseMessageInAssembly>>
            {
                _firstValidatorMock.Object,
                _secondValidatorMock.Object,
                _thirdValidatorMock.Object
            };

            var compositeValidator = new CompositeValidator<BaseMessageInAssembly>(validators, @operator);

            // Act
            var result = compositeValidator.IsValid(message.Object);

            // Assert
            Assert.AreEqual(expectedResult, result);

            _firstValidatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _firstValidatorMock.Verify(validator => validator.IsValid(message.Object), Times.Once);
            _secondValidatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _secondValidatorMock.Verify(validator => validator.IsValid(message.Object), Times.Once);
            _thirdValidatorMock.Verify(validator => validator.IsValid(It.IsAny<BaseMessageInAssembly>()), Times.Once);
            _thirdValidatorMock.Verify(validator => validator.IsValid(message.Object), Times.Once);
        }
    }
}