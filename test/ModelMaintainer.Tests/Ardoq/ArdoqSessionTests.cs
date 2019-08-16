using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Ardoq
{
    public class ArdoqSessionTests
    {
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;

        public ArdoqSessionTests()
        {
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
        }

        [Fact]
        public void GetComponentsOfType_HappyDays_GetsExpectedComponents()
        {
            // Arrange
            var workspace = new Workspace("MyWorkspace", null) {Id = "454923f0-b1b0-48b3-9015-251dad9b4cea" };
            var componentType = "MyType";

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));

            var components = new List<Component>
            {
                new Component("C1", workspace.Id, null) {Type = componentType},
                new Component("C2", workspace.Id, null) {Type = componentType},
                new Component("C3", workspace.Id, null) {Type = "SomeOtherType"}
            };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(components));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            var comps = session.GetComponentsOfType(componentType);

            // Assert
            Assert.Equal(2, comps.Count());
        }

        [Fact]
        public void GetComponentsOfType_Twice_GetsSameComponents()
        {
            // Arrange
            var workspace = new Workspace("MyWorkspace", null) { Id = "454923f0-b1b0-48b3-9015-251dad9b4cea" };
            var componentType = "MyType";

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));

            var components = new List<Component>
            {
                new Component("C1", workspace.Id, null) {Type = componentType}
            };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(components));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            var comps1 = session.GetComponentsOfType(componentType);
            var comps2 = session.GetComponentsOfType(componentType);

            // Assert
            Assert.Same(comps1.First(), comps2.First());
            _readerMock.Verify(r => r.GetAllComponents(workspace.Id), Times.Once);
        }

        [Fact]
        public void DeleteComponent_HappyDays_SendsDeletionToArdoqAndUncaches()
        {
            // Arrange
            var workspace = new Workspace("MyWorkspace", null) { Id = "454923f0-b1b0-48b3-9015-251dad9b4cea" };
            var componentType = "MyType";

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));

            var compId = "3a1949ef-8b44-4af3-939f-a49d24376248";

            var component = new Component("C1", workspace.Id, null) {Id = compId, Type = componentType};

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(new List<Component>{component}));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            session.DeleteComponent(component);

            // Assert
            _writerMock.Verify(w => w.DeleteComponent(compId), Times.Once);
            Assert.True(!session.GetComponentsOfType(componentType).Any());
        }

        [Fact]
        public void UpdateComponent_HappyDays_SendsUpdateToArdoq()
        {
            // Arrange
            var workspace = new Workspace("MyWorkspace", null) { Id = "454923f0-b1b0-48b3-9015-251dad9b4cea" };
            var componentType = "MyType";

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));

            var compId = "3a1949ef-8b44-4af3-939f-a49d24376248";

            var component = new Component("C1", workspace.Id, null) { Id = compId, Type = componentType };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(new List<Component> { component }));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            session.UpdateComponent(component);

            // Assert
            _writerMock.Verify(w => w.UpdateComponent(component), Times.Once);
        }

        [Fact]
        public void AddComponent_HappyDays_SendsComponentToArdoqAndCaches()
        {
            // Arrange
            var componentModel = "my-comp-model";
            var workspace = new Workspace("MyWorkspace", null)
            {
                Id = "454923f0-b1b0-48b3-9015-251dad9b4cea",
                ComponentModel = componentModel
            };

            var componentType = "MyType";
            var componentTypeId = "3a1949ef-8b44-4af3-939f-a49d24376248";

            var internalmodel = new global::Ardoq.Models.Model(
                "model-id", 
                "MyModel", 
                null, 
                new Dictionary<string, string>{ [componentType] = componentTypeId },
                null);

            IArdoqModel model = new ArdoqModel(internalmodel);


            var componentName = "MyNewComponent";
            var parentName = "parentCompName";
            var vals = new Dictionary<string, object>
            {
                ["k1"] = "v1",
                ["k2"] = "v2"
            };

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));
            _readerMock.Setup(r => r.GetModelById(componentModel))
                .Returns(Task.FromResult(model));

            var parentComponentId = "f7e4256e-1d84-4fba-96b7-35ed16234b45";
            var parentComponent = new Component(parentName, workspace.Id, null) {Id = parentComponentId };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(new List<Component>{ parentComponent }));

            _writerMock.Setup(w => w.CreateComponent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(new Component(componentName, null, null) {Type = componentType}));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            session.AddComponent(componentName, vals, componentType, parentName);

            // Assert
            _writerMock.Verify(w => w.CreateComponent(componentName, workspace.Id, componentType, componentTypeId, parentComponentId, vals));
            Assert.True(session.GetComponentsOfType(componentType).Count() == 1);
        }

        [Fact]
        public void GetReference_DoesNotExist_ReturnsNull()
        {
            // Arrange
            var sourceComponentType = "ST";
            var sourceKey = "source-1";
            var sourceId = "source-id-1";
            var targetComponentType = "TT";
            var targetKey = "target-2";
            var targetId = "target-id-2";

            var relationType = "my-rel";

            var componentModel = "my-comp-model";
            var workspace = new Workspace("MyWorkspace", null)
            {
                Id = "454923f0-b1b0-48b3-9015-251dad9b4cea",
                ComponentModel = componentModel
            };

            var internalmodel = new global::Ardoq.Models.Model(
                "model-id",
                "MyModel",
                null,
                null,
                new Dictionary<string, int> { ["my-rel"] = 1 });
            IArdoqModel model = new ArdoqModel(internalmodel);

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));
            _readerMock.Setup(r => r.GetModelById(componentModel))
                .Returns(Task.FromResult(model));

            var sourceComponent = new Component(sourceKey, workspace.Id, null) { Type = sourceComponentType, Id = sourceId };
            var targetComponent = new Component(targetKey, workspace.Id, null) { Type = targetComponentType, Id = targetId };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(new List<Component> { sourceComponent, targetComponent }));

            _readerMock.Setup(r => r.GetReferencesById(workspace.Id))
                .Returns(Task.FromResult(new List<Reference>()));
            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Act
            var relation = session.GetReference(relationType, sourceComponentType, sourceKey, targetComponentType, targetKey);

            // Assert
            Assert.Null(relation);
        }

        [Fact]
        public void AddReference_HappyDays_CreatesReferenceInArdoq()
        {
            // Arrange
            var sourceComponentType = "ST";
            var sourceKey = "source-1";
            var sourceId = "source-id-1";
            var targetComponentType = "TT";
            var targetKey = "target-2";
            var targetId = "target-id-2";
            var refType = "my-rel";

            var componentModel = "my-comp-model";
            var workspace = new Workspace("MyWorkspace", null)
            {
                Id = "454923f0-b1b0-48b3-9015-251dad9b4cea",
                ComponentModel = componentModel
            };

            var internalmodel = new global::Ardoq.Models.Model(
                "model-id",
                "MyModel",
                null,
                null,
                new Dictionary<string, int> { [refType] = 1 });
            IArdoqModel model = new ArdoqModel(internalmodel);

            _readerMock.Setup(r => r.GetWorkspaceById(workspace.Id))
                .Returns(Task.FromResult(workspace));
            _readerMock.Setup(r => r.GetModelById(componentModel))
                .Returns(Task.FromResult(model));

            _readerMock.Setup(r => r.GetReferencesById(workspace.Id))
                .Returns(Task.FromResult(new List<Reference>()));

            var sourceComponent = new Component(sourceKey, workspace.Id, null) { Type = sourceComponentType, Id = sourceId };
            var targetComponent = new Component(targetKey, workspace.Id, null) { Type = targetComponentType, Id = targetId };

            _readerMock.Setup(r => r.GetAllComponents(workspace.Id))
                .Returns(Task.FromResult<IEnumerable<Component>>(new List<Component> { sourceComponent, targetComponent }));

            var session = new ArdoqSession(workspace.Id, _readerMock.Object, _writerMock.Object);

            // Arrange
            session.AddReference(1, sourceComponentType, sourceKey, targetComponentType, targetKey);

            // Assert
            _writerMock.Verify(w => w.CreateReference(workspace.Id, sourceId, targetId, 1), Times.Once);
        }
    }
}
