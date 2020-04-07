using System.Threading.Tasks;

namespace TK.RaidBot.Discord.Reactions
{
    public delegate Task AsyncReactionHandler(ReactionContext ctx);
}
