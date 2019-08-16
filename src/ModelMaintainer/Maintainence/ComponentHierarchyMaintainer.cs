using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;
using ModelMaintainer.Utils;

namespace ModelMaintainer.Maintainence
{
    public class ComponentHierarchyMaintainer
    {
        private readonly IDictionary<Type, IBuiltComponentMapping> _mappings;
        private readonly IArdoqSession _session;

        public ComponentHierarchyMaintainer(IDictionary<Type, IBuiltComponentMapping> mappings, IArdoqSession session)
        {
            _mappings = mappings;
            _session = session;
        }

        public int AddMissingComponents(ParentChildRelationHierarchy hierarchy)
        {
            var result = 0;
            foreach (var relation in hierarchy.GetLevel(0))
            {
                if (AddComponent(relation))
                {
                    result++;
                }
            }

            if (hierarchy.LevelCount == 1)
            {
                return result;
            }

            for (int i = 1; i < hierarchy.LevelCount; i++)
            {
                var level = hierarchy.GetLevel(i);
                foreach (var relation in level)
                {
                    if (AddComponent(relation))
                    {
                        result++;
                    }
                }
            }

            return result;
        }

        public int DeleteComponents(IEnumerable<ParentChildRelationHierarchy> hierarchies)
        {
            var allRelations = hierarchies.Aggregate(
                new List<ParentChildRelation>(),
                (accum, h) =>
                {
                    accum.AddRange(h.GetAllParentChildRelations());
                    return accum;
                });

            var deletedCount = 0;
            foreach (var builtComponentMapping in MappingsReverseSorted())
            {
                var relations = allRelations.Where(o => o.Child.GetType() == builtComponentMapping.Key);
                var components = _session.GetComponentsOfType(builtComponentMapping.Value.ArdoqComponentTypeName).ToList();
                // Call ToList() to make components a copy, because the underlying construct in _session will mutate if any element is deleted.
                foreach (var component in components)
                {
                    bool componentMissingInSource = !relations.Any(relation =>
                        builtComponentMapping.Value.GetKeyForInstance(relation.Child) == component.Name
                        && GetParent(relation)?.Id == component.Parent);
                    if (componentMissingInSource)
                    {
                        _session.DeleteComponent(component);
                        deletedCount++;
                    }
                }
            }

            return deletedCount;
        }

        private IEnumerable<KeyValuePair<Type, IBuiltComponentMapping>> MappingsReverseSorted()
        {
            return _mappings.Reverse();
        }

        public int UpdateComponents(ParentChildRelationHierarchy hierarchy)
        {
            var updatedCount = 0;

            foreach (var relation in hierarchy.GetAllParentChildRelations())
            {
                var sourceObject = relation.Child;
                var mapping = _mappings[sourceObject.GetType()];

                var tuple = mapping.GetComponentInfo(sourceObject);
                var component = _session.GetChildComponent(relation);
                if (!HasChanges(component, tuple.Item2))
                {
                    continue;
                }

                component.Fields.Clear();

                foreach (var pair in tuple.Item2)
                {
                    component.Fields.Add(pair.Key, pair.Value);
                }

                _session.UpdateComponent(component);
                updatedCount++;
            }

            return updatedCount;
        }

        public int UpdateComponentTags(ParentChildRelationHierarchy hierarchy)
        {
            var updatedCount = 0;

            var tpl = GetComponentTagMap();
            var componentTagMap = tpl.Item2;

            var componentMap = new Dictionary<Type, List<Component>>();
            foreach (var relation in hierarchy.GetAllParentChildRelations())
            {
                var sourceObject = relation.Child;
                var mapping = _mappings[sourceObject.GetType()];
                var tuple = mapping.GetComponentInfo(sourceObject);
                var component = _session.GetChildComponent(relation);
                var tags = mapping.GetTags(sourceObject);

                if ((tags == null || !tags.Any()) && !componentTagMap.ContainsKey(component.Id))
                {
                    continue;
                }

                updatedCount += MaintainTagsForComponent(tags, component, _session.GetAllTags());
            }

            return updatedCount;
        }

        private int MaintainTagsForComponent(IEnumerable<string> tagString, Component component, IEnumerable<Tag> tags)
        {
            var updatedCount = 0;

            foreach (var ts in tagString)
            {
                var existingTag = tags.SingleOrDefault(t => t.Name == ts);
                if (existingTag == null)
                {
                    _session.CreateTag(ts, component);
                    updatedCount++;
                }
                else
                {
                    if (!existingTag.Components.Contains(component.Id))
                    {
                        _session.AddComponentToTag(existingTag, component);
                        updatedCount++;
                    }
                }
            }

            return updatedCount;
        }

        private (IEnumerable<Tag>, Dictionary<string, List<Tag>>) GetComponentTagMap()
        {
            var tags = _session.GetAllTags();
            var tagMap = tags.Aggregate(
                new Dictionary<string, List<Tag>>(),
                (map, t) =>
                {
                    if (t.Components != null)
                    {
                        foreach (var compId in t.Components)
                        {
                            List<Tag> tList;
                            if (map.ContainsKey(compId))
                            {
                                tList = map[compId];
                            }
                            else
                            {
                                tList = new List<Tag>();
                                map[compId] = tList;
                            }

                            tList.Add(t);
                        }
                    }

                    return map;
                });

            return (tags, tagMap);
        }

        private bool HasChanges(Component component, IDictionary<string, object> valueMap)
        {
            foreach (var pair in valueMap)
            {
                if (!component.Fields.ContainsKey(pair.Key))
                {
                    return true;
                }

                if (!pair.Value.Equals(component.Fields[pair.Key]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AddComponent(ParentChildRelation relation)
        {
            if (ComponentExists(relation))
            {
                return false;
            }

            var mapping = _mappings[relation.Child.GetType()];
            var key = mapping.GetKeyForInstance(relation.Child);

            if (relation.Parent == null)
            {
                _session.AddComponent(
                    key,
                    mapping.GetComponentInfo(relation.Child).Item2,
                    mapping.ArdoqComponentTypeName,
                    relation.PreexistingHierarchyReference);
            }
            else
            {
                var parentMapping = _mappings[relation.Parent.GetType()];
                var parentKey = parentMapping.GetKeyForInstance(relation.Parent);
                var existingParentComponent = GetParent(relation);
                _session.AddComponentWithParent(
                    key,
                    mapping.GetComponentInfo(relation.Child).Item2,
                    mapping.ArdoqComponentTypeName,
                    existingParentComponent);
            }

            return true;
        }

        private bool ComponentExists(ParentChildRelation relation)
        {
            return _session.GetChildComponent(relation) != null;
        }

        private Component GetParent(ParentChildRelation relation)
        {
            if (relation.Parent == null)
            {
                return null;
            }

            var parent = _session.GetParentComponent(relation);
            return parent;
        }
    }
}
