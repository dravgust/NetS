using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// Defines methods for a representation of registered features of the FullNode.
    /// </summary>
    public interface IFeatureRegistration
    {
        /// <summary>
        /// Type of the feature startup class. If it implements ConfigureServices method,
        /// it is invoked to configure the feature's services.
        /// </summary>
        Type FeatureStartupType { get; }

        /// <summary>Type of the feature class.</summary>
        Type FeatureType { get; }

        /// <summary>
        /// Initializes feature registration DI services and calls configuration delegates of each service
        /// and the startup type.
        /// </summary>
        /// <param name="serviceCollection">Collection of feature registration's DI services.</param>
        void BuildFeature(IServiceCollection serviceCollection);

        /// <summary>
        /// Initializes the list of delegates to configure DI services of the feature registration.
        /// </summary>
        /// <param name="configureServices">List of delegates to configure DI services of the feature registration.</param>
        /// <returns>This interface to allow fluent code.</returns>
        IFeatureRegistration FeatureServices(Action<IServiceCollection> configureServices);

        /// <summary>
        /// Sets the specific startup type to be used by the feature registration.
        /// </summary>
        /// <typeparam name="TStartup">Type of feature startup class to use.</typeparam>
        /// <returns>This interface to allow fluent code.</returns>
        IFeatureRegistration UseStartup<TStartup>();

        /// <summary>
        /// Adds a feature type to the dependency feature list.
        /// </summary>
        /// <typeparam name="TImplementation">Type of the registered feature class.</typeparam>
        /// <returns>This interface to allow fluent code.</returns>
        IFeatureRegistration DependOn<TImplementation>() where TImplementation : class, IApplicationFeature;

        /// <summary>
        /// Ensures dependency feature types are present in the registered features list.
        /// </summary>
        /// <param name="featureRegistrations">List of registered features.</param>
        /// <exception cref="MissingDependencyException">Thrown if feature type is missing.</exception>
        void EnsureDependencies(List<IFeatureRegistration> featureRegistrations);
    }
}
