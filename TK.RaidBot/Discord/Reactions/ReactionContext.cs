using DSharpPlus;
using DSharpPlus.Entities;

namespace TK.RaidBot.Discord.Reactions
{
    public class ReactionContext
    {
        internal ReactionContext(
            DiscordClient client,
            DiscordGuild guild,
            DiscordChannel channel,
            DiscordMessage message,
            DiscordEmoji emoji,
            DiscordUser user)
        {
            Client = client;
            Guild = guild;
            Channel = channel;
            Emoji = emoji;
            Message = message;
            User = user;
        }

        public DiscordClient Client { get; }

        public DiscordChannel Channel { get; }

        public DiscordGuild Guild { get; }

        public DiscordEmoji Emoji { get; }

        public DiscordMessage Message { get; }

        public DiscordUser User { get; }
    }
}
