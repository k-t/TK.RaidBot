using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using TK.RaidBot.Model.Raids;

namespace TK.RaidBot.Services
{
    public class RaidMessageService
    {
        private static readonly HashSet<RaidRole> Trash = new HashSet<RaidRole>() {
            RaidRole.Ranger,
            RaidRole.Druid,
            RaidRole.Soulbeast,
            RaidRole.Thief,
            RaidRole.Deadeye
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

            // grouping

            var otherGroup = CreateOtherRolesGroup();

            var groups = new[] {
                CreateRoleGroup(client, RaidRole.Firebrand),
                CreateRoleGroup(client, RaidRole.Scrapper),
                CreateRoleGroup(client, RaidRole.Reaper, RaidRole.Scourge),
                CreateRoleGroup(client, RaidRole.Herald, RaidRole.Renegade),
                CreateRoleGroup(client, RaidRole.Spellbreaker),
                otherGroup,
                CreateReserveGroup(),
                CreateCancelledGroup()
            };

            int totalCount = 0;
            int availableCount = 0;

            foreach (var participant in raid.Participants.OrderBy(x => x.Role))
            {
                var member = guild.GetMemberAsync(participant.UserId).Result;
                if (member == null)
                    continue;

                var group = groups.FirstOrDefault(x => x.Includes(participant));
                if (group != null)
                {
                    var role = group.DisplayRole
                        ? emojiService.GetRoleEmoji(client, participant.Role).ToString()
                        : null;

                    group.AddParticipant(member.DisplayName, role);
                }
                else if (participant.Status == ParticipationStatus.Available)
                {
                    var role = emojiService.GetRoleEmoji(client, RaidRole.Unknown);
                    otherGroup.AddParticipant(member.DisplayName, role);
                }

                totalCount++;

                if (participant.Status == ParticipationStatus.Available)
                {
                    availableCount++;
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

            foreach (var group in groups)
            {
                embed.AddField(group.GetTitle(), group.GetParticipants(), group.Inline);
            }

            embed.AddField(
                name: "**Всего**",
                value: $"Отметилось **{totalCount}**\nПридет **{availableCount}**");

            return embed.Build();
        }

        private RaidGroup CreateRoleGroup(DiscordClient client, params RaidRole[] roles)
        {
            if (roles == null || roles.Length == 0)
                throw new ArgumentException("At least one role must be specified", nameof(roles));

            var titleBuilder = new StringBuilder();

            foreach (var role in roles)
            {
                titleBuilder.Append(emojiService.GetRoleEmoji(client, role));
            }

            return new RaidGroup(
                title: titleBuilder.ToString(),
                filter: x => x.Status == ParticipationStatus.Available && roles.Contains(x.Role),
                displayRole: false,
                inline: true);
        }

        private RaidGroup CreateOtherRolesGroup()
        {
            return new RaidGroup(
                title: "Другое",
                filter: x => x.Status == ParticipationStatus.Available && IsAcceptableRole(x.Role),
                displayRole: true,
                inline: true);
        }

        private RaidGroup CreateReserveGroup()
        {
            return new RaidGroup(
                title: "Возможно появятся",
                filter: x => x.Status == ParticipationStatus.Maybe,
                displayRole: true,
                inline: false);
        }

        private RaidGroup CreateCancelledGroup()
        {
            return new RaidGroup(
                title: "Не смогут пойти",
                filter: x => x.Status == ParticipationStatus.NotAvailable,
                displayRole: true,
                inline: false);
        }

        private bool IsAcceptableRole(RaidRole role)
        {
            return !Trash.Contains(role);
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
