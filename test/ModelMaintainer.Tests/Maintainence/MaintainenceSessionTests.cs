using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardoq.Models;
using ArdoqFluentModels;
using ArdoqFluentModels.Ardoq;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class MaintainenceSessionTests
    {
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;
        private readonly Mock<IArdoqSearcher> _searcherMock;
        private readonly Mock<IArdoqWorkspaceCreator> _workspaceCreatorMock;
        private readonly Mock<ISourceModelProvider> _modelProviderMock;

        public MaintainenceSessionTests()
        {
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
            _searcherMock = new Mock<IArdoqSearcher>();
            _workspaceCreatorMock = new Mock<IArdoqWorkspaceCreator>();
            _modelProviderMock = new Mock<ISourceModelProvider>();
        }

        [Fact]
        public void Run_WorkspaceDoesNotExistAndNoTemplateSet_ThrowsException()
        {
            // Arrange
            string folderName = "MyFolder";

            _readerMock.Setup(r => r.GetFolder(folderName))
                .Returns(Task.FromResult(new Folder(folderName, "My folder") { Workspaces = new List<string>() }));

            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithWorkspaceNamed("Test")
                .WithFolderNamed(folderName)
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            var session = builder.Build();

            // Act
            var ex = Assert.Throws<AggregateException>(() => session.Run(_modelProviderMock.Object).Wait());
            Assert.True(ex.InnerExceptions.First() is InvalidOperationException);
            var inner = ex.InnerExceptions.First();
            Assert.Equal("Template name must be set when creating workspace.", inner.Message);
        }

        [Fact]
        public void Run_WorkspaceDoesNotExistHasTemplate_CreatesNewWorkspace()
        {
            // Arrange
            string workspaceName = "Test";
            string folderName = "MyFolder";
            string componentModel = "5bab5cc8b3da08632a73ed2f";
            string folderId = "folder-id";

            var folder = new Folder(folderName, "My folder") { Id = folderId, Workspaces = new List<string>() };

            _readerMock.Setup(r => r.GetFolder(folderName))
                .Returns(Task.FromResult(folder));

            _readerMock.Setup(r => r.GetTemplateByName(componentModel))
                .Returns(Task.FromResult((IArdoqModel)new ArdoqModel(new global::Ardoq.Models.Model() { Id = "666" })));

            _writerMock.Setup(w => w.CreateWorkspace(It.IsAny<Workspace>()))
                .Returns(Task.FromResult(new Workspace(workspaceName, "")));

            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithWorkspaceNamed(workspaceName)
                .WithFolderNamed(folderName)
                .WithTemplate(componentModel)
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            var session = builder.Build();

            // Act
            session.Run(_modelProviderMock.Object).Wait();

            // Assert
            _writerMock.Verify(w => w.CreateWorkspace(It.IsAny<Workspace>()));
        }

        [Fact]
        public void GetComponentType_AfterRegistration_GetsExpectedComponentType()
        {
            // Arrange
            var componentTypeEmployee = "EmployeeCompType";
            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            builder.AddComponentMapping<Employee>(componentTypeEmployee);

            var session = builder.Build();

            // Act
            var comType = session.GetComponentType(typeof(Employee));

            // Assert
            Assert.Equal(componentTypeEmployee, comType);
        }

        [Fact]
        public void GetComponentType_TypeNotRegistered_ReturnsNull()
        {
            // Arrange
            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            var session = builder.Build();

            // Act
            var comType = session.GetComponentType(typeof(Employee));

            // Assert
            Assert.Null(comType);
        }

        [Fact]
        public void GetKeyForInstance_RegisteredType_ReturnsExpectedKey()
        {
            var componentTypeEmployee = "EmployeeCompType";
            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithReader(_readerMock.Object)
                .WithSearcher(_searcherMock.Object)
                .WithWriter(_writerMock.Object);

            builder.AddComponentMapping<Employee>(componentTypeEmployee)
                .WithKey(emp => emp.EmployeeNumber);

            var employeeNumber = "007";
            var employee = new Employee { EmployeeNumber = employeeNumber };

            var session = builder.Build();

            // Act
            var key = session.GetKeyForInstance(employee);

            // Assert
            Assert.Equal(employeeNumber, key);
        }

        [Fact]
        public void GetKeyForInstance_NotRegistered_ReturnsNull()
        {
            var builder = new ArdoqModelMappingBuilder(null, null, null)
                .WithReader(_readerMock.Object)
                .WithWriter(_writerMock.Object)
                .WithSearcher(_searcherMock.Object);

            var employeeNumber = "007";
            var employee = new Employee { EmployeeNumber = employeeNumber };

            var session = builder.Build();

            // Act
            var key = session.GetKeyForInstance(employee);

            // Assert
            Assert.Null(key);
        }
    }
}
