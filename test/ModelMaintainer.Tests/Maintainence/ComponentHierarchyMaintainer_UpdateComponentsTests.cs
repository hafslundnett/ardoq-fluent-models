using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ModelMaintainer.Maintainence;
using ModelMaintainer.Mapping;
using ModelMaintainer.Tests.Model;
using ModelMaintainer.Utils;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ComponentHierarchyMaintainer_UpdateComponentsTests
    {
        [Fact]
        public void UpdateComponents_HasChanges_UpdatesComponentInArdoq()
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

            var employee = new Employee {Name = "Michael Jackson", EmployeeNumber = "XX99", Age = 55};
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, employee) {PreexistingHierarchyReference = "Employees"}
            };

            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent("XX99", null, "Employee", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponents(hierarchy);

            // Assert
            Assert.Equal(1, c);
            Component updatedComponent = session.GetAllComponents().Single();
            Assert.Equal("Michael Jackson", updatedComponent.Fields["employee-name"]);
            Assert.Equal(55, updatedComponent.Fields["age"]);
        }

        [Fact]
        public void UpdateComponents_HasSmallChange_UpdatesComponentInArdoq()
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

            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            var fields = new Dictionary<string, object>
            {
                ["employee-name"] = "Michael Jackson",
                ["age"] = "54"
            };
            session.AddComponent("XX99", fields, "Employee", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponents(hierarchy);

            // Assert
            Assert.Equal(1, c);
            Component updatedComponent = session.GetAllComponents().Single();
            Assert.Equal("Michael Jackson", updatedComponent.Fields["employee-name"]);
            Assert.Equal(55, updatedComponent.Fields["age"]);
        }

        [Fact]
        public void UpdateComponents_NoChanges_NoUpdatesToArdoq()
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

            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            var fields = new Dictionary<string, object>
            {
                ["employee-name"] = "Michael Jackson",
                ["age"] = 55
            };
            session.AddComponent("XX99", fields, "Employee", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponents(hierarchy);

            // Assert
            Assert.Equal(0, c);
        }

        [Fact]
        public void UpdateComponents_TwoComponentsWithSameName_NoChanges_NoUpdatesToArdoq()
        {
            // Arrange
            const string workspaceId = "workspaceId";
            var d = new ComponentMapping<Department>("Department")
                .WithKey(dd => dd.Name);

            var m = new ComponentMapping<Employee>("Employee")
                .WithKey(rg => rg.EmployeeNumber)
                .WithField(e => e.Name, "employee-name")
                .WithField(e => e.Age, "age")
                .WithPreexistingHierarchyReference("Employees");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Department)] = d,
                [typeof(Employee)] = m
            };

            var department1 = new Department { Name = "Sony Music" };
            var department2 = new Department { Name = "MTV" };
            var employee = new Employee { Name = "Michael Jackson", EmployeeNumber = "XX99", Age = 55 };
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, department1) {},
                new ParentChildRelation(department1, employee) {},
                new ParentChildRelation(department2, employee) {}
            };

            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var compDep1 = new Component("Sony Music", workspaceId, null) { Id = "parentId1", Type = "Department" };
            var compDep2 = new Component("MTV", workspaceId, null){ Id = "parentId2", Type = "Department" };
            var comp1 = new Component("XX99", workspaceId, null)
            {
                Id = "id1",
                Parent = compDep1.Id,
                Type = "Employee",
                Fields = new Dictionary<string, object>
                {
                    ["employee-name"] = "Michael Jackson",
                    ["age"] = 55
                }
            };
            var comp2 = new Component("XX99", workspaceId, null)
            {
                Id = "id2",
                Parent = compDep2.Id,
                Type = "Employee",
                Fields = new Dictionary<string, object>
                {
                    ["employee-name"] = "Michael Jackson",
                    ["age"] = 55
                }
            };

            var session = Helper.GetSession(workspaceId, new List<Component> { compDep1, compDep2, comp1, comp2 });
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponents(hierarchy);

            // Assert
            Assert.Equal(0, c);
        }
    }
}
