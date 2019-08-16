using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Tests.Model;
using ModelMaintainer.Utils;
using Xunit;

namespace ModelMaintainer.Tests.Utils
{
    public class ParentChildRelationHierarchyBuilderTests
    {
        [Fact]
        public void BuildRelationHierarchies_SingleRelation_ReturnsExpectedHierarchy()
        {
            // Arrange
            var relation = new ParentChildRelation(null, new Subscription { Name = "sub1" });
            var relations = new List<ParentChildRelation>{relation};

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace));

            // Act
            var hiers = builder.BuildRelationHierarchies(relations);

            // Assert
            Assert.Single(hiers);
            Assert.Equal(1, hiers.First().LevelCount);
        }

        [Fact]
        public void BuildRelationHierarchies_SeveralSingleRelation_ReturnsExpectedHierarchies()
        {
            // Arrange
            var relations = new List<ParentChildRelation>
            {
                new ParentChildRelation(null, new Subscription{Name = "sub1" }),
                new ParentChildRelation(null, new Subscription{Name = "sub2" }),
                new ParentChildRelation(null, new Subscription{Name = "sub3" }),
                new ParentChildRelation(null, new Subscription{Name = "sub4" }),
                new ParentChildRelation(null, new Subscription{Name = "sub5" })
            };

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub));

            // Act
            var hiers = builder.BuildRelationHierarchies(relations);

            // Assert
            Assert.Equal(5, hiers.Count());
        }

        [Fact]
        public void BuildRelationHierarchies_SingleLinearRelations_ReturnsExpectedHierarchies()
        {
            // Arrange
            var subscription = new Subscription {Name = "NETT-PLAYGROUND"};
            var subscriptionRelation = new ParentChildRelation(null, subscription);
            var resourceGroup = new ResourceGroup {Name = "PLAYGROUND-RGprod"};
            var resourceGroupRelation = new ParentChildRelation(subscription, resourceGroup);
            var relations = new List<ParentChildRelation>
            {
                subscriptionRelation,
                resourceGroupRelation
            };

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub));

            // Act
            var hiers = builder.BuildRelationHierarchies(relations);

            // Assert
            Assert.Single(hiers);
            var h = hiers.First();
            Assert.Equal(2, h.LevelCount);
            var level = h.GetLevel(0);
            Assert.Single(level);
            Assert.Contains(subscriptionRelation, level);
            level = h.GetLevel(1);
            Assert.Contains(resourceGroupRelation, level);
        }

        [Fact]
        public void BuildRelationHierarchies_DeepLinearReleations_ReturnsExpectedHierarchies()
        {
            // Arrange
            var subscription = new Subscription { Name = "NETT-PLAYGROUND" };
            var subscriptionRelation = new ParentChildRelation(null, subscription);
            var resourceGroup = new ResourceGroup { Name = "PLAYGROUND-RGprod" };
            var resourceGroupRelation = new ParentChildRelation(subscription, resourceGroup);
            var ehNamespace = new EventHubNamespace {Name = "my-namespace"};
            var ehNamespaceReleation = new ParentChildRelation(resourceGroup, ehNamespace);
            var eventHub = new EventHub {Name = "my-events"};
            var eventHubRelation = new ParentChildRelation(ehNamespace, eventHub);
            var relations = new List<ParentChildRelation>
            {
                subscriptionRelation,
                resourceGroupRelation,
                ehNamespaceReleation,
                eventHubRelation
            };

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub));

            // Act
            var hiers = builder.BuildRelationHierarchies(relations);

            // Assert
            Assert.Single(hiers);
            var h = hiers.First();
            Assert.Equal(4, h.LevelCount);
            var level = h.GetLevel(0);
            Assert.Single(level);
            Assert.Contains(subscriptionRelation, level);
            level = h.GetLevel(1);
            Assert.Contains(resourceGroupRelation, level);
            level = h.GetLevel(2);
            Assert.Contains(ehNamespaceReleation, level);
            level = h.GetLevel(3);
            Assert.Contains(eventHubRelation, level);
        }


        [Fact]
        public void BuildRelationHierarchies_BroadAndParalellHierachies_ReturnsExpectedHierarchies()
        {
            // Arrange
            var subscription = new Subscription { Name = "NETT-PLAYGROUND" };
            var subscriptionRelation = new ParentChildRelation(null, subscription);
            var resourceGroup = new ResourceGroup { Name = "PLAYGROUND-RGprod" };
            var resourceGroupRelation = new ParentChildRelation(subscription, resourceGroup);
            var ehNamespace = new EventHubNamespace { Name = "my-namespace" };
            var ehNamespaceReleation = new ParentChildRelation(resourceGroup, ehNamespace);
            var eventHub = new EventHub { Name = "my-events" };
            var eventHubRelation = new ParentChildRelation(ehNamespace, eventHub);

            var sub2 = new Subscription{Name = "NETT-MDM"};
            var sub2R = new ParentChildRelation(null, sub2);
            var rg2 = new ResourceGroup {Name = "MDM-RGtest"};
            var rg2R = new ParentChildRelation(sub2, rg2);
            var ehn1 = new EventHubNamespace{Name = "mdm-1"};
            var ehn1R = new ParentChildRelation(rg2, ehn1);
            var ehn2 = new EventHubNamespace { Name = "mdm-2" };
            var ehn2R = new ParentChildRelation(rg2, ehn2);

            var eh1 = new EventHub {Name = "eh1"};
            var eh1R = new ParentChildRelation(ehn1, eh1);

            var eh2 = new EventHub { Name = "eh2" };
            var eh2R = new ParentChildRelation(ehn2, eh2);

            var relations = new List<ParentChildRelation>
            {
                subscriptionRelation,
                resourceGroupRelation,
                ehNamespaceReleation,
                eventHubRelation,
                sub2R,
                rg2R,
                ehn1R,
                ehn2R,
                eh1R,
                eh2R
            };

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub));

            // Act
            var hiers = builder.BuildRelationHierarchies(relations).ToList();

            // Assert
            Assert.Equal(2, hiers.Count());

            var hier = hiers.Single(h => ((Subscription) h.GetLevel(0).First().Child).Name == "NETT-PLAYGROUND");
            Assert.Equal(4, hier.LevelCount);
            var level = hier.GetLevel(0);
            Assert.Single(level);
            Assert.Contains(subscriptionRelation, level);
            level = hier.GetLevel(1);
            Assert.Contains(resourceGroupRelation, level);
            level = hier.GetLevel(2);
            Assert.Contains(ehNamespaceReleation, level);
            level = hier.GetLevel(3);
            Assert.Contains(eventHubRelation, level);

            hier = hiers.Single(h => ((Subscription)h.GetLevel(0).First().Child).Name == "NETT-MDM");
            level = hier.GetLevel(0);
            Assert.Single(level);
            Assert.Contains(sub2R, level);

            level = hier.GetLevel(1);
            Assert.Contains(rg2R, level);

            level = hier.GetLevel(2);
            Assert.Equal(2, level.Count);
            Assert.Contains(ehn1R, level);
            Assert.Contains(ehn2R, level);

            level = hier.GetLevel(3);
            Assert.Equal(2, level.Count);
            Assert.Contains(eh1R, level);
            Assert.Contains(eh2R, level);
        }

        [Fact]
        public void GetAllChildren_ForBuiltHierarchy_GetsExpectedChildren()
        {
            // Arrange
            var subscription = new Subscription { Name = "NETT-PLAYGROUND" };
            var subscriptionRelation = new ParentChildRelation(null, subscription);
            var resourceGroup = new ResourceGroup { Name = "PLAYGROUND-RGprod" };
            var resourceGroupRelation = new ParentChildRelation(subscription, resourceGroup);
            var ehNamespace = new EventHubNamespace { Name = "my-namespace" };
            var ehNamespaceReleation = new ParentChildRelation(resourceGroup, ehNamespace);
            var eventHub = new EventHub { Name = "my-events" };
            var eventHubRelation = new ParentChildRelation(ehNamespace, eventHub);
            var relations = new List<ParentChildRelation>
            {
                subscriptionRelation,
                resourceGroupRelation,
                ehNamespaceReleation,
                eventHubRelation
            };

            var builder = new ParentChildRelationHierarchyBuilder(Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub));
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            // Act
            var children = hierarchy.GetAllChildren().ToList();

            // Assert
            Assert.Equal(4, children.Count());
            Assert.Contains(subscription, children);
            Assert.Contains(resourceGroup, children);
            Assert.Contains(ehNamespace, children);
            Assert.Contains(eventHub, children);
        }
    }
}
