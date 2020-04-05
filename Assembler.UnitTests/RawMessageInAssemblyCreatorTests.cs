using System;
using Assembler.Base.Creators;
using Assembler.Core;
using Assembler.Core.RawAssemblingEntities;
using DeepEqual.Syntax;
using Moq;
using NUnit.Framework;

namespace Assembler.UnitTests
{
    [TestFixture]
    public class RawMessageInAssemblyCreatorTests
    {
        private RawMessageInAssemblyCreator _creator;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;

        [SetUp]
        public void Setup()
        {
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _creator = new RawMessageInAssemblyCreator(_dateTimeProviderMock.Object);
        }

        [Test]
        public void Create_ValidInput_CreatesANewObjectWithTheProvidedDateTime()
        {
            // Arrange
            var now = DateTime.MaxValue;
            var newRawMessage = new RawMessageInAssembly(now, now);
            _dateTimeProviderMock.Setup(provider => provider.Now).Returns(now);

            // Act
            var result = _creator.Create();

            // Assert
            _dateTimeProviderMock.Verify(provider => provider.Now, Times.Once);
            newRawMessage.WithDeepEqual(result).IgnoreSourceProperty(message => message.Guid).Assert();
        }
    }
}