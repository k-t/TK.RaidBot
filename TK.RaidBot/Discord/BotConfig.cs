using System;

namespace TK.RaidBot.Discord
{
    public class BotConfig
    {
        /// <summary>
        /// Bot token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Dependency container.
        /// </summary>
        public IServiceProvider Services { get; set; }
    }
}
