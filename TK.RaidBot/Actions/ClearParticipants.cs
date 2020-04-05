using System.Threading.Tasks;

namespace TK.RaidBot.Actions
{
    public class ClearParticipants : IBotAction
    {
        public Task Execute(BotActionContext ctx)
        {
            return Task.CompletedTask;
        }
    }
}
