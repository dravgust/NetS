using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace NetS.Core.Builder
{
    /// <summary>
    /// A class providing extension methods for <see cref="IApplicationHostBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationHostBuilder UseConfiguration(this IApplicationHostBuilder hostBuilder, IConfiguration configuration)
        {
            foreach (KeyValuePair<string, string> keyValuePair in configuration.AsEnumerable())
                hostBuilder.UseSetting(keyValuePair.Key, keyValuePair.Value);

            return hostBuilder;
        }
    }
}
