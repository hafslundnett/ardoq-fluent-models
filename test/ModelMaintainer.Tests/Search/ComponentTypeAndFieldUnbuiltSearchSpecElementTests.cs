using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Search;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Search
{
    public class ComponentTypeAndFieldUnbuiltSearchSpecElementTests
    {
        [Fact]
        public void Build_WithJustComponentDefined_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetComponentTypeGetter(e => "Employee");
            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement) spec;
            Assert.Equal("Employee", cfe.ComponentType);
            Assert.Empty(cfe.FieldFilters);
        }

        [Fact]
        public void Build_WithJustFieldsDefined_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetFieldGetter(e => new Dictionary<string, object> { ["profession"] = "chess", ["nationality"] = "NOR" });
            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Null(cfe.ComponentType);
            Assert.Equal(2, cfe.FieldFilters.Count);
            Assert.Equal("chess", cfe.FieldFilters["profession"].ToString());
            Assert.Equal("NOR", cfe.FieldFilters["nationality"].ToString());
        }

        [Fact]
        public void Build_WithComponentTypeAndFieldsDefined_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();

            unbuilt.SetComponentTypeGetter(e => "Employee");
            unbuilt.SetFieldGetter(e => new Dictionary<string, object> { ["profession"] = "chess", ["nationality"] = "NOR" });

            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal("Employee", cfe.ComponentType);
            Assert.Equal(2, cfe.FieldFilters.Count);
            Assert.Equal("chess", cfe.FieldFilters["profession"].ToString());
            Assert.Equal("NOR", cfe.FieldFilters["nationality"].ToString());
        }

        [Fact]
        public void Build_WithHardcodedComponentType_ReturnsExpectedSpecElement()
        {
            // Arrange
            var hardCodedCompType = "Hardcoded";
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetHardcodedComponentType(hardCodedCompType);

            // Act
            var spec = unbuilt.Build(new Employee());

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal(hardCodedCompType, cfe.ComponentType);
        }

        [Fact]
        public void Build_WithHardcodedComponentTypeAndLambda_LambdaTakesPrecedence()
        {
            // Arrange
            var hardCodedCompType = "Hardcoded";
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();

            unbuilt.SetComponentTypeGetter(e => "Employee");
            unbuilt.SetHardcodedComponentType(hardCodedCompType);

            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal("Employee", cfe.ComponentType);
        }

        [Fact]
        public void Build_WithNameLambda_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetNameGetter(emp => new List<string> { emp.Name });

            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal("Dole Duck", cfe.NameList.Single());
        }

        [Fact]
        public void Build_WithHardcodedName_ReturnsExpectedSpecElement()
        {
            // Arrange
            var name = "MyName";
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetHardcodedName(name);

            // Act
            var spec = unbuilt.Build(new Employee());

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal(name, cfe.Name);
        }

        [Fact]
        public void Build_WithNameLambdaAndParentLambda_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new ComponentTypeAndFieldUnbuiltSearchSpecElement<Employee>();
            unbuilt.SetNameGetter(emp => new List<string> { emp.Name });
            unbuilt.SetParentNameGetter(emp => new List<string> { emp.EmployedIn.Name });

            var department = new Department { Name = "Hakkespettene" };
            var employee = new Employee { Name = "Dole Duck", Age = 12, EmployedIn = department };

            // Act
            var spec = unbuilt.Build(employee);

            // Assert
            Assert.True(spec is ComponentTypeAndFieldSearchSpecElement);
            var cfe = (ComponentTypeAndFieldSearchSpecElement)spec;
            Assert.Equal("Dole Duck", cfe.NameList.Single());
            Assert.Equal("Hakkespettene", cfe.ParentNameList.Single());
        }
    }
}
