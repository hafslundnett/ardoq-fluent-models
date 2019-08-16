using System;

namespace ArdoqFluentModels
{
    public class ConsoleLogger : ILogger
    {
        public void Flush()
        {
        }

        public void LogDebug(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogException(Exception ex)
        {
            Console.WriteLine(ex);
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.WriteLine(message);
        }
    }
}
