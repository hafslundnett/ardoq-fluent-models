using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Maintainence;
using ArdoqFluentModels.Mapping;
using ArdoqFluentModels.Utils;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ComponentHierarchyMaintainer_UpdateComponentTagsTests
    {
        [Fact]
        public void UpdateComponentTags_HasTagsNonExisting_CreatesTag()
        {
            // Arrange
            var ehNamespaceName = "my-namespace";
            var tagString = "tag1";
            var componentType = "EventHubNamespace";
            var m = new ComponentMapping<EventHubNamespace>(componentType)
                .WithKey(rg => rg.Name)
                .WithTags(e => e.Tags)
                .WithPreexistingHierarchyReference("EventHubs");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(EventHubNamespace)] = m
            };

            var ehn = new EventHubNamespace
            {
                Name = ehNamespaceName,
                Tags = new List<string> { tagString }
            };

            var relations = new List<ParentChildRelation> { new ParentChildRelation(null, ehn) { PreexistingHierarchyReference = "EventHubs" } };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var session = Helper.GetSession();
            session.AddComponent(ehNamespaceName, null, componentType, null);
            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponentTags(hierarchy);

            // Assert
            Assert.Single(session.GetAllTags());
            Assert.Equal(1, c);
        }

        [Fact]
        public void UpdateComponentTags_TagExistsButDoesNotReferenceComponent_UpdatesTag()
        {
            // Arrange
            var ehNamespaceName = "my-namespace";
            var tagString = "tag1";
            var componentType = "EventHubNamespace";
            var m = new ComponentMapping<EventHubNamespace>(componentType)
                .WithKey(rg => rg.Name)
                .WithTags(e => e.Tags)
                .WithPreexistingHierarchyReference("EventHubs");

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(EventHubNamespace)] = m
            };

            var ehn = new EventHubNamespace
            {
                Name = ehNamespaceName,
                Tags = new List<string> { tagString }
            };

            var relations = new List<ParentChildRelation> { new ParentChildRelation(null, ehn) { PreexistingHierarchyReference = "EventHubs" } };

            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var existingTag = new Tag(tagString, null, null) { Components = new List<string> { "tag2" } };
            var session = Helper.GetSession(null, null, new List<Tag> { existingTag });
            session.AddComponent(ehNamespaceName, null, componentType, null);

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponentTags(hierarchy);

            // Assert
            Assert.Equal(2, existingTag.Components.Count);
            Assert.Equal(1, c);
        }

        [Fact]
        public void UpdateComponentTags_TagExistsOnTwoComponentsWithSameName_UpdatesTag()
        {
            // Arrange
            const string componentTypeSub = "Subscription";
            var componentTypeRG = "ResourceGroup";
            var componentTypeEHN = "EventHubNamespace";
            var tagString = "tag1";
            var ehNamespaceName = "my-namespace";
            string workspaceId = Guid.NewGuid().ToString();

            var sub = new Subscription { Name = "subName" };
            var rg1 = new ResourceGroup { Name = "rg1Name" };
            var rg2 = new ResourceGroup { Name = "rg2Name" };
            var ehn1 = new EventHubNamespace
            {
                Name = ehNamespaceName,
                ResourceGroup = rg1,
                Tags = new List<string> { tagString }
            };
            var ehn2 = new EventHubNamespace
            {
                Name = ehNamespaceName,
                ResourceGroup = rg2,
                Tags = new List<string> { tagString }
            };
            List<ParentChildRelation> relations = Helper.CreateRelations((null,sub), (sub, rg1), (sub, rg2), (rg1, ehn1), (rg2, ehn2));

            var mappings = Helper.GetModel(ModelType.Subscription_ResourceGroup_EventHubNamespace);
            var builder = new ParentChildRelationHierarchyBuilder(mappings);
            var hierarchy = builder.BuildRelationHierarchies(relations).First();

            var compSub = new Component(sub.Name, workspaceId, null) { Type = componentTypeSub, Id = "subid" };
            var compRG1 = new Component(rg1.Name, workspaceId, null) { Type = componentTypeRG, Id = "rg1id", Parent = compSub.Id };
            var compRG2 = new Component(rg2.Name, workspaceId, null) { Type = componentTypeRG, Id = "rg2id", Parent = compSub.Id };
            var compEHN1 = new Component(ehNamespaceName, workspaceId, null) { Type = componentTypeEHN, Id = "ehn1componentId", Parent = compRG1.Id };
            var compEHN2 = new Component(ehNamespaceName, workspaceId, null) { Type = componentTypeEHN, Id = "ehn2componentId", Parent = compRG2.Id };
            var existingTag = new Tag(tagString, null, null) { Components = new List<string> { "tag2" } };
            var components = new List<Component> { compSub, compRG1, compRG2, compEHN1, compEHN2 };
            var session = Helper.GetSession(workspaceId, components, new List<Tag> { existingTag });

            var maintainer = new ComponentHierarchyMaintainer(mappings, session);

            // Act
            var c = maintainer.UpdateComponentTags(hierarchy);

            // Assert
            Assert.Equal(3, existingTag.Components.Count);
            Assert.Equal(2, c);
        }
    }
}
