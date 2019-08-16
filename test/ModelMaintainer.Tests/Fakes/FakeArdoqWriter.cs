using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Ardoq;

namespace ModelMaintainer.Tests.Maintainence.Fakes
{
    public class FakeArdoqWriter : IArdoqWriter
    {
        private readonly List<Component> _components;
        private readonly List<Tag> _tags;

        public FakeArdoqWriter(List<Component> components, List<Tag> tags)
        {
            _components = components;
            _tags = tags;
        }

        public Task<Component> CreateComponent(
            string name, 
            string workspaceId,
            string componentType,
            string componentTypeId,
            string parentComponentId, 
            IDictionary<string, object> props)
        {
            var component = new Component(
                name,
                workspaceId,
                "",
                componentTypeId,
                parentComponentId)
            {
                Id = Guid.NewGuid().ToString(),
                Type = componentType
            };

            foreach (var pair in props ?? new Dictionary<string, object>())
            {
                component.Fields.Add(pair.Key, pair.Value);
            }
            _components.Add(component);
            return Task.FromResult(component);
        }

        public Task DeleteComponent(string id)
        {
            var existingComponent = _components.Single(c => c.Id == id);
            _components.Remove(existingComponent);
            return Task.CompletedTask;
        }

        public Task<Component> UpdateComponent(Component component)
        {
            var existingComponent = _components.Single(c => c.Id == component.Id);
            _components.Remove(existingComponent);
            _components.Add(component);
            return Task.FromResult(component);
        }

        public Task<Tag> UpdateTag(Tag tag)
        {
            return Task.FromResult(tag);
        }

        public Task<Reference> CreateReference(string workspaceId, string sourceId, string targetId, int refType)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> CreateTag(string workspaceId, string tagContents, IEnumerable<string> componentIds)
        {
            var tag = new Tag(tagContents, workspaceId, null);

            if (componentIds != null)
            {
                tag.Components = componentIds.ToList();
            }

            return Task.FromResult(tag);
        }

        public Task<Workspace> CreateWorkspace(string workspaceName, string modelId, string folder)
        {
            throw new NotImplementedException();
        }

        public Task<Workspace> CreateWorkspace(Workspace ws)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReference(string id)
        {
            throw new NotImplementedException();
        }
    }
}
