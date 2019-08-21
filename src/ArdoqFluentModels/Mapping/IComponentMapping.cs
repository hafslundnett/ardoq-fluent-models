using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ArdoqFluentModels.Mapping.ComponentHierarchy;
using ArdoqFluentModels.Search;

namespace ArdoqFluentModels.Mapping
{
    public interface IComponentMapping<TSource> : IBuiltComponentMapping
    {
        IComponentMapping<TSource> WithKey(Expression<Func<TSource, string>> lambda);
        IComponentMapping<TSource> WithField(Expression<Func<TSource, object>> lambda, string ardoqFieldName);
        
        IComponentMapping<TSource> WithTags(Expression<Func<TSource, IEnumerable<string>>> lambda);
        IComponentMapping<TSource> WithTags(ITagGetter<TSource> tagGetter);


        IComponentMapping<TSource> WithReference(Expression<Func<TSource, object>> lambda, string ardoqRefName);

        IComponentMapping<TSource> WithExternalReference(
            string targetComponentType,
            Func<TSource, string> componentNameGetter,
            string ardoqRefName,
            string externalWorkspaceName);

        IComponentMapping<TSource> WithSearchBasedReferences(SearchBuilder<TSource> builder, string ardoqReferenceName);

        IComponentMapping<TSource> WithPreexistingHierarchyReference(string name);

        IComponentMapping<TSource> WithModelledHierarchyReference(Expression<Func<TSource, object>> expression,
            ModelledReferenceDirection direction);

        IComponentMapping<TSource> WithTagReference(ITagComponentReference tagReference);
    }
}
