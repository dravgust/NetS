using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NetS.Core.Utilities
{
    public interface IPeriodicTask
    {
        string Name { get; }

        void RunOnce();

        IPeriodicTask Start(CancellationToken cancellation, TimeSpan refreshRate, bool delayStart = false);
    }

    public class PeriodicTask : IPeriodicTask
    {
        private readonly Action<CancellationToken> _loop;
        private readonly ILogger<PeriodicTask> _logger;

        public string Name { get; }
        
        public PeriodicTask(string name, Action<CancellationToken> loop, ILogger<PeriodicTask> logger = null)
        {
            Guard.NotEmpty(name, nameof(name));
            Guard.NotNull(loop, nameof(loop));

            this._loop = loop;
            this.Name = name;
            this._logger = logger;
        }

        public void RunOnce()
        {
            _loop(CancellationToken.None);
        }

        public IPeriodicTask Start(CancellationToken cancellation, TimeSpan refreshRate, bool delayStart = false)
        {
            new Thread(() =>
            {
                Exception uncatchException = null;
                _logger.LogDebug(Name + " starting");
                try
                {
                    if (delayStart)
                        cancellation.WaitHandle.WaitOne(refreshRate);

                    while (!cancellation.IsCancellationRequested)
                    {
                        _loop(cancellation);
                        cancellation.WaitHandle.WaitOne(refreshRate);
                    }
                }
                catch (OperationCanceledException e)
                {
                    if (!cancellation.IsCancellationRequested)
                        uncatchException = e;
                }
                catch (Exception e)
                {
                    uncatchException = e;
                }
                finally
                {
                    _logger.LogDebug(Name + " stopping");
                }

                if (uncatchException != null)
                    _logger.LogCritical(new EventId(0), uncatchException, Name + " threw an unhandled exception");
            })
            {
                IsBackground = true,
                Name = Name
            }.Start();

            return this;
        }
    }
}
