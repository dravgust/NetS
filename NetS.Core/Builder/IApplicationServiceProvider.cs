using System;
using System.Collections.Generic;
using NetS.Core.Builder.Feature;

namespace NetS.Core.Builder
{
    /// <summary>
    /// Provider of access to services and features registered with the application.
    /// </summary>
    public interface IApplicationServiceProvider
    {
        /// <summary>List of registered features.</summary>
        IEnumerable<IApplicationFeature> Features { get; }

        /// <summary>Provider to registered services.</summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Determines whether the service of the specified type T is registered.
        /// </summary>
        /// <typeparam name="T">A type to query against the service provider.</typeparam>
        /// <returns>
        ///   <c>true</c> if this instance is registered; otherwise, <c>false</c>.
        /// </returns>
        bool IsServiceRegistered<T>();

        /// <summary>
        /// Guard method that check whether the service of the specified type T is registered.
        /// If it doesn't exists, thrown an exception.
        /// </summary>
        /// <typeparam name="T">A type to query against the service provider.</typeparam>
        void EnsureServiceIsRegistered<T>();
    }
}
