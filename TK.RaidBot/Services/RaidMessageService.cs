using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using TK.RaidBot.Model.Raids;
using TK.RaidBot.Model.Raids.Templates;

namespace TK.RaidBot.Services
{
    public class RaidMessageService
    {
        private static readonly RaidGroup ReserveGroup =
            new RaidGroup("Возможно появятся", x => x.Status == ParticipationStatus.Maybe)
            {
                ShowParticipantRole = true,
                Inline = false
            };

        private static readonly RaidGroup NotAvailableGroup = 
            new RaidGroup("Не смогут пойти", x => x.Status == ParticipationStatus.NotAvailable)
            {
                ShowParticipantRole = true,
                Inline = false
            };

        private readonly EmojiService emojiService;

        public RaidMessageService(EmojiService emojiService)
        {
            this.emojiService = emojiService;
        }

        public DiscordEmbed BuildEmbed(DiscordClient client, Raid raid)
        {
            // TODO: culture settings?
            var cultureInfo = CultureInfo.GetCultureInfo("ru");
            if (cultureInfo == null)
                cultureInfo = CultureInfo.CurrentCulture;

            var guild = client.GetGuildAsync(raid.GuildId).Result;
            if (guild == null)
                throw new Exception($"Couldn't get guild with id={raid.GuildId}");

            // sort by groups

            var template = GetTemplate(raid.TemplateCode);


            var groups = new List<RaidGroupEmbedBuilder>();
            groups.AddRange(template.Groups.Select(x => new RaidGroupEmbedBuilder(x)));
            groups.Add(new RaidGroupEmbedBuilder(ReserveGroup));
            groups.Add(new RaidGroupEmbedBuilder(NotAvailableGroup));

            int totalCount = 0;
            int availableCount = 0;

            foreach (var participant in raid.Participants.OrderBy(x => x.Role))
            {
                var member = guild.GetMemberAsync(participant.UserId).Result;
                if (member == null)
                    continue;

                var group = groups.FirstOrDefault(x => x.Group.Includes(participant));
                if (group != null)
                {
                    var role = group.Group.ShowParticipantRole
                        ? emojiService.GetProfessionEmoji(client, participant.Role).ToString()
                        : null;

                    group.AddParticipant(member.DisplayName, role);

                    totalCount++;

                    if (participant.Status == ParticipationStatus.Available)
                    {
                        availableCount++;
                    }
                }
            }

            // create embed

            var description = new StringBuilder();
            var date = raid.Date.AddHours(3); // MSK time
            description.Append($"**{date.ToString("f", cultureInfo)} MSK**");

            if (!string.IsNullOrEmpty(raid.OwnerDisplayName))
            {
                description.Append('\n');
                description.Append($"\u00a9 {raid.OwnerDisplayName}");
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = raid.Title,
                Description = description.ToString(),
                Color = GetEmbedColor(raid.Status)
            };

            foreach (var groupBuilder in groups)
            {
                embed.AddField(
                    name: GetGroupTitle(groupBuilder.Group, groupBuilder.Count, client),
                    value: groupBuilder.GetParticipants(),
                    inline: groupBuilder.Group.Inline);
            }

            embed.AddField(
                name: "**Всего**",
                value: $"Отметилось **{totalCount}**\nПридет **{availableCount}**");

            return embed.Build();
        }

        private IRaidTemplate GetTemplate(string templateCode)
        {
            // use wvw template by default
            if (string.IsNullOrEmpty(templateCode))
                return RaidTemplates.Wvw;

            var template = RaidTemplates.GetByCode(templateCode);
            if (template == null)
                throw new Exception($"Unknown template {templateCode}");

            return template;
        }

        private string GetGroupTitle(RaidGroup group, int count, DiscordClient client)
        {
            var titleBuilder = new StringBuilder();

            if (group.Professions.Length > 0)
            {
                foreach (var profession in group.Professions)
                    titleBuilder.Append(emojiService.GetProfessionEmoji(client, profession.Id));

                titleBuilder.Append(' ');
            }

            if (!string.IsNullOrEmpty(group.Title))
            {
                titleBuilder.Append("**");
                titleBuilder.Append(group.Title);
                titleBuilder.Append("** ");
            }

            titleBuilder.Append('(');
            titleBuilder.Append(count);
            titleBuilder.Append(')');

            return titleBuilder.ToString();
        }

        private static DiscordColor GetEmbedColor(RaidStatus status)
        {
            switch (status)
            {
                case RaidStatus.Scheduled:
                    return DiscordColor.Green;
                case RaidStatus.Cancelled:
                    return DiscordColor.Red;
                default:
                    return DiscordColor.Gray;
            }
        }
    }
}
