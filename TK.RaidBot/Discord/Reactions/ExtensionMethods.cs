using System;
using DSharpPlus;

namespace TK.RaidBot.Discord.Reactions
{
    public static class ExtensionMethods
    {
        public static ReactionsExtension UseReactionsExtension(this DiscordClient client, ReactionsExtensionConfig config)
        {
            if (client.GetExtension<ReactionsExtension>() != null)
                throw new InvalidOperationException("ReactionsExtension is already enabled for that client.");

            var instance = new ReactionsExtension(config);
            client.AddExtension(instance);
            return instance;
        }
    }
}
