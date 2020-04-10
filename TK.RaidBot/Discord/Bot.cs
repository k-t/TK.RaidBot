using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TK.RaidBot.Config;
using TK.RaidBot.Discord.Reactions;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    public class Bot : IDisposable
    {
        private const string RaidCommandPrefix = "!raid ";

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly DiscordClient client;
        private readonly RaidService raidService;

        public Bot(BotConfig config, IServiceProvider serviceProvider)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            raidService = serviceProvider.GetService<RaidService>();

            var clientConfig = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = config.Token,
                UseInternalLogHandler = false,
                LogLevel = DSharpPlus.LogLevel.Debug
            };

            client = new DiscordClient(clientConfig);
            client.DebugLogger.LogMessageReceived += HandleLogMessage;
            client.MessageDeleted += HandleMessageDeletion;

            var raidCommands = client.UseCommandsNext(
                new CommandsNextConfiguration
                {
                    StringPrefixes = new[] { RaidCommandPrefix },
                    Services = serviceProvider
                });
            raidCommands.RegisterCommands<RaidCommands>();

            client.UseInteractivity(new InteractivityConfiguration { Timeout = TimeSpan.FromSeconds(60) });
            
            var reactions = client.UseReactionsExtension(
                new ReactionsExtensionConfig
                {
                    DeleteReactions = true,
                    Services = serviceProvider
                });
            reactions.RegisterReactions<RaidReactions>();
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        public async Task Start()
        {
            await client.ConnectAsync();
        }

        public async Task Stop()
        {
            await client.DisconnectAsync();
        }

        private Task HandleMessageDeletion(MessageDeleteEventArgs e)
        {
            DoWithErrorLogging(() =>
            {
                var deleted = raidService.DeleteRaid(e.Channel.Id, e.Message.Id);
                if (deleted)
                    Log.Debug("Raid was deleted: channelId={0} messageId={0}", e.Channel.Id, e.Message.Id);
            });
            return Task.CompletedTask;
        }

        private void DoWithErrorLogging(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception");
            }
        }

        private void HandleLogMessage(object sender, DebugLogMessageEventArgs e)
        {
            switch (e.Level)
            {
                case DSharpPlus.LogLevel.Critical:
                    Log.Fatal(e.Message);
                    break;
                case DSharpPlus.LogLevel.Error:
                    Log.Error(e.Message);
                    break;
                case DSharpPlus.LogLevel.Warning:
                    Log.Warn(e.Message);
                    break;
                case DSharpPlus.LogLevel.Info:
                    Log.Info(e.Message);
                    break;
                case DSharpPlus.LogLevel.Debug:
                    Log.Debug(e.Message);
                    break;
            }
        }
    }
}
