using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Console;
using System.Threading;
using System.Threading.Tasks;

namespace NetS.Core.Logging
{
    public class ConsoleLoggerProcessor : IDisposable
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<LogMessageEntry> _messageQueue = new BlockingCollection<LogMessageEntry>(_maxQueuedMessages);
        private readonly Task _outputTask;

        public IConsole Console;

        public ConsoleLoggerProcessor()
        {
            // Start Console message queue processor
            _outputTask = Task.Factory.StartNew(
                ProcessLogQueue,
                state: this,
                cancellationToken: default(CancellationToken),
                creationOptions: TaskCreationOptions.LongRunning, scheduler: TaskScheduler.Default);
        }

        public virtual void EnqueueMessage(LogMessageEntry message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            // Adding is completed so just log the message
            WriteMessage(message);
        }

        // for testing
        internal virtual void WriteMessage(LogMessageEntry message)
        {
            if (message.LevelString != null)
            {
                Console.Write(message.LevelString, message.LevelBackground, message.LevelForeground);
            }

            Console.Write(message.Message, message.MessageColor, message.MessageColor);
            Console.Flush();
        }

        private void ProcessLogQueue()
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }

        private static void ProcessLogQueue(object state)
        {
            var consoleLogger = (ConsoleLoggerProcessor)state;

            consoleLogger.ProcessLogQueue();
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _outputTask.Wait(1500); // with timeout in-case Console is locked by user input
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }
    }
}
