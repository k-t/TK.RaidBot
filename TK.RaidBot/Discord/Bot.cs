using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using TK.RaidBot.Actions;
using TK.RaidBot.Config;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    public class Bot : IDisposable
    {
        private const string CommandPrefix = "!raid ";

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly DiscordClient client;
        private readonly CommandsNextExtension commands;

        private readonly DataService dataService;
        private readonly ActionService actionService;

        public Bot(BotConfig config, IServiceProvider serviceProvider)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var clientConfig = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                Token = config.Token,
                UseInternalLogHandler = false,
                LogLevel = DSharpPlus.LogLevel.Debug
            };

            dataService = serviceProvider.GetService<DataService>();
            actionService = serviceProvider.GetService<ActionService>();

            client = new DiscordClient(clientConfig);
            client.DebugLogger.LogMessageReceived += HandleLogMessage;
            client.MessageReactionAdded += HandleMessageReaction;
            client.MessageDeleted += HandleMessageDeletion;

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { CommandPrefix },
                Services = serviceProvider
            };

            commands = client.UseCommandsNext(commandsConfig);
            commands.RegisterCommands<BotCommands>();

            client.UseInteractivity(new InteractivityConfiguration { Timeout = TimeSpan.FromSeconds(60) });
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

        private async Task HandleMessageReaction(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot)
                return;

            await HandleWithErrorLogging(async () =>
            {
                var actionContext = new BotActionContext(e.Client, e.Channel, e.Message, e.User);
                var action = actionService.GetBotActionByEmoji(e.Emoji, actionContext);
                if (action != null)
                {
                    await action.Execute(actionContext);
                    await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                }
            });
        }

        private Task HandleMessageDeletion(MessageDeleteEventArgs e)
        {
            HandleWithErrorLogging(() =>
            {
                var deleted = dataService.DeleteRaid(e.Channel.Id, e.Message.Id);
                if (deleted)
                    Log.Debug("Raid was deleted: channelId={0} messageId={0}", e.Channel.Id, e.Message.Id);
            });
            return Task.CompletedTask;
        }

        public async Task HandleWithErrorLogging(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception");
            }
        }

        private void HandleWithErrorLogging(Action action)
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
