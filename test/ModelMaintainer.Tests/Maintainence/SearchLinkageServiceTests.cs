using System.Collections.Generic;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Maintainence;
using ModelMaintainer.Mapping;
using ModelMaintainer.Search;
using ModelMaintainer.Tests.Model;
using Moq;
using Xunit;

namespace ModelMaintainer.Tests.Maintainence
{
    public class SearchLinkageServiceTests
    {
        private readonly Mock<IArdoqSearcher> _searcherMock;
        private readonly Mock<IArdoqReader> _readerMock;
        private readonly Mock<IArdoqWriter> _writerMock;
        private readonly Mock<IArdoqSession> _sessionMock;
        private readonly Mock<ILogger> _loggerMock;

        public SearchLinkageServiceTests()
        {
            _searcherMock = new Mock<IArdoqSearcher>();
            _readerMock = new Mock<IArdoqReader>();
            _writerMock = new Mock<IArdoqWriter>();
            _sessionMock = new Mock<IArdoqSession>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void LinkBySearch_FindsObjectsNoPriorRefs_CreatesNewReference()
        {
            // Arrange
            var refName = "uses";
            var refType = 99;
            var countryComponentId = "country-1";
            var employeeNumber = "A221133";
            const string workspaceId = "workspaceId";
            
            var mapping = new ComponentMapping<Employee>("Employee").WithKey(emp => emp.EmployeeNumber);
            var service = GetService(mapping, refName);

            var searchBuilder = GetSearchBuilder();
            var employee = new Employee {Name = "Magnus Carlsen", Age = 28, EmployeeNumber = employeeNumber };
            var employeeComponent = new Component(employeeNumber, workspaceId, null){Type = "Employee"};

            var countryComponent = new Component("Norway", workspaceId, null) {Id = countryComponentId};
            IEnumerable<Component> searchResComponents = new List<Component> {countryComponent};
            _searcherMock.Setup(s => s.Search(It.IsAny<SearchSpec>()))
                .Returns(Task.FromResult(searchResComponents));

            ParentChildRelation parentChildRelation = new ParentChildRelation(null, employee);
            _sessionMock.Setup(s => s.GetChildComponent(parentChildRelation))
                .Returns(employeeComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(refName))
                .Returns(refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(parentChildRelation))
                .Returns(new List<Reference>());

            // Act
            service.LinkBySearch(searchBuilder, new List<ParentChildRelation> { parentChildRelation });

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, employeeComponent, countryComponent), Times.Once);
        }

        [Fact]
        public void LinkBySearch_FindsObjectsAlreadyHasRefs_NoReferencesCreated()
        {
            // Arrange
            var refName = "uses";
            var refType = 99;
            
            var employeeNumber = "A221133";
            var employeeComponentId = "employee-123";
            var countryComponentId = "country-1";

            var mapping = new ComponentMapping<Employee>("Employee").WithKey(emp => emp.EmployeeNumber);
            var service = GetService(mapping, refName);

            var searchBuilder = GetSearchBuilder();
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28, EmployeeNumber = employeeNumber };
            var employeeComponent = new Component(employeeNumber, null, null) { Type = "Employee" };

            var countryComponent = new Component("Norway", null, null) { Id = countryComponentId };
            IEnumerable<Component> searchResComponents = new List<Component> { countryComponent };
            _searcherMock.Setup(s => s.Search(It.IsAny<SearchSpec>()))
                .Returns(Task.FromResult(searchResComponents));

            ParentChildRelation parentChildRelation = new ParentChildRelation(null, employee);
            _sessionMock.Setup(s => s.GetChildComponent(parentChildRelation))
                .Returns(employeeComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(refName))
                .Returns(refType);

            var existingRef = new Reference(null, null, employeeComponentId, countryComponentId, refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(parentChildRelation))
                .Returns(new List<Reference> { existingRef });

