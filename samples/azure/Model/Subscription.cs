using System.Collections.Generic;

namespace ModelMaintainer.Samples.Azure.Model
{
    /// <summary>
    /// This class is defined in "Hafslund.Ardoq.Azure" nuget package if you use this nuget package.
    /// </summary>
    /// 
    public class Subscription
    {
        public string Name { get; }
        public List<ResourceGroup> ResourceGroups { get; } = new List<ResourceGroup>();

        public Subscription(string name)
        {
            Name = name;
        }


        public IEnumerable<object> ToFlattenedStructure()
        {
            var list = new List<object>();
            list.Add(this);

            if (ResourceGroups != null)
            {
                foreach (var resourceGroup in ResourceGroups)
                {
                    resourceGroup.AppendToFlattenedStructure(list);
                }
            }

            return list;
        }
    }
}
