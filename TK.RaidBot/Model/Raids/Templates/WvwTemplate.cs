using System;
using System.Collections.Generic;

namespace TK.RaidBot.Model.Raids.Templates
{
    public class WvwTemplate : IRaidTemplate
    {
        private static readonly HashSet<int> Trash = new HashSet<int>() {
            Professions.Ranger.Id,
            Professions.Druid.Id,
            Professions.Soulbeast.Id,
            Professions.Thief.Id,
            Professions.Deadeye.Id
        };

        private static readonly RaidGroup[] WvwGroups = new []
        {
            CreateGroup(Professions.Firebrand),
            CreateGroup(Professions.Scrapper),
            CreateGroup(Professions.Reaper, Professions.Scourge),
            CreateGroup(Professions.Herald, Professions.Renegade),
            CreateGroup(Professions.Spellbreaker),
            CreateGroup("Другое"),
        };

        public string Code => "wvw";

        public IEnumerable<RaidGroup> Groups => WvwGroups;

        public bool AllowsProfession(Profession profession)
        {
            return !Trash.Contains(profession.Id);
        }

        private static RaidGroup CreateGroup(string title)
        {
            return new RaidGroup(title, p => p.Status == ParticipationStatus.Available)
            {
                Inline = true,
                ShowParticipantRole = true,
            };
        }

        private static RaidGroup CreateGroup(params Profession[] professions)
        {
            return new RaidGroup("", professions)
            {
                Inline = true,
                ShowParticipantRole = false,
            };
        }
    }
}
