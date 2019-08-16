using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Ardoq
{
    public class ArdoqWorkspaceCreatorTests
    {
        private const string folderName = "test-folder";
        private const string templateName = "test-template";
        private const string workspaceName = "test-ws";

        private Mock<IArdoqReader> _readerMock;
        private Mock<IArdoqWriter> _writerMock;

        private ArdoqWorkspaceCreator _workspaceCreator;

        public ArdoqWorkspaceCreatorTests()
        {
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();

            _workspaceCreator = new ArdoqWorkspaceCreator(_readerMock.Object, _writerMock.Object);
        }

        [Fact]
        public void CreateWorkspaceIfMissing_ExistingWorkspace_ShouldNotCreateWorkspace()
        {
            var folder = SetupMockWithFolder();

            var workspaceId = "679";
            folder.Workspaces.Add(workspaceId);

            var ws = new Workspace(workspaceName, "");
            ws.Id = workspaceId;

            _readerMock.Setup(r => r.GetWorkspaceById(workspaceId))
                .Returns(Task.FromResult(ws));

            var workspace = _workspaceCreator.CreateWorkspaceIfMissing(folderName, templateName, workspaceName).Result;


            Assert.Equal(ws, workspace);
        }

        [Fact]
        public void CreateWorkspaceIfMissing_NoWorkspace_ShouldCreateNewWorkspace()
        {
            var folder = SetupMockWithFolder();

            var internalmodel = new global::Ardoq.Models.Model();
            IArdoqModel template = new ArdoqModel(internalmodel);
            var templateId = "987";
            template.Id = templateId;

            _readerMock.Setup(r => r.GetTemplateByName(templateName))
                .Returns(Task.FromResult(template));

            var ws = new Workspace(workspaceName, "");

            _writerMock
                .Setup(w => w.CreateWorkspace(It.IsAny<Workspace>()))
                .Returns(Task.FromResult(ws));

            //_writerMock
            //    .Setup(client => client.WorkspaceService.CreateWorkspace(
            //            It.Is<Workspace>(w => w.Name == workspaceName && w.ComponentTemplate == templateId), null))
            //    .Returns(Task.FromResult(ws));


            var workspace = _workspaceCreator.CreateWorkspaceIfMissing(folderName, templateName, workspaceName).Result;


            Assert.Equal(ws, workspace);
        }

        private Folder SetupMockWithFolder()
        {
            var folderId = "234";
            var folder = new Folder(folderName, "");
            folder.Id = folderId;
            folder.Workspaces = new List<string>();

            _readerMock
                .Setup(client => client.GetFolder(folderName))
                .Returns(Task.FromResult(folder));

            return folder;
        }

    }
}
