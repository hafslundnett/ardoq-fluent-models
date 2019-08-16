using System;
using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests
{
    public class ArdoqModelMappingBuilderTests
    {
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;
        private readonly Mock<IArdoqSearcher> _searcherMock;

        public ArdoqModelMappingBuilderTests()
        {
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
            _searcherMock = new Mock<IArdoqSearcher>();
        }

        [Fact]
        public void AddComponentMapping_SameClassTwice_ThrowsArgumentException()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);

            // Act
            builder.AddComponentMapping<Employee>("Employee")
                .WithPreexistingHierarchyReference("Employees");

            // Assert
            var ex = Assert.Throws<ArgumentException>(() => builder.AddComponentMapping<Employee>("Employee2"));
            Assert.Equal("Source type ModelMaintainer.Tests.Model.Employee already registered.", ex.Message);
        }

        [Fact]
        public void WithKey_HappyDays_KeySetInBuilder()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);

            // Act
            builder.AddComponentMapping<Role>("Role")
                .WithPreexistingHierarchyReference("Roles")
                .WithKey(s => s.Name);

            // Assert
            var mapping = builder.GetBuildableComponentMapping<Role>();
            Assert.NotNull(mapping.KeyGetter);
            Assert.Equal("Name", mapping.KeyGetter.Name);
        }

        [Fact]
        public void WithSafeMode_True_IsSafeMode()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);

            // Act
            builder
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object)
                .WithSafeMode(true);

            var session = builder.Build();

            // Assert
            Assert.True(session.IsSafeMode);
        }

        [Fact]
        public void WithSafeMode_False_IsNotSafeMode()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);

            // Act
            builder
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object)
                .WithSafeMode(false);

            var session = builder.Build();

            // Assert
            Assert.False(session.IsSafeMode);
        }

        [Fact]
        public void WithSafeMode_NotSpecified_IsNotSafeMode()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);

            // Act
            builder
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            var session = builder.Build();

            // Assert
            Assert.False(session.IsSafeMode);
        }

        [Fact]
        public void SampleConfig()
        {
            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithWorkspaceNamed("Blank Workspace")
                .WithFolderNamed("Ståle");

            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(e => e.EmployeeNumber)
                .WithPreexistingHierarchyReference("Employees");
        }

        [Fact]
        public void Build_EmptyModel_ReturnsEmptyMap()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.Build();

            // Assert
            Assert.Empty(builder.ComponentMappings);
        }

        private ArdoqModelMappingBuilder Builder()
        {
            return new ArdoqModelMappingBuilder(null, null, null)
                            .WithReader(_readerMock.Object)
                            .WithWriter(_writerMock.Object)
                            .WithSearcher(_searcherMock.Object);
        }

        [Fact]
        public void Build_ProperModel_ParentsComputedCorrectly()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Department>("Department")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent);
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.Build();

            // Assert
            var list = builder.ComponentMappings.ToList();
            Assert.Equal("Department", list[0].ArdoqComponentTypeName);
            Assert.Null(list[0].GetParent()?.ArdoqComponentTypeName);
            Assert.Equal("Employee", list[1].ArdoqComponentTypeName);
            Assert.Equal("Department", list[1].GetParent().ArdoqComponentTypeName);
            Assert.Equal("Role", list[2].ArdoqComponentTypeName);
            Assert.Equal("Employee", list[2].GetParent().ArdoqComponentTypeName);
        }

        [Fact]
        public void Build_ModelAlreadySorted_ComponentMappingsSorted()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Department>("Department")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent);
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.Build();

            // Assert
            var list = builder.ComponentMappings.ToList();
            Assert.Equal("Department", list[0].ArdoqComponentTypeName);
            Assert.Equal("Employee", list[1].ArdoqComponentTypeName);
            Assert.Equal("Role", list[2].ArdoqComponentTypeName);
        }

        [Fact]
        public void Build_SimpleModelNotSorted_ComponentMappingsSorted()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent);
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Department>("Department")
                .WithKey(s => s.Name);
            builder.Build();

            // Assert
            var list = builder.ComponentMappings.ToList();
            Assert.Equal("Department", list[0].ArdoqComponentTypeName);
            Assert.Equal("Employee", list[1].ArdoqComponentTypeName);
            Assert.Equal("Role", list[2].ArdoqComponentTypeName);
        }

        [Fact]
        public void Build_SimpleModelNotSorted2_ComponentMappingsSorted()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Department>("Department")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent);
            builder.Build();

            // Assert
            var list = builder.ComponentMappings.ToList();
            Assert.Equal("Department", list[0].ArdoqComponentTypeName);
            Assert.Equal("Employee", list[1].ArdoqComponentTypeName);
            Assert.Equal("Role", list[2].ArdoqComponentTypeName);
        }

        [Fact]
        public void Build_ModelWithMultipleChildren_ComponentMappingsSorted()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Department>("Department")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Office>("Office")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent)
                .WithModelledHierarchyReference(e => e.Office, ModelledReferenceDirection.Parent);
            builder.Build();

            // Assert
            var list = builder.ComponentMappings.ToList();
            Assert.Equal("Department", list[0].ArdoqComponentTypeName);
            Assert.Equal("Employee", list[1].ArdoqComponentTypeName);
            Assert.Equal(new HashSet<string> { "Role", "Office" }, new HashSet<string> { list[2].ArdoqComponentTypeName, list[3].ArdoqComponentTypeName });
        }

        [Fact]
        public void Build_ModelNotValid_ThrowsException()
        {
            // Arrange
            ArdoqModelMappingBuilder builder = Builder();

            // Act
            builder.AddComponentMapping<Role>("Role")
                .WithKey(s => s.Name);
            builder.AddComponentMapping<Employee>("Employee")
                .WithKey(s => s.Name)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent);

            // Assert
            Assert.ThrowsAny<Exception>(() => builder.Build());
        }
    }
}
