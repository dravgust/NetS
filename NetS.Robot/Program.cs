using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetS.Core;
using NetS.Core.Builder;
using NetS.Core.Logging;
using NetS.Robot.Configuration;
using NetS.Robot.Features;
using NLog.Extensions.Logging;

namespace NetS.Robot
{
    class Program
    {
       public static async Task Main(string[] args)
       {
           args = new[] { "--telegram:proxy=true"};

            using var loggerProcessor = new ConsoleLoggerProcessor();
            var consoleLogProvider = new CustomConsoleLogProvider(loggerProcessor);
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddFilter("Default", LogLevel.Information)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System.Net.Http.HttpClient", LogLevel.Critical)
                    .AddFilter("Microsoft.AspNetCore.Antiforgery.Internal", LogLevel.Critical)
                    .AddFilter("Microsoft.AspNetCore", LogLevel.Error)

                    .AddProvider(consoleLogProvider)
                    .AddNLog());

            var logger = loggerFactory.CreateLogger("configuration");

            try
            {
                var conf = new DefaultConfiguration
                {
                    Logger = logger
                }.CreateConfiguration(args);
                if (conf == null)
                    return;

                //var settings = new ApplicationSettings(args: args);

                IApplicationHostBuilder applicationHostBuilder = new ApplicationHostHostBuilder();
                using var app = applicationHostBuilder
                    .UseConfiguration(conf)
                    .UseLoggerFactory(loggerFactory)
                    .UseBaseFeature()
                    .UseTelegramBotFeature(conf.GetSection("telegram"))
                    .Build();

                if (app != null)
                    await app.RunAsync();
            }
            catch (Exception e)
            {
                logger.LogError("There was a problem initializing the application. Details: '{0}'", e.ToString());
            }
       }
    }
}
