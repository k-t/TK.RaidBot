using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using NLog;

namespace TK.RaidBot.Actions
{
    public class UnknownAction : IBotAction
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly DiscordEmoji emoji;

        public UnknownAction(DiscordEmoji emoji)
        {
            this.emoji = emoji;
        }

        public Task Execute(BotActionContext ctx)
        {
            Log.Debug("Unknown reaction used: user={0} emoji={1} messageId={2}",
                ctx.User.Username, emoji.GetDiscordName(), ctx.Message.Id);

            return Task.CompletedTask;
        }
    }
}
