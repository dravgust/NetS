using Microsoft.Extensions.Logging;

namespace NetS.Core.Logging
{
    public class CustomConsoleLogProvider : ILoggerProvider
    {
        ConsoleLoggerProcessor _Processor;
        public CustomConsoleLogProvider(ConsoleLoggerProcessor processor)
        {
            _Processor = processor;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomerConsoleLogger(categoryName, (a, b) => true, null, _Processor);
        }

        public void Dispose()
        {
        }
    }
}
