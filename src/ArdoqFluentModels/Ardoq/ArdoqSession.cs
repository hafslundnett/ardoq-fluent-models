using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Mapping;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqSession : IArdoqSession
    {
        private readonly string _workspaceId;
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;

        private List<Component> _components;
        private List<Reference> _references;
        private List<Tag> _tags;
        private IArdoqModel _model;

        public ArdoqSession(string workspaceId, IArdoqReader reader, IArdoqWriter writer)
        {
            _workspaceId = workspaceId;
            _reader = reader;
            _writer = writer;
        }

        public IEnumerable<Component> GetAllComponents()
        {
            InitIfNecessary();

            return _components;
        }

        public IEnumerable<Component> GetComponentsOfType(string typeName)
        {
            InitIfNecessary();

            return _components.Where(c => c.Type == typeName);
        }

        public IEnumerable<Component> GetComponents(string typeName, string name)
        {
            InitIfNecessary();

            return _components.Where(c => c.Type == typeName && c.Name == name);
        }

        public void AddComponent(
            string name,
            IDictionary<string, object> values,
            string componentType,
            string parentComponentNameOrId)
        {
            InitIfNecessary();
            var parentComponent = string.IsNullOrWhiteSpace(parentComponentNameOrId) ? null : _components.SingleOrDefault(c => c.Name == parentComponentNameOrId);

            AddComponentWithParent(name, values, componentType, parentComponent);
        }

        public void AddComponentWithParent(
            string name,
            IDictionary<string, object> values,
            string componentType,
            Component parentComponent)
        {
            InitIfNecessary();

            string componentTypeId;
            try
            {
                componentTypeId = _model.GetComponentTypeByName(componentType);
            }
            catch (KeyNotFoundException e)
            {
                throw new KeyNotFoundException($"No component type named {componentType} found in model {_model.Name}.", e);
            }

            var component = _writer.CreateComponent(
                name,
                _workspaceId,
                componentType,
                componentTypeId,
                parentComponent?.Id,
                values).Result;

            _components.Add(component);
        }

        public void UpdateComponent(Component component)
        {
            InitIfNecessary();

            var existing = _components.Single(c => c.Id == component.Id);
            _components.Remove(existing);

            var updated = _writer.UpdateComponent(component).Result;
            _components.Add(updated);
        }

        public void DeleteComponent(Component component)
        {
            InitIfNecessary();

            var existing = _components.Single(c => c.Id == component.Id);
            _writer.DeleteComponent(existing.Id).Wait();
            _components.Remove(existing);
        }

        public Reference GetReference(
            string referenceType,
            string sourceComponentType,
            string sourceKey,
            string targetComponentType,
            string targetKey)
        {
            InitIfNecessary();

            var source = _components.Single(c => c.Type == sourceComponentType && c.Name == sourceKey);
            var target = _components.Single(c => c.Type == targetComponentType && c.Name == targetKey);

            var refType = _model.GetReferenceTypeByName(referenceType);
            return _references.SingleOrDefault(r => r.Source == source.Id && r.Target == target.Id && r.Type == refType);
        }

        public IEnumerable<Reference> GetAllSourceReferencesFromChild(ParentChildRelation relation)
        {
            InitIfNecessary();

            var sourceComponent = GetChildComponent(relation);

            return _references.Where(r => r.Source == sourceComponent.Id);
        }

        public int GetReferenceTypeForName(string refernceName)
        {
            InitIfNecessary();

            return _model.GetReferenceTypeByName(refernceName);
        }

        public void DeleteReference(Reference reference)
        {
            InitIfNecessary();

            _writer.DeleteReference(reference.Id).Wait();

            var existing = _references.Single(r => r.Id == reference.Id);
            _references.Remove(existing);
        }

        public IEnumerable<Tag> GetAllTags()
        {
            InitIfNecessary();

            return _tags;
        }

        public Tag CreateTag(string tag, Component component)
        {
            InitIfNecessary();

            var t = _writer.CreateTag(_workspaceId, tag, new List<string> { component.Id }).Result;
            _tags.Add(t);
            return t;
        }

        public Tag AddComponentToTag(Tag tag, Component component)
        {
            InitIfNecessary();

            tag.Components.Add(component.Id);

            var updatedTag = _writer.UpdateTag(tag).Result;

            _tags.Remove(tag);
            _tags.Add(updatedTag);

            return updatedTag;
        }

        public void AddReference(
            int refType,
            string sourceType,
            string sourceName,
            string targetType,
            string targetName)
        {
            InitIfNecessary();

            var sourceComponent = _components.Single(c => c.Type == sourceType && c.Name == sourceName); // TODO: I think this could fail if the source does not have a unique type and name
            var targetComponent = _components.Single(c => c.Type == targetType && c.Name == targetName); // TODO: I think this could fail if the target does not have a unique type and name

            AddReference(refType, sourceComponent, targetComponent);
        }

        public void AddReference(
            int refType,
            ParentChildRelation source,
            ParentChildRelation target)
        {
            InitIfNecessary();

            var sourceComponent = GetChildComponent(source);
            var targetComponent = GetChildComponent(target);

            //var sourceComponent = _components.Single(c => c.Type == sourceType && c.Name == sourceName); // TODO: I think this could fail if the source does not have a unique type and name
            //var targetComponent = _components.Single(c => c.Type == targetType && c.Name == targetName); // TODO: I think this could fail if the target does not have a unique type and name

            AddReference(refType, sourceComponent, targetComponent);
        }

        public void AddReference(int referenceType, Component sourceComponent, Component targetComponent)
        {
            InitIfNecessary();

            AddReference(referenceType, sourceComponent.Id, targetComponent.Id);
        }

        public void AddReference(int referenceType, string sourceComponentId, string targetComponentId)
        {
            InitIfNecessary();

            var reference = _writer.CreateReference(_workspaceId, sourceComponentId, targetComponentId, referenceType).Result;

            _references.Add(reference);
        }

        public Component GetParentComponent(ParentChildRelation relation)
        {
            InitIfNecessary();

            var parentComponent = _components.SingleOrDefault(c => UniqueName(c) == relation.ParentUniqueName);
            return parentComponent;
        }

        public Component GetChildComponent(ParentChildRelation relation)
        {
            InitIfNecessary();

            var list = _components.Where(c => UniqueName(c) == relation.ChildUniqueName);

            if (list.Count() > 1)
            {
                throw new Exception($"Found multiple Components with same UniqueName: {relation.ChildUniqueName}");
            }

            return list.SingleOrDefault();
        }

        private string UniqueName(Component component)
        {
            var localName = component.Name + " " + component.Type;
            if (component.Parent == null)
            {
                return localName;
            }

            var parentComponent = _components.Single(c => c.Id == component.Parent);
            var parentUniqueName = UniqueName(parentComponent);

            return parentUniqueName + " -> " + localName;
        }

        private void InitIfNecessary()
        {
            if (_components != null)
            {
                return;
            }

            _components = _reader.GetAllComponents(_workspaceId).Result.ToList();
            _references = _reader.GetReferencesById(_workspaceId).Result;
            _tags = _reader.GetAllTags(_workspaceId).Result;
            var workspace = _reader.GetWorkspaceById(_workspaceId).Result;
            _model = _reader.GetModelById(workspace.ComponentModel).Result;
        }
    }
}
