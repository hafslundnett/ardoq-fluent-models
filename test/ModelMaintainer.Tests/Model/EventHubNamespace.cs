using System.Collections.Generic;

namespace ModelMaintainer.Tests.Model
{
    public class EventHubNamespace
    {
        public string Name { get; set; }
        public ResourceGroup ResourceGroup { get; set; }
        public IEnumerable<EventHub> EventHubs { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IDictionary<string, string> TagMap { get; set; }
    }
}
