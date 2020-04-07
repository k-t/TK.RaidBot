using System;

namespace TK.RaidBot.Discord.Reactions
{
    public class ReactionsExtensionConfig
    {
        public ReactionsExtensionConfig()
        {
        }

        public ReactionsExtensionConfig(ReactionsExtensionConfig other)
        {
            this.DeleteReactions = other.DeleteReactions;
            this.Services = other.Services;
        }

        public bool DeleteReactions { get; set; }

        public IServiceProvider Services { get; set; }
    }
}
