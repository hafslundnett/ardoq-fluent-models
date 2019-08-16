using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ModelMaintainer.Maintainence;
using ModelMaintainer.Mapping;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using ModelMaintainer.Utils;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ComponentHierarchyMaintainer_AddMissingComponentsTests
    {
        [Fact]
        public void AddMissingComponents_SingleMissingComponent_CreatesComponentInArdoq()
        {
            // Arrange
            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithPreexistingHierarchyReference("ResourceGroups");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(ResourceGroup)] = m
            };

            var resourceGroup = new ResourceGroup { Name = "myRG" };
            var relations = new List<ParentChildRelation> { new ParentChildRelation(null, resourceGroup) { PreexistingHierarchyReference = "ResourceGroups" } };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            var comp = session.GetComponents("ResourceGroup", "myRG").Single();
            Assert.NotNull(comp);
            Assert.Equal(1, c);
        }

        [Fact]
        public void AddMissingComponents_WithTags_CreatesComponentWithTagsInArdoq()
        {
            // Arrange
            var m = new ComponentMapping<EventHubNamespace>("EventHubNamespace")
                .WithKey(rg => rg.Name)
                .WithTags(e => e.Tags)
                .WithPreexistingHierarchyReference("EventHubs");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(EventHubNamespace)] = m
            };

            var ehn = new EventHubNamespace() { Name = "events-1", Tags = new List<string> { "v1", "v2" } };
            var relations = new List<ParentChildRelation> { new ParentChildRelation(null, ehn) { PreexistingHierarchyReference = "EventHubs" } };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            var comp = session.GetComponents("EventHubNamespace", "events-1").Single();
            Assert.NotNull(comp);
            Assert.Equal(1, c);
        }

        [Fact]
        public void AddMissingComponents_WithModelledParent_CreatesComponentInArdoq()
        {
            // Arrange
            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithModelledHierarchyReference(rg => rg.Subscription, ModelledReferenceDirection.Child);
            var m2 = new ComponentMapping<Subscription>("Subscription")
                .WithKey(s => s.Name)
                .WithPreexistingHierarchyReference("Subs");
            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(ResourceGroup)] = m,
                [typeof(Subscription)] = m2
            };

            var subscription = new Subscription { Name = "MySub" };
            var resourceGroup = new ResourceGroup { Name = "myRG", Subscription = subscription };
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, subscription) { PreexistingHierarchyReference = "Subs" },
                new ParentChildRelation(subscription, resourceGroup)
            };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var subComponent = new Component("MySub", "workspaceId", null) { Type = "Subscription" };
            var session = Helper.GetSession("workspaceId", new List<Component> { subComponent });
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            var addedComp = session.GetComponents("ResourceGroup", "myRG").Single();
            Assert.Equal(1, c);
        }

        [Fact]
        public void AddMissingComponents_IsNew_ExpectedFieldValuesSentToArdoq()
        {
            // Arrange
            var m = new ComponentMapping<Employee>("Employee")
                .WithKey(e => e.Name)
                .WithField(e => e.EmployeeNumber, "employee-number");
            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Employee)] = m
            };

            var employee = new Employee { Name = "Don Johnsen", EmployeeNumber = "808" };
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, employee) { PreexistingHierarchyReference = "Employees" }
            };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            maintainer.AddMissingComponents(hierarchy);

            // Assert
            var comp = session.GetComponents("Employee", "Don Johnsen").Single();
            Assert.Single(comp.Fields);
            Assert.Equal("808", comp.Fields["employee-number"]);
        }

        [Fact]
        public void AddMissingComponents_ComponentAlreadyExists_DoesNotCreateComponentInArdoq()
        {
            // Arrange
            var m = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithPreexistingHierarchyReference("ResourceGroups");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(ResourceGroup)] = m
            };

            var resourceGroup = new ResourceGroup { Name = "myRG" };
            var relations = new List<ParentChildRelation> { new ParentChildRelation(null, resourceGroup) { PreexistingHierarchyReference = "ResourceGroups" } };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent("myRG", null, "ResourceGroup", null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            Assert.Single(session.GetAllComponents());
            Assert.Equal(0, c);
        }

        [Fact]
        public void AddMissingComponents_MissingComponentWithExistingNameDifferentParent_CreatesComponentInArdoq()
        {
            // Arrange
            const string subscriptionName = "mySub";
            const string rg1Name = "myRG1";
            const string rg2Name = "myRG2";
            const string eventHubNamespaceName = "myEventHubNamespace";

            var sub = new Subscription { Name = subscriptionName };
            var rg1 = new ResourceGroup { Name = rg1Name, Subscription = sub };
            var rg2 = new ResourceGroup { Name = rg2Name, Subscription = sub };
            var ehn1 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg1 };
            var ehn2 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg2 }; // Same name as ehn1

            var relations = Helper.CreateRelations((null, sub), (sub, rg1), (sub, rg2), (rg1, ehn1), (rg2, ehn2));

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace);
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent(subscriptionName, null, "Subscription", null);
            session.AddComponent(rg1Name, null, "ResourceGroup", subscriptionName);
            session.AddComponent(rg2Name, null, "ResourceGroup", subscriptionName);
            session.AddComponent(eventHubNamespaceName, null, "EventHubNamespace", rg1Name);

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            Assert.Equal(1, c);
        }

        [Fact]
        public void AddMissingComponents_MissingComponentWithExistingNameDifferentGrandParent_CreatesComponentInArdoq()
        {
            // Arrange
            const string subscriptionName = "mySub";
            const string rg1Name = "myRG1";
            const string rg2Name = "myRG2";
            const string eventHubNamespaceName = "myEventHubNamespace";
            const string eventHubName = "myEventHub";
            const string workspaceId = "workspaceId";

            var sub = new Subscription { Name = subscriptionName };
            var rg1 = new ResourceGroup { Name = rg1Name, Subscription = sub };
            var rg2 = new ResourceGroup { Name = rg2Name, Subscription = sub };
            var eh1 = new EventHub { Name = eventHubName };
            var eh2 = new EventHub { Name = eventHubName }; // Same name as eh1
            var ehn1 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg1, EventHubs = new List<EventHub> { eh1 } };
            var ehn2 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg2, EventHubs = new List<EventHub> { eh2 } };  // Same name as ehn1

            var relations = Helper.CreateRelations((null, sub), (sub, rg1), (sub, rg2), (rg1, ehn1), (rg2, ehn2), (ehn1, eh1), (ehn2, eh2));

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub);
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var subComp = new Component(subscriptionName, workspaceId, null) { Type = "Subscription", Id = "subId" };
            var rg1Comp = new Component(rg1Name, workspaceId, null) { Type = "ResourceGroup", Id = "rg1Id", Parent = subComp.Id };
            var rg2Comp = new Component(rg2Name, workspaceId, null) { Type = "ResourceGroup", Id = "rg2Id", Parent = subComp.Id };
            var ehn1Comp = new Component(eventHubNamespaceName, workspaceId, null) { Type = "EventHubNamespace", Id = "ehn1Id", Parent = rg1Comp.Id };
            var ehn2Comp = new Component(eventHubNamespaceName, workspaceId, null) { Type = "EventHubNamespace", Id = "ehn2Id", Parent = rg2Comp.Id };
            var ehComp = new Component(eventHubName, workspaceId, null) { Type = "EventHub", Id = "ehId", Parent = ehn1Comp.Id };
            var session = Helper.GetSession(workspaceId, new List<Component> { subComp, rg1Comp, rg2Comp, ehn1Comp, ehn2Comp, ehComp });

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            Assert.Equal(1, c);
        }

        [Fact]
        public void AddMissingComponents_TwoMissingComponentsWithSameNameDifferentGrandParents_CreatesComponentInArdoq()
        {
            // Arrange
            const string subscriptionName = "mySub";
            const string rg1Name = "myRG1";
            const string rg2Name = "myRG2";
            const string eventHubNamespaceName = "myEventHubNamespace";
            const string eventHubName = "myEventHub";

            var sub = new Subscription { Name = subscriptionName };
            var rg1 = new ResourceGroup { Name = rg1Name, Subscription = sub };
            var rg2 = new ResourceGroup { Name = rg2Name, Subscription = sub };
            var eh1 = new EventHub { Name = eventHubName };
            var eh2 = new EventHub { Name = eventHubName }; // Same name as eh1
            var ehn1 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg1, EventHubs = new List<EventHub> { eh1 } };
            var ehn2 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg2, EventHubs = new List<EventHub> { eh2 } };  // Same name as ehn1

            var relations = Helper.CreateRelations((null, sub), (sub, rg1), (sub, rg2), (rg1, ehn1), (rg2, ehn2), (ehn1, eh1), (ehn2, eh2));

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub);
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent(subscriptionName, null, "Subscription", null);
            session.AddComponent(rg1Name, null, "ResourceGroup", subscriptionName);
            session.AddComponent(rg2Name, null, "ResourceGroup", subscriptionName);
            session.AddComponent(eventHubNamespaceName, null, "EventHubNamespace", rg1Name);
            session.AddComponent(eventHubNamespaceName, null, "EventHubNamespace", rg2Name);

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            Assert.Equal(2, c);
        }

        [Fact]
        public void AddMissingComponents_MissingSubtree_CreatesComponentsInArdoq()
        {
            // Arrange
            const string subscriptionName = "mySub";
            const string rg1Name = "myRG1";
            const string rg2Name = "myRG2";
            const string eventHubNamespaceName = "myEventHubNamespace";
            const string eventHubName = "myEventHub";

            var sub = new Subscription { Name = subscriptionName };
            var rg1 = new ResourceGroup { Name = rg1Name, Subscription = sub };
            var rg2 = new ResourceGroup { Name = rg2Name, Subscription = sub };
            var eh1 = new EventHub { Name = eventHubName };
            var eh2 = new EventHub { Name = eventHubName }; // Same name as eh1
            var ehn1 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg1, EventHubs = new List<EventHub> { eh1 } };
            var ehn2 = new EventHubNamespace { Name = eventHubNamespaceName, ResourceGroup = rg2, EventHubs = new List<EventHub> { eh2 } };  // Same name as ehn1

            var relations = Helper.CreateRelations((null, sub), (sub, rg1), (sub, rg2), (rg1, ehn1), (rg2, ehn2), (ehn1, eh1), (ehn2, eh2));

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub);
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent(subscriptionName, null, "Subscription", null);
            session.AddComponent(rg1Name, null, "ResourceGroup", subscriptionName);
            session.AddComponent(eventHubNamespaceName, null, "EventHubNamespace", rg1Name);
            session.AddComponent(eventHubName, null, "EventHub", eventHubNamespaceName);

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.AddMissingComponents(hierarchy);

            // Assert
            Assert.Equal(3, c);
        }
    }
}
