using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqReader : IArdoqReader
    {
        private readonly ArdoqClient _client;
        private readonly ILogger _logger;

        public ArdoqReader(
            string url,
            string token,
            string organization,
            ILogger logger)
        {
            _client = new ArdoqClient(new HttpClient(), url, token, organization);
            _logger = logger;
        }

        public async Task<Workspace> GetWorkspaceNamed(string workspaceName, Folder folder = null)
        {
            var workspaces = await _client.WorkspaceService.GetAllWorkspaces();
            return folder == null
                ? workspaces.SingleOrDefault(w => w.Name == workspaceName)
                : workspaces.SingleOrDefault(w => w.Name == workspaceName && w.Folder == folder.Id);
        }

        public async Task<Workspace> GetWorkspaceById(string workspaceId)
        {
            return await _client.WorkspaceService.GetWorkspaceById(workspaceId);
        }

        public async Task<Folder> GetFolder(string folderName)
        {
            var folders = await _client.FolderService.GetAllFolders();
            return folders.SingleOrDefault(f => f.Name == folderName);
        }

        public async Task<IEnumerable<Component>> GetAllComponents(string workspaceId)
        {
            return await _client.ComponentService.GetComponentsByWorkspace(workspaceId);
        }

        public async Task<IArdoqModel> GetModelById(string modelId)
        {
            var model = await _client.ModelService.GetModelById(modelId);
            return new ArdoqModel(model);
        }

        public async Task<IArdoqModel> GetTemplateByName(string templateName)
        {
            var all = await _client.ModelService.GetAllTemplates();
            var model = all.Single(t => t.Name == templateName);
            return new ArdoqModel(model);
        }

        public async Task<List<Reference>> GetAllReferences()
        {
            return await _client.ReferenceService.GetAllReferences();
        }

        public async Task<List<Reference>> GetReferencesById(string workspaceId)
        {
            return await _client.ReferenceService.GetReferencesRelatedToWorkspace(workspaceId);
        }

        public async Task<List<Tag>> GetAllTags(string workspaceId = null)
        {
            var tags = await _client.TagService.GetAllTags();
            if (string.IsNullOrWhiteSpace(workspaceId))
            {
                return tags;
            }

            return tags.Where(tag => tag.RootWorkspace == workspaceId).ToList();
        }

        public async Task<List<Tag>> GetAllTagsInFolder(string searchFolder)
        {
            var tags = await _client.TagService.GetAllTags();

            if (string.IsNullOrWhiteSpace(searchFolder))
            {
                return tags;
            }

            var folder = await _client.FolderService.GetFolderByName(searchFolder);

            return tags.Where(t => folder.Workspaces.Contains(t.RootWorkspace)).ToList();
        }
    }
}
