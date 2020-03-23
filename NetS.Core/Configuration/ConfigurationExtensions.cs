using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NetS.Core.Utilities;

namespace NetS.Core.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder builder, string path)
        {
            return builder.AddConfigFile((IFileProvider)null, path, false, false);
        }

        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return builder.AddConfigFile((IFileProvider)null, path, optional, false);
        }

        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return builder.AddConfigFile((IFileProvider) null, path, optional, reloadOnChange);
        }

        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            Guard.NotNull(builder, nameof(builder));

            //var fileProvider = builder.GetFileProvider();

            Guard.NotNull(path, nameof(path));

            var configurationSource = new TextFileConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };

            builder.Add(configurationSource);
            return builder;
        }

        //private static IDictionary<string, string> GetSwitchMappings(
        //    IReadOnlyDictionary<string, string> configStrings)
        //{
        //    return configStrings.Select(item => new KeyValuePair<string, string>("-" + item.Key.Substring(item.Key.LastIndexOf(':') + 1), item.Key))
        //        .ToDictionary(item => item.Key, item => item.Value);
        //}

        //public string ApplicationName
        //{
        //    get;
        //    set;
        //}

        //public string Config
        //{
        //    get;
        //    set;
        //}

        //public string DataDirectory
        //{
        //    get;
        //    set;
        //}

        //public override void Load()
        //{
        //this.Config = Data[nameof(Config)];
        //this.DataDirectory = Data[nameof(DataDirectory)];
        //this.ApplicationName = Data[nameof(ApplicationName)];

        //if (this.DataDirectory != null && this.Config != null)
        //{
        //    var isRelativePath = Path.GetFullPath(this.Config).Length > this.Config.Length;
        //    if (isRelativePath)
        //    {
        //        this.Config = Path.Combine(this.DataDirectory, this.Config);
        //    }
        //}

        //if (this.Config != null)
        //{
        //    AssetConfigFileExists();
        //    var data = File.ReadAllText(this.Config);
        //    var configFile = JsonUtils.Desserialize<dynamic>(data);
        //    if (configFile.DataDirectory != null)
        //    {
        //        this.DataDirectory = configFile.DataDirectory;
        //    }
        //}

        //if (this.DataDirectory == null)
        //{
        //    this.DataDirectory = DefaultDataDirectory;
        //}

        //if (this.Config == null)
        //{
        //    this.Config = DefaultConfiguration;
        //}

        //if(!Directory.Exists(this.DataDirectory))
        //    throw new ConfigurationException("Data directory does not exists");

        //Data[nameof(Config)] = this.Config;
        //Data[nameof(DataDirectory)] = this.DataDirectory;
        //Data[nameof(ApplicationName)] = this.ApplicationName;
        //}

        //private string DefaultDataDirectory
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(this.ApplicationName))
        //        {
        //            this.ApplicationName = $"{Guid.NewGuid()}";
        //        }

        //        string directory;
        //        var home = Environment.GetEnvironmentVariable("HOME");
        //        if (!string.IsNullOrEmpty(home))
        //        {
        //            directory = home;
        //            directory = Path.Combine(directory, "." + this.ApplicationName.ToLowerInvariant());
        //        }
        //        else
        //        {
        //            var localAppData = Environment.GetEnvironmentVariable("APPDATA");
        //            if (!string.IsNullOrEmpty(localAppData))
        //            {
        //                directory = localAppData;
        //                directory = Path.Combine(directory, this.ApplicationName);
        //            }
        //            else
        //            {
        //                throw new DirectoryNotFoundException("Could not find suitable datadir");
        //            }
        //        }

        //        if (!Directory.Exists(directory))
        //        {
        //            Directory.CreateDirectory(directory);
        //        }
        //        return directory;
        //    }
        //}

        //private string DefaultConfiguration
        //{
        //    get
        //    {
        //        var config = Path.Combine(this.DataDirectory, $"{this.ApplicationName}.conf");
        //        if (!File.Exists(config))
        //        {
        //            var builder = new StringBuilder();
        //            builder.AppendLine("{}");
        //            //builder.AppendLine("####RPC Settings####");
        //            //builder.AppendLine("#Activate RPC Server (default: 0)");
        //            //builder.AppendLine("#server=0");
        //            //builder.AppendLine("#Where the RPC Server binds (default: 127.0.0.1 and ::1)");
        //            //builder.AppendLine("#rpcbind=127.0.0.1");
        //            //builder.AppendLine("#Ip address allowed to connect to RPC (default all: 0.0.0.0 and ::)");
        //            //builder.AppendLine("#rpcallowedip=127.0.0.1");
        //            File.WriteAllText(config, builder.ToString());
        //        }
        //        return config;
        //    }
        //}

        //private void AssetConfigFileExists()
        //{
        //    if (!File.Exists(this.Config))
        //        throw new ConfigurationException("Configuration file does not exists");
        //}
    }
}
