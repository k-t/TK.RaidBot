namespace TK.RaidBot.Discord.Reactions
{
    public interface IReactionsModule
    {
        AsyncReactionHandler GetReactionHandler(string emojiDiscordName, ReactionContext context);
    }
}
