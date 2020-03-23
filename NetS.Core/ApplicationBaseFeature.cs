using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetS.Core.AsyncWork;
using NetS.Core.Builder;
using NetS.Core.Builder.Feature;
using NetS.Core.Configuration;
using NetS.Core.EventBus;
using NetS.Core.Utilities;

namespace NetS.Core
{
    public class ApplicationBaseFeature : ApplicationFeature
    {
        /// <summary>Global application life cycle control - triggers when application shuts down.</summary>
        private readonly IApplicationLifeTime _appLifetime;

        /// <summary>Logger for the ApplicationHost.</summary>
        private readonly ILogger _logger;

        /// <summary>Provider for creating and managing background async loop tasks.</summary>
        private readonly IAsyncProvider _asyncProvider;

        /// <summary>Factory for creating loggers.</summary>
        private readonly ILoggerFactory _loggerFactory;

        private readonly ApplicationHostOptions _options;

        private readonly IApplicationStats _appStats;

        public ApplicationBaseFeature(ApplicationHostOptions options, IApplicationLifeTime appLifetime,
            IDateTimeProvider dateTimeProvider, IAsyncProvider asyncProvider, ILoggerFactory loggerFactory,
            IApplicationStats appStats)
        {
            this._options = Guard.NotNull(options, nameof(options));
            this._appLifetime = Guard.NotNull(appLifetime, nameof(appLifetime));

            this._appStats = appStats;
            this._asyncProvider = asyncProvider;
            this._loggerFactory = loggerFactory;

            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        public override Task InitializeAsync()
        {
            this._appStats.RegisterStats(sb => 
                sb.Append(this._asyncProvider.GetStatistics(!this._options.DebugArgs.Any())), StatsType.Component, this.GetType().Name, 100);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    /// <summary>
    /// A class providing extension methods for <see cref="IApplicationHostBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtension
    {
        /// <summary>
        /// Makes the application use all the required features - <see cref="ApplicationBaseFeature"/>.
        /// </summary>
        /// <param name="applicationHostBuilder">Builder responsible for creating the application.</param>
        /// <returns>ApplicationHost builder's interface to allow fluent code.</returns>
        public static IApplicationHostBuilder UseBaseFeature(this IApplicationHostBuilder applicationHostBuilder)
        {
            applicationHostBuilder.ConfigureFeature(features =>
            {
                features
                    .AddFeature<ApplicationBaseFeature>()
                    .FeatureServices(services =>
                    { 
                        services.AddSingleton<IApplicationLifeTime, ApplicationLifeTime>();
                        services.AddSingleton<ApplicationFeatureExecutor>();
                        services.AddSingleton<IApplicationHost, ApplicationHost>();
                        services.AddSingleton<ISubscriptionErrorHandler, DefaultSubscriptionErrorHandler>();
                        services.AddSingleton<ISignals, Signals>();
                        services.AddSingleton<IDateTimeProvider>(DateTimeProvider.Default);
                        services.AddSingleton<IAsyncProvider, AsyncProvider>();
                        services.AddSingleton<IApplicationStats, ApplicationStats>();
                    });
            });

            return applicationHostBuilder;
        }
    }
}
