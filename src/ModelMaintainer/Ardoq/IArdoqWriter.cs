using Ardoq.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelMaintainer.Ardoq
{
    public interface IArdoqWriter
    {
        Task<Workspace> CreateWorkspace(string workspaceName, string modelId, string folder);
        Task<Workspace> CreateWorkspace(Workspace ws);

        Task<Component> CreateComponent(
            string name,
            string workspaceId,
            string componentType,
            string componentTypeId,
            string parentComponentId,
            IDictionary<string, object> props);

        Task<Component> UpdateComponent(Component component);

        Task DeleteComponent(string id);

        Task DeleteReference(string id);

        Task<Reference> CreateReference(string workspaceId, string sourceId, string targetId, int refType);

        Task<Tag> CreateTag(string workspaceId, string tagContents, IEnumerable<string> componentIds);
        Task<Tag> UpdateTag(Tag tag);
    }
}
