using System;
using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Mapping;

namespace ModelMaintainer.Utils
{
    public class ParentChildRelationHierarchyBuilder
    {
        private readonly Dictionary<Type, IBuiltComponentMapping> _mappingsMap;

        public ParentChildRelationHierarchyBuilder(Dictionary<Type, IBuiltComponentMapping> mappingsMap)
        {
            _mappingsMap = mappingsMap;
        }

        public IEnumerable<ParentChildRelationHierarchy> BuildRelationHierarchies(IEnumerable<ParentChildRelation> rels)
        {
            var relations = rels.ToList();
            var parentChildRelationHierarchies = relations
                .Where(r => r.Parent == null)
                .Select(r =>
                {
                    var hierarchy = new ParentChildRelationHierarchy(r);
                    hierarchy.Expand(relations);
                    return hierarchy;
                });

            foreach (var hierarchy in parentChildRelationHierarchies)
            {
                ComputeUniqueNames(hierarchy);
            }

            return parentChildRelationHierarchies;
        }

        private void ComputeUniqueNames(ParentChildRelationHierarchy hierarchy)
        {
            foreach(var rel in hierarchy.GetAllParentChildRelations())
            {
                var uniqueNames = ComputeUniqueName(hierarchy, rel);
                rel.ParentUniqueName = uniqueNames.Item1;
                rel.ChildUniqueName = uniqueNames.Item2;
            }
        }

        public (string, string) ComputeUniqueName(ParentChildRelationHierarchy hierarchy, ParentChildRelation relation)
        {
            var mapping = _mappingsMap[relation.Child.GetType()];
            var sourceName = mapping.GetKeyForInstance(relation.Child);
            var sourceTypeName = mapping.ArdoqComponentTypeName;
            var localName = sourceName + " " + sourceTypeName;

            if (relation.Parent == null)
            {
                return (null, localName);
            }
            var parentRelations = hierarchy.GetAllParentChildRelations().Where(rel => rel.Child == relation.Parent);
            if (parentRelations.Count() > 1)
            {
                var parentMapping = _mappingsMap[relation.Parent?.GetType()];
                var parentType = parentMapping.ArdoqComponentTypeName;
                var parentName = parentMapping.GetKeyForInstance(relation.Parent);
                throw new Exception($"Multiple elements found matching type={parentType} with key={parentName}");
            }
            ParentChildRelation parentRelation = hierarchy.GetAllParentChildRelations().SingleOrDefault(rel => rel.Child == relation.Parent);
            var parentUniqueName = parentRelation.ChildUniqueName ?? ComputeUniqueName(hierarchy, parentRelation).Item2;

            return (parentUniqueName, parentUniqueName + " -> " + localName);
        }
    }
}
