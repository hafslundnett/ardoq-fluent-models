using System;

namespace ArdoqFluentModels.Mapping
{
    public interface IModelAccessor
    {
        string GetComponentType(Type type);
    }
}
