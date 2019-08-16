using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMaintainer.Samples.Azure.Model
{
    public class SqlServer
    {
        public string Name { get; }

        public SqlServer(string name)
        {
            Name = name;
        }

        public List<SqlDatabase> Databases { get; } = new List<SqlDatabase>();
        
        public void AppendToFlattenedStructure(List<object> list)
        {
            list.Add(this);
            list.AddRange(Databases);
        }
    }
}
