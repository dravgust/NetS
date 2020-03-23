using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using NetS.Tools.CommandLine;

namespace NetS.Robot.Configuration
{
    public class DefaultConfiguration : Tools.StandardConfiguration.DefaultConfiguration
    {
        protected override CommandLineApplication CreateCommandLineApplicationCore()
        {
            CommandLineApplication app = new CommandLineApplication(true)
            {
                FullName = "Robot\r\nSocial network bot hub.",
                Name = "Robot"
            };

            app.HelpOption("-? | -h | --help");
            app.Option("-tk | --telegram:key", $"Telegram API key (default: empty)", CommandOptionType.SingleValue);
            app.Option("-tp | --telegram:proxy", $"Use proxy (default: false)", CommandOptionType.SingleValue);

            return app;
        }

        protected override string GetDefaultDataDir(IConfiguration conf)
        {
            return RobotDefaultSettings.GetDefaultSettings().DefaultDataDirectory;
        }

        protected override string GetDefaultConfigurationFile(IConfiguration conf)
        {
            var defaultSettings = RobotDefaultSettings.GetDefaultSettings();
            var dataDir = conf["datadir"];
            if (dataDir == null)
                return defaultSettings.DefaultConfigurationFile;
            var fileName = Path.GetFileName(defaultSettings.DefaultConfigurationFile);
            var chainDir = Path.GetFileName(Path.GetDirectoryName(defaultSettings.DefaultConfigurationFile));
            chainDir = Path.Combine(dataDir, chainDir);
            try
            {
                if (!Directory.Exists(chainDir))
                    Directory.CreateDirectory(chainDir);
            }
            catch { }
            return Path.Combine(chainDir, fileName);
        }

        protected override string GetDefaultConfigurationFileTemplate(IConfiguration conf)
        {
            var defaultSettings = RobotDefaultSettings.GetDefaultSettings();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("### Global settings ###");
            //builder.AppendLine("telegram:key=922924719:AAGOT3457ejrnuax9tzzLv4ddJS8tXecrVA");
            builder.AppendLine();
            builder.AppendLine("[telegram]");
            builder.AppendLine("key=922924719:AAGOT3457ejrnuax9tzzLv4ddJS8tXecrVA");
            //builder.AppendLine("#port=" + defaultSettings.DefaultPort);
            //builder.AppendLine("#bind=127.0.0.1");
            //builder.AppendLine("#httpscertificatefilepath=devtest.pfx");
            //builder.AppendLine("#httpscertificatefilepassword=toto");
            //builder.AppendLine();
            //builder.AppendLine("### Database ###");
            //builder.AppendLine("#postgres=User ID=root;Password=myPassword;Host=localhost;Port=5432;Database=myDataBase;");
            //builder.AppendLine("#mysql=User ID=root;Password=myPassword;Host=localhost;Port=3306;Database=myDataBase;");

            return builder.ToString();
        }

        public override string EnvironmentVariablePrefix => "ROBOT_";
        protected override IPEndPoint GetDefaultEndpoint(IConfiguration conf)
        {
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"),RobotDefaultSettings.GetDefaultSettings().DefaultPort);
        }
    }
}
