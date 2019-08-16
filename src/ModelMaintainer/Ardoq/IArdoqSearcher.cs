using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ardoq.Models;
using ModelMaintainer.Search;

namespace ModelMaintainer.Ardoq
{
    public interface IArdoqSearcher
    {
        Task<IEnumerable<Component>> Search(SearchSpec spec);
    }
}
