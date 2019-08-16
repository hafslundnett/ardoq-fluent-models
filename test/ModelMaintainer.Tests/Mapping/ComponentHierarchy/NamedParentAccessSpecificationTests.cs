using System;
using ModelMaintainer.Mapping;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Mapping.ComponentHierarchy
{
    public class NamedParentAccessSpecificationTests
    {
        [Fact]
        public void IsNamed_HappyDays_ReturnsTrue()
        {
            // Arrange
            var name = "ParentNode";
            var spec = new NamedReferenceAccessSpecification(name);

            // Act
            var isNamed = spec.IsNamed;

            // Assert
            Assert.True(isNamed);
        }

        [Fact]
        public void GetName_WithConstructedName_ReturnsExpectedName()
        {
            // Arrange
            var name = "ParentNode";
            var spec = new NamedReferenceAccessSpecification(name);

            // Act
            var receivedName = spec.GetName();

            // Assert
            Assert.Equal(name, receivedName);
        }
    }
}
