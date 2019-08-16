using System;

namespace ModelMaintainer.Mapping
{
    public interface IExternalReferenceSpecification
    {
        string TargetComponentType { get; }
        string WorkspaceName { get; }
        Type SourceType { get; }
        string ReferenceName { get; }
        string GetTargetComponentKey(object obj);
    }
}
