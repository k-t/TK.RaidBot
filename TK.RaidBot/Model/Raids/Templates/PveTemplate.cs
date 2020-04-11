using System;
using System.Collections.Generic;

namespace TK.RaidBot.Model.Raids.Templates
{
    public class PveTemplate : IRaidTemplate
    {
        private static readonly RaidGroup[] PveGroups = new []
        {
            // just include all professions to a single group
            CreateGroup("Появятся"),
        };

        public string Code => "pve";

        public IEnumerable<RaidGroup> Groups => PveGroups;

        public bool AllowsProfession(Profession _)
        {
            return true;
        }

        private static RaidGroup CreateGroup(string title)
        {
            return new RaidGroup(title, p => p.Status == ParticipationStatus.Available)
            {
                Inline = false,
                ShowParticipantRole = true,
            };
        }
    }
}
