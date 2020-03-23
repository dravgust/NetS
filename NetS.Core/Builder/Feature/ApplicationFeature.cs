using System.Threading.Tasks;

namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// A feature is used to extend functionality into the ApplicationHost.
    /// It can manage its life time or use the ApplicationHost disposable resources.
    /// <para>
    /// If a feature adds an option of a certain functionality to be available to be used by the app
    /// (it may be disabled/enabled by the configuration) the naming convention is
    /// <c>Add[Feature]()</c>. Conversely, when a feature is inclined to be used if included,
    /// the naming convention should be <c>Use[Feature]()</c>.
    /// </para>
    /// </summary>
    public abstract class ApplicationFeature : IApplicationFeature
    {
        /// <inheritdoc />
        public bool InitializeBeforeBase { get; set; }

        /// <inheritdoc />
        public string State { get; set; }

        /// <inheritdoc />
        public abstract Task InitializeAsync();

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <inheritdoc />
        public virtual void ValidateDependencies(IApplicationServiceProvider services)
        {
        }
    }
}
