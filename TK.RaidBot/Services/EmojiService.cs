using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using TK.RaidBot.Model.Data;

namespace TK.RaidBot.Services
{
    public class EmojiService
    {
        private readonly Dictionary<RaidRole, DiscordEmoji> roles;
        private readonly Dictionary<ParticipationStatus, DiscordEmoji> statues;

        public EmojiService()
        {
            roles = new Dictionary<RaidRole, DiscordEmoji>();
            statues = new Dictionary<ParticipationStatus, DiscordEmoji>();
        }

        public DiscordEmoji GetRoleEmoji(DiscordClient client, RaidRole role)
        {
            if (!roles.TryGetValue(role, out DiscordEmoji result))
            {
                result = DiscordEmoji.FromName(client, GetEmojiName(role));
                roles[role] = result;
            }
            return result;
        }

        public DiscordEmoji GetStatusEmoji(DiscordClient client, ParticipationStatus status)
        {
            if (!statues.TryGetValue(status, out DiscordEmoji result))
            {
                result = DiscordEmoji.FromName(client, GetEmojiName(status));
                statues[status] = result;
            }
            return result;
        }

        public ParticipationStatus? GetStatusByEmoji(DiscordEmoji emoji)
        {
            switch (emoji.GetDiscordName())
            {
                case ":white_check_mark:":
                    return ParticipationStatus.Available;
                case ":no_entry_sign:":
                    return ParticipationStatus.NotAvailable;
                case ":grey_question:":
                    return ParticipationStatus.Maybe;
                default:
                    return null;
            }
        }

        public RaidRole? GetRoleByEmoji(DiscordEmoji emoji)
        {
            var roleName = emoji.GetDiscordName().Trim(':');

            if (Enum.TryParse(roleName, out RaidRole role))
                return role;

            return null;
        }

        private static string GetEmojiName(RaidRole role)
        {
            switch (role)
            {
                //case RaidRole.Daredevil:
                //    return ":wastebasket:";
                case RaidRole.Unknown:
                    return ":question:";
                default:
                    return $":{role}:";
            }
        }

        private static string GetEmojiName(ParticipationStatus status)
        {
            switch (status)
            {
                case ParticipationStatus.Available:
                    return ":white_check_mark:";
                case ParticipationStatus.NotAvailable:
                    return ":no_entry_sign:";
                default:
                    return ":grey_question:";
            }
        }
    }
}
