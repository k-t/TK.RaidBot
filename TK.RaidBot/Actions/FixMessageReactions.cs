using System;
using System.Linq;
using System.Threading.Tasks;
using TK.RaidBot.Services;

namespace TK.RaidBot.Actions
{
    public class FixMessageReactions : IBotAction
    {
        private readonly ActionService actionService;

        public FixMessageReactions(ActionService actionService)
        {
            this.actionService = actionService;
        }

        public async Task Execute(BotActionContext ctx)
        {
            var message = ctx.Message;

            foreach (var reaction in message.Reactions.ToArray())
            {
                var users = await message.GetReactionsAsync(reaction.Emoji);

                foreach (var user in users)
                {
                    if (user.IsBot)
                        continue;

                    var action = actionService.GetBotActionByEmoji(reaction.Emoji, ctx);
                    if (action != null)
                    {
                        await action.Execute(ctx);
                    }

                    await message.DeleteReactionAsync(reaction.Emoji, user);
                }
            }
        }
    }
}
