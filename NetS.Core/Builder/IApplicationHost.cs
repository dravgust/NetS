using System;
using Microsoft.Extensions.Configuration;

namespace NetS.Core.Builder
{
    /// <inheritdoc />
    /// <summary>Represents a configured ApplicationHost.</summary>
    public interface IApplicationHost : IDisposable
    {
        /// <summary>Global application life cycle control - triggers when application shuts down.</summary>
        IApplicationLifeTime ApplicationLifeTime { get; }

        /// <summary>Provider of date time functionality.</summary>
        IDateTimeProvider DateTimeProvider { get; }

        /// <summary>
        /// The IConfigurationRoot of the ApplicationHost.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// The IApplicationServiceProvider for the ApplicationHost.
        /// </summary>
        IApplicationServiceProvider Services { get; }

        /// <summary>Software version of the ApplicationHost.</summary>
        Version Version { get; }

        /// <summary>Provides current state of the ApplicationHost.</summary>
        ApplicationState State { get; }

        /// <summary>Time the ApplicationHost started.</summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Start ApplicationHost and its features.
        /// </summary>
        void Start();

        /// <summary>
        /// Initializes DI services that the ApplicationHost needs.
        /// </summary>
        /// <param name="serviceProvider">Provider of DI services.</param>
        /// <returns>ApplicationHost itself to allow fluent code.</returns>
        IApplicationHost Initialize(IApplicationServiceProvider serviceProvider);

        /// <summary>
        /// Find a service of a particular type
        /// </summary>
        /// <typeparam name="T">Class of type</typeparam>
        /// <param name="failWithDefault">Set to true to return null instead of throwing an error</param>
        /// <returns></returns>
        T ApplicationService<T>(bool failWithDefault = false);

        /// <summary>
        /// Find a feature of a particular type or having a given interface
        /// </summary>
        /// <typeparam name="T">Class of interface type</typeparam>
        /// <param name="failWithError">Set to false to return null instead of throwing an error</param>
        /// <returns></returns>
        T ApplicationFeature<T>(bool failWithError = false);
    }

    /// <summary>Represents <see cref="IApplicationHost"/> state.</summary>
    public enum ApplicationState
    {
        /// <summary>Assigned when <see cref="IApplicationHost"/> instance is created.</summary>
        Created,

        /// <summary>Assigned when <see cref="IApplicationHost.Initialize"/> is called.</summary>
        Initializing,

        /// <summary>Assigned when <see cref="IApplicationHost.Initialize"/> finished executing.</summary>
        Initialized,

        /// <summary>Assigned when <see cref="IApplicationHost.Start"/> is called.</summary>
        Starting,

        /// <summary>Assigned when <see cref="IApplicationHost.Start"/> finished executing.</summary>
        Started,

        /// <summary>Assigned when <see cref="IApplicationHost.Dispose"/> is called.</summary>
        Disposing,

        /// <summary>Assigned when <see cref="IApplicationHost.Dispose"/> finished executing.</summary>
        Disposed
    }
}
