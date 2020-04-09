using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using TK.RaidBot.Config;
using TK.RaidBot.Discord;
using TK.RaidBot.Services;

namespace TK.RaidBot
{
    internal class Program
    {
        private const string DefaultDatabaseName = "raidBotDB";

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddIniFile("config.ini")
                .AddEnvironmentVariables()
                .Build();

            ConfigureLogging();

            var botConfig = GetBotConfig(config);
            var services = ConfigureServices(config);

            using (var bot = new Bot(botConfig, services))
            {
                bot.Start().Wait();

                // wait until Ctrl+C is pressed
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    QuitEvent.Set();
                    eventArgs.Cancel = true;
                };
                QuitEvent.WaitOne();
            }
        }

        private static void ConfigureLogging()
        {
            var consoleLog = new ColoredConsoleTarget("Console");
            consoleLog.Layout = "${longdate}|${pad:padding=5:inner=${level:uppercase=true}}|${message} ${exception:format=tostring}";
            consoleLog.UseDefaultRowHighlightingRules = true;

            var config = new LoggingConfiguration();
            config.AddRuleForAllLevels(consoleLog);

            LogManager.Configuration = config;
        }

        private static IServiceProvider ConfigureServices(IConfiguration config)
        {
            var databaseConfig = GetDatabaseConfig(config);

            var services = new ServiceCollection()
                .AddSingleton(new DataService(databaseConfig))
                .AddSingleton<RaidService>()
                .AddSingleton<EmojiService>()
                .AddSingleton<MessageBuilderService>();

            return services.BuildServiceProvider();
        }

        private static BotConfig GetBotConfig(IConfiguration config)
        {
            var section = config.GetSection("bot");
            if (section == null)
                throw new BotConfigurationException("Missing required config section 'bot'");

            var token = section["botToken"];
            if (string.IsNullOrEmpty(token))
                throw new BotConfigurationException("Missing required config value 'bot:botToken'");

            return new BotConfig { Token = token };
        }

        private static DatabaseConfig GetDatabaseConfig(IConfiguration config)
        {
            var section = config.GetSection("db");
            if (section == null)
                throw new BotConfigurationException("Missing required config section 'db'");

            var databaseUrl = section["databaseUrl"];
            if (string.IsNullOrEmpty(databaseUrl))
                throw new BotConfigurationException("Missing required config value 'db:databaseUrl'");

            var databaseName = section["databaseName"];
            if (string.IsNullOrEmpty(databaseName))
                databaseName = DefaultDatabaseName;

            return new DatabaseConfig
            {
                DatabaseUrl = databaseUrl,
                DatabaseName = databaseName
            };
        }
    }
}
