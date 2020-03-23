using System;
using System.Threading;

namespace NetS.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Allows consumers to perform cleanup during a graceful shutdown.
    /// Borrowed from asp.net core
    /// </summary>
    public class ApplicationLifeTime : IApplicationLifeTime
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();

        /// <summary>
        /// Triggered when the application host has fully started and is about to wait
        /// for a graceful shutdown.
        /// </summary>
        public CancellationToken ApplicationStarted
        {
            get { return this._startedSource.Token; }
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// Request may still be in flight. Shutdown will block until this event completes.
        /// </summary>
        public CancellationToken ApplicationStopping
        {
            get { return _stoppingSource.Token; }
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// All requests should be complete at this point. Shutdown will block
        /// until this event completes.
        /// </summary>
        public CancellationToken ApplicationStopped
        {
            get { return _stoppedSource.Token; }
        }

        /// <summary>
        /// Signals the ApplicationStopping event and blocks until it completes.
        /// </summary>
        public void StopApplication()
        {
            CancellationTokenSource stoppingSource = this._stoppingSource;
            var lockTaken = false;
            try
            {
                Monitor.Enter((object)stoppingSource, ref lockTaken);
                try
                {
                    this._stoppingSource.Cancel(false);
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                if(lockTaken)
                    Monitor.Exit((object)stoppingSource);
            }
        }

        /// <summary>
        /// Signals the ApplicationStarted event and blocks until it completes.
        /// </summary>
        public void NotifyStarted()
        {
            try
            {
                this._startedSource.Cancel(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Signals the ApplicationStopped event and blocks until it completes.
        /// </summary>
        public void NotifyStopped()
        {
            try
            {
                this._stoppedSource.Cancel(false);
            }
            catch (Exception)
            {
            }
        }
    }
}
