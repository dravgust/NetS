using System;

namespace NetS.Core.Configuration
{
    /// <summary>
    /// Exception that is used when a problem in command line or configuration file configuration is found.
    /// </summary>
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message)
        {
        }
    }
}
