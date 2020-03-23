using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using NetS.Core.AsyncWork;
using NetS.Core.Builder;
using NetS.Core.Builder.Feature;
using NetS.Core.Configuration;
using NetS.Core.Utilities;

namespace NetS.Core
{
    public class ApplicationHost : IApplicationHost
    {
        /// <summary>ApplicationHost life cycle control - triggers when application shuts down.</summary>
        private ApplicationLifeTime _applicationLifeTime;

        /// <inheritdoc />
        public IApplicationLifeTime ApplicationLifeTime
        {
            get => this._applicationLifeTime;
            set => this._applicationLifeTime = (ApplicationLifeTime) value;
        }

        public IDateTimeProvider DateTimeProvider { get; set; }

        /// <summary>Provider of notification about events.</summary>
        public ISignals Signals { get; set; }

        /// <inheritdoc />
        public ApplicationState State { get; private set; }

        public DateTime StartTime { get; private set; }

        /// <summary>Component responsible for starting and stopping all the application's features.</summary>
        private ApplicationFeatureExecutor _applicationFeatureExecutor;

        /// <summary>Instance logger.</summary>
        private ILogger _logger;

        public IConfiguration Configuration { get; private set; }

        internal ApplicationHostOptions Options { get; private set; }

        /// <summary>Contains path locations to folders and files on disk.</summary>
        public DataFolder DataFolder { get; set; }

        /// <see cref="IApplicationStats"/>
        private IApplicationStats ApplicationStats { get; set; }

        /// <summary>Factory for creating and execution of asynchronous loops.</summary>
        public IAsyncProvider AsyncProvider { get; set; }

        private IAsyncLoop _periodicLogLoop;

        private IAsyncLoop _periodicBenchmarkLoop;

        /// <inheritdoc />
        public IApplicationServiceProvider Services { get; set; }

        /// <inheritdoc />
        public Version Version
        {
            get
            {
                string versionString = typeof(ApplicationHost).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ??
                                       PlatformServices.Default.Application.ApplicationVersion;

                if (!string.IsNullOrEmpty(versionString))
                {
                    try
                    {
                        return new Version(versionString);
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (OverflowException)
                    {
                    }
                }

                return new Version(0, 0);
            }
        }

        public T ApplicationService<T>(bool faiWithDefault = false)
        {
            if (Services?.ServiceProvider != null)
            {
                var service = this.Services.ServiceProvider.GetService<T>();
                if (service != null)
                    return service;
            }

            if (faiWithDefault)
                return default(T);

            throw new InvalidOperationException($"The {typeof(T)} service is not supported");
        }

        public T ApplicationFeature<T>(bool faiWithDefault = false)
        {
            if (this.Services != null)
            {
                var feature = this.Services.Features.OfType<T>().FirstOrDefault();
                if (feature != null)
                    return feature;
            }

            if (faiWithDefault)
                return default(T);

            throw new InvalidOperationException($"The {typeof(T)} feature is not supported");
        }

        /// <inheritdoc />
        public IApplicationHost Initialize(IApplicationServiceProvider serviceProvider)
        {
            this.State = ApplicationState.Initializing;

            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.Services = serviceProvider;
            _logger = this.Services.ServiceProvider.GetService<ILoggerFactory>().CreateLogger(this.GetType().FullName);
            this.DataFolder = this.Services.ServiceProvider.GetService<DataFolder>();

            this.DateTimeProvider = this.Services.ServiceProvider.GetService<IDateTimeProvider>();

            Configuration = this.Services.ServiceProvider.GetService<IConfiguration>();
            Options = this.Services.ServiceProvider.GetService<ApplicationHostOptions>();

            this.Signals = this.Services.ServiceProvider.GetService<ISignals>();

            this.ApplicationStats = this.Services.ServiceProvider.GetService<IApplicationStats>();

            this.AsyncProvider = this.Services.ServiceProvider.GetService<IAsyncProvider>();

            this._logger.LogInformation($"ApplicationHost initialized.");

            this.State = ApplicationState.Initialized;
            this.StartTime = this.DateTimeProvider.GetUtcNow();
            return this;
        }

        /// <inheritdoc />
        public void Start()
        {
            this.State = ApplicationState.Starting;

            if (this.State == ApplicationState.Disposing || this.State == ApplicationState.Disposed)
                throw new ObjectDisposedException(nameof(ApplicationHost));

            this._applicationLifeTime = this.Services?.ServiceProvider.GetRequiredService<IApplicationLifeTime>() as ApplicationLifeTime;
            if (this._applicationLifeTime == null)
                throw new InvalidOperationException($"{nameof(IApplicationLifeTime)} must be set.");

            this._applicationFeatureExecutor = this.Services?.ServiceProvider.GetRequiredService<ApplicationFeatureExecutor>();
            if (this._applicationFeatureExecutor == null)
                throw new InvalidOperationException($"{nameof(ApplicationFeatureExecutor)} must be set.");

            this._logger.LogInformation("Starting application...");

            //start all registered features
            this._applicationFeatureExecutor.Initialize();

            // Fire IApplicationLifetime.Started.
            this._applicationLifeTime.NotifyStarted();

            this.StartPeriodicLog();

            this.State = ApplicationState.Started;
        }

        /// <summary>
        /// Starts a loop to periodically log statistics about application's status very couple of seconds.
        /// <para>
        /// These logs are also displayed on the console.
        /// </para>
        /// </summary>
        private void StartPeriodicLog()
        {
            this._periodicLogLoop = this.AsyncProvider.CreateAndRunAsyncLoop("PeriodicLog", (cancellation) =>
                {
                    string stats = this.ApplicationStats.GetStats();

                    this._logger.LogInformation(stats);
                    this.LastLogOutput = stats;

                    return Task.CompletedTask;
                },
                this._applicationLifeTime.ApplicationStopping,
                repeatEvery: TimeSpans.FiveSeconds,
                startAfter: TimeSpans.FiveSeconds);

            this._periodicBenchmarkLoop = this.AsyncProvider.CreateAndRunAsyncLoop("PeriodicBenchmarkLog", (cancellation) =>
                {
                    string benchmark = this.ApplicationStats.GetBenchmark();
                    this._logger.LogInformation(benchmark);

                    return Task.CompletedTask;
                },
                this._applicationLifeTime.ApplicationStopping,
                repeatEvery: TimeSpan.FromSeconds(17),
                startAfter: TimeSpan.FromSeconds(17));
        }

        public string LastLogOutput { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.State == ApplicationState.Disposing || this.State == ApplicationState.Disposed)
                return;

            this.State = ApplicationState.Disposing;

            this._logger.LogInformation("Closing application pending.");

            // Fire IApplicationLifetime.Stopping.
            this._applicationLifeTime.StopApplication();

            this._logger.LogInformation("Disposing periodic logging loops.");
            this._periodicLogLoop?.Dispose();
            this._periodicBenchmarkLoop?.Dispose();

            // Fire the ApplicationFeatureExecutor.Stop.
            this._logger.LogInformation("Disposing the ApplicationHost feature executor.");
            this._applicationFeatureExecutor?.Dispose();
            (this.Services.ServiceProvider as IDisposable)?.Dispose();

            // Fire IApplicationLifetime.Stopped.
            this._logger.LogInformation("Notify application has stopped.");
            this._applicationLifeTime.NotifyStopped();

            this.State = ApplicationState.Disposed;
        }
    }
}
