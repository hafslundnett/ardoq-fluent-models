using System;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqWorkspaceCreator : IArdoqWorkspaceCreator
    {
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;

        public ArdoqWorkspaceCreator(IArdoqReader reader, IArdoqWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public async Task<Workspace> CreateWorkspaceIfMissing(string folderName, string templateName, string workspaceName)
        {
            //var folder = _ardoqClient.FolderService.GetFolderByName(folderName).Result;
            var folder = await _reader.GetFolder(folderName);

            var ws = GetWorkspaceByNameAndFolder(workspaceName, folder);
            if (ws != null)
            {
                return ws;
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new InvalidOperationException("Template name must be set when creating workspace.");
            }

            var template = await _reader.GetTemplateByName(templateName);
            var workspace = new Workspace(workspaceName, "")
            {
                Folder = folder.Id,
                ComponentTemplate = template.Id
            };

            //_writer.CreateWorkspace()
            return await _writer.CreateWorkspace(workspace);
        }

        private Workspace GetWorkspaceByNameAndFolder(string name, Folder folder)
        {
            foreach (var workspaceId in folder.Workspaces) // TODO: check folder not null
            {
                var workspace = _reader.GetWorkspaceById(workspaceId).Result;
                if (workspace.Name == name)
                {
                    return workspace;
                }
            }
            return null;
        }

    }
}
