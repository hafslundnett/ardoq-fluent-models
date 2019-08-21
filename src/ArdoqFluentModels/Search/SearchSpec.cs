using System.Collections.Generic;

namespace ArdoqFluentModels.Search
{
    public class SearchSpec
    {
        private readonly List<ISearchSpecElement> _elements = new List<ISearchSpecElement>();

        public SearchSpec(string searchFolder)
        {
            SearchFolder = searchFolder;
        }

        public IList<ISearchSpecElement> Elements => _elements;

        public string SearchFolder { get; }

        public void AddElement(ISearchSpecElement element)
        {
            _elements.Add(element);
        }
    }
}
