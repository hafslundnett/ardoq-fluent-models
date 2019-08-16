using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;

namespace ModelMaintainer.Search
{
    public abstract class SearchSpecElementBase : ISearchSpecElement
    {
        public IEnumerable<Component> Search(
            IEnumerable<Tag> allTags, 
            IEnumerable<Component> allComponents, 
            IEnumerable<Component> searchSpace = null)
        {
            var found =  SearchCore(allTags, allComponents);

            return searchSpace == null
                ? found
                : found.Intersect(searchSpace);
        }

        protected abstract IEnumerable<Component> SearchCore(
            IEnumerable<Tag> allTags,
            IEnumerable<Component> allComponents);
    }
}
