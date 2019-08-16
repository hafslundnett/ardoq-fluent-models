using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Maintainence;
using ArdoqFluentModels.Mapping;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using ArdoqFluentModels.Utils;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ComponentHierarchyMaintainer_DeleteComponentsTests
    {
        [Fact]
        public void DeleteComponents_NothingToDelete_NoDeletesSendToArdoq()
        {
            // Arrange
            var m = new ComponentMapping<Employee>("Employee")
                .WithKey(rg => rg.EmployeeNumber)
                .WithField(e => e.Name, "employee-name")
                .WithField(e => e.Age, "age")
                .WithPreexistingHierarchyReference("Employees");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Employee)] = m
            };

            var employee = new Employee { Name = "Michael Jackson", EmployeeNumber = "XX99", Age = 55 };
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, employee) {PreexistingHierarchyReference = "Employees"}
            };

            var hierarchies = builder.BuildRelationHierarchies(relations);

            var session = Helper.GetSession();
            session.AddComponent("XX99", null, "Employee", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.DeleteComponents(hierarchies);

            // Assert
            Assert.Single(session.GetAllComponents());
            Assert.Equal(0, c);
        }

        [Fact]
        public void DeleteComponents_SuperfluousComponentInArdoq_DeletesComponentInArdoq()
        {
            // Arrange
            var m = new ComponentMapping<Employee>("Employee")
                .WithKey(rg => rg.EmployeeNumber)
                .WithField(e => e.Name, "employee-name")
                .WithField(e => e.Age, "age")
                .WithPreexistingHierarchyReference("Employees");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Employee)] = m
            };

            var employee = new Employee { Name = "Michael Jackson", EmployeeNumber = "XX99", Age = 55 };
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, employee) {PreexistingHierarchyReference = "Employees"}
            };

            var hierarchies = builder.BuildRelationHierarchies(relations);

            var session = Helper.GetSession();
            session.AddComponent("XX99", null, "Employee", null);
            session.AddComponent("non-existing", null, "Employee", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.DeleteComponents(hierarchies);

            // Assert
            Assert.Single(session.GetAllComponents());
            Assert.Equal(1, c);
        }

        [Fact]
        public void DeleteComponents_SuperfluousComponentWithSameNameInArdoq_DeletesComponentInArdoq()
        {
            // Arrange
            const string componentTypeSub = "Subscription";
            const string componentTypeRG = "ResourceGroup";
            const string componentTypeEHN = "EventHubNamespace";
            const string tagString = "tag1";
            Dictionary<Type, IBuiltComponentMapping> mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace);

            var sub = new Subscription { Name = "subName" };
            var rg1 = new ResourceGroup { Name = "rg1Name" };
            var rg2 = new ResourceGroup { Name = "rg2Name" };
            var ehn1 = new EventHubNamespace
            {
                Name = "ehNamespaceName",
                ResourceGroup = rg1,
                Tags = new List<string> { tagString }
            };

            var relations = Helper.CreateRelations((null, sub),(sub, rg1),(sub, rg2),(rg1, ehn1));

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchies = builder.BuildRelationHierarchies(relations);

            var existingTag = new Tag(tagString, null, null) { Components = new List<string> { "tag2" } };
            var session = Helper.GetSession(null, null, new List<Tag> { existingTag });

            session.AddComponent(sub.Name, null, componentTypeSub, null);
            session.AddComponent(rg1.Name, null, componentTypeRG, sub.Name);
            session.AddComponent(rg2.Name, null, componentTypeRG, sub.Name);
            session.AddComponent(ehn1.Name, null, componentTypeEHN, rg1.Name);
            session.AddComponent(ehn1.Name, null, componentTypeEHN, rg2.Name);

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.DeleteComponents(hierarchies);

            // Assert
            Assert.Equal(4, session.GetAllComponents().Count());
            Assert.Equal(1, c);
        }

        [Fact]
        public void DeleteComponents_SuperfluousTreeInArdoq_DeletesComponentInArdoqOnCorrectOrder()
        {
            // Arrange
            var md = new ComponentMapping<Department>("Department")
                .WithKey(rg => rg.Name);
            var me = new ComponentMapping<Employee>("Employee")
                .WithKey(rg => rg.EmployeeNumber)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(e => e.Roles, ModelledReferenceDirection.Parent)
                .WithField(e => e.Name, "employee-name")
                .WithField(e => e.Age, "age");
            var mr = new ComponentMapping<Role>("Role")
                .WithKey(rg => rg.Name);

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Department)] = md,
                [typeof(Employee)] = me,
                [typeof(Role)] = mr,
            };

            var relations = new List<ParentChildRelation> { };

            var hierarchies = new ParentChildRelationHierarchyBuilder(mappings).BuildRelationHierarchies(relations);
            var strickMock = new Mock<IArdoqSession>(MockBehavior.Strict);
            var mockSequence = new MockSequence();

            var compDepartment = new Component("Sony", null, null) { Type = "Department", Id = "depId" };
            strickMock.Setup(s => s.GetComponentsOfType("Department"))
                .Returns(new List<Component> { compDepartment });

            var compEmployee = new Component("XX99", null, null) { Type = "Employee", Parent = compDepartment.Id };
            strickMock.Setup(s => s.GetComponentsOfType("Employee"))
                .Returns(new List<Component> { compEmployee });

            var compRole = new Component("Role132", null, null) { Type = "Role", Parent = compDepartment.Id };
            strickMock.Setup(s => s.GetComponentsOfType("Role"))
                .Returns(new List<Component> { compRole });

            strickMock.InSequence(mockSequence).Setup(s => s.DeleteComponent(compRole));
            strickMock.InSequence(mockSequence).Setup(s => s.DeleteComponent(compEmployee));
            strickMock.InSequence(mockSequence).Setup(s => s.DeleteComponent(compDepartment));

            var maintainer = new ComponentHierarchyMaintainer(mappings, strickMock.Object);

            // Act
            var c = maintainer.DeleteComponents(hierarchies);

            // Assert
            strickMock.VerifyAll();
            Assert.Equal(3, c);
        }

        [Fact]
        public void DeleteComponents_WithRealSession_SessionStateModified()
        {
            // Arrange
            var componentType = "Employee";

            var m = new ComponentMapping<Employee>(componentType)
                .WithKey(rg => rg.EmployeeNumber)
                .WithField(e => e.Name, "employee-name")
                .WithField(e => e.Age, "age")
                .WithPreexistingHierarchyReference("Employees");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Employee)] = m
            };

            var employee = new Employee { Name = "Michael Jackson", EmployeeNumber = "XX99", Age = 55 };
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, employee) {PreexistingHierarchyReference = "Employees"}
            };

            var hierarchies = builder.BuildRelationHierarchies(relations);

            var session = Helper.GetSession();
            session.AddComponent("XX99", null, componentType, null);
            session.AddComponent("non-existing", null, componentType, null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.DeleteComponents(hierarchies);

            // Assert
            Assert.Equal(1, c);
        }

    }
}
