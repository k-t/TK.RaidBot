using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using TK.RaidBot.Discord;
using TK.RaidBot.Services;

namespace TK.RaidBot
{
    internal class Program
    {
        private const string BotTokenVariableName = "BOT_TOKEN";

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            ConfigureLogging();

            var botToken = Environment.GetEnvironmentVariable(BotTokenVariableName);
            var botConfig = new BotConfig
            {
                Token = botToken,
                Services = ConfigureServices()
            };

            using (var bot = new Bot(botConfig))
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

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<EmojiService>()
                .AddSingleton<DataService>()
                .AddSingleton<MessageBuilderService>()
                .AddSingleton<ActionService>();

            return services.BuildServiceProvider();
        }
    }
}
