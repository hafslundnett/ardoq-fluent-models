using System.Collections.Generic;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Maintainence;
using ModelMaintainer.Mapping;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ExternalLinkageServiceTests
    {
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;
        private readonly Mock<IMaintainenceSession> _maintenanceSessionMock;

        public ExternalLinkageServiceTests()
        {
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
            _maintenanceSessionMock = new Mock<IMaintainenceSession>();
        }

        [Fact]
        public void LinkAll_NoReferencesPreexist_ReferenceCreatedInArdoq()
        {
            // Arrange
            var sourceWorkspaceId = "my-workspace-id";
            var sourceWorkspaceName = "RoleWorkspace";

            var refName = "typical_role_in";
            var refId = 707;
            var targetCompModel = "TargetComponentModel";
            var sourceCompModel = "SourceComponentModel";

            var targetWorkspaceId = "industry-workspace";
            var targetWorkspaceName = "IndustryWorkspace";
            var session = new ArdoqSession(sourceWorkspaceId, _readerMock.Object, _writerMock.Object);

            var sourceObj = new Role() {Name = "Sales"};
            var relation = new ParentChildRelation(null, sourceObj) { ChildUniqueName = "Sales Role" };
            var relations = new List<ParentChildRelation> { relation };

            var internalmodel = new global::Ardoq.Models.Model(
                "model-id",
                "MyModel",
                null,
                null,
                new Dictionary<string, int>{[refName] = refId});
            IArdoqModel model = new ArdoqModel(internalmodel);

            var referenceSpec = new ExternalReferenceSpecification<Role>(
                "Industry",
                r => r.GetNameOfIndustry(),
                refName,
                targetWorkspaceName);

            var sourceWorkspace = new Workspace(sourceWorkspaceName, null) {Id = sourceWorkspaceId, ComponentModel = sourceCompModel };
            var targetWorkspace = new Workspace(targetWorkspaceName, null) { Id = targetWorkspaceId, ComponentModel = targetCompModel };
            _readerMock.Setup(r => r.GetWorkspaceNamed(sourceWorkspaceName, null))
                .Returns(Task.FromResult(sourceWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceById(sourceWorkspaceId))
                .Returns(Task.FromResult(sourceWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceNamed(targetWorkspaceName, null))
                .Returns(Task.FromResult(targetWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceById(targetWorkspaceId))
                .Returns(Task.FromResult(targetWorkspace));
            _readerMock.Setup(r => r.GetModelById(targetCompModel))
                .Returns(Task.FromResult(model));
            _readerMock.Setup(r => r.GetModelById(sourceCompModel))
                .Returns(Task.FromResult(model));

            _readerMock.Setup(r => r.GetReferencesById(sourceWorkspaceId))
                .Returns(Task.FromResult(new List<Reference>()));

            _readerMock.Setup(r => r.GetReferencesById(targetWorkspaceId))
                .Returns(Task.FromResult(new List<Reference>()));

            IEnumerable<Component> sourceComps = new List<Component>
            {
                new Component("Sales", sourceWorkspaceId, null) {Type = "Role", Id = "role-comp-1"}
            };

            IEnumerable<Component> targetComps = new List<Component>
            {
                new Component("Sales and marketing", targetWorkspaceId, null){Type = "Industry",  Id = "industry-comp-2"}
            };

            _readerMock.Setup(r => r.GetAllComponents(sourceWorkspaceId))
                .Returns(Task.FromResult(sourceComps));
            _readerMock.Setup(r => r.GetAllComponents(targetWorkspaceId))
                .Returns(Task.FromResult(targetComps));

            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Role))).Returns("Role");
            _maintenanceSessionMock.Setup(m => m.GetKeyForInstance(sourceObj)).Returns("Sales");
            

            var linkageService = new ExternalLinkageService(_readerMock.Object, _writerMock.Object);

            // Act
            linkageService.LinkAll(referenceSpec, relations, session, _maintenanceSessionMock.Object);

            // Assert
            _writerMock.Verify(w => w.CreateReference(targetWorkspaceId, "role-comp-1", "industry-comp-2", refId), Times.Once);
        }

        [Fact]
        public void LinkAll_ReferenceAlreadyExists_NothingSentToArdoq()
        {
            // Arrange
            var sourceWorkspaceId = "my-workspace-id";
            var sourceWorkspaceName = "RoleWorkspace";

            var refName = "typical_role_in";
            var refId = 707;
            var targetCompModel = "TargetComponentModel";
            var sourceCompModel = "SourceComponentModel";

            var targetWorkspaceId = "industry-workspace";
            var targetWorkspaceName = "IndustryWorkspace";
            var session = new ArdoqSession(sourceWorkspaceId, _readerMock.Object, _writerMock.Object);

            var sourceObj = new Role() { Name = "Sales" };
            var relation = new ParentChildRelation(null, sourceObj) { ChildUniqueName = "Sales Role" };
            var relations = new List<ParentChildRelation> { relation };

            var internalmodel = new global::Ardoq.Models.Model(
                "model-id",
                "MyModel",
                null,
                null,
                new Dictionary<string, int> { [refName] = refId });
            IArdoqModel model = new ArdoqModel(internalmodel);

            var referenceSpec = new ExternalReferenceSpecification<Role>(
                "Industry",
                r => r.GetNameOfIndustry(),
                refName,
                targetWorkspaceName);

            var sourceWorkspace = new Workspace(sourceWorkspaceName, null) { Id = sourceWorkspaceId, ComponentModel = sourceCompModel };
            var targetWorkspace = new Workspace(targetWorkspaceName, null) { Id = targetWorkspaceId, ComponentModel = targetCompModel };
            _readerMock.Setup(r => r.GetWorkspaceNamed(sourceWorkspaceName, null))
                .Returns(Task.FromResult(sourceWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceById(sourceWorkspaceId))
                .Returns(Task.FromResult(sourceWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceNamed(targetWorkspaceName, null))
                .Returns(Task.FromResult(targetWorkspace));
            _readerMock.Setup(r => r.GetWorkspaceById(targetWorkspaceId))
                .Returns(Task.FromResult(targetWorkspace));
            _readerMock.Setup(r => r.GetModelById(targetCompModel))
                .Returns(Task.FromResult(model));
            _readerMock.Setup(r => r.GetModelById(sourceCompModel))
                .Returns(Task.FromResult(model));

            var sourceComp = new Component("Sales", sourceWorkspaceId, null) {Type = "Role", Id = "role-comp-1"};
            IEnumerable<Component> sourceComps = new List<Component>{sourceComp};

            var targetComp = new Component("Sales and marketing", targetWorkspaceId, null) { Type = "Industry", Id = "industry-comp-2" };
            IEnumerable<Component> targetComps = new List<Component> { targetComp };

            _readerMock.Setup(r => r.GetAllComponents(sourceWorkspaceId))
                .Returns(Task.FromResult(sourceComps));
            _readerMock.Setup(r => r.GetAllComponents(targetWorkspaceId))
                .Returns(Task.FromResult(targetComps));

            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Role))).Returns("Role");
            _maintenanceSessionMock.Setup(m => m.GetKeyForInstance(sourceObj)).Returns("Sales");

            var existingReference = new Reference(sourceWorkspaceId, null, sourceComp.Id, targetComp.Id, refId){ TargetWorkspace = targetWorkspaceId };
            _readerMock.Setup(r => r.GetReferencesById(sourceWorkspaceId))
                .Returns(Task.FromResult(new List<Reference>{ existingReference }));


            var linkageService = new ExternalLinkageService(_readerMock.Object, _writerMock.Object);

            // Act
            linkageService.LinkAll(referenceSpec, relations, session, _maintenanceSessionMock.Object);

            // Assert
            _writerMock.Verify(w => w.CreateReference(targetWorkspaceId, "role-comp-1", "industry-comp-2", refId), Times.Never);
        }
    }
}
