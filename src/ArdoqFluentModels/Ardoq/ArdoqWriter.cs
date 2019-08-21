using Ardoq;
using Ardoq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqWriter : IArdoqWriter
    {
        private readonly ArdoqClient _client;
        private readonly ILogger _logger;

        public ArdoqWriter(
            string url,
            string token,
            string organization,
            ILogger logger)
        {
            _client = new ArdoqClient(new HttpClient(), url, token, organization);
            _logger = logger;
        }

        public async Task<Workspace> CreateWorkspace(string workspaceName, string modelId, string folder)
        {
            var workspace = new Workspace(workspaceName, modelId, "");

            _logger.LogMessage($"Creating workspace {workspaceName} with model {modelId}");
            workspace = await _client.WorkspaceService.CreateWorkspace(workspace);
            workspace.Folder = folder;

            _logger.LogMessage($"Updating workspace {workspace?.Name}");
            return await _client.WorkspaceService.UpdateWorkspace(workspace.Id, workspace);
        }

        public async Task<Workspace> CreateWorkspace(Workspace ws)
        {
            _logger.LogMessage($"Creating workspace {ws?.Name} with model {ws?.ComponentTemplate}");
            return await _client.WorkspaceService.CreateWorkspace(ws);
        }

        public async Task<Component> CreateComponent(
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
                parentComponentId);

            foreach (var pair in props)
            {
                component.Fields.Add(pair.Key, pair.Value);
            }

            _logger.LogMessage($"Creating component {name} of type {componentTypeId}");

            return await _client.ComponentService.CreateComponent(component);
        }

        public async Task<Component> UpdateComponent(Component component)
        {
            _logger.LogMessage($"Updating component {component.Name} of type {component.Type}");
            return await _client.ComponentService.UpdateComponent(component.Id, component);
        }

        public async Task DeleteComponent(string id)
        {
            _logger.LogMessage($"Deleting component {id}");
            await _client.ComponentService.DeleteComponent(id);
        }

        public async Task<Reference> CreateReference(string workspaceId, string sourceId, string targetId, int refType)
        {
            var reference = new Reference(
                workspaceId,
                null,
                sourceId,
                targetId, refType);

            _logger.LogMessage($"Creating reference of type {refType} between {sourceId} and {targetId}");
            return await _client.ReferenceService.CreateReference(reference);
        }

        public async Task<Tag> CreateTag(string workspaceId, string tagContents, IEnumerable<string> componentIds)
        {
            var tag = new Tag(tagContents, workspaceId, null);

            if (componentIds != null)
            {
                tag.Components = componentIds.ToList();
            }

            _logger.LogMessage($"Creating tag {tagContents}.");
            Tag tag1 = await _client.TagService.CreateTag(tag);

            // Ardoq has a bug where a tag is reused, but the list of components is not updated
            // We manually add the missing componentIds
            var missingComponents = componentIds.Except(tag1.Components);
            if (missingComponents.Any())
            {
                tag1.Components.AddRange(missingComponents);
                var tag2 = await _client.TagService.UpdateTag(tag1.Id, tag1);
                return tag2;
            }
            else
            {
                return tag1;
            }
        }

        public async Task<Tag> UpdateTag(Tag tag)
        {
            _logger.LogMessage($"Updating tag {tag.Name} {tag.Id} {tag}.");
            try
            {
                return await _client.TagService.UpdateTag(tag.Id, tag);
            }
            catch (Refit.ApiException ex)
            {
                // The Ardoq API will sometimes fail with "Conflict" if we call the same operation (with same input) twice. We just ignore these errors. 
                if (ex.ReasonPhrase == "Conflict")
                {
                    return await Task.FromResult(tag);
                }
                throw;
            }
        }

        public async Task DeleteReference(string id)
        {
            _logger.LogMessage($"Deleting reference {id}");
            try
            {
                await _client.ReferenceService.DeleteReference(id);
            }
            catch (Refit.ApiException ex)
            {
                // The Ardoq API will sometimes fail with "Not Found" when deleting references. We just ignore these errors. 
                if (ex.ReasonPhrase == "Not Found")
                {
                    return;
                }
                throw;
            }
        }
    }
}
