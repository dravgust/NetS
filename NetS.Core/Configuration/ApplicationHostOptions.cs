using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NetS.Core.Utilities;

namespace NetS.Core.Configuration
{
    public static class ApplicationDefaults
    {
        public static readonly string ApplicationKey = "appname";
        public static readonly string EnvironmentKey;
        public static readonly string DebugArgsKey = "debugargs";
    }

    public class ApplicationHostOptions
    {
        public string ApplicationName { get; set; }

        public List<string> DebugArgs { get; private set; }

        public ApplicationHostOptions() { }

        public ApplicationHostOptions(IConfiguration configuration) : this(configuration, string.Empty){ }

        public ApplicationHostOptions(IConfiguration configuration, string applicationNameFallback)
        {
            Guard.NotNull(configuration, nameof(configuration));

            this.ApplicationName = configuration[ApplicationDefaults.ApplicationKey] ?? applicationNameFallback;

            var debugArgs = configuration[ApplicationDefaults.DebugArgsKey] ??"";
            this.DebugArgs = debugArgs.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private static bool ParseBool(IConfiguration configuration, string key)
        {
            if (!string.Equals("true", configuration[key], StringComparison.OrdinalIgnoreCase))
                return string.Equals("1", configuration[key], StringComparison.OrdinalIgnoreCase);
            return true;
        }
    }
}
