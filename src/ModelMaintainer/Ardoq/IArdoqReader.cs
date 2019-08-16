using System.Collections.Generic;
using System.Threading.Tasks;
using Ardoq.Models;

namespace ArdoqFluentModels.Ardoq
{
    public interface IArdoqReader
    {
        Task<Workspace> GetWorkspaceNamed(string workspaceName, Folder folder = null);
        Task<Folder> GetFolder(string folderName);
        Task<IEnumerable<Component>> GetAllComponents(string workspaceId);
        Task<IArdoqModel> GetModelById(string modelId);
        Task<IArdoqModel> GetTemplateByName(string templateName);
        Task<Workspace> GetWorkspaceById(string workspaceId);
        Task<List<Reference>> GetAllReferences();
        Task<List<Reference>> GetReferencesById(string workspaceId);
        Task<List<Tag>> GetAllTags(string workspaceId = null);
        Task<List<Tag>> GetAllTagsInFolder(string searchFolder);
    }
}
