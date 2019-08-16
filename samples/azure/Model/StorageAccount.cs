using System.Collections.Generic;

namespace ModelMaintainer.Samples.Azure.Model
{
    public class StorageAccount
    {
        public string Name { get; }
        public List<string> Tags { get; internal set; }

        public StorageAccount(string name)
        {
            Name = name;
        }
    }
}
