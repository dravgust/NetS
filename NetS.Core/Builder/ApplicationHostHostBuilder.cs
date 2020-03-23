using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetS.Core.Builder.Feature;
using NetS.Core.Configuration;
using NetS.Core.Utilities;

namespace NetS.Core.Builder
{
    /// <summary>
    /// ApplicationHost builder allows constructing a application using specific components.
    /// </summary>
    public class ApplicationHostHostBuilder : IApplicationHostBuilder
    {
        /// <summary>List of delegates that configure the service providers.</summary>
        private readonly List<Action<IServiceProvider>> _configureDelegates;

        /// <summary>List of delegates that add services to the builder.</summary>
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;

        /// <summary>List of delegates that add features to the collection.</summary>
        private readonly List<Action<IFeatureCollection>> _featuresRegistrationDelegates;

        private readonly List<Action<ILoggerFactory>> _configureLoggingDelegates;

        private readonly IConfiguration _configuration;

        private ILoggerFactory _loggerFactory;

        private bool _applicationBuilt;

        public ApplicationHostHostBuilder() :
            this(new List<Action<IServiceCollection>>(),
                new List<Action<IServiceProvider>>(),
                new List<Action<IFeatureCollection>>(),
                new List<Action<ILoggerFactory>>())
        {

        }

        internal ApplicationHostHostBuilder(List<Action<IServiceCollection>> configureServicesDelegates, List<Action<IServiceProvider>> configureDelegates,
            List<Action<IFeatureCollection>> featuresRegistrationDelegates, List<Action<ILoggerFactory>> configureLoggingDelegates)
        {
            this._configureDelegates = Guard.NotNull(configureDelegates, nameof(configureDelegates));
            this._configureServicesDelegates = Guard.NotNull(configureServicesDelegates, nameof(configureServicesDelegates));
            this._featuresRegistrationDelegates = Guard.NotNull(featuresRegistrationDelegates, nameof(featuresRegistrationDelegates));
            this._configureLoggingDelegates = Guard.NotNull(configureLoggingDelegates, nameof(configureLoggingDelegates));

            this._configuration = (IConfiguration) new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
        }

        /// <summary>
        /// Specify the ILoggerFactory to be used by the web host.
        /// </summary>
        /// <param name="loggerFactory">The ILoggerFactory to be used.</param>
        /// <returns>The IApplicationHostBuilder.</returns>
        public IApplicationHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this._loggerFactory = loggerFactory;
            return (IApplicationHostBuilder)this;
        }

        /// <summary>
        /// Adds a delegate for configuring the provided ILoggerFactory. This may be called multiple times.
        /// </summary>
        /// <param name="configureLogging">The delegate that configures the ILoggerFactory.</param>
        /// <returns>The IApplicationHostBuilder.</returns>
        public IApplicationHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            Guard.NotNull(configureLogging, nameof(configureLogging));

            this._configureLoggingDelegates.Add(configureLogging);
            return (IApplicationHostBuilder)this;
        }

        /// <summary>
        /// Adds features to the builder. 
        /// </summary>
        /// <param name="configureFeatures">A method that adds features to the collection</param>
        /// <returns>An IApplicationHostBuilder</returns>
        public IApplicationHostBuilder ConfigureFeature(Action<IFeatureCollection> configureFeatures)
        {
            Guard.NotNull(configureFeatures, nameof(configureFeatures));

            _featuresRegistrationDelegates.Add(configureFeatures);
            return (IApplicationHostBuilder)this;
        }

        /// <summary>
		/// Add configurations for the service provider.
		/// </summary>
		/// <param name="configure">A method that configures the service provider.</param>
		/// <returns>An IApplicationHostBuilder</returns>
		public IApplicationHostBuilder ConfigureServiceProvider(Action<IServiceProvider> configure)
        {
            Guard.NotNull(configure, nameof(configure));

            _configureDelegates.Add(configure);
            return (IApplicationHostBuilder)this;
        }

        /// <summary>Add or replace a setting in the configuration.</summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The IApplicationHostBuilder.</returns>
        public IApplicationHostBuilder UseSetting(string key, string value)
        {
            this._configuration[key] = value;
            return (IApplicationHostBuilder)this;
        }

        // <summary>Get the setting value from the configuration.</summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        public string GetSetting(string key)
        {
            return this._configuration[key];
        }

        /// <summary>
		/// Adds services to the builder. 
		/// </summary>
		/// <param name="configureServices">A method that adds services to the builder</param>
		/// <returns>An IApplicationHostBuilder</returns>
        public IApplicationHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            Guard.NotNull(configureServices, nameof(configureServices));

            _configureServicesDelegates.Add(configureServices);
            return (IApplicationHostBuilder)this;
        }

        public IApplicationHost Build()
        {
            if (_applicationBuilt)
                throw new InvalidOperationException("The ApplicationHost already built");
            _applicationBuilt = true;

            var (services, features) = this.BuildServicesAndFeatures();

            var serviceProvider = services.BuildServiceProvider();
            this.ConfigureServices(serviceProvider);

            var application = serviceProvider.GetService<IApplicationHost>();
            if (application == null)
                throw new InvalidOperationException($"{nameof(IApplicationHost)} not registered with provider");

            application.Initialize(new ApplicationServiceProvider(serviceProvider,
                features.FeatureRegistrations.Select(s => s.FeatureType).ToList()));

            return application;
        }

        /// <summary>
        /// Constructs and configures services and features to be used by the application.
        /// </summary>
        /// <returns>Collection of registered services and features.</returns>
        private (IServiceCollection services, IFeatureCollection features) BuildServicesAndFeatures()
        {
            var services = (IServiceCollection) new ServiceCollection();

            if (this._loggerFactory == null)
            {
                this._loggerFactory = (ILoggerFactory) new LoggerFactory();
            }
            services.AddSingleton<ILoggerFactory>(this._loggerFactory);

            // configure logger before services
            foreach (var configureLogging in this._configureLoggingDelegates)
                configureLogging(this._loggerFactory);
            services.AddLogging();

            services.AddSingleton<IConfiguration>(this._configuration);

            //configure options
            services.AddOptions();
            var options = new ApplicationHostOptions(_configuration, Assembly.GetEntryAssembly()?.GetName().Name);
            services.AddSingleton<ApplicationHostOptions>(options);

            // register services before features 
            // as some of the features may depend on independent services
            foreach (var configureServices in this._configureServicesDelegates)
                configureServices(services);

            var features = (IFeatureCollection)new FeatureCollection();
            // configure features
            foreach (var configureFeature in this._featuresRegistrationDelegates)
                configureFeature(features);

            // configure features startup
            foreach (IFeatureRegistration featureRegistration in features.FeatureRegistrations)
            {
                try
                {
                    featureRegistration.EnsureDependencies(features.FeatureRegistrations);
                }
                catch (MissingDependencyException e)
                {
                    //this.logger.LogCritical("Feature {0} cannot be configured because it depends on other features that were not registered",
                    //    featureRegistration.FeatureType.Name);

                    throw e;
                }

                featureRegistration.BuildFeature(services);
            }

            return (services, features);
        }

        /// <summary>
        /// Configure registered services.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void ConfigureServices(IServiceProvider serviceProvider)
        {
            // configure registered services
            foreach (var configure in _configureDelegates)
                configure(serviceProvider);
        }
    }
}
