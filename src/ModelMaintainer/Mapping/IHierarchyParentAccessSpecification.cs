using System;
using System.Collections.Generic;
using ModelMaintainer.Mapping.ComponentHierarchy;

namespace ModelMaintainer.Mapping
{
    public interface IHierarchyReferenceAccessSpecification
    {
        IEnumerable<(object, ModelledReferenceDirection)> GetRelatedObjects(object obj);

        Type GetRelatedType();

        bool IsChild { get; }
    }
}
