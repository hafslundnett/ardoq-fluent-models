using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMaintainer.Tests.Model
{
    public class Employee
    {
        public string EmployeeNumber { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public Department EmployedIn { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public Office Office { get; set; }

        public string GetAgeTag()
        {
            return Age.ToString();
        }
    }
}
