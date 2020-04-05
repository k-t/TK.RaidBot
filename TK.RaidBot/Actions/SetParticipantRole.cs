using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using TK.RaidBot.Model.Data;
using TK.RaidBot.Services;

namespace TK.RaidBot.Actions
{
    public class SetParticipantRole : IBotAction
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Raid raid;
        private readonly RaidRole role;

        private readonly DataService dataService;
        private readonly MessageBuilderService messageBuilder;

        public SetParticipantRole(
            Raid raid,
            RaidRole role,
            DataService dataService,
            MessageBuilderService messageBuilder)
        {
            this.raid = raid;
            this.role = role;
            this.dataService = dataService;
            this.messageBuilder = messageBuilder;
        }

        public async Task Execute(BotActionContext ctx)
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

        public override string ToString()
        {
            return $"{nameof(SetParticipantRole)}({role})";
        }
    }
}
