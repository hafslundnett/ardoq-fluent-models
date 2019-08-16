using System.Collections.Generic;
using Ardoq;
using Ardoq.Models;

namespace ModelMaintainer.Search
{
    public interface ISearchSpecElement
    {
        IEnumerable<Component> Search(
            IEnumerable<Tag> allTags, 
            IEnumerable<Component> allComponents, 
            IEnumerable<Component> searchSpace = null);
    }
}
