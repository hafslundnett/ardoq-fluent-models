using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArdoqFluentModels.Mapping;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Mapping
{
    public class ComponentMappingTests
    {
        [Fact]
        public void GetComponentInfo_HappyDays_GetsExpectedKey()
        {
            // Arrange
            var mapping = new ComponentMapping<Employee>(null);
            mapping.WithKey(emp => emp.EmployeeNumber);

            var employee = new Employee {EmployeeNumber = "666"};

            // Act
            var componentInfo = mapping.GetComponentInfo(employee);

            // Assert
            Assert.Equal("666", componentInfo.Item1);
        }

        [Fact]
        public void GetComponentInfo_HappyDays_GetsCorrectFieldsValues()
        {
            // Arrange
            var mapping = new ComponentMapping<Employee>(null);
            mapping.WithKey(emp => emp.EmployeeNumber);
            mapping.WithField(e => e.Name, "employee-name");

            var employee = new Employee { EmployeeNumber = "666", Name = "Donald Duck"};

            // Act
            var componentInfo = mapping.GetComponentInfo(employee);

            // Assert
            Assert.Equal("Donald Duck", componentInfo.Item2["employee-name"]);
        }

        [Fact]
        public void ReferenceSetup_WithFirstPreexistingThenModelled_ThrowsInvalidOperationException()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => mapping
                .WithPreexistingHierarchyReference("Namespace")
                .WithModelledHierarchyReference(eh => eh.ResourceGroup, ModelledReferenceDirection.Child));

            Assert.Equal("Can only configure a single reference where the class is the child.", ex.Message);
        }

        [Fact]
        public void ReferenceSetup_WithFirstModelledThenPreexisting_ThrowsInvalidOperationException()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => mapping
                .WithModelledHierarchyReference(eh => eh.ResourceGroup, ModelledReferenceDirection.Child)
                .WithPreexistingHierarchyReference("Namespace"));

            Assert.Equal("Can only configure a single reference where the class is the child.", ex.Message);
        }

        [Fact]
        public void GetTags_NoAccessorSetup_ReturnsNull()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null);

            // Act
            var tags = mapping.GetTags(new EventHubNamespace());

            // Assert
            Assert.Null(tags);
        }

        [Fact]
        public void GetTags_WithTagAccessor_ReturnsTags()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null)
                .WithTags(enh => enh.Tags);

            var obj = new EventHubNamespace
            {
                Tags = new List<string>
                {
                    "value1",
                    "value2"
                }
            };

            // Act
            var tags = mapping.GetTags(obj);

            // Assert
            Assert.Equal(2, tags.Count());
            Assert.Contains("value1", tags);
            Assert.Contains("value2", tags);
        }

        [Fact]
        public void GetTags_WithTagAccessorButNullInObject_ReturnsNull()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null)
                .WithTags(enh => enh.Tags);

            var obj = new EventHubNamespace();

            // Act
            var tags = mapping.GetTags(obj);

            // Assert
            Assert.Null(tags);
        }

        [Fact]
        public void WithTags_PropertyBasedWithGetterAlreadyDefined_ThrowsArgumentException()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null)
                .WithTags(Tags.FromExpression<EventHubNamespace>(ns => ns.TagMap.Select(p => $"{p.Key}::{p.Value}")));

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => mapping.WithTags(enh => enh.Tags));
            Assert.Equal("Tag getter already defined.", ex.Message);
        }

        [Fact]
        public void WithTags_GetterBasedWithGetterAlreadyDefined_ThrowsArgumentException()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null)
                .WithTags(enh => enh.Tags);
 
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => mapping.WithTags(Tags.FromExpression<EventHubNamespace>(ns => ns.TagMap.Select(p => $"{p.Key}::{p.Value}"))));
            Assert.Equal("Tag getter already defined.", ex.Message);
        }

        [Fact]
        public void GetTags_WithTagGetterDefined_GetsExpectedTags()
        {
            // Arrange
            var mapping = new ComponentMapping<EventHubNamespace>(null)
                .WithTags(Tags.FromExpression<EventHubNamespace>(ns => ns.TagMap.Select(p => $"{p.Key}::{p.Value}")));

            var eh = new EventHubNamespace
            {
                TagMap = new Dictionary<string, string>
                {
                    ["a"] = "b",
                    ["c"] = "d"
                }
            };

            // Act
            var tags = mapping.GetTags(eh);

            // Assert
            Assert.Equal(2, tags.Count());
            Assert.Contains("a::b", tags);
            Assert.Contains("c::d", tags);
        }
    }
}
