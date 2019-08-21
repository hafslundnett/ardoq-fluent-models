using System.Collections.Generic;
using Ardoq.Models;

namespace ArdoqFluentModels.Search
{
    public interface ISearchSpecElement
    {
        IEnumerable<Component> Search(
            IEnumerable<Tag> allTags, 
            IEnumerable<Component> allComponents, 
            IEnumerable<Component> searchSpace = null);
    }
}
