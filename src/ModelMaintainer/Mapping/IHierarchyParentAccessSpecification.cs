using ArdoqFluentModels.Mapping.ComponentHierarchy;
using System;
using System.Collections.Generic;

namespace ArdoqFluentModels.Mapping
{
    public interface IHierarchyReferenceAccessSpecification
    {
        IEnumerable<(object, ModelledReferenceDirection)> GetRelatedObjects(object obj);

        Type GetRelatedType();

        bool IsChild { get; }
    }
}
