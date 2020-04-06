using System;
using System.Threading.Tasks;
using NLog;
using TK.RaidBot.Model.Data;
using TK.RaidBot.Services;

namespace TK.RaidBot.Actions
{
    public class SetRaidStatus : IBotAction
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Raid raid;
        private readonly RaidStatus status;

        private readonly DataService dataService;
        private readonly EmojiService emojiService;
        private readonly MessageBuilderService messageBuilder;

        public SetRaidStatus(
            Raid raid,
            RaidStatus status,
            DataService dataService,
            EmojiService emojiService,
            MessageBuilderService messageBuilder)
        {
            this.raid = raid;
            this.status = status;
            this.dataService = dataService;
            this.emojiService = emojiService;
            this.messageBuilder = messageBuilder;
        }

        public async Task Execute(BotActionContext ctx)
        {
            // if nothing changed
            if (raid.Status == status)
            {
                return;
            }

            dataService.SetRaidStatus(raid.Id, status);

            var updatedRaid = dataService.GetRaidById(raid.Id);

            Log.Debug("Updating message: user={0}, messageId={1} action={2}",
                ctx.User.Username, ctx.Message.Id, this);

            switch (status)
            {
                case RaidStatus.Scheduled:
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Available));
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.Maybe));
                    await ctx.Message.CreateReactionAsync(emojiService.GetStatusEmoji(ctx.Client, ParticipationStatus.NotAvailable));
                    break;
                case RaidStatus.Expired:
                    await ctx.Message.DeleteAllReactionsAsync();
                    break;
            }

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, updatedRaid));
        }
    }
}
