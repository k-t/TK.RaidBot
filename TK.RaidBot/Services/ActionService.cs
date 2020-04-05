using System;
using DSharpPlus.Entities;
using TK.RaidBot.Actions;
using TK.RaidBot.Model.Data;

namespace TK.RaidBot.Services
{
    public class ActionService
    {
        private readonly DataService dataService;
        private readonly EmojiService emojiService;
        private readonly MessageBuilderService messageBuilder;

        public ActionService(
            DataService dataService,
            EmojiService emojiService,
            MessageBuilderService messageBuilder)
        {
            this.dataService = dataService;
            this.emojiService = emojiService;
            this.messageBuilder = messageBuilder;
        }

        public IBotAction GetBotActionByEmoji(DiscordEmoji emoji, BotActionContext ctx)
        {
            var raid = dataService.GetRaidByMessage(ctx.Channel.Id, ctx.Message.Id);
            if (raid != null)
            {
                //if (emoji.GetDiscordName().Contains("🗑️"))
                //    return new ClearParticipantsAction();

                if (emoji.GetDiscordName() == ":question:")
                    return new SetParticipantRole(raid, RaidRole.Unknown, dataService, messageBuilder);

                var status = emojiService.GetStatusByEmoji(emoji);
                if (status.HasValue)
                    return new SetParticipantStatus(raid, status.Value, dataService, messageBuilder);

                var role = emojiService.GetRoleByEmoji(emoji);
                if (role.HasValue)
                    return new SetParticipantRole(raid, role.Value, dataService, messageBuilder);
            }
            return null;
        }
    }
}
