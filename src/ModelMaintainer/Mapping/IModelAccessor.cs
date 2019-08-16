using System;

namespace ModelMaintainer.Mapping
{
    public interface IModelAccessor
    {
        string GetComponentType(Type type);
    }
}
