using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using NetS.Core.Configuration.Logging;
using NetS.Core.Configuration.Settings;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace NetS.Core.Configuration
{
    internal static class NormalizeDirectorySeparatorExt
    {
        /// <summary>
        /// Fixes incorrect directory separator characters in path (if any).
        /// </summary>
        public static string NormalizeDirectorySeparator(this string path)
        {
            // Replace incorrect with correct
            return path.Replace((Path.DirectorySeparatorChar == '/') ? '\\' : '/', Path.DirectorySeparatorChar);
        }
    }

    public class ApplicationSettings : IDisposable
    {
        /// <summary>A factory responsible for creating a application logger instance.</summary>
        public ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>An instance of the ApplicationHost logger, which reports on the ApplicationHost's activity.</summary>
        public ILogger Logger { get; private set; }

        /// <summary>The settings of the ApplicationHost's logger.</summary>
        public LogSettings Log { get; private set; }

        /// <summary>A list of paths to folders which application components use to store data. These folders are found
        /// in the <see cref="DataDir"/>.
        /// </summary>
        public DataFolder DataFolder { get; private set; }

        /// <summary>The path to the data directory, which contains, for example, the configuration file.</summary>
        public string DataDir { get; private set; }

        /// <summary>The path to the root data directory, which holds all app data on the machine.</summary>
        public string DataDirRoot { get; private set; }

        /// <summary>The path to the ApplicationHost's configuration file.
        /// This value is read-only and can only be set via the ApplicationSettings constructor's arguments.
        /// </summary>
        public string ConfigurationFile { get; private set; }

        /// <summary>A combination of the settings from the ApplicationHost's configuration file and the command
        /// line arguments supplied to the ApplicationHost when it was run. This places the settings from both sources
        /// into a single object, which is referenced at runtime.
        /// </summary>
        public TextFileConfiguration ConfigReader { get; private set; }

        public ApplicationSettings(string[] args = null)
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder
                    .AddFilter("Default", LogLevel.Information)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.AspNetCore", LogLevel.Error)
                    .AddConsole()
                    .AddNLog());

            this.Logger = this.LoggerFactory.CreateLogger(typeof(ApplicationSettings).FullName);

            this.ConfigReader = new TextFileConfiguration(args ?? new string[] { });

            // Log arguments.
            this.Logger.LogDebug("Arguments: args='{0}'.", args == null ? "(None)" : string.Join(" ", args));

            // both the data directory and the configuration file path may be changed using the -datadir and -conf command-line arguments.
            this.ConfigurationFile = this.ConfigReader.GetOrDefault<string>("conf", null, this.Logger)?.NormalizeDirectorySeparator();
            this.DataDir = this.ConfigReader.GetOrDefault<string>("datadir", null, this.Logger)?.NormalizeDirectorySeparator();
            this.DataDirRoot = this.ConfigReader.GetOrDefault<string>("datadirroot", "/app", this.Logger);

            // If the configuration file is relative then assume it is relative to the data folder and combine the paths.
            if (this.DataDir != null && this.ConfigurationFile != null)
            {
                bool isRelativePath = Path.GetFullPath(this.ConfigurationFile).Length > this.ConfigurationFile.Length;
                if (isRelativePath)
                    this.ConfigurationFile = Path.Combine(this.DataDir, this.ConfigurationFile);
            }

            // If the configuration file has been specified on the command line then read it now
            if (this.ConfigurationFile != null)
            {
                // If the configuration file was specified on the command line then it must exist.
                if (!File.Exists(this.ConfigurationFile))
                    throw new ConfigurationException($"Configuration file does not exist at {this.ConfigurationFile}.");

                // Sets the ConfigReader based on the arguments and the configuration file if it exists.
                this.ReadConfigurationFile();
            }

            // Set the full data directory path.
            if (this.DataDir == null)
            {
                // Create the data directories if they don't exist.
                this.DataDir = this.CreateDefaultDataDirectories(this.DataDirRoot);
            }
            else
            {
                this.DataDir = Directory.CreateDirectory(this.DataDir).FullName;
                this.Logger.LogDebug("Data directory initialized with path {0}.", this.DataDir);
            }

            // Set the data folder.
            this.DataFolder = new DataFolder(this.DataDir);

            // Attempt to load NLog configuration from the DataFolder.
            string configPath = Path.Combine(this.DataFolder.RootPath, "NLog.config");
            if (File.Exists(configPath))
                LogManager.LoadConfiguration(configPath);

            // Create the custom logger factory.
            this.Log = new LogSettings();
            this.Log.Load(this.ConfigReader);
            this.LoggerFactory.AddFilters(this.Log, this.DataFolder);

        }

        /// <summary>
        /// Reads the configuration file and merges it with the command line arguments.
        /// </summary>
        private void ReadConfigurationFile()
        {
            this.Logger.LogDebug("Reading configuration file '{0}'.", this.ConfigurationFile);

            // Add the file configuration to the command-line configuration.
            var fileConfig = new TextFileConfiguration(File.ReadAllText(this.ConfigurationFile));
            fileConfig.MergeInto(this.ConfigReader);
        }

        /// <summary>
        /// Creates default data directories respecting different operating system specifics.
        /// </summary>
        /// <param name="appName">Name of the ApplicationHost, which will be reflected in the name of the data directory.</param>
        /// <returns>The top-level data directory path.</returns>
        private string CreateDefaultDataDirectories(string appName)
        {
            string directoryPath;

            // Directory paths are different between Windows or Linux/OSX systems.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string home = Environment.GetEnvironmentVariable("HOME");
                if (!string.IsNullOrEmpty(home))
                {
                    this.Logger.LogDebug("Using HOME environment variable for initializing application data.");
                    directoryPath = Path.Combine(home, "." + appName.ToLowerInvariant());
                }
                else
                {
                    throw new DirectoryNotFoundException("Could not find HOME directory.");
                }
            }
            else
            {
                string localAppData = Environment.GetEnvironmentVariable("APPDATA");
                if (!string.IsNullOrEmpty(localAppData))
                {
                    this.Logger.LogDebug("Using APPDATA environment variable for initializing application data.");
                    directoryPath = Path.Combine(localAppData, appName);
                }
                else
                {
                    throw new DirectoryNotFoundException("Could not find APPDATA directory.");
                }
            }

            // Create the data directories if they don't exist.
            Directory.CreateDirectory(directoryPath);

            this.Logger.LogDebug("Data directory initialized with path {0}.", directoryPath);
            return directoryPath;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.LoggerFactory.Dispose();
        }
    }
}
