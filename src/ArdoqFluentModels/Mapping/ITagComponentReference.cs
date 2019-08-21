using System.Collections.Generic;

namespace ArdoqFluentModels.Mapping
{
    public interface ITagComponentReference
    {
        (bool, IEnumerable<string>) GetTags(object sourceObject);
        string ArdoqReferenceName { get; }
        string GetSearchFolder();
    }
}
