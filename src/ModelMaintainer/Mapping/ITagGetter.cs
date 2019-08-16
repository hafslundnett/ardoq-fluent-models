using System.Collections.Generic;

namespace ModelMaintainer.Mapping
{
    public interface ITagGetter<T>
    {
        IEnumerable<string> GetTags(T obj);
    }
}
