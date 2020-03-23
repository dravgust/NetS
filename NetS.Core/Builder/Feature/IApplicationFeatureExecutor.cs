namespace NetS.Core.Builder.Feature
{
    /// <summary>
    /// Starts and stops all features registered with a application.
    /// </summary>
    public interface IApplicationFeatureExecutor
    {
        /// <summary>
        /// Starts all registered features of the associated application.
        /// </summary>
        void Initialize();
    }
}
