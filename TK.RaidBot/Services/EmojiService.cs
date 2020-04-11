using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using TK.RaidBot.Model;
using TK.RaidBot.Model.Raids;

namespace TK.RaidBot.Services
{
    public class EmojiService
    {
        private readonly Dictionary<int, DiscordEmoji> roles;
        private readonly Dictionary<ParticipationStatus, DiscordEmoji> statues;

        public EmojiService()
        {
            roles = new Dictionary<int, DiscordEmoji>();
            statues = new Dictionary<ParticipationStatus, DiscordEmoji>();
        }

        public DiscordEmoji GetProfessionEmoji(DiscordClient client, int professionId)
        {
            if (!roles.TryGetValue(professionId, out DiscordEmoji result))
            {
                var profession = Professions.GetById(professionId);
                result = profession != null
                    ? DiscordEmoji.FromName(client, profession.EmojiName)
                    : null;
                roles[profession.Id] = result;
            }
            return result;
        }

        public DiscordEmoji GetRoleEmoji(DiscordClient client, Profession profession)
        {
            if (profession == null)
                throw new ArgumentNullException(nameof(profession));

            return GetProfessionEmoji(client, profession.Id);
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

        public ParticipationStatus? GetStatusByEmoji(string emojiName)
        {
            switch (emojiName)
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
