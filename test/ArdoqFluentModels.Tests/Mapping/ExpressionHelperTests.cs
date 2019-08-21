using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArdoqFluentModels.Mapping;
using ModelMaintainer.Tests.Model;
using Xunit;

namespace ModelMaintainer.Tests.Mapping
{
    public class ExpressionHelperTests
    {
        [Fact]
        public void GetMemberName_ForFunc_GetsExpectedName()
        {
            //// Arrange
            //var func = new Func<Employee, string>(e => e.EmployeeNumber);
            //Expression<Func<Employee, string>> expression =

            //// Act
            //var memberName = ExpressionHelper.GetMemberName();
        }

        [Fact]
        public void GetReferencedObjects_Null_ReturnsNull()
        {
            // Arrange
            var instance = new Employee();
            var pi = typeof(Employee).GetProperty("EmployedIn", BindingFlags.Instance | BindingFlags.Public);

            // Act
            var refs = ExpressionHelper.GetReferencedObjects(instance, pi.GetMethod);

            // Assert
            Assert.Null(refs);
        }

        [Fact]
        public void GetReferencedObjects_Undary_ReturnsExpectedList()
        {
            // Arrange
            var dep = new Department();
            var instance = new Employee{EmployedIn = dep};
            var pi = typeof(Employee).GetProperty("EmployedIn", BindingFlags.Instance | BindingFlags.Public);

            // Act
            var refs = ExpressionHelper.GetReferencedObjects(instance, pi.GetMethod);

            // Assert
            Assert.Single(refs);
            Assert.Same(dep, refs.First());
        }

        [Fact]
        public void GetReferencedObjects_List_ReturnsExpectedList()
        {
            // Arrange
            var r1 = new Role();
            var r2 = new Role();
            var instance = new Employee { Roles = new List<Role>{r1, r2}};
            var pi = typeof(Employee).GetProperty("Roles", BindingFlags.Instance | BindingFlags.Public);

            // Act
            var refs = ExpressionHelper.GetReferencedObjects(instance, pi.GetMethod);

            // Assert
            Assert.Equal(2, refs.Count());
        }
    }
}
