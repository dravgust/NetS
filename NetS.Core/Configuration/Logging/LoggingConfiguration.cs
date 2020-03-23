using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using NetS.Core.Configuration.Settings;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NetS.Core.Configuration.Logging
{
    /// <summary>
    /// Integration of NLog with Microsoft.Extensions.Logging interfaces.
    /// </summary>
    public static class LoggingConfiguration
    {
        /// <summary>Width of a column for pretty console/log outputs.</summary>
        public const int ColumnLength = 24;

        /// <summary>Currently used node's log settings.</summary>
        private static LogSettings logSettings;

        /// <summary>Currently used data folder to determine path to logs.</summary>
        private static DataFolder folder;

        /// <summary>Mappings of keys to class name spaces to be used when filtering log categories.</summary>
        private static readonly Dictionary<string, string> KeyCategories = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Short Names
            { "configuration", $"{nameof(NetS)}.{nameof(Configuration)}.*" },
            { "application", $"{nameof(NetS)}.{nameof(ApplicationHost)}" }
        };

        public static void RegisterFeatureNamespace<T>(string key)
        {
            lock (KeyCategories)
            {
                KeyCategories[key] = typeof(T).Namespace + ".*";
            }
        }

        public static void RegisterFeatureClass<T>(string key)
        {
            lock (KeyCategories)
            {
                KeyCategories[key] = typeof(T).Namespace + "." + typeof(T).Name;
            }
        }

        /// <summary>
        /// Initializes application logging.
        /// </summary>
        static LoggingConfiguration()
        {
            // If there is no NLog.config file, we need to initialize the configuration ourselves.
            if (LogManager.Configuration == null) LogManager.Configuration = new NLog.Config.LoggingConfiguration();

            // Installs handler to be called when NLog's configuration file is changed on disk.
            LogManager.ConfigurationReloaded += NLogConfigurationReloaded;
        }

        /// <summary>
        /// Event handler to be called when logging <see cref="NLog.LogManager.Configuration"/> gets reloaded.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        public static void NLogConfigurationReloaded(object sender, LoggingConfigurationReloadedEventArgs e)
        {
            AddFilters(logSettings, folder);
        }

        /// <summary>
        /// Extends the logging rules in the "NLog.config" with node log settings rules.
        /// </summary>
        /// <param name="settings">Node log settings to extend the rules from the configuration file, or null if no extension is required.</param>
        /// <param name="dataFolder">Data folder to determine path to log files.</param>
        private static void AddFilters(LogSettings settings = null, DataFolder dataFolder = null)
        {
            if (settings == null) return;

            logSettings = settings;
            folder = dataFolder;

            // If we use "debug*" targets, which are defined in "NLog.config", make sure they log into the correct log folder in data directory.
            List<Target> debugTargets = LogManager.Configuration.AllTargets.Where(t => (t.Name != null) && t.Name.StartsWith("debug")).ToList();
            foreach (Target debugTarget in debugTargets)
            {
                FileTarget debugFileTarget = debugTarget is AsyncTargetWrapper ? (FileTarget)((debugTarget as AsyncTargetWrapper).WrappedTarget) : (FileTarget)debugTarget;
                string currentFile = debugFileTarget.FileName.Render(new LogEventInfo { TimeStamp = DateTime.UtcNow });
                debugFileTarget.FileName = Path.Combine(folder.LogPath, Path.GetFileName(currentFile));

                if (debugFileTarget.ArchiveFileName != null)
                {
                    string currentArchive = debugFileTarget.ArchiveFileName.Render(new LogEventInfo { TimeStamp = DateTime.UtcNow });
                    debugFileTarget.ArchiveFileName = Path.Combine(folder.LogPath, currentArchive);
                }
            }

            // Remove rule that forbids logging before the logging is initialized.
            LoggingRule nullPreInitRule = null;
            foreach (LoggingRule rule in LogManager.Configuration.LoggingRules)
            {
                if (rule.Final && rule.NameMatches("*") && (rule.Targets.Count > 0) && (rule.Targets[0].Name == "null"))
                {
                    nullPreInitRule = rule;
                    break;
                }
            }

            LogManager.Configuration.LoggingRules.Remove(nullPreInitRule);

            // Configure main file target, configured using command line or node configuration file settings.
            var mainTarget = new FileTarget
            {
                Name = "main",
                FileName = Path.Combine(folder.LogPath, "app.log"),
                ArchiveFileName = Path.Combine(folder.LogPath, "app-${date:universalTime=true:format=yyyy-MM-dd}.log"),
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 7,
                //Layout = "[${longdate:universalTime=true} ${threadid}${mdlc:item=id}] ${level:uppercase=true}: ${callsite} ${message}",
                Layout = "${longdate:universalTime=true}|${threadid}${mdlc:item=id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}",
                Encoding = Encoding.UTF8
            };

            LogManager.Configuration.AddTarget(mainTarget);

            // Default logging level is Info for all components.
            var defaultRule = new LoggingRule($"{nameof(NetS)}.*", settings.LogLevel, mainTarget);

            if (settings.DebugArgs.Any() && settings.DebugArgs[0] != "1")
            {
                lock (KeyCategories)
                {
                    var usedCategories = new HashSet<string>(StringComparer.Ordinal);

                    // Increase selected categories to Debug.
                    foreach (string key in settings.DebugArgs)
                    {
                        if (!KeyCategories.TryGetValue(key.Trim(), out string category))
                        {
                            // Allow direct specification - e.g. "-debug=Stratis.Bitcoin.Miner".
                            category = key.Trim();
                        }

                        if (!usedCategories.Contains(category))
                        {
                            usedCategories.Add(category);
                            var rule = new LoggingRule(category, settings.LogLevel, mainTarget);
                            LogManager.Configuration.LoggingRules.Add(rule);
                        }
                    }
                }
            }

            LogManager.Configuration.LoggingRules.Add(defaultRule);

            // Apply new rules.
            LogManager.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Extends the logging rules in the "NLog.config" with node log settings rules.
        /// </summary>
        /// <param name="loggerFactory">Not used.</param>
        /// <param name="settings">Node log settings to extend the rules from the configuration file, or null if no extension is required.</param>
        /// <param name="dataFolder">Data folder to determine path to log files.</param>
        public static void AddFilters(this ILoggerFactory loggerFactory, LogSettings settings, DataFolder dataFolder)
        {
            AddFilters(settings, dataFolder);
        }
    }
}
