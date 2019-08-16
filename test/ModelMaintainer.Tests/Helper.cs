using System;
using System.Collections.Generic;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Mapping.ComponentHierarchy;
using ModelMaintainer.Tests.Maintainence.Fakes;
using ModelMaintainer.Tests.Model;

namespace ModelMaintainer.Tests
{
    public static class Helper
    {
        public static IArdoqSession GetSession(string workspaceId = null, List<Component> components = null, List<Tag> tags = null)
        {
            workspaceId = workspaceId ?? Guid.NewGuid().ToString();
            components = components ?? new List<Component>();
            tags = tags ?? new List<Tag>();

            var reader = new FakeArdoqReader(components, tags);
            var writer = new FakeArdoqWriter(components, tags);

            var session = new ArdoqSession(workspaceId, reader, writer);
            return session;
        }

        public static List<ParentChildRelation> CreateRelations(
            ValueTuple<object, object> first,
            ValueTuple<object, object> second,
            ValueTuple<object, object> third)
        {
            return new List<ParentChildRelation>
            {
                new ParentChildRelation(null, first.Item2) { },
                new ParentChildRelation(second.Item1, second.Item2) { },
                new ParentChildRelation(third.Item1, third.Item2) { }
            };
        }

        public static List<ParentChildRelation> CreateRelations(
            ValueTuple<object, object> first,
            ValueTuple<object, object> second,
            ValueTuple<object, object> third,
            ValueTuple<object, object> forth)
        {
            return new List<ParentChildRelation>
            {
                new ParentChildRelation(null, first.Item2) { },
                new ParentChildRelation(second.Item1, second.Item2) { },
                new ParentChildRelation(third.Item1, third.Item2) { },
                new ParentChildRelation(forth.Item1, forth.Item2) { }
            };
        }

        public static List<ParentChildRelation> CreateRelations(
            ValueTuple<object, object> first,
            ValueTuple<object, object> second,
            ValueTuple<object, object> third,
            ValueTuple<object, object> forth,
            ValueTuple<object, object> fifth)
        {
            return new List<ParentChildRelation>
            {
                new ParentChildRelation(null, first.Item2) { },
                new ParentChildRelation(second.Item1, second.Item2) { },
                new ParentChildRelation(third.Item1, third.Item2) { },
                new ParentChildRelation(forth.Item1, forth.Item2) { },
                new ParentChildRelation(fifth.Item1, fifth.Item2) { }
            };
        }

        public static List<ParentChildRelation> CreateRelations(
            ValueTuple<object, object> first,
            ValueTuple<object, object> second,
            ValueTuple<object, object> third,
            ValueTuple<object, object> forth,
            ValueTuple<object, object> fifth,
            ValueTuple<object, object> sixth)
        {
            return new List<ParentChildRelation>
            {
                new ParentChildRelation(null, first.Item2) { },
                new ParentChildRelation(second.Item1, second.Item2) { },
                new ParentChildRelation(third.Item1, third.Item2) { },
                new ParentChildRelation(forth.Item1, forth.Item2) { },
                new ParentChildRelation(fifth.Item1, fifth.Item2) { },
                new ParentChildRelation(sixth.Item1, sixth.Item2) { }
            };
        }

        public static List<ParentChildRelation> CreateRelations(
            ValueTuple<object, object> first,
            ValueTuple<object, object> second,
            ValueTuple<object, object> third,
            ValueTuple<object, object> forth,
            ValueTuple<object, object> fifth,
            ValueTuple<object, object> sixth,
            ValueTuple<object, object> seventh)
        {
            return new List<ParentChildRelation>
            {
                new ParentChildRelation(null, first.Item2) { },
                new ParentChildRelation(second.Item1, second.Item2) { },
                new ParentChildRelation(third.Item1, third.Item2) { },
                new ParentChildRelation(forth.Item1, forth.Item2) { },
                new ParentChildRelation(fifth.Item1, fifth.Item2) { },
                new ParentChildRelation(sixth.Item1, sixth.Item2) { },
                new ParentChildRelation(seventh.Item1, seventh.Item2) { }
            };
        }

        public static Dictionary<Type, IBuiltComponentMapping> GetModel(ModelType model)
        {
            switch(model)
            {
                case ModelType.Subscription_ResourceGroup_EventHubNamespace:
                    return GetSub_RG_EHN_Model();
                case ModelType.Subscription_ResourceGroup_EventHubNamespace_EventHub:
                    return GetSub_RG_EHN_EH_Model();
            }
            throw new Exception("Unsupported model");
        }

        private static Dictionary<Type, IBuiltComponentMapping> GetSub_RG_EHN_Model()
        {
            var componentMappingSubscription = new ComponentMapping<Subscription>("Subscription")
                            .WithKey(s => s.Name);
            var componentMappingResourceGroup = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithModelledHierarchyReference(rg => rg.Subscription, ModelledReferenceDirection.Child);
            var componentMappingEventHubNamespace = new ComponentMapping<EventHubNamespace>("EventHubNamespace")
                .WithKey(ehn => ehn.Name)
                .WithTags(e => e.Tags)
                .WithModelledHierarchyReference(ehn => ehn.ResourceGroup, ModelledReferenceDirection.Child);

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Subscription)] = componentMappingSubscription,
                [typeof(ResourceGroup)] = componentMappingResourceGroup,
                [typeof(EventHubNamespace)] = componentMappingEventHubNamespace
            };
            return mappings;
        }

        private static Dictionary<Type, IBuiltComponentMapping> GetSub_RG_EHN_EH_Model()
        {
            var cmSub = new ComponentMapping<Subscription>("Subscription")
                            .WithKey(s => s.Name);
            var cmRG = new ComponentMapping<ResourceGroup>("ResourceGroup")
                .WithKey(rg => rg.Name)
                .WithModelledHierarchyReference(rg => rg.Subscription, ModelledReferenceDirection.Child);
            var cmEHN = new ComponentMapping<EventHubNamespace>("EventHubNamespace")
                .WithKey(ehn => ehn.Name)
                .WithTags(e => e.Tags)
                .WithModelledHierarchyReference(ehn => ehn.ResourceGroup, ModelledReferenceDirection.Child)
                .WithModelledHierarchyReference(ehn => ehn.EventHubs, ModelledReferenceDirection.Parent);
            var cmEH = new ComponentMapping<EventHub>("EventHub")
                .WithKey(eh => eh.Name);

            var mappings = new Dictionary<Type, IBuiltComponentMapping>
            {
                [typeof(Subscription)] = cmSub,
                [typeof(ResourceGroup)] = cmRG,
                [typeof(EventHubNamespace)] = cmEHN,
                [typeof(EventHub)] = cmEH
            };
            return mappings;
        }
    }

    public enum ModelType
    {
        Subscription_ResourceGroup_EventHubNamespace,
        Subscription_ResourceGroup_EventHubNamespace_EventHub,
    }
}
