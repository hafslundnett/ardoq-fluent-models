using System.Collections.Generic;

namespace ArdoqFluentModels.Mapping
{
    public interface ITagGetter<T>
    {
        IEnumerable<string> GetTags(T obj);
    }
}
