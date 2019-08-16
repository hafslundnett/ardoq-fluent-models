using System.Collections.Generic;
using System.Linq;
using ModelMaintainer.Search;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Search
{
    public class TagUnbuiltSearchSpecElementTests
    {
        [Fact]
        public void Build_HappyDays_ReturnsExpectedSpecElement()
        {
            // Arrange
            var unbuilt = new TagUnbuiltSearchSpecElement<Employee>(
                emp =>
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

                    return new List<string> { tag };
                });

            var employee = new Employee { Name = "Dole Duck", Age = 12 };

            // Act
            var spec = unbuilt.Build(employee);
            var tagSpec = (TagSearchSpecElement) spec;
            Assert.Single(tagSpec.Tags);
            Assert.Equal("young", tagSpec.Tags.First());
        }
    }
}
