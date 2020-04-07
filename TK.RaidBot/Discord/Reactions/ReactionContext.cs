using DSharpPlus;
using DSharpPlus.Entities;

namespace TK.RaidBot.Discord.Reactions
{
    public class ReactionContext
    {
        internal ReactionContext(
            DiscordClient client,
            DiscordChannel channel,
            DiscordEmoji emoji,
            DiscordMessage message,
            DiscordUser user)
        {
            Client = client;
            Channel = channel;
            Emoji = emoji;
            Message = message;
            User = user;
        }

        public DiscordClient Client { get; }

        public DiscordChannel Channel { get; }

        public DiscordEmoji Emoji { get; }

        public DiscordMessage Message { get; }

        public DiscordUser User { get; }
    }
}
