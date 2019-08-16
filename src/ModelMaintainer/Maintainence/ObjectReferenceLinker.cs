using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Mapping;

namespace ArdoqFluentModels.Maintainence
{
    public class ObjectReferenceLinker
    {
        private readonly IBuiltComponentMapping _mapping;
        private readonly Workspace _workspace;
        private readonly IArdoqSession _session;
        private readonly IMaintainenceSession _maintainenceSession;

        public ObjectReferenceLinker(IBuiltComponentMapping mapping, Workspace workspace, IArdoqSession session, IMaintainenceSession maintainenceSession)
        {
            _mapping = mapping;
            _workspace = workspace;
            _session = session;
            _maintainenceSession = maintainenceSession;
        }

        public (int, int) LinkAll(IEnumerable<ParentChildRelation> allRelations, IEnumerable<ParentChildRelation> relations)
        {
            var createdCount = 0;
            var removedCount = 0;

            foreach (var relation in relations)
            {
                var tuple = Link(allRelations, relation);
                createdCount += tuple.Item1;
                removedCount += tuple.Item2;
            }

            return (createdCount, removedCount);
        }

        public (int, int) Link(IEnumerable<ParentChildRelation> relations, ParentChildRelation relation)
        {
            var sourceObject = relation.Child;
            var existingReferences = _session.GetAllSourceReferencesFromChild(relation)
                .Where(r => r.TargetWorkspace == _workspace.Id)
                .ToList();

            var expectedReferencesRelations = GetExpectedReferencesRelations(relations, sourceObject).ToList();
            var missingReferences = new List<(ParentChildRelation,int)>();
            
            foreach (var expectedReferencesRelation in expectedReferencesRelations)
            {
                var targetComponent = _session.GetChildComponent(expectedReferencesRelation.Item1);
                var existing = existingReferences.SingleOrDefault(r => r.Target == targetComponent.Id);
                if (existing != null)
                {
                    existingReferences.Remove(existing);
                }
                else
                {
                    missingReferences.Add(expectedReferencesRelation);
                }
            }

            AddMissingReferences(relation, missingReferences);
            RemoveStaleReferences(existingReferences);

            return (missingReferences.Count, existingReferences.Count);
        }

        private void RemoveStaleReferences(List<Reference> existingReferences)
        {
            if (existingReferences == null)
            {
                return;
            }

            foreach (var reference in existingReferences)
            {
                Console.WriteLine($"DeleteReference. referenceType={reference.Type} source={reference.Source} target={reference.Target}");
                _session.DeleteReference(reference);
            }
        }

        private void AddMissingReferences(ParentChildRelation relation, List<(ParentChildRelation, int)> missingReferences)
        {
            if (missingReferences == null)
            {
                return;
            }
            var sourceObject = relation.Child;
            var sourceType = _maintainenceSession.GetComponentType(sourceObject.GetType());
            var sourceKey = _maintainenceSession.GetKeyForInstance(sourceObject);
            foreach (var missingReference in missingReferences)
            {
                Console.WriteLine($"Adding missing reference. referenceType={missingReference.Item2} sourceComponentType={sourceType} sourceKey={sourceKey}");
                _session.AddReference(missingReference.Item2, relation, missingReference.Item1);
            }
        }

        private IEnumerable<(ParentChildRelation,int)> GetExpectedReferencesRelations(IEnumerable<ParentChildRelation> relations, object sourceObject)
        {
            var result = new List<(ParentChildRelation, int)>();
            foreach (var mappingRefGetter in _mapping.RefGetters)
            {
                var referencedChildObjects = ExpressionHelper.GetReferencedObjects(sourceObject, mappingRefGetter.Value.GetMethod);
                if (referencedChildObjects == null)
                {
                    continue;
                }

                foreach (var referencedChildObject in referencedChildObjects)
                {
                    var refType = _session.GetReferenceTypeForName(mappingRefGetter.Key.Item2);
                    var referencedRelation = relations.Single(r => r.Child == referencedChildObject);
                    result.Add((referencedRelation,refType) );
                }
            }

            return result;
        }
    }
}
