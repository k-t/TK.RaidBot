using System.Threading.Tasks;

namespace TK.RaidBot.Actions
{
    public interface IBotAction
    {
        Task Execute(BotActionContext ctx);
    }
}
