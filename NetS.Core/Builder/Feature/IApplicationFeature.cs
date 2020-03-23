using System;
using System.Threading.Tasks;

namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// Defines methods for features that are managed by the ApplicationHost.
    /// </summary>
    public interface IApplicationFeature : IDisposable
    {
        /// <summary>
        /// Instructs the <see cref="ApplicationFeatureExecutor"/> to start this feature before the <see cref="ApplicationBaseFeature"/>.
        /// </summary>
        bool InitializeBeforeBase { get; set; }

        /// <summary>
        /// The state in which the feature currently is.
        /// </summary>
        string State { get; set; }

        /// <summary>
        /// Triggered when the FullNode host has fully started.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Validates the feature's required dependencies are all present.
        /// </summary>
        /// <exception cref="MissingDependencyException">should be thrown if dependency is missing</exception>
        /// <param name="services">Services and features registered to node.</param>
        void ValidateDependencies(IApplicationServiceProvider services);
    }
}
