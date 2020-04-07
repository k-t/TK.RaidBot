using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace TK.RaidBot.Discord.Reactions
{
    public class ReactionsExtension : BaseExtension
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ReactionsExtensionConfig config;
        private readonly List<IReactionsModule> modules;

        public ReactionsExtension(ReactionsExtensionConfig config)
        {
            this.config = new ReactionsExtensionConfig(config);
            this.modules = new List<IReactionsModule>();

            if (this.config.Services == null)
                this.config.Services = new ServiceCollection().BuildServiceProvider();
        }

        public void RegisterReactions<T>() where T : class, IReactionsModule
        {
            var module = ActivatorUtilities.CreateInstance<T>(config.Services);
            modules.Add(module);
        }

        protected override void Setup(DiscordClient client)
        {
            client.MessageReactionAdded += HandleMessageReaction;
        }

        private Task HandleMessageReaction(MessageReactionAddEventArgs e)
        {
            var ctx = new ReactionContext(e.Client, e.Channel, e.Emoji, e.Message, e.User);

            foreach (var module in modules)
            {
                var reactionHandler = module.GetReactionHandler(e.Emoji.GetDiscordName(), ctx);
                if (reactionHandler != null)
                {
                    return HandleWithErrorLogging(async () =>
                    {
                        await reactionHandler(ctx);
                        if (config.DeleteReactions)
                            await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                    });
                }
            }

            return Task.CompletedTask;
        }

        private Task HandleWithErrorLogging(Func<Task> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception");
                return Task.CompletedTask;
            }
        }
    }
}
