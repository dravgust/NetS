using System;
using System.Collections.Generic;
using System.IO;

namespace NetS.Robot.Configuration
{
    public class RobotDefaultSettings
    {
        private static readonly RobotDefaultSettings Settings;

        public string DefaultDataDirectory { get; set; }
        public string DefaultConfigurationFile { get; set; }

        public int DefaultPort { get; set; }

        static RobotDefaultSettings()
        {
            Settings = new RobotDefaultSettings();
            Settings.DefaultDataDirectory = Tools.StandardConfiguration.DefaultDataDirectory.GetDirectory("Robot", "");
            Settings.DefaultConfigurationFile = Path.Combine(Settings.DefaultDataDirectory, "settings.config");
            Settings.DefaultPort = 5000;
        }

        public static RobotDefaultSettings GetDefaultSettings()
        {
            return Settings;
        }
    }
}
