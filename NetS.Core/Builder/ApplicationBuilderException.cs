using System;

namespace NetS.Core.Builder
{
    /// <summary>
    /// Exception thrown by ApplicationHostBuilder.Build.
    /// </summary>
    /// <seealso cref="ApplicationHostHostBuilder.Build"/>
    public class ApplicationBuilderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApplicationBuilderException(string message) : base(message)
        {
        }
    }
}
