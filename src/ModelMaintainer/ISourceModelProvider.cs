using System.Collections.Generic;

namespace ModelMaintainer
{
    public interface ISourceModelProvider
    {
        IEnumerable<object> GetSourceModel();
    }
}
