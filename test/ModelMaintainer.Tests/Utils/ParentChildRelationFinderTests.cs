using System;
using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using ModelMaintainer.Utils;
using Xunit;

namespace ModelMaintainer.Tests.Utils
{
    public class ParentChildRelationFinderTests
    {
        [Fact]
        public void ParentChildRelationFinder_NoMappingForType_ThrowsInvalidOperationException()
        {
            // Arrange
            var finder = new ParentChildRelationFinder(new Dictionary<Type, IBuiltComponentMapping>());
            var objs = new List<object>{new ResourceGroup{Name = "MDM-RGdev"}};

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => finder.FindRelations(objs));
            Assert.Equal("Type ModelMaintainer.Tests.Model.ResourceGroup not registered.", ex.Message);
        }

        [Fact]
        public void ParentChildRelationFinder_NoHierarchy_ReturnsParentlessRelations()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);
            var m1 = builder.AddComponentMapping<Subscription>("Subscription");
            var m2 = builder.AddComponentMapping<ResourceGroup>("ResourceGroup");
            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Subscription)] = m1,
                [typeof(ResourceGroup)] = m2
            };

            var o1 = new Subscription {Name = "NETT-MDM"};
            var o2 = new ResourceGroup {Name = "MDM-RGdev"};
            var o3 = new ResourceGroup { Name = "MDM-RGtest" };
            var o4 = new ResourceGroup { Name = "MDM-RGprod" };
            var objs = new List<object> { o1, o2, o3, o4 };

            var finder = new ParentChildRelationFinder(mappings);

            // Act
            var relations = finder.FindRelations(objs).ToList();

            // Assert
            Assert.Equal(4, relations.Count());
            Assert.Contains(relations, r => r.Child == o1);
            Assert.Contains(relations, r => r.Child == o2);
            Assert.Contains(relations, r => r.Child == o3);
            Assert.Contains(relations, r => r.Child == o4);
        }

        [Fact]
        public void ParentChildRelationFinder_SingleObjectWithPreexistingHierarchyReference_RelationHasPreexistingHierarchyReference()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);
            var m2 = builder.AddComponentMapping<ResourceGroup>("ResourceGroup");
            m2.WithPreexistingHierarchyReference("ResourceGroups");
            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(ResourceGroup)] = m2
            };

            var objs = new List<object> { new ResourceGroup { Name = "MDM-RGprod" } };

            var finder = new ParentChildRelationFinder(mappings);

            // Act
            var relations = finder.FindRelations(objs).ToList();

            // Assert
            Assert.Single(relations);
            var r = relations.Single();
            Assert.Equal("ResourceGroups", r.PreexistingHierarchyReference);
        } 

        [Fact]
        public void ParentChildRelationFinder_SmallHierarchy_ReturnsParentlessRelations()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);
            var m1 = builder.AddComponentMapping<Subscription>("Subscription");
            var m2 = builder.AddComponentMapping<ResourceGroup>("ResourceGroup")
                .WithModelledHierarchyReference(rg => rg.Subscription, ModelledReferenceDirection.Child);

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Subscription)] = m1,
                [typeof(ResourceGroup)] = m2
            };

            var sub = new Subscription { Name = "NETT-MDM" };
            var resGroup = new ResourceGroup { Name = "MDM-RGdev", Subscription = sub};
            var objs = new List<object> { sub, resGroup };

            var finder = new ParentChildRelationFinder(mappings);

            // Act
            var relations = finder.FindRelations(objs).ToList();

            // Assert
            Assert.Equal(2, relations.Count);
            Assert.Contains(relations, r => r.Child == sub && r.Parent == null);
            Assert.Contains(relations, r => r.Child == resGroup && r.Parent == sub);
        }

        [Fact]
        public void ParentChildRelationFinder_ComplexHierarchy_ReturnsParentlessRelations()
        {
            // Arrange
            var sub = new Subscription { Name = "NETT-MDM" };
            var rgDev = new ResourceGroup { Name = "MDM-RGdev", Subscription = sub };
            var rgTest = new ResourceGroup { Name = "MDM-RGtest", Subscription = sub };
            var eXdev = new EventHub {Name = "x-dev"};
            var eYdev = new EventHub { Name = "y-dev" };
            var nsDev = new EventHubNamespace { Name = "dev", ResourceGroup = rgDev, EventHubs = new List<EventHub>{eXdev, eYdev}};
            var eXtest = new EventHub { Name = "x-test" };
            var eYtest = new EventHub { Name = "y-test" };
            var nsTest = new EventHubNamespace { Name = "test", ResourceGroup = rgTest, EventHubs = new List<EventHub> { eXtest, eYtest } };
            var objs = new List<object> { sub, rgDev, rgTest, eXdev, eYdev, nsDev, eXtest, eYtest, nsTest };

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub);
            var finder = new ParentChildRelationFinder(mappings);

            // Act
            var relations = finder.FindRelations(objs).ToList();

            // Assert
            Assert.Equal(9, relations.Count);
            Assert.Contains(relations, r => r.Child == sub && r.Parent == null);
            Assert.Contains(relations, r => r.Child == rgDev && r.Parent == sub);
            Assert.Contains(relations, r => r.Child == rgTest && r.Parent == sub);

            Assert.Contains(relations, r => r.Child == nsDev && r.Parent == rgDev);
            Assert.Contains(relations, r => r.Child == eXdev && r.Parent == nsDev);
            Assert.Contains(relations, r => r.Child == eYdev && r.Parent == nsDev);

            Assert.Contains(relations, r => r.Child == nsTest && r.Parent == rgTest);
            Assert.Contains(relations, r => r.Child == eXtest && r.Parent == nsTest);
            Assert.Contains(relations, r => r.Child == eYtest && r.Parent == nsTest);
        }

        [Fact]
        public void FindRelations_ThenBuildHierarchy_BuildsExpectedHiearchy()
        {
            // Arrange
            var sub = new Subscription { Name = "NETT-MDM" };
            var rgDev = new ResourceGroup { Name = "MDM-RGdev", Subscription = sub };
            var rgTest = new ResourceGroup { Name = "MDM-RGtest", Subscription = sub };
            var eXdev = new EventHub { Name = "x-dev" };
            var eYdev = new EventHub { Name = "y-dev" };
            var nsDev = new EventHubNamespace { Name = "dev", ResourceGroup = rgDev, EventHubs = new List<EventHub> { eXdev, eYdev } };
            var eXtest = new EventHub { Name = "x-test" };
            var eYtest = new EventHub { Name = "y-test" };
            var nsTest = new EventHubNamespace { Name = "test", ResourceGroup = rgTest, EventHubs = new List<EventHub> { eXtest, eYtest } };
            var nakedEventHub = new EventHub {Name = "naked"};
            var objs = new List<object> { sub, rgDev, rgTest, eXdev, eYdev, nsDev, eXtest, eYtest, nsTest, nakedEventHub };

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub);
            var finder = new ParentChildRelationFinder(mappings);
            var hierarchyBuilder = new ParentChildRelationHierarchyBuilder(mappings);

            // Act
            var relations = finder.FindRelations(objs);
            var hierarchies = hierarchyBuilder.BuildRelationHierarchies(relations).ToList();

            // Assert
            Assert.Equal(2, hierarchies.Count);
            var h = hierarchies.Single(hi => hi.LevelCount == 4);
            Assert.Single(h.GetLevel(0));
            Assert.Contains(h.GetLevel(0), r => r.Parent == null && r.Child == sub);

            Assert.Equal(2, h.GetLevel(1).Count);
            Assert.Contains(h.GetLevel(1), r => r.Parent == sub && r.Child == rgDev);
            Assert.Contains(h.GetLevel(1), r => r.Parent == sub && r.Child == rgTest);

            Assert.Equal(2, h.GetLevel(2).Count);
            Assert.Contains(h.GetLevel(2), r => r.Parent == rgTest && r.Child == nsTest);
            Assert.Contains(h.GetLevel(2), r => r.Parent == rgDev && r.Child == nsDev);

            Assert.Equal(4, h.GetLevel(3).Count);
            Assert.Contains(h.GetLevel(3), r => r.Parent == nsTest && r.Child == eXtest);
            Assert.Contains(h.GetLevel(3), r => r.Parent == nsTest && r.Child == eYtest);
            Assert.Contains(h.GetLevel(3), r => r.Parent == nsDev && r.Child == eXdev);
            Assert.Contains(h.GetLevel(3), r => r.Parent == nsDev && r.Child == eYdev);

            h = hierarchies.Single(hi => hi.LevelCount == 1);
            Assert.Single(h.GetLevel(0));
            Assert.Contains(h.GetLevel(0), r => r.Parent == null && r.Child == nakedEventHub);
        }

        [Fact]
        public void FindRelations_DeclaredReferenceButNullReference_ReturnsChildReference()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null);
            var m1 = builder.AddComponentMapping<Department>("Department")
                .WithKey(d => d.Name);
            var m2 = builder.AddComponentMapping<Employee>("Employee")
                .WithKey(e => e.EmployeeNumber)
                .WithModelledHierarchyReference(e => e.EmployedIn, ModelledReferenceDirection.Child);

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Department)] = m1,
                [typeof(Employee)] = m2
            };

            var employee = new Employee {Name = "Bjørn Borg"};
            var objs = new List<object> { employee };

            var finder = new ParentChildRelationFinder(mappings);

            // Act
            var relations = finder.FindRelations(objs).ToList();

            // Assert
            Assert.Single(relations);
            var r = relations.First();
            Assert.Null(r.Parent);
            Assert.Same(employee, r.Child);
        }
    }
}
