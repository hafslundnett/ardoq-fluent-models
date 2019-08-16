using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Search
{
    public class TagUnbuiltSearchSpecElement<TSource> : IUnbuiltSearchSpecElement
    {
        private readonly Func<TSource, IEnumerable<string>> _lambda;

        public TagUnbuiltSearchSpecElement(Func<TSource, IEnumerable<string>> lambda)
        {
            _lambda = lambda;
        }

        public ISearchSpecElement Build(object obj)
        {
            var element = new TagSearchSpecElement();
            element.AddTags(_lambda.Invoke((TSource)obj));
            return element;
        }
    }
}
