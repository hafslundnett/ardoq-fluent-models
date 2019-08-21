using System;

namespace ArdoqFluentModels
{
    public interface ILogger
    {
        void LogMessage(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception ex);
        void LogDebug(string message);
        void Flush();
    }
}
