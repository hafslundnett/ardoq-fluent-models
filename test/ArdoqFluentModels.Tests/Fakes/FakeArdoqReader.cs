using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using Moq;

namespace ModelMaintainer.Tests.Maintainence.Fakes
{
    public class FakeArdoqReader : IArdoqReader
    {
        private readonly List<Component> _components;
        private readonly List<Tag> _tags;

        public FakeArdoqReader(List<Component> components, List<Tag> tags)
        {
            _components = components;
            _tags = tags;
        }

        public Task<IEnumerable<Component>> GetAllComponents(string workspaceId)
        {
            return Task.FromResult(_components.Where(c => c.RootWorkspace == workspaceId));
        }

        public Task<List<Reference>> GetAllReferences()
        {
            return Task.FromResult(new List<Reference>());
        }

        public Task<List<Tag>> GetAllTags(string workspaceId = null)
        {
            return Task.FromResult(_tags);
        }

        public Task<List<Tag>> GetAllTagsInFolder(string searchFolder)
        {
            throw new NotImplementedException();
        }

        public Task<Folder> GetFolder(string folderName)
        {
            throw new NotImplementedException();
        }

        public Task<IArdoqModel> GetModelById(string modelId)
        {
            var model = new Mock<IArdoqModel>();
            return Task.FromResult(model.Object);
        }

        public Task<List<Reference>> GetReferencesById(string workspaceId)
        {
            return Task.FromResult(new List<Reference>());
        }

        public Task<IArdoqModel> GetTemplateByName(string templateName)
        {
            throw new NotImplementedException();
        }

        public Task<Workspace> GetWorkspaceById(string workspaceId)
        {
            return Task.FromResult(new Workspace());
        }

        public Task<Workspace> GetWorkspaceNamed(string workspaceName, Folder folder = null)
        {
            throw new NotImplementedException();
        }
    }
}
