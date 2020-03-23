using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetS.Core;
using NetS.Core.Builder;
using NetS.Core.Builder.Feature;
using NetS.Core.Configuration.Logging;
using NetS.Core.Utilities;
using NetS.Robot.Commands;
using NetS.Robot.Configuration;

namespace NetS.Robot.Features
{
    public class TelegramBotFeature : ApplicationFeature
    {
        /// <summary>Global application life cycle control - triggers when application shuts down.</summary>
        private readonly IApplicationLifeTime _appLifetime;

        private readonly TelegramBotClientManager _telegramBotManager;

        /// <summary>Logger for the node.</summary>
        private readonly ILogger _logger;

        public TelegramBotFeature(TelegramBotClientManager telegramBotManager, IApplicationLifeTime appLifetime, ILoggerFactory loggerFactory, IApplicationStats appStats)
        {
            this._appLifetime = Guard.NotNull(appLifetime, nameof(appLifetime));
            this._telegramBotManager = Guard.NotNull(telegramBotManager, nameof(telegramBotManager));

            this._logger = loggerFactory.CreateLogger(this.GetType().FullName);

            appStats.RegisterStats(this.AddComponentStats, StatsType.Component, this.GetType().Name);
            appStats.RegisterStats(this.AddInlineStats, StatsType.Inline, this.GetType().Name, 800);
        }

        public override Task InitializeAsync()
        {
            _telegramBotManager.Start();

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _telegramBotManager.Stop();
        }

        private void AddInlineStats(StringBuilder log)
        {
            log.AppendLine("bot inline stats");
        }

        private void AddComponentStats(StringBuilder log)
        {
            log.AppendLine();
            log.AppendLine("bot component stats");
        }
    }

    public static class TelegramBotFeatureExtension
    {
        public static IApplicationHostBuilder UseTelegramBotFeature(this IApplicationHostBuilder applicationHostBuilder, IConfigurationSection configuration)
        {
            //LoggingConfiguration.RegisterFeatureNamespace<TelegramBotFeature>("telegramBot");

            applicationHostBuilder.ConfigureFeature(features => features
                    .AddFeature<TelegramBotFeature>()
                    .FeatureServices(services => services
                        .Configure<TelegramBotOptions>(configuration)
                        .AddTransient<List<Command>>(provider => 
                            new List<Command>
                            {
                                new DefaultCommand(),
                                new StartCommand()
                            })
                        .AddSingleton<IBotController, BotController>()
                        .AddSingleton<TelegramBotClientManager>()
                    ));

            return applicationHostBuilder;
        }
    }
}
