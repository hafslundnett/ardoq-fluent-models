using System;
using System.Threading.Tasks;

namespace ModelMaintainer
{
    public interface IMaintainenceSession
    {
        Task Run(ISourceModelProvider modelProvider);

        string GetComponentType(Type sourceType);
        string GetKeyForInstance(object instance);

        bool IsSafeMode { get; }
    }
}
