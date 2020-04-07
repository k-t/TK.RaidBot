using NLog;
using System.Linq;
using System.Threading.Tasks;
using TK.RaidBot.Discord.Reactions;
using TK.RaidBot.Model.Data;
using TK.RaidBot.Services;

namespace TK.RaidBot.Discord
{
    public class BotReactions : IReactionsModule
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly DataService dataService;
        private readonly EmojiService emojiService;
        private readonly MessageBuilderService messageBuilder;

        public BotReactions(DataService dataService, MessageBuilderService messageBuilder, EmojiService emojiService)
        {
            this.dataService = dataService;
            this.messageBuilder = messageBuilder;
            this.emojiService = emojiService;
        }

        public AsyncReactionHandler GetReactionHandler(string emojiName, ReactionContext ctx)
        {
            var raid = dataService.GetRaidByMessage(ctx.Channel.Id, ctx.Message.Id);
            if (raid != null)
            {
                //if (emoji.GetDiscordName().Contains("🗑️"))
                //    return new ClearParticipantsAction();

                switch (emojiName)
                {
                    case ":question:":
                        return ctx => SetParticipantRole(raid, RaidRole.Unknown, ctx);
                    case ":stop_button:":
                        return ctx => SetRaidStatus(raid, RaidStatus.Expired, ctx);
                    case ":arrow_forward:":
                        return ctx => SetRaidStatus(raid, RaidStatus.Scheduled, ctx);
                    default:
                        {
                            // allow to change participation state only for scheduled raids
                            if (raid.Status == RaidStatus.Scheduled)
                            {
                                var status = emojiService.GetStatusByEmoji(emojiName);
                                if (status.HasValue)
                                    return ctx => SetParticipantStatus(raid, status.Value, ctx);

                                var role = emojiService.GetRoleByEmoji(emojiName);
                                if (role.HasValue)
                                    return ctx => SetParticipantRole(raid, role.Value, ctx);
                            }
                            return HandleUnknownEmoji;
                        }
                }

            }
            return null;
        }

        private async Task SetRaidStatus(Raid raid, RaidStatus status, ReactionContext ctx)
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

        private async Task SetParticipantRole(Raid raid, RaidRole role, ReactionContext ctx)
        {
            var participant = raid.Participants.FirstOrDefault(x => x.UserId == ctx.User.Id);

            // if nothing changed
            if (participant != null &&
                participant.Role == role)
            {
                return;
            }

            dataService.SetParticipantRole(raid.Id, ctx.User.Id, role);

            var updatedRaid = dataService.GetRaidById(raid.Id);

            Log.Debug("Updating message: user={0}, messageId={1} action={2}",
                ctx.User.Username, ctx.Message.Id, this);

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, updatedRaid));
        }

        private async Task SetParticipantStatus(Raid raid, ParticipationStatus status, ReactionContext ctx)
        {
            var participant = raid.Participants.FirstOrDefault(x => x.UserId == ctx.User.Id);

            // if nothing changed
            if (participant != null &&
                participant.Status == status)
            {
                return;
            }

            dataService.SetParticipantStatus(raid.Id, ctx.User.Id, status);

            var updatedRaid = dataService.GetRaidById(raid.Id);

            Log.Debug("Updating message: user={0}, messageId={1} action={2}",
                ctx.User.Username, ctx.Message.Id, this);

            await ctx.Message.ModifyAsync(embed: messageBuilder.BuildEmbed(ctx.Client, updatedRaid));
        }

        // TODO:
        private Task FixReactions(ReactionContext ctx)
        {
            //var message = ctx.Message;

            //foreach (var reaction in message.Reactions.ToArray())
            //{
            //    var users = await message.GetReactionsAsync(reaction.Emoji);

            //    foreach (var user in users)
            //    {
            //        if (user.IsBot)
            //            continue;

            //        var action = actionService.GetBotActionByEmoji(reaction.Emoji, ctx);
            //        if (action != null)
            //        {
            //            await action.Execute(ctx);
            //        }

            //        await message.DeleteReactionAsync(reaction.Emoji, user);
            //    }
            //}
            return Task.CompletedTask;
        }

        private Task HandleUnknownEmoji(ReactionContext ctx)
        {
            return Task.CompletedTask;
        }
    }
}
