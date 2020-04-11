using System;
using System.Collections.Generic;
using System.Linq;

namespace TK.RaidBot.Model.Raids
{
    public class RaidGroup
    {
        private static readonly Profession[] NoProfessions = new Profession[0];

        private readonly Func<RaidParticipant, bool> filter;

        public RaidGroup(string title, IEnumerable<Profession> includedProfessions)
        {
            this.Title = title;
            this.Professions = includedProfessions.ToArray();
            this.filter = participant =>
                participant.Status == ParticipationStatus.Available &&
                includedProfessions.Any(p => p.Id == participant.Role);
        }

        public RaidGroup(string title, Func<RaidParticipant, bool> filter)
        {
            this.Title = title;
            this.Professions = NoProfessions;
            this.filter = filter;
        }

        public string Title { get; set; }

        public Profession[] Professions { get; }

        public bool ShowParticipantRole { get; set; }

        public bool Inline { get; set; }

        public bool Includes(RaidParticipant participant)
        {
            return filter(participant);
        }
    }
}
