using System.Collections.Generic;
using Ardoq.Models;
using ArdoqFluentModels;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Maintainence;
using ArdoqFluentModels.Mapping;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class ObjectReferenceLinkerTests
    {
        private readonly Mock<IArdoqSession> _ardogSessionMock;
        private readonly Mock<IMaintainenceSession> _maintenanceSessionMock;

        public ObjectReferenceLinkerTests()
        {
            _ardogSessionMock = new Mock<IArdoqSession>();
            _maintenanceSessionMock = new Mock<IMaintainenceSession>();
        }

        [Fact]
        public void Link_NoExistingReferences_AllExpectedReferencesAreAdded2()
        {
            // Arrange
            var linker = new ObjectReferenceLinker(GetMapping(), null, _ardogSessionMock.Object, _maintenanceSessionMock.Object);
            var relations = GetAllRelations();
            var relation = relations[1];

            // Act
            linker.Link(relations, relation);

            // Assert
            _ardogSessionMock.Verify(s => s.AddReference(
                It.IsAny<int>(),
                It.IsAny<ParentChildRelation>(),
                It.IsAny<ParentChildRelation>()), Times.Exactly(3));

            _ardogSessionMock.Verify(s => s.DeleteReference(It.IsAny<Reference>()), Times.Never);
        }

        [Fact]
        public void Link_MultipleRefsOfSameTypeFromSource_AllReferencesAreAdded()
        {
            // Arrange
            var linker = new ObjectReferenceLinker(GetMappingWithSameReferenceType(), null, _ardogSessionMock.Object, _maintenanceSessionMock.Object);
            var relations = GetAllRelations();
            var relation = relations[1];

            // Act
            linker.Link(relations, relation);

            // Assert
            _ardogSessionMock.Verify(s => s.AddReference(
                It.IsAny<int>(),
                It.IsAny<ParentChildRelation>(),
                It.IsAny<ParentChildRelation>()), Times.Exactly(3));
        }

        [Fact]
        public void Link_ArdoqHasStaleReference_StaleReferenceIsDeletedInArdoq()
        {
            // Arrange
            var workspaceId = "my-workspace";
            var workspace = new Workspace("MyWorkspace", null){Id = workspaceId};
            var compType = "Employee";
            var sourceKey = "12345";
            var linker = new ObjectReferenceLinker(GetMapping(), workspace, _ardogSessionMock.Object, _maintenanceSessionMock.Object);
            var relations = GetAllRelations(workspaceId);
            var relation = relations[1];

            _ardogSessionMock.Setup(m => m.GetChildComponent(relation)).Returns(new Component(sourceKey, workspaceId, null) { Type = compType });

            var staleReference = new Reference(null, null, "sourceid", "targetid", 1) { TargetWorkspace = workspaceId };
            _ardogSessionMock.Setup(a => a.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference>{ staleReference });

            // Act
            linker.Link(relations, relation);

            // Assert
            _ardogSessionMock.Verify(s => s.AddReference(
                It.IsAny<int>(),
                It.IsAny<ParentChildRelation>(),
                It.IsAny<ParentChildRelation>()), Times.Exactly(3));

            _ardogSessionMock.Verify(s => s.DeleteReference(staleReference), Times.Once);
        }

        [Fact]
        public void Link_TargetWorkspaceNotMatching_NothingSentToArdoq()
        {
            // Arrange
            var workspaceId = "my-workspace";
            var workspace = new Workspace("MyWorkspace", null) { Id = workspaceId };
            var compType = "Employee";
            var sourceKey = "12345";
            var linker = new ObjectReferenceLinker(GetMapping(), workspace, _ardogSessionMock.Object, _maintenanceSessionMock.Object);
            var relations = GetAllRelations(workspaceId);
            var relation = relations[1];

            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Employee))).Returns(compType);
            _maintenanceSessionMock.Setup(m => m.GetKeyForInstance(relation.Child)).Returns(sourceKey);
            _ardogSessionMock.Setup(m => m.GetChildComponent(relation)).Returns(new Component(sourceKey, workspaceId, null) { Type = compType });

            var staleReference = new Reference(null, null, "sourceid", "targetid", 1) { TargetWorkspace = "some-other-workspace" };
            _ardogSessionMock.Setup(a => a.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference> { staleReference });

            // Act
            linker.Link(relations, relation);

            // Assert
            _ardogSessionMock.Verify(s => s.AddReference(
                It.IsAny<int>(),
                It.IsAny<ParentChildRelation>(),
                It.IsAny<ParentChildRelation>()), Times.Exactly(3));

            _ardogSessionMock.Verify(s => s.DeleteReference(staleReference), Times.Never);
        }

        [Fact]
        public void Link_NoneMissingNoneStale_NothingSentToArdoq()
        {
            // Arrange
            var workspaceId = "my-workspace";
            var workspace = new Workspace("MyWorkspace", null) { Id = workspaceId };
            var compType = "Employee";
            var compTypeDepartment = "Department";
            var compTypeRole = "Role";
            var sourceKey = "12345";
            var sourceKeyDepartment = "HQ";
            var roleKey1 = "CEO";
            var roleKey2 = "BoardMember";

            var dep = new Department {Name = sourceKeyDepartment};
            var role1 = new Role {Name = roleKey1};
            var role2 = new Role {Name = roleKey2};
            var employee = new Employee
            {
                EmployeeNumber = sourceKey,
                Name = "Mickey Mouse",
                EmployedIn = dep,
                Roles = new List<Role> { role1, role2 }
            };

            var relations = new List<ParentChildRelation> {
                new ParentChildRelation(null, dep),
                new ParentChildRelation(dep, employee),
                new ParentChildRelation(employee, role1),
                new ParentChildRelation(employee, role2) };
            var relation = relations[1];

            var linker = new ObjectReferenceLinker(GetMapping(), workspace, _ardogSessionMock.Object, _maintenanceSessionMock.Object);

            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Employee))).Returns(compType);
            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Department))).Returns(compTypeDepartment);
            _maintenanceSessionMock.Setup(m => m.GetComponentType(typeof(Role))).Returns(compTypeRole);

            _ardogSessionMock.Setup(s => s.GetReferenceTypeForName("works_in")).Returns(1);
            _ardogSessionMock.Setup(s => s.GetReferenceTypeForName("has_role")).Returns(2);

            var depCompId = "dep-comp-id";
            _ardogSessionMock.Setup(s => s.GetChildComponent(relations[0]))
                .Returns(new Component {Id = depCompId});
            var roleCompId1 = "role-comp-id-1";
            _ardogSessionMock.Setup(s => s.GetChildComponent(relations[2]))
                .Returns(new Component { Id = roleCompId1 });
            var roleCompId2 = "role-comp-id-2";
            _ardogSessionMock.Setup(s => s.GetChildComponent(relations[3]))
                .Returns(new Component { Id = roleCompId2 });


            var ref1 = new Reference(null, null, sourceKey, depCompId, 1){TargetWorkspace = workspaceId};
            var ref2 = new Reference(null, null, sourceKey, roleCompId1, 2) { TargetWorkspace = workspaceId }; 
            var ref3 = new Reference(null, null, sourceKey, roleCompId2, 2) { TargetWorkspace = workspaceId }; 
            _ardogSessionMock.Setup(a => a.GetAllSourceReferencesFromChild(relation))
                .Returns(new List<Reference> { ref1, ref2, ref3 });

            // Act
            linker.Link(relations, relation);

            // Assert
            _ardogSessionMock.Verify(s => s.AddReference(
                It.IsAny<int>(),
                It.IsAny<ParentChildRelation>(),
                It.IsAny<ParentChildRelation>()), Times.Never);

            _ardogSessionMock.Verify(s => s.DeleteReference(It.IsAny<Reference>()), Times.Never);
        }

        private IBuiltComponentMapping GetMapping(string componentType = "MyType", string rootComponentName = "Employees")
        {
            var mapping = new ComponentMapping<Employee>(componentType);
            mapping.WithReference(emp => emp.EmployedIn, "works_in");
            mapping.WithReference(emp => emp.Roles, "has_role");
            mapping.WithPreexistingHierarchyReference(rootComponentName);
            return mapping;
        }

        private IBuiltComponentMapping GetMappingWithSameReferenceType(string componentType = "MyType", string rootComponentName = "Employees")
        {
            var mapping = new ComponentMapping<Employee>(componentType);
            mapping.WithReference(emp => emp.EmployedIn, "test_ref");
            mapping.WithReference(emp => emp.Roles, "test_ref");
            mapping.WithPreexistingHierarchyReference(rootComponentName);
            return mapping;
        }

        private ParentChildRelation GetSourceRelation(string employeeNumber = "12345")
        {
            var child = new Employee
            {
                EmployeeNumber = employeeNumber,
                Name = "Mickey Mouse",
                EmployedIn = new Department { Name = "HQ" },
                Roles = new List<Role>
                {
                    new Role {Name = "CEO"},
                    new Role {Name = "BoardMember"}
                }
            };
            return new ParentChildRelation(null, child);
        }
 
        private List<ParentChildRelation> GetAllRelations(string workspaceId = "workspaceId")
        {
            Role role1 = new Role { Name = "CEO" };
            Role role2 = new Role { Name = "BoardMember" };
            Department department = new Department { Name = "HQ" };
            var employee = new Employee
            {
                EmployeeNumber = "12345",
                Name = "Mickey Mouse",
                EmployedIn = department,
                Roles = new List<Role>
                {
                    role1,
                    role2
                }
            };

            var relations = new List<ParentChildRelation> {
                new ParentChildRelation(null, department),
                new ParentChildRelation(department, employee),
                new ParentChildRelation(employee, role1),
                new ParentChildRelation(employee, role2)
            };

            _ardogSessionMock.Setup(m => m.GetChildComponent(relations[0])).Returns(new Component("myDep", workspaceId, null) { Type = "Department" });
            _ardogSessionMock.Setup(m => m.GetChildComponent(relations[1])).Returns(new Component("myEmployee", workspaceId, null) { Type = "Employee" });
            _ardogSessionMock.Setup(m => m.GetChildComponent(relations[2])).Returns(new Component("myRole1", workspaceId, null) { Type = "Role" });
            _ardogSessionMock.Setup(m => m.GetChildComponent(relations[3])).Returns(new Component("myRole2", workspaceId, null) { Type = "Role" });

            return relations;
        }
    }
}
