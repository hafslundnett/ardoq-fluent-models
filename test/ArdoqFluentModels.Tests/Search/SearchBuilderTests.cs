using System.Collections.Generic;
using System.Linq;
using ArdoqFluentModels.Search;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Search
{
    public class SearchBuilderTests
    {
        [Fact]
        public void BuildSearch_WithSingleTagElement_BuildsExpectedSpec()
        {
            // Arrange
            var builder = MakeSearch<Employee>.WithTags(emp =>
            {
                string tag = "average";

                if (emp.Age < 18)
                {
                    tag = "young";
                }
                else if (emp.Age >= 60)
                {
                    tag = "old";
                }

                return new List<string>{tag};
            });

            var employee = new Employee{Name = "Magnus Carlsen", Age = 28};

            // Act
            var spec = builder.BuildSearch(employee);

            // Assert
            Assert.NotNull(spec);
            Assert.Single(spec.Elements);
            Assert.True(spec.Elements.First() is TagSearchSpecElement);
        }

        [Fact]
        public void BuildSearch_WithComponentType_BuildsExpectedSpec()
        {
            // Arrange
            var builder = MakeSearch<Employee>.WithComponentType(e => "Employee");
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28 };

            // Act
            var spec = builder.BuildSearch(employee);

            // Assert
            Assert.NotNull(spec);
            Assert.Single(spec.Elements);
            Assert.True(spec.Elements.First() is ComponentTypeAndFieldSearchSpecElement);
        }

        [Fact]
        public void BuildSearch_WithFieldGetters_BuildsExpectedSpec()
        {
            // Arrange
            var builder = MakeSearch<Employee>.WithFields(e => new Dictionary<string, object>{["profession"] = "chess", ["nationality"] = "NOR"});
            var employee = new Employee { Name = "Magnus Carlsen", Age = 28 };

            // Act
            var spec = builder.BuildSearch(employee);

            // Assert
            Assert.NotNull(spec);
            Assert.Single(spec.Elements);
            Assert.True(spec.Elements.First() is ComponentTypeAndFieldSearchSpecElement);
        }

        [Fact]
        public void BuildSearch_WithComponentTypeAndFieldGetters_BuildsExpectedSpec()
        {
            // Arrange
            var builder = MakeSearch<Employee>.WithComponentTypeAndFields(
                e => "Employee",
                e => new Dictionary<string, object> { ["profession"] = "chess", ["nationality"] = "NOR" });

            var employee = new Employee { Name = "Magnus Carlsen", Age = 28 };

            // Act
            var spec = builder.BuildSearch(employee);

            // Assert
            Assert.NotNull(spec);
            Assert.Single(spec.Elements);
            Assert.True(spec.Elements.First() is ComponentTypeAndFieldSearchSpecElement);
        }
    }
}
