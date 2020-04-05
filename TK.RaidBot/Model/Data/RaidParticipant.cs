namespace TK.RaidBot.Model.Data
{
    public class RaidParticipant
    {
        public ulong UserId { get; set; }

        public ParticipationStatus Status { get; set; }

        public RaidRole Role { get; set; }
    }
}
