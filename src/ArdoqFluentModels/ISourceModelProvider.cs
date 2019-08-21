using System.Collections.Generic;

namespace ArdoqFluentModels
{
    public interface ISourceModelProvider
    {
        IEnumerable<object> GetSourceModel();
    }
}
