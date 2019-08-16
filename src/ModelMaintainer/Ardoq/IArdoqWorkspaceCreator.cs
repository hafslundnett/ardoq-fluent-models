using System.Threading.Tasks;
using Ardoq.Models;

namespace ModelMaintainer.Ardoq
{
    public interface IArdoqWorkspaceCreator
    {
        Task<Workspace> CreateWorkspaceIfMissing(string folderName, string templateName, string workspaceName);
    }
}
