using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModelMaintainer.Mapping;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Mapping
{
    public class TagComponentReferenceTests
    {
        [Fact]
        public void GetTags_WithInternalError_ReturnsFalseAndNullTags()
        {
            // Arrange
            var tcr = new TagComponentReference<Employee>("pointer-1");
            tcr
                .AddReferenceTagGetter(emp => emp.EmployedIn.Name.ToLower())
                .AddReferenceTagGetter(emp => emp.GetAgeTag());

            var employee = new Employee
            {
                Age = 66
            };

            // Act
            var tuple = tcr.GetTags(employee);

            // Assert
            Assert.False(tuple.Item1);
            Assert.Null(tuple.Item2);
        }

        [Fact]
        public void GetTags_WithSourceObject_ReturnsExpectedTags()
        {
            // Arrange
            var tcr = new TagComponentReference<Employee>("pointer-1");
            tcr
                .AddReferenceTagGetter(emp => emp.EmployedIn.Name.ToLower())
                .AddReferenceTagGetter(emp => emp.GetAgeTag());

            var employee = new Employee
            {
                Age = 66,
                EmployedIn = new Department { Name = "TopManagement" }
            };

            // Act
            var tuple = tcr.GetTags(employee);

            // Assert
            Assert.True(tuple.Item1);
            var tags = tuple.Item2;
            Assert.Equal(2, tags.Count());
            Assert.Contains("66", tags);
            Assert.Contains("topmanagement", tags);
        }

        [Fact]
        public void GetTags_WithEmptyTag_ReturnsExpectedTags()
        {
            // Arrange
            var tcr = new TagComponentReference<Employee>("pointer-1");
            tcr
                .AddReferenceTagGetter(emp => emp.EmployedIn.Name.ToLower())
                .AddReferenceTagGetter(emp => emp.GetAgeTag());

            var employee = new Employee
            {
                Age = 66,
                EmployedIn = new Department { Name = "" }
            };

            // Act
            var tuple = tcr.GetTags(employee);

            // Assert
            Assert.False(tuple.Item1);
            Assert.Null(tuple.Item2);
        }
    }
}
