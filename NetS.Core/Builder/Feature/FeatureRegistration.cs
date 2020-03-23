using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetS.Core.Utilities;

namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// Default implementation of a representation of a registered feature of the ApplicationHost.
    /// </summary>
    /// <typeparam name="TImplementation">Type of the registered feature class.</typeparam>
    public class FeatureRegistration<TImplementation> : IFeatureRegistration where TImplementation : class , IApplicationFeature
    {
        /// <summary>List of delegates to configure services of the feature.</summary>
        public readonly List<Action<IServiceCollection>> ConfigureServicesDelegates;

        /// <inheritdoc />
        public Type FeatureStartupType { get; private set; }

        /// <inheritdoc />
        public Type FeatureType { get; private set; }

        /// <summary> List of dependency features that should be registered in order to add this feature.</summary>
        private List<Type> _dependencies;

        public FeatureRegistration()
        {
            this.ConfigureServicesDelegates = new List<Action<IServiceCollection>>();
            this.FeatureType = typeof(TImplementation);

            this._dependencies = new List<Type>();
        }

        /// <inheritdoc />
        public void BuildFeature(IServiceCollection serviceCollection)
        {
            Guard.NotNull(serviceCollection, nameof(serviceCollection));

            //feature can only be singleton
            serviceCollection
                .AddSingleton(FeatureType)
                .AddSingleton(typeof(IApplicationFeature), provider => provider.GetService(FeatureType));

            foreach (var configureServicesDelegate in this.ConfigureServicesDelegates)
                configureServicesDelegate(serviceCollection);

            if (FeatureStartupType != null)
                FeatureStartup(serviceCollection, FeatureStartupType);
        }

        /// <inheritdoc />
        public IFeatureRegistration FeatureServices(Action<IServiceCollection> configureServices)
        {
            Guard.NotNull(configureServices, nameof(configureServices));

            this.ConfigureServicesDelegates.Add(configureServices);

            return this;
        }

        /// <inheritdoc />
        public IFeatureRegistration UseStartup<TStartup>()
        {
            FeatureStartupType = typeof(TStartup);

            return this;
        }

        /// <inheritdoc />
        public IFeatureRegistration DependOn<TFeatureImplementation>() where TFeatureImplementation : class, IApplicationFeature
        {
            this._dependencies.Add(typeof(TFeatureImplementation));

            return this;
        }

        /// <inheritdoc />
        public void EnsureDependencies(List<IFeatureRegistration> featureRegistrations)
        {
            foreach (Type dependency in this._dependencies)
            {
                if (featureRegistrations.All(x => !dependency.IsAssignableFrom(x.FeatureType)))
                    throw new MissingDependencyException($"Dependency feature {dependency.Name} cannot be found.");
            }
        }

        /// <summary>
        /// A feature can use specified method to configure its services.
        /// The specified method needs to have the following signature to be invoked:
        /// <c>void ConfigureServices(IServiceCollection serviceCollection)</c>.
        /// </summary>
        /// <param name="serviceCollection">Collection of service descriptors to be passed to the ConfigureServices method of the feature registration startup class.</param>
        /// <param name="startupType">Type of the feature registration startup class. If it implements ConfigureServices method, it is invoked to configure the feature's services.</param>
        private void FeatureStartup(IServiceCollection serviceCollection, Type startupType)
        {
            var method = startupType.GetMethod("ConfigureServices");
            var parameters = method?.GetParameters();
            if (method != null && method.IsStatic && (parameters?.Length == 1) && (parameters.First().ParameterType == typeof(IServiceCollection)))
                method.Invoke(null, new object[] { serviceCollection });
        }
    }
}
