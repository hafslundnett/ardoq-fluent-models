using ModelMaintainer.Mapping.ComponentHierarchy;
using System;
using System.Collections.Generic;
using System.Reflection;
using ModelMaintainer.Search;

namespace ModelMaintainer.Mapping
{
    public interface IBuiltComponentMapping
    {
        Type SourceType { get; }
        PropertyInfo KeyGetter { get; }

        string ArdoqComponentTypeName { get; }

        (string, IDictionary<string, object>) GetComponentInfo(object obj);

        Dictionary<(string, string), PropertyInfo> RefGetters { get; }

        IEnumerable<IExternalReferenceSpecification> ExternalReferenceSpecifications { get; }

        IEnumerable<NamedReferenceAccessSpecification> NamedHierarchyParentAccessSpecifications { get; }
        IEnumerable<IHierarchyReferenceAccessSpecification> ModelledHierarchyAccessSpecifications { get; }

        IEnumerable<ITagComponentReference> TagReferenceGetters { get; }

        string GetKeyForInstance(object obj);

        IEnumerable<string> GetTags(object obj);

        IEnumerable<(string, ISearchBuilder)> SearchReferenceBuilders { get; }
        IEnumerable<Type> GetChildrenTypes();
        Type GetParentType();
        IBuiltComponentMapping GetParent();
        void SetParent(IBuiltComponentMapping p);
    }
}
