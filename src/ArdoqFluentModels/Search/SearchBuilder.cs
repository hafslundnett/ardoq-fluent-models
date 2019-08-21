using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Search
{
    public class SearchBuilder<TSource> : ISearchBuilder
    {
        private readonly List<IUnbuiltSearchSpecElement> _unbuiltElements = new List<IUnbuiltSearchSpecElement>();
        private string _searchFolder;

        public SearchBuilder<TSource> WithTags(Func<TSource, IEnumerable<string>> lambda)
        {
            _unbuiltElements.Add(new TagUnbuiltSearchSpecElement<TSource>(lambda));
            return this;
        }

        public SearchBuilder<TSource> WithComponentType(Func<TSource, string> lambda)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetComponentTypeGetter(lambda);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchBuilder<TSource> WithComponentType(string componentType)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetHardcodedComponentType(componentType);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchBuilder<TSource> WithFields(Func<TSource, IDictionary<string, object>> lambda)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetFieldGetter(lambda);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchBuilder<TSource> WithComponentTypeAndFields(Func<TSource, string> componentTypeLambda, Func<TSource, IDictionary<string, object>> fieldLambda)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetComponentTypeGetter(componentTypeLambda);
            element.SetFieldGetter(fieldLambda);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchSpec BuildSearch(object obj)
        {
            var spec = new SearchSpec(_searchFolder);
            foreach (var unbuiltElement in _unbuiltElements)
            {
                spec.AddElement(unbuiltElement.Build(obj));
            }

            return spec;
        }

        public SearchBuilder<TSource> SearchFolder(string searchFolder)
        {
            _searchFolder = searchFolder;
            return this;
        }

        public SearchBuilder<TSource> WithName(Func<TSource, List<string>> nameLambda)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetNameGetter(nameLambda);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchBuilder<TSource> WithName(string name)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetHardcodedName(name);
            _unbuiltElements.Add(element);
            return this;
        }

        public SearchBuilder<TSource> WithParentName(Func<TSource, List<string>> parentNameLambda)
        {
            var element = new ComponentTypeAndFieldUnbuiltSearchSpecElement<TSource>();
            element.SetParentNameGetter(parentNameLambda);
            _unbuiltElements.Add(element);
            return this;
        }
    }
}
