using DSharpPlus;
using DSharpPlus.Entities;

namespace TK.RaidBot.Actions
{
    public class BotActionContext
    {
        public BotActionContext(
            DiscordClient client,
            DiscordChannel channel,
            DiscordMessage message,
            DiscordUser user)
        {
            Client = client;
            Channel = channel;
            Message = message;
            User = user;
        }

        public DiscordClient Client { get; }

        public DiscordChannel Channel { get; }

        public DiscordMessage Message { get; }

        public DiscordUser User { get; }
    }
}
