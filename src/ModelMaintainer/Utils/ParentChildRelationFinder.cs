using ArdoqFluentModels.Mapping;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArdoqFluentModels.Utils
{
    public class ParentChildRelationFinder
    {
        private readonly IDictionary<Type, IBuiltComponentMapping> _mappings;

        public ParentChildRelationFinder(IDictionary<Type, IBuiltComponentMapping> mappings)
        {
            _mappings = mappings;
        }

        public IEnumerable<ParentChildRelation> FindRelations(IEnumerable<object> objs)
        {
            var objects = objs.ToList();
            var result = objects.Aggregate(
                new List<ParentChildRelation>(),
                (accum, obj) =>
                {
                    accum.AddRange(FindRelationsCore(obj));
                    return accum;
                });

            foreach (var obj in objects)
            {
                if (result.Any(r => r.Child == obj && r.Parent == null) &&
                    result.Any(r => r.Child == obj && r.Parent != null))
                {
                    result.RemoveAll(r => r.Child == obj && r.Parent == null);
                }
            }

            return result;
        }

        private List<ParentChildRelation> FindRelationsCore(object obj)
        {
            if (!_mappings.ContainsKey(obj.GetType()))
            {
                throw new InvalidOperationException($"Type {obj.GetType().FullName} not registered.");
            }

            var result = _mappings[obj.GetType()].ModelledHierarchyAccessSpecifications
                .Aggregate(
                    new List<ParentChildRelation>(),
                    (list, accessor) =>
                    {
                        var relatedObjects = accessor.GetRelatedObjects(obj);
                        if (relatedObjects == null)
                        {
                            list.Add(new ParentChildRelation(null, obj));
                        }
                        else
                        {
                            foreach (var tuple in relatedObjects)
                            {
                                var r = tuple.Item2 == ModelledReferenceDirection.Child
                                    ? new ParentChildRelation(tuple.Item1, obj)
                                    : new ParentChildRelation(obj, tuple.Item1);

                                list.Add(r);
                            }
                        }

                        return list;
                    });

            if (result.All(r => r.Child != obj))
            {
                var r = new ParentChildRelation(null, obj);
                if (_mappings[obj.GetType()].NamedHierarchyParentAccessSpecifications.Any())
                {
                    var np = _mappings[obj.GetType()].NamedHierarchyParentAccessSpecifications.Single();
                    r.PreexistingHierarchyReference = np.GetName();
                }

                result.Add(r);
            }

            return result;
        }
    }
}
