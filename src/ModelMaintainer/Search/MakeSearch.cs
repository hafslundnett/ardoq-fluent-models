using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Search
{
    public static class MakeSearch<TSource>
    {
        public static SearchBuilder<TSource> WithTags(Func<TSource, IEnumerable<string>> lambda)
        {
            return new SearchBuilder<TSource>().WithTags(lambda);
        }

        public static SearchBuilder<TSource> WithComponentType(Func<TSource, string> lambda)
        {
            return new SearchBuilder<TSource>().WithComponentType(lambda);
        }

        public static SearchBuilder<TSource> WithComponentType(string componentType)
        {
            return new SearchBuilder<TSource>().WithComponentType(componentType);
        }

        public static SearchBuilder<TSource> WithFields(Func<TSource, IDictionary<string, object>> lambda)
        {
            return new SearchBuilder<TSource>().WithFields(lambda);
        }

        public static SearchBuilder<TSource> WithComponentTypeAndFields(
            Func<TSource, string> componentTypeLambda,
            Func<TSource, IDictionary<string, object>> fieldLambda)
        {
            return new SearchBuilder<TSource>().WithComponentTypeAndFields(componentTypeLambda, fieldLambda);
        }

        public static SearchBuilder<TSource> WithName(Func<TSource, List<string>> nameLambda)
        {
            return new SearchBuilder<TSource>().WithName(nameLambda);
        }

        public static SearchBuilder<TSource> WithName(string name)
        {
            return new SearchBuilder<TSource>().WithName(name);
        }

        public static SearchBuilder<TSource> WithParentName(Func<TSource, List<string>> parentNameLambda)
        {
            return new SearchBuilder<TSource>().WithParentName(parentNameLambda);
        }
    }
}
