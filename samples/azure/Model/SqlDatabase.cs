using System.Collections.Generic;

namespace ModelMaintainer.Samples.Azure.Model
{
    public class SqlDatabase
    {
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; internal set; }
        public string Uri { get; internal set; }

        public SqlDatabase(string name)
        {
            Name = name;
        }
    }
}
