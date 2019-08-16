using System;
using System.Collections.Generic;
using System.Linq;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Mapping.ComponentHierarchy
{
    public class ModelledParentAccessSpecificationTests
    {
        [Fact]
        public void GetRelatedObjects_HasSingleReference_ReturnsSingleExpectedRelation()
        {
            // Arrange
            var spec = new ModelledReferenceAccessSpecification<ResourceGroup>(
                rg => rg.Subscription,
                ModelledReferenceDirection.Child);

            var subscription = new Subscription {Name = "my-subscription"};
            var resourceGroup = new ResourceGroup {Name = "my-rg", Subscription = subscription};

            // Act
            var relations = spec.GetRelatedObjects(resourceGroup);

            // Assert
            Assert.Single(relations);
            var r = relations.First();
            Assert.Equal(ModelledReferenceDirection.Child, r.Item2);
            Assert.Same(subscription, r.Item1);
        }

        [Fact]
        public void GetRelatedObjects_HasListReference_ReturnsExpectedRelations()
        {
            // Arrange
            var spec = new ModelledReferenceAccessSpecification<EventHubNamespace>(
                rg => rg.EventHubs,
                ModelledReferenceDirection.Parent);

            var e1 = new EventHub {Name = "e1"};
            var e2 = new EventHub {Name = "e2"};
            var ehn = new EventHubNamespace
            {
                Name = "ehn",
                EventHubs = new List<EventHub>{e1,e2}
            };

            // Act
            var relations = spec.GetRelatedObjects(ehn).ToList();

            // Assert
            Assert.Equal(2, relations.Count());
            Assert.True(relations.All(t => t.Item2 == ModelledReferenceDirection.Parent));
            Assert.Contains(relations, r => r.Item1 == e1);
            Assert.Contains(relations, r => r.Item1 == e2);
        }
    }
}
