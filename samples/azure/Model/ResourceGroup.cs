using System.Collections.Generic;

namespace ModelMaintainer.Samples.Azure.Model
{
    public class ResourceGroup
    {
        public string Name { get; }

        public List<SqlServer> SqlServers { get; } = new List<SqlServer>();

        public List<StorageAccount> StorageAccounts { get; } = new List<StorageAccount>();
        public IEnumerable<string> Tags { get; internal set; }

        public ResourceGroup(string name)
        {
            Name = name;
        }
        
        public void AppendToFlattenedStructure(List<object> list)
        {
            list.Add(this);

            foreach (var server in SqlServers)
            {
                server.AppendToFlattenedStructure(list);
            }

            list.AddRange(StorageAccounts);
        }
    }
}
