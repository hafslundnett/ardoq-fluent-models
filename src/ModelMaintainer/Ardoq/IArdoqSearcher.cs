using System.Collections.Generic;
using System.Threading.Tasks;
using Ardoq.Models;
using ArdoqFluentModels.Search;

namespace ArdoqFluentModels.Ardoq
{
    public interface IArdoqSearcher
    {
        Task<IEnumerable<Component>> Search(SearchSpec spec);
    }
}