            // Act
            service.LinkBySearch(searchBuilder, new List<ParentChildRelation> { parentChildRelation });

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, employeeComponent, countryComponent), Times.Never);
        }

        [Fact]
        public void LinkBySearch_FindsSeveralObjectsAlreadyHasRefs_CreatedExpectedReferences()
        {
            // Arrange
            var refName = "uses";
            var refType = 99;

            var employeeNumber = "A221133";
            var countryComponentId = "country-1";

            var mapping = new ComponentMapping<Employee>("Employee").WithKey(emp => emp.EmployeeNumber);
            var service = GetService(mapping, refName);

            var searchBuilder = GetSearchBuilder();
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28, EmployeeNumber = employeeNumber };
            var employeeComponent = new Component(employeeNumber, null, null) { Type = "Employee" };

            var countryComponent1 = new Component("Norway", null, null) { Id = countryComponentId };
            var countryComponent2 = new Component("Sweden", null, null) { Id = "swe" };
            var countryComponent3 = new Component("Denmark", null, null) { Id = "dk" };
            IEnumerable<Component> searchResComponents = new List<Component> { countryComponent1, countryComponent2, countryComponent3 };
            _searcherMock.Setup(s => s.Search(It.IsAny<SearchSpec>()))
                .Returns(Task.FromResult(searchResComponents));

            ParentChildRelation parentChildRelation = new ParentChildRelation(null, employee);
            _sessionMock.Setup(s => s.GetChildComponent(parentChildRelation))
                .Returns(employeeComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(refName))
                .Returns(refType);

            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(parentChildRelation))
                .Returns(new List<Reference>());

            // Act
            service.LinkBySearch(searchBuilder, new List<ParentChildRelation> { parentChildRelation });

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, employeeComponent, It.IsAny<Component>()), Times.Exactly(3));
        }

        [Fact]
        public void LinkBySearch_FindsSeveralObjectsOnlySomeHasRefs_CreatedExpectedReferences()
        {
            // Arrange
            var refName = "uses";
            var refType = 99;

            var employeeNumber = "A221133";
            var employeeComponentId = "employee-123";
            var countryComponentId = "country-1";

            var mapping = new ComponentMapping<Employee>("Employee").WithKey(emp => emp.EmployeeNumber);
            var service = GetService(mapping, refName);

            var searchBuilder = GetSearchBuilder();
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28, EmployeeNumber = employeeNumber };
            var employeeComponent = new Component(employeeNumber, null, null) { Type = "Employee" };

            var countryComponent1 = new Component("Norway", null, null) { Id = countryComponentId };
            var countryComponent2 = new Component("Sweden", null, null) { Id = "swe" };
            var countryComponent3 = new Component("Denmark", null, null) { Id = "dk" };
            IEnumerable<Component> searchResComponents = new List<Component> { countryComponent1, countryComponent2, countryComponent3 };
            _searcherMock.Setup(s => s.Search(It.IsAny<SearchSpec>()))
                .Returns(Task.FromResult(searchResComponents));

            ParentChildRelation parentChildRelation = new ParentChildRelation(null, employee);
            _sessionMock.Setup(s => s.GetChildComponent(parentChildRelation))
                .Returns(employeeComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(refName))
                .Returns(refType);

            var existingRef = new Reference(null, null, employeeComponentId, countryComponentId, refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(parentChildRelation))
                .Returns(new List<Reference> { existingRef });

            // Act
            service.LinkBySearch(searchBuilder, new List<ParentChildRelation> { parentChildRelation });

            // Assert
            _sessionMock.Verify(s => s.AddReference(refType, employeeComponent, It.IsAny<Component>()), Times.Exactly(2));
        }

        [Fact]
        public void LinkBySearch_NoObjectsFound_NoReferencesCreated()
        {
            // Arrange
            var refName = "uses";
            var refType = 99;
            var employeeNumber = "A221133";

            var mapping = new ComponentMapping<Employee>("Employee").WithKey(emp => emp.EmployeeNumber);
            var service = GetService(mapping, refName);

            var searchBuilder = GetSearchBuilder();
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28, EmployeeNumber = employeeNumber };
            var employeeComponent = new Component(employeeNumber, null, null) { Type = "Employee" };

            _searcherMock.Setup(s => s.Search(It.IsAny<SearchSpec>()))
                .Returns(Task.FromResult((IEnumerable<Component>)new List<Component>()));

            ParentChildRelation parentChildRelation = new ParentChildRelation(null, employee);
            _sessionMock.Setup(s => s.GetChildComponent(parentChildRelation))
                .Returns(employeeComponent);
            _sessionMock.Setup(s => s.GetReferenceTypeForName(refName))
                .Returns(refType);
            _sessionMock.Setup(s => s.GetAllSourceReferencesFromChild(parentChildRelation))
                .Returns(new List<Reference>());

            // Act
            service.LinkBySearch(searchBuilder, new List<ParentChildRelation> { parentChildRelation });

            // Assert
            _sessionMock.Verify(s => s.AddReference(It.IsAny<int>(), It.IsAny<Component>(), It.IsAny<Component>()), Times.Never);
        }

        private static SearchBuilder<Employee> GetSearchBuilder()
        {
            return MakeSearch<Employee>.WithComponentTypeAndFields(
                emp => "Country",
                emp =>
                {
                    var ageGroup = "old";
                    if (emp.Age <= 12)
                    {
                        ageGroup = "child";
                    }
                    else if (emp.Age <= 18)
                    {
                        ageGroup = "teenager";
                    }
                    else if (emp.Age <= 60)
                    {
                        ageGroup = "adult";
                    }

                    return new Dictionary<string, object> {["age_group"] = ageGroup};
                });
        }

        private SearchLinkageService GetService(IBuiltComponentMapping mapping, string refName)
        {
            return new SearchLinkageService(
                _readerMock.Object,
                _writerMock.Object,
                _sessionMock.Object,
                _searcherMock.Object,
                mapping,
                refName,
                _loggerMock.Object);
        }
    }
}
