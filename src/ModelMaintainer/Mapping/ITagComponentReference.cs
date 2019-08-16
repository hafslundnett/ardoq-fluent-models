using System.Collections.Generic;

namespace ModelMaintainer.Mapping
{
    public interface ITagComponentReference
    {
        (bool, IEnumerable<string>) GetTags(object sourceObject);
        string ArdoqReferenceName { get; }
        string GetSearchFolder();
    }
}
