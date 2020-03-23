using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetS.Core.Utilities;

namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// Starts and stops all features registered with a application.
    /// </summary>
    /// <remarks>Borrowed from ASP.NET.</remarks>
    public class ApplicationFeatureExecutor : IApplicationFeatureExecutor
    {
        /// <summary>ApplicationHost which features are to be managed by this executor.</summary>
        private readonly IApplicationHost _app;

        /// <summary>Object logger.</summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes an instance of the object with specific full node and logger factory.
        /// </summary>
        /// <param name="application">Full node which features are to be managed by this executor.</param>
        /// <param name="loggerFactory">Factory to be used to create logger for the object.</param>
        public ApplicationFeatureExecutor(IApplicationHost app, ILoggerFactory loggerFactory)
        {
            Guard.NotNull(app, nameof(app));

            this._app = app;
            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            try
            {
                this.Execute(service => service.ValidateDependencies(this._app.Services));

                this.Execute(service =>
                {
                    service.State = "Initializing";
                    service.InitializeAsync().GetAwaiter().GetResult();
                    service.State = "Initialized";
                });
            }
            catch
            {
                this._logger.LogError("An error occurred starting the application.");
                this._logger.LogTrace("(-)[INITIALIZE_EXCEPTION]");
                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                this.Execute(feature =>
                {
                    feature.State = "Disposing";
                    feature.Dispose();
                    feature.State = "Disposed";
                }, true);
            }
            catch
            {
                this._logger.LogError("An error occurred stopping the application.");
                this._logger.LogTrace("(-)[DISPOSE_EXCEPTION]");
                throw;
            }
        }

        /// <summary>
        /// Executes start or stop method of all the features registered with the associated application.
        /// </summary>
        /// <param name="callback">Delegate to run start or stop method of the feature.</param>
        /// <param name="disposing">Reverse the order of which the features are executed.</param>
        /// <exception cref="AggregateException">Thrown in case one or more callbacks threw an exception.</exception>
        private void Execute(Action<IApplicationFeature> callback, bool disposing = false)
        {
            if (this._app.Services == null)
            {
                this._logger.LogTrace("(-)[NO_SERVICES]");
                return;
            }

            List<Exception> exceptions = null;

            if (disposing)
            {
                // When the application is shutting down, we need to dispose all features, so we don't break on exception.
                foreach (IApplicationFeature feature in this._app.Services.Features.Reverse())
                {
                    try
                    {
                        callback(feature);
                    }
                    catch (Exception exception)
                    {
                        if (exceptions == null)
                            exceptions = new List<Exception>();

                        this.LogAndAddException(exceptions, exception);
                    }
                }
            }
            else
            {
                // When the application is starting we don't continue initialization when an exception occurs.
                try
                {
                    // Initialize features that are flagged to start before the base feature.
                    foreach (IApplicationFeature feature in this._app.Services.Features.OrderByDescending(f => f.InitializeBeforeBase))
                    {
                        callback(feature);
                    }
                }
                catch (Exception exception)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();

                    this.LogAndAddException(exceptions, exception);
                }
            }

            // Throw an aggregate exception if there were any exceptions.
            if (exceptions != null)
            {
                this._logger.LogTrace("(-)[EXECUTION_FAILED]");
                throw new AggregateException(exceptions);
            }
        }

        private void LogAndAddException(List<Exception> exceptions, Exception exception)
        {
            exceptions.Add(exception);

            this._logger.LogError("An error occurred: '{0}'", exception.ToString());
        }
    }
}
